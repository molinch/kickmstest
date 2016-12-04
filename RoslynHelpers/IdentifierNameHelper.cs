using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoslynHelpers
{
    public static class IdentifierNameHelper
    {
        public static GenericNameSyntax CreateGenericName(string genericType, string type)
        {
            return CreateGenericName(genericType, new[] { type });
        }

        public static GenericNameSyntax CreateGenericName(string genericType, IEnumerable<string> types)
        {
            var argTypes = types.Select(t => SyntaxFactory.IdentifierName(t));

            return SyntaxFactory.GenericName(SyntaxFactory.Identifier(genericType))
                .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList<TypeSyntax>(argTypes)));
        }
    }
}
