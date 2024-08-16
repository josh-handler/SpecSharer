using SpecSharer.CommandLineInterface.CliHelp;
using SpecSharer.Data;
using SpecSharer.Logic;
using SpecSharer.Storage;
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
        private IMethodReader reader;
        private BindingsFileData extractedBindings = new BindingsFileData();
        private List<BindingsFileData> retreivedBindings = [];
        private IGithubManager manager;
        private IConsole console;
        private IFileStorer? storer;
        private HelpResponder? responder;

        internal BindingsFileData ExtractedBindings { get => extractedBindings; set => extractedBindings = value; }
        internal List<BindingsFileData> RetreivedBindings { get => retreivedBindings; set => retreivedBindings = value; }

        public CommandLineInterfaceController(IGithubManager manager, IConsole console, IMethodReader reader)
        {
            this.reader = reader;
            this.manager = manager;
            this.console = console;
        }

        public string RequestPath(bool firstAttempt)
        {
            if (!firstAttempt) 
            { 
             console.WriteLine("The input was not a valid file path");
            }

            console.WriteLine("Pleaser enter the path of the bindings file you want extracted");
            string filePath = "";
            filePath += console.ReadLine();

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

        public BindingsFileData ProcessFileAtPath(string filePath)
        {
            reader.SetFilePath(filePath);
            ExtractedBindings = reader.ProcessBindingsFile();
            return ExtractedBindings;
        }

        public void DisplayExtractedBindings()
        {
            console.WriteLine("The following methods and associated bindings were extracted:");
            this.DisplayPassedBindings(ExtractedBindings);
        }

        public void DisplayPassedBindings(BindingsFileData bindings)
        {
            foreach (string method in bindings.Bindings.Keys)
            {
                console.WriteLine("Method Name:");
                console.WriteLine("\t" + method);
                console.WriteLine("Associated Bindings:");
                bindings.Bindings[method].ForEach(bindingString => console.WriteLine("\t" + bindingString));
            }
        }

        //TODO: Update GiveHelp function
        public void GiveHelp(Dictionary<string, string> argDict)
        {
            if(!argDict.ContainsKey("help") && !argDict.ContainsKey("h")){
                throw new UnreachableException("You have reached the help function without an argument requesting help.");
            }

            responder = new HelpResponder(console);

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

        public bool VerifyLocalTarget(string ?target)
        {
            if(target == null)
            {
                return false;
            }

            bool fileExists = File.Exists(target);
            bool directoryExists = Directory.Exists(target);


            if (fileExists || directoryExists)
            {
                return true;
            }

            try
            {
                DirectoryInfo? parentDirectory = Directory.GetParent(target);

                if (parentDirectory == null)
                {
                    return false;
                }

                if (parentDirectory.Exists)
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        public void StoreExtractedBindingsLocally(string target)
        {
            storer = new FileStorer();
            storer.StoreBindings(target, ExtractedBindings, false, reader);
            console.WriteLine("Bindings have been succesfully stored on your local machine");
        }

        public async Task StoreExtractedBindingsInGithub(string targetValue)
        {            
            Task<Octokit.RepositoryContentChangeSet> storeBindingsTask = manager.StoreBindings(ExtractedBindings, targetValue);
            console.WriteLine("Attempting to store bindings in the GitHub Repository");
            await storeBindingsTask;
            if (storeBindingsTask.IsCompletedSuccessfully)
            {
                console.WriteLine("Bindings have been succesfully stored in the GitHub Repository");
            }
            else
            {
                console.WriteLine("There was an issue storing bindings in the GitHub Repository");
            }
        }

        public async Task RetrieveBindingsFromGithub(string pathValue)
        {
            Task<IReadOnlyList<Octokit.RepositoryContent>> retrieveFilesTask = manager.RetrieveFiles(pathValue);
            console.WriteLine("Attempting to retrieve bindings from the GitHub Repository");
            await retrieveFilesTask;
            if (!retrieveFilesTask.IsCompletedSuccessfully)
            {
                console.WriteLine("There was an issue retrieving bindings from the GitHub Repository");
                return;
            }

            //Maybe this should be in GithubManager
            IReadOnlyList<Octokit.RepositoryContent> files = retrieveFilesTask.Result;

            if (files.Count == 0)
            {
                console.WriteLine("No matching files were found in the GitHub Repository");
                return;
            }

            BindingsFileData data;
            foreach (Octokit.RepositoryContent file in files)
            {
                data = reader.ProcessBindingsFileFromRepository(file);
                RetreivedBindings.Add(data);
            }

            console.WriteLine("Bindings have been succesfully retrieved from the GitHub Repository");


        }

        public void DisplayRetrievedBindings()
        {
            console.WriteLine("The following methods and associated bindings were retreived:");
            foreach (BindingsFileData bindings in  RetreivedBindings)
            {
            this.DisplayPassedBindings(bindings);
            }
        }

        public void StoreRetrievedBindingsLocally(string target)
        {
            FileStorer storer = new FileStorer();

            foreach (BindingsFileData bindings in RetreivedBindings)
            {
                storer.StoreBindings(target, bindings, false, reader);
            }

            console.WriteLine("Bindings have been succesfully stored on your local machine");
        }
    }
}
