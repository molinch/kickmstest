using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RoslynHelpers
{
    public static class ClassDeclarationSyntaxExtensions
    {
        public static bool HasAttribute(this ClassDeclarationSyntax node, string attributeName)
        {
            foreach (var attrList in node.AttributeLists)
            {
                foreach (var attr in attrList.Attributes)
                {
                    if (attr.Name.ToString() == attributeName)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool InheritsFrom(this ClassDeclarationSyntax node, string typeFullName, SemanticModel semanticModel)
        {
            var symbol = semanticModel.GetDeclaredSymbol(node);
            return symbol.InheritsFrom(typeFullName);
        }
    }
}
