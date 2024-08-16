using Octokit;
using SpecSharer.Data;
using SpecSharer.Logic;
using System.Reflection;
using Xunit.Abstractions;


namespace SpecSharerTests.Data_Layer_Tests
{
    [Collection("Github Accessed")]
    public class GithubManagerTests
    {

        private readonly ITestOutputHelper testOutputHelper;
        private readonly GithubManager github;

        private static readonly string pat = File.ReadAllText(Path.Combine(""+Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\PatStore.txt"));

        private readonly string githubStaticTestFilePath = "Data/StaticTestFile";
        private readonly string githubDynamicTestFilePath = "Data/DynamicTestFile";
        private readonly string githubDeletedTestFilePath = "Data/CreatedEachTestUseFile";
        private readonly string githubStaticTestFileContent = "Static Test File\n";

        private readonly string singleBindingFilePath = Path.Combine(""+Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\SingleBindingFile.cs");
        private readonly string multiBindingFilePath = Path.Combine(""+Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\MultipleBindingFile.cs");

        public GithubManagerTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
            github = new GithubManager();
        }

        internal static async Task SetFileContent(string filepath, string content) 
        {
            GitHubClient client = new GitHubClient(new ProductHeaderValue("SpecSharer"));
            Credentials tokenAuth = new Credentials(pat);
            client.Credentials = tokenAuth;

            string owner = "joshhandler";
            string repoName = "SpecSharerBindings";

            IReadOnlyList<RepositoryContent> files = await client.Repository.Content.GetAllContentsByRef(owner, repoName, filepath, "main");

            RepositoryContentChangeSet updateresult = await client.Repository.Content.CreateFile(owner, repoName, filepath, new UpdateFileRequest("Setting File Content for use in testing", content, files[0].Sha));
        }

        private async Task DeleteFile(string filepath)
        {
            GitHubClient client = new GitHubClient(new ProductHeaderValue("SpecSharer"));
            Credentials tokenAuth = new Credentials(pat);
            client.Credentials = tokenAuth;

            string owner = "joshhandler";
            string repoName = "SpecSharerBindings";

            try
            {
                IReadOnlyList<RepositoryContent> files = await client.Repository.Content.GetAllContentsByRef(owner, repoName, filepath, "main");

                if (files.Count > 0)
                {
                    Task deleteTask = client.Repository.Content.DeleteFile(owner, repoName, filepath, new DeleteFileRequest($"Deleting {filepath} as part of automated testing", files[0].Sha));
                    await deleteTask;
                }
            }
            catch (Octokit.NotFoundException) { }
        }

        [Fact]
        public async void RetrieveExistingFileTest()
        {
            Task<IReadOnlyList<RepositoryContent>> filesTask = github.RetrieveFiles(githubStaticTestFilePath);
            IReadOnlyList<RepositoryContent> files = await filesTask;

            Assert.NotEmpty(files);

            RepositoryContent? file = null;

            foreach (RepositoryContent content in files)
            {
                if (content.Path == githubStaticTestFilePath)
                {
                    file = content;
                }
            }
            Assert.NotNull(file);
            Assert.Equal(githubStaticTestFileContent, file.Content);
        }

        [Fact]
        public async void UpdateEmptyFileTest()
        {
            string expectedOutput = $"namespace SpecSharer.TestData{Environment.NewLine}{{{Environment.NewLine}    public class TestBindingsFile{Environment.NewLine}    {{{Environment.NewLine}{Environment.NewLine}[Given(@\"there is a binding\")]{Environment.NewLine}\tpublic void Binding(string input){Environment.NewLine}\t{{{Environment.NewLine}            //Example Comment{Environment.NewLine}            Console.WriteLine(\"Example binding\");{Environment.NewLine}        }}    }}{Environment.NewLine}}}";

            await SetFileContent(githubDynamicTestFilePath, "");

            MethodReader reader = new MethodReader();

            reader.SetFilePath(singleBindingFilePath);

            github.FileNamespace = "SpecSharer.TestData";
            github.FileClass = "TestBindingsFile";

            await github.StoreBindings(reader.ProcessBindingsFile(), githubDynamicTestFilePath);

            IReadOnlyList<RepositoryContent> files = await github.RetrieveFiles(githubDynamicTestFilePath);

            Assert.Single(files);
            Assert.Equal(expectedOutput, files[0].Content);
        }

        [Fact]
        public async void UpdateUnchangedFileTest()
        {
            string expectedOutput = $"namespace SpecSharer.TestData{Environment.NewLine}{{{Environment.NewLine}    public class TestBindingsFile{Environment.NewLine}    {{{Environment.NewLine}{Environment.NewLine}[Given(@\"there is a binding\")]{Environment.NewLine}\tpublic void Binding(string input){Environment.NewLine}\t{{{Environment.NewLine}            //Example Comment{Environment.NewLine}            Console.WriteLine(\"Example binding\");{Environment.NewLine}        }}{Environment.NewLine}{Environment.NewLine}    }}{Environment.NewLine}}}";

            await SetFileContent(githubDynamicTestFilePath, expectedOutput);

            MethodReader reader = new MethodReader();

            reader.SetFilePath(singleBindingFilePath);

            github.FileNamespace = "SpecSharer.TestData";
            github.FileClass = "TestBindingsFile";

            await github.StoreBindings(reader.ProcessBindingsFile(), githubDynamicTestFilePath);

            IReadOnlyList<RepositoryContent> files = await github.RetrieveFiles(githubDynamicTestFilePath);

            Assert.Single(files);
            Assert.Equal(expectedOutput, files[0].Content);
        }

        [Fact]
        public async void UpdateChangedFileTest()
        {
            string startingState = $"namespace SpecSharer.TestData{Environment.NewLine}{{{Environment.NewLine}    public class TestBindingsFile{Environment.NewLine}    {{{Environment.NewLine}{Environment.NewLine}[Given(@\"there is the binding\")]{Environment.NewLine}\tpublic void Binding(string input){Environment.NewLine}\t{{{Environment.NewLine}            //An Example Comment{Environment.NewLine}            Console.Write(\"Example binding\");{Environment.NewLine}        }}{Environment.NewLine}{Environment.NewLine}    }}{Environment.NewLine}}}";

            string expectedOutput = $"namespace SpecSharer.TestData{Environment.NewLine}{{{Environment.NewLine}    public class TestBindingsFile{Environment.NewLine}    {{{Environment.NewLine}{Environment.NewLine}[Given(@\"there is a binding\")]{Environment.NewLine}\tpublic void Binding(string input){Environment.NewLine}\t{{{Environment.NewLine}            //Example Comment{Environment.NewLine}            Console.WriteLine(\"Example binding\");{Environment.NewLine}        }}{Environment.NewLine}{Environment.NewLine}    }}{Environment.NewLine}}}";

            await SetFileContent(githubDynamicTestFilePath, startingState);

            MethodReader reader = new MethodReader();

            reader.SetFilePath(singleBindingFilePath);

            github.FileNamespace = "SpecSharer.TestData";
            github.FileClass = "TestBindingsFile";

            await github.StoreBindings(reader.ProcessBindingsFile(), githubDynamicTestFilePath);

            IReadOnlyList<RepositoryContent> files = await github.RetrieveFiles(githubDynamicTestFilePath);

            Assert.Single(files);

            Assert.Equal(expectedOutput, files[0].Content);
        }

        [Fact]
        public async void UpdateNotExistingFileTest()
        {
            string expectedOutput = $"namespace SpecSharer.TestData{Environment.NewLine}{{{Environment.NewLine}    public class CreatedEachTestUseFile{Environment.NewLine}    {{{Environment.NewLine}{Environment.NewLine}[Given(@\"there is a binding\")]{Environment.NewLine}\tpublic void Binding(string input){Environment.NewLine}\t{{{Environment.NewLine}            //Example Comment{Environment.NewLine}            Console.WriteLine(\"Example binding\");{Environment.NewLine}        }}    }}{Environment.NewLine}}}";

            await DeleteFile(githubDeletedTestFilePath);

            MethodReader reader = new MethodReader();

            reader.SetFilePath(singleBindingFilePath);

            github.FileNamespace = "SpecSharer.TestData";
            github.FileClass = "CreatedEachTestUseFile";

            await github.StoreBindings(reader.ProcessBindingsFile(), githubDeletedTestFilePath);

            IReadOnlyList<RepositoryContent> files = await github.RetrieveFiles(githubDeletedTestFilePath);

            Assert.Single(files);

            Assert.Equal(expectedOutput, files[0].Content);

            await DeleteFile(githubDeletedTestFilePath);
        }
    }
}
