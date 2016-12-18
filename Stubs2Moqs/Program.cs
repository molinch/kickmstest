using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

                ParseCsharpFiles(options.SolutionPath, options.Preview).Wait();
            }
        }

        private static async Task ParseCsharpFiles(string solutionFullPath, bool previewOnly)
        {
            foreach (var item in await GetUnitTestItems(solutionFullPath))
            {
                var stub2Moq = new MsTestStub2Moq(item.Workspace, item.File, item.SemanticModel);
                var rewriter = new MsTestStubRewriter(stub2Moq);
                var newTestClass = rewriter.Visit(item.Root);

                if (newTestClass != item.Root)
                {
                    var root = newTestClass.SyntaxTree.GetRoot();
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
                            newTestClass = compilationUnitSyntax
                                .AddUsings(moqUsing);
                        }
                    }

                    var newDoc = item.File.WithSyntaxRoot(newTestClass);
                    var newSemanticModel = await newDoc.GetSemanticModelAsync().ConfigureAwait(false);

                    if (previewOnly)
                    {
                        Console.WriteLine(newTestClass.ToFullString());
                    }
                    else
                    {
                        using (var writer = new StreamWriter(item.File.FilePath))
                        {
                            newTestClass.WriteTo(writer);
                        }
                    }
                }
            }
        }

        private static async Task<List<UnitTestClassItem>> GetUnitTestItems(string solutionFullPath)
        {
            var unitTestItems = new List<UnitTestClassItem>();

            foreach (var project in await SolutionHelper.GetProjectsAsync(solutionFullPath).ConfigureAwait(false))
            {
                foreach (var csFile in project.GetCSharpFiles())
                {
                    var root = await csFile.GetSyntaxRootAsync().ConfigureAwait(false);
                    var semanticModel = await csFile.GetSemanticModelAsync().ConfigureAwait(false);

                    foreach (var unitTestClass in root.DescendantNodes()
                        .OfType<ClassDeclarationSyntax>()
                        .Where(c => c.HasAttribute("TestClass")))
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

            return unitTestItems;
        }
    }
}