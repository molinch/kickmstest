using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Semantics;
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

        public SyntaxNode SwitchFieldDeclarationType(FieldDeclarationSyntax node)
        {
            var declarationTypeSymbol = semanticModel.GetTypeInfo(node.Declaration.Type).ConvertedType as INamedTypeSymbol;
            if (declarationTypeSymbol == null)
                return null;

            string typeToMock;
            if (!msTestHelper.IsStub(declarationTypeSymbol, out typeToMock))
                return null;
            
            var mockType = CreateMockType(typeToMock);

            return node.WithDeclaration(node.Declaration.WithType(mockType.WithTriviaFrom(node.Declaration.Type)).WithTriviaFrom(node.Declaration));
        }

        public SyntaxNode SwitchPropertyDeclarationType(PropertyDeclarationSyntax node)
        {
            var declarationTypeSymbol = semanticModel.GetTypeInfo(node.Type).ConvertedType as INamedTypeSymbol;
            if (declarationTypeSymbol == null)
                return null;

            string typeToMock;
            if (!msTestHelper.IsStub(declarationTypeSymbol, out typeToMock))
                return null;
            
            var mockType = CreateMockType(typeToMock).WithTriviaFrom(node.Type);

            return node.WithType(mockType);
        }

        public SyntaxNode SwitchType(AssignmentExpressionSyntax assignment)
        {
            var left = assignment.Left as MemberAccessExpressionSyntax;
            if (left == null)
                return null;

            var right = assignment.Right as ObjectCreationExpressionSyntax;

            var declarationTypeSymbol = semanticModel.GetTypeInfo(left).ConvertedType as INamedTypeSymbol;
            if (declarationTypeSymbol == null)
                return null;

            string typeToMock;
            if (msTestHelper.IsStub(declarationTypeSymbol, out typeToMock))
            {
                if (typeToMock == null)
                    return null;

                var identifierName = left.Name.Identifier.ValueText;
                var stubName = char.ToLowerInvariant(identifierName[0]) + identifierName.Substring(1);
                var stubDefDeclaration = CreateStubDefinitionDeclaration(stubName, typeToMock)
                    .NormalizeWhitespace()
                    .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);
                
                var initializerExpression = CreateStubInitializerDeclarations(assignment, stubName, declarationTypeSymbol);

                var newAssignment = SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, left, MoqStub.GetStubDefinitionIdentifierName(stubName))
                    .NormalizeWhitespace()
                    .WithLeadingTrivia(assignment.GetLeadingTrivia());

                var statements = new SyntaxList<StatementSyntax>();
                statements = statements.AddRange(new StatementSyntax[] { stubDefDeclaration }.Union(initializerExpression).Union(new StatementSyntax[] { SyntaxFactory.ExpressionStatement(newAssignment) }));
                var wrapper = SyntaxFactory.Block(statements);
                wrapper = wrapper.WithOpenBraceToken(SyntaxFactory.MissingToken(SyntaxKind.OpenBraceToken)) // to remove scope {}
                    .WithCloseBraceToken(SyntaxFactory.MissingToken(SyntaxKind.CloseBraceToken));

                return wrapper
                    .WithLeadingTrivia(assignment.GetLeadingTrivia())
                    .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);
            }

            return null;
        }


        public SyntaxNode SwitchType(LocalDeclarationStatementSyntax variableDeclaration)
        {
            var declarationTypeSymbol = variableDeclaration.Declaration.GetTypeSymbol(semanticModel) as INamedTypeSymbol;
            if (declarationTypeSymbol == null)
                return null;

            string typeToMock;
            if (msTestHelper.IsStub(declarationTypeSymbol, out typeToMock))
            {
                if (typeToMock == null)
                    return null;

                return CreateLocalVariableDeclaration(variableDeclaration, typeToMock);
            }

            return null;
        }
        
        private LocalDeclarationStatementSyntax CreateStubDefinitionDeclaration(string variableName, string typeToMock)
        {
            var mockedType = CreateMockType(typeToMock);

            return SyntaxFactory.LocalDeclarationStatement(
                SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName("var"))
                    .WithVariables(SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.VariableDeclarator(MoqStub.GetStubDefinitionIdentifier(variableName))
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
        }

        private IEnumerable<ExpressionStatementSyntax> CreateStubInitializerDeclarations(SyntaxNode node, string variableName, INamedTypeSymbol symbol)
        {
            var objectCreationExpression = node.DescendantNodes(s => !(s is ObjectCreationExpressionSyntax)).OfType<ObjectCreationExpressionSyntax>().FirstOrDefault();
            IEnumerable<ExpressionStatementSyntax> initializerExpressions = new ExpressionStatementSyntax[0];
            if (objectCreationExpression?.Initializer != null && symbol != null)
            {
                initializerExpressions =
                    InitializersToExpressions.Expand(objectCreationExpression.Initializer, SyntaxFactory.IdentifierName(variableName))
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

            return initializerExpressions;
        }

        private VariableDeclarationSyntax CreateStubDeclaration(SyntaxNode node, string variableName, string variableIdentifierName)
        {
            var stubDeclaration = SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName("var"))
                .WithVariables(SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.VariableDeclarator(variableIdentifierName)
                        .WithInitializer(
                            SyntaxFactory.EqualsValueClause(
                                SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, MoqStub.GetStubDefinitionIdentifierName(variableName), SyntaxFactory.IdentifierName("Object"))
                            )
                        )
                ))
                .NormalizeWhitespace()
                .WithLeadingTrivia(node.GetLeadingTrivia());

            return stubDeclaration;
        }

        public SyntaxNode CreateLocalVariableDeclaration(LocalDeclarationStatementSyntax node, string typeToMock)
        {
            var variable = node.Declaration.Variables.FirstOrDefault();
            if (variable == null)
                return null;
            
            var stubName = variable.Identifier.ValueText;

            var stubDefDeclaration = CreateStubDefinitionDeclaration(stubName, typeToMock);
            var symbol = node.Declaration.GetTypeSymbol(semanticModel) as INamedTypeSymbol;
            var initializerExpression = CreateStubInitializerDeclarations(node, stubName, symbol);
            var stubDeclaration = CreateStubDeclaration(node, stubName, variable.Identifier.ValueText);

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
                var member = assignment.ChildNodes().FirstOrDefault() as MemberAccessExpressionSyntax;
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

            string typeToMock;
            if (!msTestHelper.IsStub(typeSymbol, out typeToMock))
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
            
            var lambdaArguments = new List<ITypeSymbol>();
            ITypeSymbol returnType = null;
            if (fakesDelegateType.TypeParameters.Length > 0)
            {
                IEnumerable<ITypeSymbol> typeArguments = null;
                if (fakesDelegateType.TypeParameters.Last().Name == "TResult")
                {
                    var last = fakesDelegateType.TypeArguments.Last();
                    typeArguments = fakesDelegateType.TypeArguments.Where((t, i) => i < (fakesDelegateType.TypeArguments.Length - 1));
                    returnType = last;
                }
                else
                {
                    typeArguments = fakesDelegateType.TypeArguments;
                }

                if (typeArguments != null)
                {
                    lambdaArguments = typeArguments.ToList();
                }
            }

            var concreteLambdaArguments = lambdaArguments;
            var hashsetGenericType = new Dictionary<ITypeSymbol, TypeSyntax>();
            var genericMemberName = member.Name as GenericNameSyntax;
            if (genericMemberName != null)
            {
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

            var originalMethodOrPropertySymbol = msTestHelper.GetOriginalSymbolFromFakeCallName(fakeCallName, lambdaArguments, originalType, fakesDelegateType.TypeParameters);
            if (originalMethodOrPropertySymbol == null)
                return null;

            var originalMethodSymbol = originalMethodOrPropertySymbol as IMethodSymbol;
            if (originalMethodSymbol != null && originalMethodSymbol.TypeArguments.Length > 0)
            {
                var typeArguments = originalMethodSymbol.TypeArguments.Select(t => {
                    var concreteType = hashsetGenericType.Where(pair => pair.Key.Name == t.Name).Select(pair => pair.Value).FirstOrDefault();
                    if (concreteType != null)
                    {
                        return semanticModel.GetTypeInfo(concreteType).Type;
                    }
                    else
                    {
                        // can happen for stubbed methods like Find<E>(), in that case generic type E must be explicitely passed
                        // however MsTest tests may declare it this way: instance.FindOf1(() => ...), where ... has to return an instance of E
                        // in that case the type is inferred
                        var invocationExpression = originalNode as InvocationExpressionSyntax;
                        if (invocationExpression != null)
                        {
                            var expressionArgument = invocationExpression.ArgumentList.Arguments.FirstOrDefault()?.Expression;
                            if (expressionArgument != null)
                            {
                                var expressionType = semanticModel.GetTypeInfo(expressionArgument).ConvertedType as INamedTypeSymbol;
                                if (expressionType != null)
                                {
                                    return expressionType?.TypeArguments.FirstOrDefault(); // type which is inferred
                                }
                            }
                        }

                        return null;
                    }
                });
                originalMethodOrPropertySymbol = originalMethodSymbol.Construct(typeArguments.Where(t => t != null).ToArray());
            }

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

            var typeArguments = (msStub.Stubbed.MethodOrPropertySymbol as IMethodSymbol)?.TypeArguments.Select(a => a.ToDisplayString());
            var stubLambdaAccess = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName("c"),
                IdentifierNameHelper.CreateName(msStub.Stubbed.MethodOrPropertySymbol.Name, typeArguments));
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
            if (msStub.Stubbed.ReturnType == null)
            {
                ret = IdentifierNameHelper.CreateName("Callback", msStub.Stubbed.Arguments.Select(a => a.ToDisplayString()));
            }
            else
            {
                ret = IdentifierNameHelper.CreateName("Returns", msStub.Stubbed.Arguments.Select(a => a.ToDisplayString()));
            }
            
            var retArgsList = SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(msStub.StubReturn));
            var retArgs = SyntaxFactory.ArgumentList(retArgsList);

            var fullExpression = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, stubCall, ret);
            var fullWithArgs = SyntaxFactory.InvocationExpression(fullExpression, retArgs);

            return new MoqStub(msStub, fullWithArgs);
        }
    }
}
