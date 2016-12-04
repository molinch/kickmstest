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
    }
}
