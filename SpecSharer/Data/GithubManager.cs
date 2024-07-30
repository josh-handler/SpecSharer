using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.OLE.Interop;
using Octokit;
using SpecSharer.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace SpecSharer.Data
{
    internal class GithubManager : IGithubManager
    {
        private static readonly string pat = File.ReadAllText(Path.Combine(""+Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\PatStore.txt"));

        //Internal for testing purposes
        internal readonly GitHubClient github = ConnectAndAuthentiate();

        string owner = "joshhandler";
        string repoName = "SpecSharerBindings";
        string filePath = "Data/Shared";
        string branch = "main";

        private string fileNamespace = "SpecSharer.Data";
        private string fileClass = "BindingsFile";

        public string FileNamespace { get => fileNamespace; set => fileNamespace = value; }
        public string FileClass { get => fileClass; set => fileClass = value; }

        public GithubManager() { }

        public async Task<RepositoryContentChangeSet> StoreBindings(BindingsFileData localBindings)
        {
            return await StoreBindings(localBindings, filePath, branch);
        }

        public async Task<RepositoryContentChangeSet> StoreBindings(BindingsFileData localBindings, string targetFilePath)
        {
            if(targetFilePath.Length == 0)
            {
                targetFilePath = filePath;
            }
            return await StoreBindings(localBindings, targetFilePath, branch);

        }
        public async Task<RepositoryContentChangeSet> StoreBindings(BindingsFileData localBindings, string targetFilePath, string targetBranch)
        {
            IReadOnlyList<RepositoryContent> fileDetails = await RetrieveFiles(targetFilePath, targetBranch);

            if (fileDetails.Count == 0)
            {
                return await CreateFile(localBindings, targetFilePath, fileDetails);
            }

            return await UpdateFile(localBindings, targetFilePath, fileDetails);
        }

        private async Task<RepositoryContentChangeSet> CreateFile(BindingsFileData localBindings, string targetFilePath, IReadOnlyList<RepositoryContent> fileDetails)
        {
            string content = "namespace " + FileNamespace + "\r\n{\r\n    public class " + FileClass + "\r\n    {\r\n\r\n";

            content += localBindings.ConvertToString();

            content += "    }\r\n}";
            RepositoryContentChangeSet createResult = await github.Repository.Content.CreateFile(owner, repoName, targetFilePath, new CreateFileRequest($"Creating File at size {content.Length}", content));

            return createResult;
        }

        private async Task<RepositoryContentChangeSet> UpdateFile(BindingsFileData localBindings, string targetFilePath, IReadOnlyList<RepositoryContent> fileDetails)
        {
            MethodReader reader = new MethodReader();
            BindingsFileData githubBindings = reader.ProcessBindingsFileFromRepository(fileDetails[0]);

            string content = "namespace " + FileNamespace + "\r\n{\r\n    public class " + FileClass + "\r\n    {\r\n\r\n";

            content += CompareBindingsAndUpdateContent(localBindings, githubBindings);

            content += "    }\r\n}";

            RepositoryContentChangeSet updateResult = await github.Repository.Content.UpdateFile(owner, repoName, targetFilePath,
                new UpdateFileRequest($"Update File to size {content.Length} ", content, fileDetails[0].Sha));
            return updateResult;
        }

        private string CompareBindingsAndUpdateContent(BindingsFileData localBindings, BindingsFileData githubBindings)
        {         
            githubBindings.UpdateAllUpdatedMethodsAndBindings(localBindings);
            localBindings.RemoveSharedData(githubBindings);
                
            string content = githubBindings.ConvertToString();
            if(content.Length != 0)
            {
                content += $"\r\n\r\n";
            }
            
            content += localBindings.ConvertToString();

            return content;
        }

        public async Task<IReadOnlyList<RepositoryContent>> RetrieveFiles()
        {
            return await RetrieveFiles(filePath, branch);
        }

        public async Task<IReadOnlyList<RepositoryContent>> RetrieveFiles(string path)
        {
            return await RetrieveFiles(path, branch);
        }

        public async Task<IReadOnlyList<RepositoryContent>> RetrieveFiles(string path, string targetBranch)
        {
            try
            {
                IReadOnlyList<RepositoryContent> fileDetails = await github.Repository.Content.GetAllContentsByRef(owner, repoName,
                    path, targetBranch);
                return fileDetails;
            }
            catch (Octokit.NotFoundException)
            {
                return [];
            }
        }

        public static GitHubClient ConnectAndAuthentiate()
        {
            GitHubClient github = new GitHubClient(new ProductHeaderValue("SpecSharer"));

            Credentials tokenAuth = new Credentials(pat);

            github.Credentials = tokenAuth;

            return github;
        }

    }
}
