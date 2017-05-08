using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynHelpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Stubs2Moqs
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                if (string.IsNullOrEmpty(options.SolutionPath))
                {
                    throw new ArgumentException("Solution path is required");
                }

                ParseCsharpFiles(options.SolutionPath, options.Preview, options.TfsExePath).Wait();
            }
        }

        private static async Task ParseCsharpFiles(string solutionFullPath, bool previewOnly, string tfsExePath)
        {
            foreach (var item in await GetUnitTestItems(solutionFullPath))
            {
                var stub2Moq = new MsTestStub2Moq(item.Workspace, item.File, item.SemanticModel);
                var rewriter = new MsTestStubRewriter(stub2Moq);
                var newTestFile = rewriter.Visit(item.Root);

                if (newTestFile != item.Root)
                {
                    var root = newTestFile.SyntaxTree.GetRoot();
                    var compilationUnitSyntax = root as CompilationUnitSyntax;

                    if (compilationUnitSyntax != null)
                    {
                        bool hasMoqUsing = compilationUnitSyntax.Usings
                            .Select(u => u.Name)
                            .OfType<IdentifierNameSyntax>()
                            .Any(u => u.Identifier.ValueText.Equals("Moq", StringComparison.InvariantCulture));

                        if (!hasMoqUsing)
                        {
                            var moq = SyntaxFactory.ParseName("Moq");
                            var moqUsing = SyntaxFactory.UsingDirective(moq).NormalizeWhitespace().WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);
                            root = compilationUnitSyntax.AddUsings(moqUsing);
                        }
                    }

                    var newDoc = item.File.WithSyntaxRoot(root);
                    var newSemanticModel = await newDoc.GetSemanticModelAsync().ConfigureAwait(false);
                    root = await newDoc.GetSyntaxRootAsync();

                    // remove useless usings (.Fakes and others)
                    root = UsingsHelper.RemoveUnusedImportDirectives(newSemanticModel, root, CancellationToken.None, (r, usingNode) =>
                    {
                        if (usingNode.ToString().Contains("Moq"))
                        {
                            return !r.ToString().Contains("Mock");
                        }
                        else
                        {
                            return true;
                        }
                    });
   
                    if (previewOnly)
                    {
                        Console.WriteLine(newTestFile.ToFullString());
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(tfsExePath))
                        {
                            TfsCheckOut(tfsExePath, item.File.FilePath);
                        }

                        using (var writer = new StreamWriter(item.File.FilePath, false, System.Text.UTF8Encoding.UTF8))
                        {
                            root.WriteTo(writer);
                        }
                    }
                }
            }
        }

        private static void TfsCheckOut(string tfsExePath, string path)
        {
            var startInfo = new ProcessStartInfo(tfsExePath, "checkout " + path);
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            var process = Process.Start(startInfo);
            process.WaitForExit();
        }

        private static async Task<List<UnitTestClassItem>> GetUnitTestItems(string solutionFullPath)
        {
            var unitTestItems = new List<UnitTestClassItem>();

            foreach (var project in await SolutionHelper.GetProjectsAsync(solutionFullPath).ConfigureAwait(false))
            {
                foreach (var csFile in project.GetCSharpFiles())
                {
                    var root = await csFile.GetSyntaxRootAsync().ConfigureAwait(false);
                    if (root.ToFullString().Contains(".Fakes"))
                    {
                        var semanticModel = await csFile.GetSemanticModelAsync().ConfigureAwait(false);

                        foreach (var unitTestClass in root.DescendantNodes()
                            .OfType<ClassDeclarationSyntax>())
                        {
                            unitTestItems.Add(new UnitTestClassItem()
                            {
                                Workspace = project.Solution.Workspace,
                                File = csFile,
                                Root = root,
                                SemanticModel = semanticModel,
                                UnitTestClass = unitTestClass
                            });
                        }
                    }
                }
            }

            return unitTestItems;
        }
    }
}