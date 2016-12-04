using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shims2Moqs
{
    class Options
    {
        [Option('s', "solution", HelpText = "Solution full path", Required = true)]
        public string SolutionPath { get; set; }

        [Option('p', "preview", HelpText = "Preview changes only, this won't change anything", Required = false, DefaultValue = false)]
        public bool Preview { get; set; }
    }
}
