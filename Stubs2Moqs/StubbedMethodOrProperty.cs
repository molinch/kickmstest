using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Shims2Moqs
{
    public class StubbedMethodOrProperty
    {
        public StubbedMethodOrProperty(INamedTypeSymbol type, ISymbol methodOrPropertySymbol, IEnumerable<ITypeSymbol> arguments, ITypeSymbol returnType)
        {
            Type = type;
            MethodOrPropertySymbol = methodOrPropertySymbol;
            Arguments = arguments;
            ReturnType = returnType;
        }

        public INamedTypeSymbol Type { get; }

        public ISymbol MethodOrPropertySymbol { get; }

        public IEnumerable<ITypeSymbol> Arguments { get; }

        public ITypeSymbol ReturnType { get; }
    }
}
