using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynHelpers;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stubs2Moqs
{
    public class MsTestHelper
    {
        private readonly Document document;
        private readonly SemanticModel semanticModel;

        public MsTestHelper(Document document, SemanticModel semanticModel)
        {
            this.document = document;
            this.semanticModel = semanticModel;
        }

        public bool IsStub(INamedTypeSymbol typeSymbol)
        {
            return typeSymbol.InheritsFrom("Microsoft.QualityTools.Testing.Fakes.Stubs.StubBase");
        }

        public bool IsFakesDelegateProperty(ISymbol member, out INamedTypeSymbol fakesDelegateType, out ImmutableArray<ITypeSymbol> methodTypeArguments)
        {
            fakesDelegateType = null;

            if (member.DeclaredAccessibility == Accessibility.Public)
            {
                switch (member.Kind)
                {
                    case SymbolKind.Field:
                        var field = (IFieldSymbol)member;
                        var fieldTypeName = field.Type.ToDisplayString();
                        bool isFake = fieldTypeName.StartsWith("Microsoft.QualityTools.Testing.Fakes.FakesDelegates", StringComparison.InvariantCulture);
                        if (isFake)
                        {
                            fakesDelegateType = (INamedTypeSymbol)field.Type;
                        }
                        return isFake;

                    case SymbolKind.Property:
                        var property = (IPropertySymbol)member;
                        var propertyTypeName = property.Type.ToDisplayString();
                        isFake =  propertyTypeName.StartsWith("Microsoft.QualityTools.Testing.Fakes.FakesDelegates", StringComparison.InvariantCulture);
                        if (isFake)
                        {
                            fakesDelegateType = (INamedTypeSymbol)property.Type;
                        }
                        return isFake;

                    case SymbolKind.Method:
                        var method = (IMethodSymbol)member;
                        if (method.Parameters.Length == 1)
                        {
                            var parameter = method.Parameters[0];
                            var methodTypeName = parameter.ToString();
                            if (methodTypeName != null)
                            {
                                isFake = methodTypeName.StartsWith("Microsoft.QualityTools.Testing.Fakes.FakesDelegates", StringComparison.InvariantCulture);
                                if (isFake)
                                {
                                    fakesDelegateType = (INamedTypeSymbol)parameter.Type;
                                    methodTypeArguments = method.TypeArguments;
                                }
                                return isFake;
                            }
                        }
                        return false;
                }

            }

            return false;
        }

        public bool IsFakesDelegateMethodOrPropertySetter(INamedTypeSymbol fakeStub, string memberName, out INamedTypeSymbol fakesDelegateType, out ImmutableArray<ITypeSymbol> methodTypeArguments)
        {
            fakesDelegateType = null;
            var typeMembers = fakeStub.GetMembers();
            foreach (var typeMember in typeMembers)
            {
                if (IsFakesDelegateProperty(typeMember, out fakesDelegateType, out methodTypeArguments) && typeMember.Name == memberName)
                {
                    return true;
                }
            }

            return false;
        }

        public ISymbol GetOriginalSymbolFromFakeCallName(string fakeCallName, List<ITypeSymbol> lambdaArguments, INamedTypeSymbol originalType)
        {
            string originalName = GetOriginalMethodNameOrPropertyName(fakeCallName, lambdaArguments, originalType);
            return GetOriginalSymbol(originalName, lambdaArguments, originalType);
        }

        private ISymbol GetOriginalSymbol(string originalName, List<ITypeSymbol> lambdaArguments, INamedTypeSymbol originalType)
        {
            var originalMethodOrPropertySymbol = originalType.GetMemberWithSameSignature(originalName, lambdaArguments);
            if (originalMethodOrPropertySymbol == null)
            {
                // special case to handle stubbed generic methods. In that case the name becomes freaking weird thanks to MsTest.
                var lastIndexOf = originalName.LastIndexOf("Of");
                if (lastIndexOf > 0)
                {
                    var partName = originalName.Substring(0, lastIndexOf);
                    return GetOriginalSymbol(partName, lambdaArguments, originalType);
                }
            }

            return originalMethodOrPropertySymbol;
        }

        private string GetOriginalMethodNameOrPropertyName(string fakeCallName, List<ITypeSymbol> lambdaArguments, INamedTypeSymbol originalType)
        {
            // More information can be found on MSDN: https://msdn.microsoft.com/en-us/en-en/library/hh549174.aspx

            // For properties we should remove the Get/Set part at the end of name
            if (fakeCallName.EndsWith("Get", StringComparison.InvariantCulture) || fakeCallName.EndsWith("Set", StringComparison.InvariantCulture))
            {
                string propertyName = fakeCallName.Substring(0, fakeCallName.Length - 3);
                if (originalType.HasPropertyWithSameName(propertyName))
                {
                    return propertyName;
                }
            }

            // For methods we should remove extra Type names at the end of name
            string originalMethodOrPropertyName = fakeCallName;
            var reverseLambdaArguments = lambdaArguments.Reverse<ITypeSymbol>().ToList();
            for (int i = 0; i < reverseLambdaArguments.Count; i++)
            {
                var argument = reverseLambdaArguments[i];

                var argumentParts = argument.ToDisplayParts(SymbolDisplayFormat.MinimallyQualifiedFormat);
                var genericFakeName = GetFakeDisplayParts(argumentParts).ToList().Aggregate((a, b) => a + b);

                if (!originalMethodOrPropertyName.EndsWith(genericFakeName))
                    break;

                // remove type name from name, when needed only (for example MultiplyInt32Int32 --> Multiply)
                originalMethodOrPropertyName = originalMethodOrPropertyName.Substring(0, originalMethodOrPropertyName.Length - genericFakeName.Length);
            }

            return originalMethodOrPropertyName;
        }

        private IEnumerable<string> GetFakeDisplayParts(ImmutableArray<SymbolDisplayPart> parts)
        {
            foreach (var part in parts)
            {
                var sPart = part.Symbol == null ? part.ToString() : part.Symbol.Name;
                switch (sPart)
                {
                    case "<":
                        yield return "Of";
                        break;

                    case " ":
                    case ">":
                    case ",":
                        break;

                    default:
                        yield return sPart;
                        break;
                }
            }
        }
    }
}
