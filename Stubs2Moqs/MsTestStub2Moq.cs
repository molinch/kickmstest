using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynHelpers;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Stubs2Moqs
{
    public class MsTestStub2Moq
    {
        private readonly Workspace workspace;
        private readonly Document document;
        private readonly SemanticModel semanticModel;
        private readonly MsTestHelper msTestHelper;

        public MsTestStub2Moq(Workspace workspace, Document document, SemanticModel semanticModel)
        {
            this.workspace = workspace;
            this.document = document;
            this.semanticModel = semanticModel;
            this.msTestHelper = new MsTestHelper(document, semanticModel);
        }
        
        public SyntaxNode SwitchType(LocalDeclarationStatementSyntax variableDeclaration)
        {
            var declarationTypeSymbol = variableDeclaration.Declaration.GetTypeSymbol(semanticModel) as INamedTypeSymbol;
            if (declarationTypeSymbol == null)
                return null;

            if (msTestHelper.IsStub(declarationTypeSymbol))
            {
                var typeToMock = declarationTypeSymbol.BaseType.GetGenericTypeArgument();
                if (typeToMock == null)
                    return null;

                return CreateLocalVariableDeclaration(variableDeclaration, typeToMock);
            }

            return null;
        }

        public SyntaxNode CreateLocalVariableDeclaration(LocalDeclarationStatementSyntax node, string typeToMock)
        {
            var variable = node.Declaration.Variables.FirstOrDefault();
            if (variable == null)
                return null;

            var mockedType = CreateMockType(typeToMock);
            var stubName = variable.Identifier.ValueText;

            var stubDefDeclaration = SyntaxFactory.LocalDeclarationStatement(
                SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName("var"))
                    .WithVariables(SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.VariableDeclarator(MoqStub.GetStubDefinitionIdentifier(stubName))
                            .WithInitializer(
                                SyntaxFactory.EqualsValueClause(
                                    SyntaxFactory.ObjectCreationExpression(mockedType)
                                        .WithArgumentList(SyntaxFactory.ArgumentList()
                                    )
                                )
                            )
                    ))
                )
                .NormalizeWhitespace()
                .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);

            var objectCreationExpression = node.DescendantNodes(s => !(s is ObjectCreationExpressionSyntax)).OfType<ObjectCreationExpressionSyntax>().FirstOrDefault();
            IEnumerable<ExpressionStatementSyntax> initializerExpression = new ExpressionStatementSyntax[0];
            if (objectCreationExpression?.Initializer != null)
            {
                var symbol = node.Declaration.GetTypeSymbol(semanticModel) as INamedTypeSymbol;

                initializerExpression =
                    InitializersToExpressions.Expand(objectCreationExpression.Initializer, SyntaxFactory.IdentifierName(stubName))
                        .Select(expressionStatementSyntax =>
                        {
                            var statementSyntax = expressionStatementSyntax.WithLeadingTrivia(node.GetLeadingTrivia());

                            var newAssignment = TryReplaceAssignmentExpressionWithMethodCall(statementSyntax, symbol);
                            if (newAssignment == null)
                            {
                                return statementSyntax;
                            }

                            return newAssignment
                                .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);
                        });
            }

            var stubDeclaration = SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName("var"))
                .WithVariables(SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.VariableDeclarator(variable.Identifier.ValueText)
                        .WithInitializer(
                            SyntaxFactory.EqualsValueClause(
                                SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, MoqStub.GetStubDefinitionIdentifierName(stubName), SyntaxFactory.IdentifierName("Object"))
                            )
                        )
                ))
                .NormalizeWhitespace()
                .WithLeadingTrivia(node.GetLeadingTrivia());

            var statements = new SyntaxList<StatementSyntax>();
            statements = statements.AddRange(new StatementSyntax[] { stubDefDeclaration }.Union(initializerExpression).Union(new StatementSyntax[] { SyntaxFactory.LocalDeclarationStatement(stubDeclaration) }));
            var wrapper = SyntaxFactory.Block(statements);
            wrapper = wrapper.WithOpenBraceToken(SyntaxFactory.MissingToken(SyntaxKind.OpenBraceToken)) // to remove scope {}
                .WithCloseBraceToken(SyntaxFactory.MissingToken(SyntaxKind.CloseBraceToken));

            return wrapper
                .WithLeadingTrivia(node.GetLeadingTrivia())
                .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);
        }
        
        public ExpressionStatementSyntax TryReplaceAssignmentExpressionWithMethodCall(ExpressionStatementSyntax node, INamedTypeSymbol typeSymbol = null)
        {
            // There are 2 cases to consider
            // Either we stub a method/property by setting a property; set value being the stub lambda
            // Or by invoking a method having a single argument; method argument being the stub lambda

            StubbedCall msStub = null;

            var assignment = node.ChildNodes().OfType<AssignmentExpressionSyntax>().FirstOrDefault();
            if (assignment != null)
            {
                var member = node.ChildNodes().FirstOrDefault() as MemberAccessExpressionSyntax;
                if (member == null)
                    return null;

                var resultExpression = assignment.Right;

                msStub = StubMsTestMethodOrPropertyCall(assignment, member, resultExpression, typeSymbol);
            }
            else
            {
                var invocation = node.ChildNodes().OfType<InvocationExpressionSyntax>().FirstOrDefault();
                if (invocation == null)
                    return null;

                var member = invocation.ChildNodes().FirstOrDefault() as MemberAccessExpressionSyntax;
                if (member == null)
                    return null;

                var argument = invocation.ArgumentList.ChildNodes().FirstOrDefault() as ArgumentSyntax;
                if (argument == null)
                    return null;

                msStub = StubMsTestMethodOrPropertyCall(invocation, member, argument.Expression, typeSymbol);
            }
            
            if (msStub != null)
            {
                var moqStub = MockMethodOrPropertyCall(msStub);

                var stubDefinition = SyntaxFactory.ExpressionStatement(moqStub.NewNode)
                    .WithLeadingTrivia(node.GetLeadingTrivia())
                    .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);

                return stubDefinition;
            }

            return null;
        }
        
        private GenericNameSyntax CreateMockType(string type)
        {
            return IdentifierNameHelper.CreateGenericName("Mock", type);
        }

        private StubbedCall StubMsTestMethodOrPropertyCall(ExpressionSyntax originalNode, MemberAccessExpressionSyntax member, ExpressionSyntax returnExpression, INamedTypeSymbol typeSymbol = null)
        {
            if (typeSymbol == null)
            {
                var identifier = member.Expression as IdentifierNameSyntax;
                if (identifier == null)
                    return null;

                var symbol = semanticModel.GetSymbolInfo(identifier).Symbol;
                typeSymbol = (symbol as ILocalSymbol)?.Type as INamedTypeSymbol;

                if (typeSymbol == null)
                    return null;
            }

            if (!msTestHelper.IsStub(typeSymbol))
                return null;

            var variableName = member.ChildNodes().OfType<IdentifierNameSyntax>().FirstOrDefault();
            if (variableName == null)
                return null;

            var methodOrPropertyCallIdentifier = member.ChildNodes().OfType<SimpleNameSyntax>().LastOrDefault();
            if (methodOrPropertyCallIdentifier == null)
                return null;

            INamedTypeSymbol fakesDelegateType;
            ImmutableArray<ITypeSymbol> methodTypeArguments;
            if (!msTestHelper.IsFakesDelegateMethodOrPropertySetter(typeSymbol, methodOrPropertyCallIdentifier.Identifier.ValueText, out fakesDelegateType, out methodTypeArguments))
                return null;
            
            string fakeCallName = methodOrPropertyCallIdentifier.Identifier.ValueText;
            
            List<ITypeSymbol> lambdaArguments = null;
            ITypeSymbol returnType = null;
            if (fakesDelegateType.TypeParameters.Length > 0)
            {
                if (fakesDelegateType.TypeParameters.Last().Name == "TResult")
                {
                    var last = fakesDelegateType.TypeArguments.Last();
                    lambdaArguments = fakesDelegateType.TypeArguments.Where((t, i) => i < (fakesDelegateType.TypeArguments.Length - 1)).ToList();
                    returnType = last;
                }
                else
                {
                    lambdaArguments = fakesDelegateType.TypeArguments.ToList();
                }
            }
            var concreteLambdaArguments = lambdaArguments;

            var genericMemberName = member.Name as GenericNameSyntax;
            if (genericMemberName != null)
            {
                var hashsetGenericType = new Dictionary<ITypeSymbol, TypeSyntax>();
                var realTypeArguments = genericMemberName.TypeArgumentList.Arguments;
                for (int i = 0; i < realTypeArguments.Count; i++)
                {
                    var genericType = methodTypeArguments[i];
                    var realType = realTypeArguments[i];
                    hashsetGenericType[genericType] = realType;
                }

                concreteLambdaArguments = lambdaArguments.ToList();
                for (int i = 0; i < lambdaArguments.Count; i++)
                {
                    var lambdaArgument = concreteLambdaArguments[i] as INamedTypeSymbol;
                    if (lambdaArgument.IsGenericType)
                    {
                        var concreteArguments = lambdaArgument.TypeArguments.Select(arg => semanticModel.GetTypeInfo(hashsetGenericType[arg]).Type);
                        var unbound = lambdaArgument.ConstructUnboundGenericType();

                        var newLambdaArgument = lambdaArgument.ConstructedFrom.Construct(concreteArguments.ToArray());
                        concreteLambdaArguments[i] = newLambdaArgument;
                    }
                }
            }
            
            var originalType = typeSymbol.BaseType.TypeArguments.FirstOrDefault() as INamedTypeSymbol;
            if (originalType == null)
                return null;

            var originalMethodOrPropertySymbol = msTestHelper.GetOriginalSymbolFromFakeCallName(fakeCallName, lambdaArguments, originalType);
            if (originalMethodOrPropertySymbol == null)
                return null;

            var msStubbed = new StubbedMethodOrProperty(originalType, originalMethodOrPropertySymbol, concreteLambdaArguments, returnType);
            var msStub = new StubbedCall(variableName, msStubbed, returnExpression, originalNode.GetLeadingTrivia(), originalNode.GetTrailingTrivia());
            return msStub;
        }
        
        private MoqStub MockMethodOrPropertyCall(StubbedCall msStub)
        {
            var trivia = new List<SyntaxTrivia>() { SyntaxFactory.CarriageReturnLineFeed };
            trivia.AddRange(msStub.OriginalLeadingTrivia);
            trivia.Add(SyntaxFactory.Tab);
            
            var stubIdentifier = MoqStub.GetStubDefinitionIdentifierName(msStub.Identifier.Identifier.ValueText)
                .WithTrailingTrivia(trivia);
            var setup = SyntaxFactory.IdentifierName("Setup");
            var memberAccess = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, stubIdentifier, setup);

            var stubArguments = new List<ArgumentSyntax>();
            var arguments = msStub.Stubbed.Arguments.ToList();
            foreach (var arg in msStub.Stubbed.Arguments)
            {
                var typeName = arg.ToDisplayString();

                var it = SyntaxFactory.IdentifierName("It");
                var isAny = IdentifierNameHelper.CreateGenericName("IsAny", typeName);
                var anyValue = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, it, isAny);
                var anyValueInvocation = SyntaxFactory.InvocationExpression(anyValue);

                var stubArgument = SyntaxFactory.Argument(anyValueInvocation);
                stubArguments.Add(stubArgument);
            }
            var argumentList = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(stubArguments));

            var stubLambdaAccess = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName("c"), SyntaxFactory.IdentifierName(msStub.Stubbed.MethodOrPropertySymbol.Name));
            var stubLambdaParam = SyntaxFactory.Parameter(SyntaxFactory.Identifier("c"));
            SimpleLambdaExpressionSyntax stubLambda;
            if (msStub.Stubbed.MethodOrPropertySymbol.Kind == SymbolKind.Property)
            {
                stubLambda = SyntaxFactory.SimpleLambdaExpression(stubLambdaParam, stubLambdaAccess).NormalizeWhitespace();
            }
            else
            {
                var stubLambdaBody = SyntaxFactory.InvocationExpression(stubLambdaAccess, argumentList);
                stubLambda = SyntaxFactory.SimpleLambdaExpression(stubLambdaParam, stubLambdaBody).NormalizeWhitespace();
            }
                        
            var stubCall = SyntaxFactory
                .InvocationExpression(memberAccess, SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new[] { SyntaxFactory.Argument(stubLambda) })))
                .WithTrailingTrivia(trivia);

            SimpleNameSyntax ret;
            if (arguments.Count == 0)
            {
                ret = SyntaxFactory.IdentifierName("Returns");
            }
            else
            {
                ret = IdentifierNameHelper.CreateGenericName("Returns", msStub.Stubbed.Arguments.Select(a => a.ToDisplayString()));
            }
            
            var retArgsList = SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(msStub.StubReturn));
            var retArgs = SyntaxFactory.ArgumentList(retArgsList);

            var fullExpression = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, stubCall, ret);
            var fullWithArgs = SyntaxFactory.InvocationExpression(fullExpression, retArgs);

            return new MoqStub(msStub, fullWithArgs);
        }
    }
}
