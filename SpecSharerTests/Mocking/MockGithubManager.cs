using Octokit;
using SpecSharer.Data;
using SpecSharer.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SpecSharerTests.Mocking
{
    public class MockGithubManager : IGithubManager
    {

        string owner = "joshhandler";
        string repoName = "SpecSharerBindings";
        string filePath = Path.Combine("" + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources");
        string branch = "main";

        private string fileNamespace = "SpecSharer.Data";
        private string fileClass = "BindingsFile";
        public string FileNamespace { get => fileNamespace; set => fileNamespace = value; }
        public string FileClass { get => fileClass; set => fileClass = value; }

        public Task<IReadOnlyList<RepositoryContent>> RetrieveFiles()
        {
            return RetrieveFiles(filePath, branch);
        }

        public Task<IReadOnlyList<RepositoryContent>> RetrieveFiles(string path)
        {
            return RetrieveFiles(path, branch);
        }

        private string EncodeToBase64(string content)
        {
            byte[] contentBytes = System.Text.Encoding.UTF8.GetBytes(content);
            return Convert.ToBase64String(contentBytes);
        }

        public Task<IReadOnlyList<RepositoryContent>> RetrieveFiles(string path, string targetBranch)
        {
            List<RepositoryContent> returned = new List<RepositoryContent>();
            string content;
            if (File.Exists(path))
            {
                content = File.ReadAllText(path);
                string content64 = EncodeToBase64(content);
                returned.Add(new RepositoryContent(name: "DummyRepo", path: path, sha: "dummySha0", size: content.Length, type: ContentType.File, downloadUrl: "dummyDownloadUrl", url: "dummyUrl", gitUrl: "dummyGitUrl", htmlUrl: "dummyHtmlUrl", encoding: "base64", encodedContent: content64, target: null, submoduleGitUrl: null));
            }
            else
            {
                string[] files = Directory.GetFiles(path);

                foreach(string file in files)
                {
                    content = File.ReadAllText(file);
                    string content64 = EncodeToBase64(content);
                    returned.Add(new RepositoryContent(name: "DummyRepo", path: path, sha: "dummySha0", size: content.Length, type: ContentType.File, downloadUrl: "dummyDownloadUrl", url: "dummyUrl", gitUrl: "dummyGitUrl", htmlUrl: "dummyHtmlUrl", encoding: "base64", encodedContent: content64, target: null, submoduleGitUrl: null));
                }
            }

            IReadOnlyList<RepositoryContent> result = returned;

            Task<IReadOnlyList<RepositoryContent>> responseTask = Task.FromResult(result);
            return responseTask;
        }

        public Task<RepositoryContentChangeSet> StoreBindings(BindingsFileData localBindings)
        {
            return StoreBindings(localBindings, filePath, branch);
        }

        public Task<RepositoryContentChangeSet> StoreBindings(BindingsFileData localBindings, string targetFilePath)
        {
            return StoreBindings(localBindings, targetFilePath, branch);
        }

        public Task<RepositoryContentChangeSet> StoreBindings(BindingsFileData localBindings, string targetFilePath, string targetBranch)
        {

            if(File.Exists(targetFilePath))
            {
                UpdateFile(localBindings, targetFilePath);
            }
            else
            {
                CreateFile(localBindings, targetFilePath);
            }

            RepositoryContentChangeSet result = new RepositoryContentChangeSet();
            Task<RepositoryContentChangeSet> responseTask = Task.FromResult(result);
            return responseTask;
        }

        private void CreateFile(BindingsFileData localBindings, string path)
        {
            string content = "namespace " + FileNamespace + "\r\n{\r\n    public class " + FileClass + "\r\n    {\r\n\r\n";

            content += localBindings.ConvertToString();

            content += "    }\r\n}";
            
            File.WriteAllText(path, content);
        }

        private void UpdateFile(BindingsFileData localBindings, string path)
        {
            MethodReader reader = new MethodReader();
            reader.SetFilePath(path);
            BindingsFileData githubBindings = reader.ProcessBindingsFile();

            string content = "namespace " + FileNamespace + "\r\n{\r\n    public class " + FileClass + "\r\n    {\r\n\r\n";

            content += CompareBindingsAndUpdateContent(localBindings, githubBindings);

            content += "    }\r\n}";

            File.WriteAllText(path, content);
        }

        private string CompareBindingsAndUpdateContent(BindingsFileData localBindings, BindingsFileData githubBindings)
        {
            githubBindings.UpdateAllUpdatedMethodsAndBindings(localBindings);
            localBindings.RemoveSharedData(githubBindings);

            string content = githubBindings.ConvertToString();
            if (content.Length != 0)
            {
                content += $"\r\n\r\n";
            }

            content += localBindings.ConvertToString();

            return content;
        }
    }
}
