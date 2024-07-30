using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecSharer.CommandLineInterface.CliHelp
{
    internal class HelpResponder
    {
        IConsole console;
        internal HelpResponder(IConsole console)
        {
            this.console = console;
        }
        internal void GiveGeneralHelpResponse()
        {
            console.Write(HelpStringData.generalHelpString);
        }

        internal void GiveArgumentExplanationResponse(Dictionary<string, string> argDict)
        {
            argDict.Remove("h");
            argDict.Remove("help");

            console.Write(HelpStringData.GenerateArgumentHelpString(argDict.Keys.Single()));
        }

        internal void GiveMultipleArgumentsExplanation()
        {
            console.Write(HelpStringData.multipleArgumentsExplanationString);
            this.GiveGeneralHelpResponse();
        }
    }
}
