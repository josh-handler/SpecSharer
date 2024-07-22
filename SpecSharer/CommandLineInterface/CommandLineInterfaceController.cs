using SpecSharer.CommandLineInterface.CliHelp;
using SpecSharer.Logic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSLangProj80;

namespace SpecSharer.CommandLineInterface
{
    public class CommandLineInterfaceController
    {
        private MethodReader reader;
        private BindingsFileData extractedBindings;

        public CommandLineInterfaceController()
        {
            reader = new MethodReader();
            extractedBindings = new BindingsFileData();
        }

        public string RequestPath(bool firstAttempt)
        {
            if (!firstAttempt) 
            { 
             Console.WriteLine("The input was not a valid file path");
            }

            Console.WriteLine("Pleaser enter the path of the bindings file you want extracted");
            string filePath = "";
            filePath += Console.ReadLine();

            return filePath;
        }

        public bool VerifyPath(string? path)
        {
                if (path == null)
                {
                    return false;
                }

                return reader.SetFilePath(path);
            
        }

        public string GetPath()
        {
            return reader.GetFilePath();
        }

        private bool ConfirmProcessFile(string filePath)
        {
            bool validResponse = false;
            string? response = "";

            Console.WriteLine($"Extract Bindings from {filePath}");
            Console.WriteLine($"y / n");
            while (!validResponse)
            {
                response = Console.ReadLine();
                if(response == "y" || response == "n")
                {
                    validResponse = true;
                }
                else
                {
                    Console.WriteLine("Please respond with 'y' or 'n'");
                }
            }

            if(response == "y")
            {
                extractedBindings = reader.ProcessBindingsFile();
                return true;
            }
            else
            {
                return false;
            }
        }

        internal void Startup()
        {
            string filePath = SetFilePathFromInput();

            ConfirmAndProcessFileAtPath(filePath);

            DisplayExtractedBindings();

            throw new NotImplementedException();
        }

        public string SetFilePathFromInput()
        {
            string pathRequestResult = RequestPath(true);
            bool validPath = VerifyPath(pathRequestResult);
            while (!validPath)
            {
                pathRequestResult = RequestPath(false);
                validPath = VerifyPath(pathRequestResult);
            }
            return pathRequestResult;
        }

        public void ConfirmAndProcessFileAtPath(string filePath)
        {
            bool fileProcessed = ConfirmProcessFile(filePath);

            while (!fileProcessed)
            {
                filePath = SetFilePathFromInput();
                fileProcessed = ConfirmProcessFile(filePath);
            }
        }

        internal BindingsFileData ProcessFileAtPath(string filePath)
        {
            reader.SetFilePath(filePath);
            extractedBindings = reader.ProcessBindingsFile();
            return extractedBindings;
        }

        public void DisplayExtractedBindings()
        {
            Console.WriteLine("The following methods and associated bindings were extracted:");
            this.DisplayPassedBindings(extractedBindings);
        }

        public void DisplayPassedBindings(BindingsFileData bindings)
        {
            foreach (string method in bindings.Bindings.Keys)
            {
                Console.WriteLine("Method Name:");
                Console.WriteLine("\t" + method);
                Console.WriteLine("Associated Bindings:");
                bindings.Bindings[method].ForEach(bindingString => Console.WriteLine("\t" + bindingString));
            }
        }

        internal void GiveHelp(Dictionary<string, string> argDict)
        {
            if(!argDict.ContainsKey("help") && !argDict.ContainsKey("h")){
                throw new UnreachableException("You have reached the help function without an argument requesting help.");
            }

            HelpResponder responder = new HelpResponder();

            if(argDict.Keys.Count == 1)
            {
                responder.GiveGeneralHelpResponse();
            }
            else if (argDict.Keys.Count == 2)
            {
                responder.GiveArgumentExplanationResponse(argDict);
            }
            else
            {
                responder.GiveMultipleArgumentsExplanation();
                return;
            }
        }

        internal bool VerifyTarget(string ?target)
        {
            if(target == null)
            {
                return false;
            }

            if(File.Exists(target) || Directory.Exists(target))
            {
                return true;
            }

            return false;
        }

        internal bool WriteToTarget(string target, BindingsFileData data)
        {

            return false;
        }
    }
}
