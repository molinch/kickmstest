using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shims2Moqs
{
    public class UnitTestClassItem
    {
        public Workspace Workspace { get; set; }

        public Document File { get; set; }

        public SyntaxNode Root { get; set; }

        public SemanticModel SemanticModel { get; set; }

        public ClassDeclarationSyntax UnitTestClass { get; set; }
    }
}
