using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynHelpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Stubs2Moqs
{
    public class MsTestStubRewriter : CSharpSyntaxRewriter
    {
        private readonly MsTestStub2Moq stub2Moq;

        public MsTestStubRewriter(MsTestStub2Moq stub2Moq)
        {
            this.stub2Moq = stub2Moq;
        }

        public override SyntaxNode VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            var newNode = stub2Moq.SwitchType(node);
            if (newNode != null)
            {
                return newNode;
            }

            return base.VisitLocalDeclarationStatement(node);
        }

        public override SyntaxNode VisitExpressionStatement(ExpressionStatementSyntax node)
        {
            var newNode = stub2Moq.TryReplaceAssignmentExpressionWithMethodCall(node);
            if (newNode != null)
            {
                return newNode;
            }

            return base.VisitExpressionStatement(node);
        }
    }
}
