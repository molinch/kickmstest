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
        
        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            var newNode = stub2Moq.SwitchPropertyDeclarationType(node);
            if (newNode != null)
                return newNode;

            return base.VisitPropertyDeclaration(node);
        }

        public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            var newNode = stub2Moq.SwitchFieldDeclarationType(node);
            if (newNode != null)
                return newNode;

            return base.VisitFieldDeclaration(node);
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
            SyntaxNode newNode = stub2Moq.TryReplaceAssignmentExpressionWithMethodCall(node);
            if (newNode != null)
            {
                return newNode;
            }

            var assignementExpression = node.ChildNodes().OfType<AssignmentExpressionSyntax>().FirstOrDefault();
            if (assignementExpression != null)
            {
                newNode = stub2Moq.SwitchType(assignementExpression);
                if (newNode != null)
                {
                    return newNode;
                }
            }

            return base.VisitExpressionStatement(node);
        }
    }
}
