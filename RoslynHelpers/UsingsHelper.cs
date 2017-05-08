using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RoslynHelpers
{
    public static class UsingsHelper
    {
        public static SyntaxNode RemoveUnusedImportDirectives(SemanticModel semanticModel, SyntaxNode root, CancellationToken cancellationToken, Func<SyntaxNode, UsingDirectiveSyntax, bool> markUsingAsUnused)
        {
            var oldUsings = root.DescendantNodesAndSelf().Where(s => s is UsingDirectiveSyntax);
            var unusedUsings = GetUnusedImportDirectives(semanticModel, root, cancellationToken, markUsingAsUnused);
            var leadingTrivia = root.GetLeadingTrivia();

            root = root.RemoveNodes(oldUsings, SyntaxRemoveOptions.KeepNoTrivia);
            var newUsings = SyntaxFactory.List(oldUsings.Except(unusedUsings));

            root = ((CompilationUnitSyntax)root)
                .WithUsings(newUsings)
                .WithLeadingTrivia(leadingTrivia);

            return root;
        }

        private static HashSet<SyntaxNode> GetUnusedImportDirectives(SemanticModel model, SyntaxNode root, CancellationToken cancellationToken, Func<SyntaxNode, UsingDirectiveSyntax, bool> markUsingAsUnused)
        {
            var unusedImportDirectives = new HashSet<SyntaxNode>();
            foreach (var diagnostic in model.GetDiagnostics(null, cancellationToken).Where(d => d.Id == "CS8019" || d.Id == "CS0105"))
            {
                var usingDirectiveSyntax = root.FindNode(diagnostic.Location.SourceSpan, false, false) as UsingDirectiveSyntax;
                if (usingDirectiveSyntax != null)
                {
                    if (markUsingAsUnused(root, usingDirectiveSyntax))
                    {
                        unusedImportDirectives.Add(usingDirectiveSyntax);
                    }
                }
            }

            return unusedImportDirectives;
        }
    }
}
