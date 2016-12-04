using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Shims2Moqs
{
    public class Stub
    {
        public Stub(IdentifierNameSyntax identifier, StubbedMethodOrProperty stubbed, ExpressionSyntax stubReturn, AssignmentExpressionSyntax originalStubNode)
        {
            Identifier = identifier;
            Stubbed = stubbed;
            StubReturn = stubReturn;
            OriginalStubNode = originalStubNode;
        }

        public IdentifierNameSyntax Identifier { get; }

        public StubbedMethodOrProperty Stubbed { get; }

        public ExpressionSyntax StubReturn { get; }

        public AssignmentExpressionSyntax OriginalStubNode { get; }
    }
}
