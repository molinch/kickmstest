using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Stubs2Moqs
{
    public class StubbedCall
    {
        public StubbedCall(IdentifierNameSyntax identifier, StubbedMethodOrProperty stubbed, ExpressionSyntax stubReturn,
            SyntaxTriviaList originalLeadingTrivia, SyntaxTriviaList originalTrailingTrivia)
        {
            Identifier = identifier;
            Stubbed = stubbed;
            StubReturn = stubReturn;
            OriginalLeadingTrivia = originalLeadingTrivia;
            OriginalTrailingTrivia = originalTrailingTrivia;
        }

        public IdentifierNameSyntax Identifier { get; }

        public StubbedMethodOrProperty Stubbed { get; }

        public ExpressionSyntax StubReturn { get; }

        public SyntaxTriviaList OriginalLeadingTrivia { get; }

        public SyntaxTriviaList OriginalTrailingTrivia { get; }
    }
}