using Microsoft.CodeAnalysis;
using RoslynHelpers;
using System;
using System.Collections.Generic;
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

        public bool IsFakesDelegateProperty(ISymbol member)
        {
            if (member.DeclaredAccessibility == Accessibility.Public && member.Kind == SymbolKind.Property)
            {
                var property = (IPropertySymbol)member;
                var propertyTypeName = property.Type.ToDisplayString();
                return propertyTypeName.StartsWith("Microsoft.QualityTools.Testing.Fakes.FakesDelegates", StringComparison.InvariantCulture);
            }

            return false;
        }

        public bool IsFakesDelegatePropertySetter(INamedTypeSymbol fakeStub, string memberName, out IPropertySymbol fakesDelegateProperty)
        {
            fakesDelegateProperty = null;

            var typeMembers = fakeStub.GetMembers();
            foreach (var typeMember in typeMembers)
            {
                if (IsFakesDelegateProperty(typeMember) && typeMember.Name == memberName)
                {
                    fakesDelegateProperty = (IPropertySymbol)typeMember;
                    return true;
                }
            }

            return false;
        }
    }
}
