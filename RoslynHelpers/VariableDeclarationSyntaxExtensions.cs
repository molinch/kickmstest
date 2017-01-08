using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RoslynHelpers
{
    public static class VariableDeclarationSyntaxExtensions
    {
        public static TypeInfo? GetDeclarationTypeInfo(this VariableDeclarationSyntax variableDeclaration, SemanticModel semanticModel)
        {
            if (variableDeclaration.Variables.Count != 1)
                return null;

            ExpressionSyntax initializerValue = variableDeclaration.Variables[0].Initializer?.Value;
            if (initializerValue == null)
                return null;

            return semanticModel.GetTypeInfo(variableDeclaration.Type);
        }

        public static TypeInfo? GetExpressionTypeInfo(this VariableDeclarationSyntax variableDeclaration, SemanticModel semanticModel)
        {
            if (variableDeclaration.Variables.Count != 1)
                return null;

            ExpressionSyntax initializerValue = variableDeclaration.Variables[0].Initializer?.Value;
            if (initializerValue == null)
                return null;

            return semanticModel.GetTypeInfo(initializerValue);
        }

        public static TypeInfo? GetTypeInfo(this VariableDeclarationSyntax variableDeclaration, SemanticModel semanticModel)
        {
            var expressionTypeInfo = GetExpressionTypeInfo(variableDeclaration, semanticModel);
            if (expressionTypeInfo != null)
                return expressionTypeInfo;

            return GetDeclarationTypeInfo(variableDeclaration, semanticModel);
        }

        public static ITypeSymbol GetTypeSymbol(this VariableDeclarationSyntax variableDeclaration, SemanticModel semanticModel)
        {
            var typeInfo = GetTypeInfo(variableDeclaration, semanticModel);
            return typeInfo?.ConvertedType;
        }

        public static bool IsSameType(this VariableDeclarationSyntax variableDeclaration, SemanticModel semanticModel, string typeFullName)
        {
            var typeSymbol = GetTypeSymbol(variableDeclaration, semanticModel);
            if (typeSymbol == null)
                return false;

            return typeSymbol.ToDisplayString().Equals(typeFullName, System.StringComparison.InvariantCulture);
        }
    }
}
