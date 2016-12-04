using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace RoslynHelpers
{
    public static class ProjectExtensions
    {
        private const string CSharp = "C#";

        public static IEnumerable<Document> GetCSharpFiles(this Project project)
        {
            return CSharp == project?.Language ?
                project?.Documents?.Where(Document => Document.SupportsSemanticModel) : new List<Document>();
        }

    }
}