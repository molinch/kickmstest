using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoslynHelpers
{
    public static class SolutionHelper
    {
        public static async Task<IEnumerable<Project>> GetProjectsAsync(string solutionPath)
        {
            var msWorkspace = MSBuildWorkspace.Create();
            var solution = await msWorkspace.OpenSolutionAsync(solutionPath).ConfigureAwait(false);
            return solution.Projects;
        }
    }
}
