﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoslynHelpers
{
    public static class TypeSymbolExtensions
    {
        public static bool InheritsFrom(this ITypeSymbol symbol, string typeFullName)
        {
            while (true)
            {
                if (symbol.ToString() == typeFullName)
                {
                    return true;
                }
                if (symbol.BaseType != null)
                {
                    symbol = symbol.BaseType;
                    continue;
                }
                break;
            }
            return false;
        }

        public static IEnumerable<ISymbol> GetMatchingPublicMembers(this ITypeSymbol symbol, Func<ISymbol, bool> predicate)
        {
            return symbol.GetMembers()
                .Where(m => m.DeclaredAccessibility == Accessibility.Public)
                .Where(m => predicate(m));
        }

        public static bool HasPropertyWithSameName(this ITypeSymbol symbol, string propertyName)
        {
            return symbol.GetMembers()
                .Where(m => m.DeclaredAccessibility == Accessibility.Public && m.Kind == SymbolKind.Property)
                .Any(m => m.Name.Equals(propertyName, StringComparison.InvariantCulture));
        }

        public static ISymbol GetMemberWithSameSignature(this ITypeSymbol symbol, string memberName, IEnumerable<ISymbol> memberArguments)
        {
            var arguments = memberArguments.ToList();

            var stubbedSymbol = symbol.GetMembers()
                .Where(m => m.DeclaredAccessibility == Accessibility.Public)
                .Where(m =>
                {
                    if (!m.Name.Equals(memberName, StringComparison.InvariantCulture))
                        return false;

                    var method = m as IMethodSymbol;
                    if (method == null) // it is null if it's a property or a field
                        return arguments.Count == 0;

                    if (arguments.Count != method.Parameters.Count())
                        return false;

                    for (int i=0; i<arguments.Count; i++)
                    {
                        var argument = arguments[i];
                        var methodParameter = method.Parameters[i].Type;

                        if (argument.ToString() != methodParameter.ToString()) // .Equals won't work in some cases, that's why we compare string representation
                            return false;
                    }

                    return true;
                }).FirstOrDefault();

            return stubbedSymbol;
        }
    }
}
