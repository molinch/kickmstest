using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Borrowed & changed code from Roslynator in order to expand initializers
// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace RoslynHelpers
{
    public static class InitializersToExpressions
    {
        public static IEnumerable<ExpressionStatementSyntax> Expand(InitializerExpressionSyntax initializer, ExpressionSyntax initializedExpression)
        {
            foreach (ExpressionSyntax expression in initializer.Expressions)
            {
                SyntaxKind kind = expression.Kind();

                if (kind == SyntaxKind.SimpleAssignmentExpression)
                {
                    var assignment = (AssignmentExpressionSyntax)expression;
                    ExpressionSyntax left = assignment.Left;
                    ExpressionSyntax right = assignment.Right;

                    if (left.IsKind(SyntaxKind.ImplicitElementAccess))
                    {
                        yield return SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.ElementAccessExpression(
                                    initializedExpression,
                                    ((ImplicitElementAccessSyntax)left).ArgumentList),
                                right)).NormalizeWhitespace();
                    }
                    else
                    {
                        yield return SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                    initializedExpression,
                                    (IdentifierNameSyntax)left),
                                right)).NormalizeWhitespace();
                    }
                }
                else if (kind == SyntaxKind.ComplexElementInitializerExpression)
                {
                    var elementInitializer = (InitializerExpressionSyntax)expression;

                    yield return SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.ElementAccessExpression(
                                initializedExpression,
                                SyntaxFactory.BracketedArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(elementInitializer.Expressions[0])))),
                            elementInitializer.Expressions[1])).NormalizeWhitespace();
                }
                else
                {
                    yield return SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                initializedExpression,
                                SyntaxFactory.IdentifierName("Add")),
                            SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new[] { SyntaxFactory.Argument(expression) }))
                        )
                    ).NormalizeWhitespace();
                }
            }
        }
    }
}
