using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shims2Moqs
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

                ParseCsharpFiles(options.SolutionPath).Wait();
            }
        }

        private static async Task ParseCsharpFiles(string solutionFullPath)
        {
            foreach (var item in await GetUnitTestItems(solutionFullPath))
            {
                var msTestHelper = new MsTestHelper(item.File, item.SemanticModel);
                
                var rewriter = new MsTestStub2Moq(item.Workspace, item.File, item.SemanticModel, msTestHelper);
                var newTestClass = rewriter.Visit(item.UnitTestClass);

                if (newTestClass != item.Root)
                {
                    Console.WriteLine(newTestClass.ToFullString());
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