using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecSharer.CommandLineInterface.CliHelp
{
    internal class HelpResponder
    {
        internal void GiveGeneralHelpResponse()
        {
            Console.Write(HelpStringData.generalHelpString);
        }

        internal void GiveArgumentExplanationResponse(Dictionary<string, string> argDict)
        {
            argDict.Remove("h");
            argDict.Remove("help");

            Console.Write(HelpStringData.GenerateArgumentHelpString(argDict.Keys.Single()));
        }

        internal void GiveMultipleArgumentsExplanation()
        {
            Console.Write(HelpStringData.multipleArgumentsExplanationString);
            this.GiveGeneralHelpResponse();
        }
    }
}
