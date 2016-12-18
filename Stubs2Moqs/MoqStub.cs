using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stubs2Moqs
{
    public class MoqStub
    {
        public MoqStub(StubbedCall originalMsStub, InvocationExpressionSyntax newNode)
        {
            OriginalMsStub = originalMsStub;
            NewNode = newNode;
        }

        public StubbedCall OriginalMsStub { get; }

        public InvocationExpressionSyntax NewNode { get; }

        public SyntaxToken StubDefinitionIdentifier
        {
            get
            {
                return GetStubDefinitionIdentifier(OriginalMsStub.Identifier.Identifier.ValueText);
            }
        }

        public IdentifierNameSyntax StubDefinitionIdentifierName
        {
            get
            {
                return GetStubDefinitionIdentifierName(OriginalMsStub.Identifier.Identifier.ValueText);
            }
        }

        public static SyntaxToken GetStubDefinitionIdentifier(string originalName)
        {
            return SyntaxFactory.Identifier(originalName + "Def");
        }

        public static IdentifierNameSyntax GetStubDefinitionIdentifierName(string originalName)
        {
            return SyntaxFactory.IdentifierName(originalName + "Def");
        }
    }
}
