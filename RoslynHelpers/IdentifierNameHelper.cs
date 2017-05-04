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

        public static SimpleNameSyntax CreateName(string name, IEnumerable<string> typeArguments)
        {
            var arguments = typeArguments?.ToList();
            if (arguments == null || arguments.Count == 0)
            {
                return SyntaxFactory.IdentifierName(name);
            }
            else
            {
                return CreateGenericName(name, typeArguments);
            }
        }

        public static GenericNameSyntax CreateGenericName(string name, IEnumerable<string> types)
        {
            var argTypes = types.Select(t => SyntaxFactory.IdentifierName(t));

            return SyntaxFactory.GenericName(SyntaxFactory.Identifier(name))
                .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList<TypeSyntax>(argTypes)));
        }
    }
}
