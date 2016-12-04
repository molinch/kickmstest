using Microsoft.CodeAnalysis;
using System.Linq;

namespace RoslynHelpers
{
    public static class NamedTypeSymbolExtensions
    {
        public static string GetGenericTypeArgument(this INamedTypeSymbol typeSymbol)
        {
            var typeArgument = typeSymbol.TypeArguments.FirstOrDefault();
            return typeArgument?.ToString();
        }
    }
}
