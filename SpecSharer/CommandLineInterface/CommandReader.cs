using Microsoft.CodeAnalysis.CSharp.Syntax;
using SpecSharer.Data;
using SpecSharer.Storage;
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
        IConsole console;

        string[] shortArgs =
        {
            "h",
            "e",
            "p",
            "t",
            "g",
            "r"
        };

        string[] longArgs =
        {
            "help",
            "extractRequested",
            "path",
            "target",
            "github",
            "retrieve"
        };

        public CommandReader(IGithubManager manager, IConsole console) 
        {
            controller = new CommandLineInterfaceController(manager, console);
            this.console = console;
        }

        public CommandReader()
        {
            console = new ConsoleWrapper();
            controller = new CommandLineInterfaceController(new GithubManager(), console);
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

        internal async Task Interpret(Dictionary<string, string> argDict)
        {
            bool extractRequested = false;
            bool helpRequested = false;
            bool pathSet = false;
            bool validTargetSet = false;
            string targetValue = "";
            string pathValue = "";
            bool storeInGithub = false;
            bool retrieveFromGithub = false;
            foreach (string key in argDict.Keys)
            {
                switch (key)
                {
                    case "h":
                    case "help":
                        helpRequested = true;
                        break;
                    case "p":
                    case "path":
                        pathSet = true;
                        pathValue = argDict[key];
                        break;
                    case "e":
                    case "extract":
                        extractRequested = true;
                        break;
                    case "t":
                    case "target":
                        targetValue = argDict[key];
                        break;
                    case "g":
                    case "github":
                        storeInGithub = true;
                        break;
                    case "r":
                    case "retrieve":
                        retrieveFromGithub = true;
                        break;
                }
            }

            if (helpRequested)
            {
                controller.GiveHelp(argDict);
                return;
            }

            //TODO: Decide if there is verification needed for github paths
            //else if(targetKey.Length != 0 && storeToGithub)
            //{
            //    controller.VerifyGithubTarget(argDict[targetKey]);
            //    validTargetSet = true;
            //}

            if (!pathSet)
            {
                throw new ArgumentException($"p|path argument is required. Please input a valid path");
            }

            bool validPath = controller.VerifyPath(pathValue);


            if (targetValue.Length == 0 && retrieveFromGithub)
            {
                await controller.RetrieveBindingsFromGithub(pathValue);
                controller.DisplayRetrievedBindings();
                return;
            }
            else if (targetValue.Length == 0)
            {
                if (!validPath)
                {
                    throw new ArgumentException($"p|path argument is invalid. {pathValue} is not a valid file path");
                };

                controller.ProcessFileAtPath(pathValue);
                controller.DisplayExtractedBindings();
                return;
            }

            if (retrieveFromGithub)
            {
                await controller.RetrieveBindingsFromGithub(pathValue);
                controller.StoreRetrievedBindingsLocally(targetValue);
                return;
            }
            
            if (storeInGithub)
            {
                controller.ProcessFileAtPath(pathValue);
                await controller.StoreExtractedBindingsInGithub(targetValue);
                return;
            }

            validTargetSet = controller.VerifyLocalTarget(targetValue);
            if (!validTargetSet)
            {
                throw new ArgumentException($"t|target argument is invalid. You are attempting to store bindings locally and {targetValue} is not a valid file path");
            }
            else
            {
                if (!validPath)
                {
                    throw new ArgumentException($"p|path argument is invalid. {pathValue} is not a valid file path");
                };

                controller.ProcessFileAtPath(pathValue);
                controller.StoreExtractedBindingsLocally(targetValue);
                return;
            }
            //if (extractRequested)
            //{

            //}
        }
    }
}
