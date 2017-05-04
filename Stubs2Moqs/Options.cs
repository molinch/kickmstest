using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stubs2Moqs
{
    class Options
    {
        [Option('s', "solution", HelpText = "Solution full path", Required = true)]
        public string SolutionPath { get; set; }

        [Option('p', "preview", HelpText = "Preview changes only, this won't change anything", Required = false, DefaultValue = false)]
        public bool Preview { get; set; }

        [Option('t', "tf", HelpText = "tf.exe path in order to automatically perform a TFS checkout before editing a file", Required = false)]
        public string TfsExePath { get; set; }
    }
}
