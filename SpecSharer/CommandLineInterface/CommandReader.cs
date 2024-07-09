using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecSharer.CommandLineInterface
{
    public class CommandReader
    {

        CommandLineInterfaceController controller;

        string[] shortArgs =
        {
            "h",
            "e",
            "p"
        };

        string[] longArgs =
        {
            "help",
            "extractRequested",
            "path"
        };

        public CommandReader() 
        {
            controller = new CommandLineInterfaceController();
        }

        internal void MainLoop()
        {
            bool awaitingCommand = true;
            string? receivedArg;
            while (awaitingCommand)
            {
                receivedArg = Console.ReadLine();
                
                
            }
        }

        internal List<string> ValidateArgs(Dictionary<string, string>.KeyCollection keys)
        {
            List<string> invalidKeys = new();
            foreach (string key in keys)
            {
                if (key.Length == 1 && !shortArgs.Contains(key))
                {
                    invalidKeys.Add(key);
                }
                else if (key.Length > 1 && !longArgs.Contains(key))
                {
                    invalidKeys.Add(key);
                }
            }

            return invalidKeys;
        }

        internal void Interpret(Dictionary<string, string> argDict)
        {
            bool extractRequested = false;
            bool helpRequested = false;
            bool pathSet = false;
            foreach (string key in argDict.Keys)
            {
                switch (key)
                {
                    case "h":
                    case "help":
                        helpRequested = true;
                        break;
                    case "p":
                    case "path"
                        pathSet = true;
                        break;
                    case "e":
                    case "extract":
                        extractRequested = true;
                        break;
                }
            }

            if (helpRequested)
            {
                controller.GiveHelp(argDict);
                return;
            }

            if (pathSet)
            {

            }

            if (extractRequested)
            {
                
            }

            throw new NotImplementedException();
        }
    }
}
