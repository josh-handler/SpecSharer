using Octokit;
using SpecSharer.Logic;
using System.IO;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace SpecSharerTests
{
    public class MethodReaderTests
    {
        readonly string singleBindingFilePath = Path.Combine(""+Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\SingleBindingFile.cs");

        readonly string multiBindingFilePath = Path.Combine(""+Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\MultipleBindingFile.cs");

        readonly string invalidFilePath = "Not A Path";

        private MethodReader reader;

        private readonly ITestOutputHelper testOutputHelper;

        public MethodReaderTests(ITestOutputHelper testOutputHelper)
        {

            reader = new MethodReader();
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void SetValidFilePath()
        {
            Assert.True(reader.SetFilePath(singleBindingFilePath));

            Assert.Equal(singleBindingFilePath, reader.GetFilePath());
        }

        [Fact]
        public void SetInvalidFilePath()
        {
            Assert.False(reader.SetFilePath(invalidFilePath));

            Assert.Equal("Not A Path", reader.GetFilePath());
        }

        [Fact]
        public void ProcessBindingsFileSingleBindingTest()
        {
            reader.SetFilePath(singleBindingFilePath);
            BindingsFileData results = reader.ProcessBindingsFile();

            Assert.Single(results.Methods);
            Assert.Equal("Binding", results.Methods.Single());

            Assert.Single(results.Bodies);
            Assert.Equal("{\r\n" +
                "            //Example Comment\r\n" +
                "            Console.WriteLine(\"Example binding\");\r\n" +
                "        }",
                results.Bodies.Single().Value);

            Assert.Single(results.Modifiers);
            Assert.Equal("public void ", results.Modifiers.Single().Value);

            Assert.Single(results.Parameters);
            Assert.Equal("string input", results.Parameters.Single().Value.Single());

            Assert.Single(results.Bindings);
            Assert.Equal("public void Binding(string input)", results.Bindings.Single().Key);
            Assert.Equal("[Given(@\"there is a binding\")]", results.Bindings.Single().Value.Single());

            Assert.Equal("public void Binding(string input)", results.GetMethodLine("Binding"));

        }

        [Fact]
        public void MapMethodsToBindingsMultiBindingTest()
        {
            reader.SetFilePath(multiBindingFilePath);
            BindingsFileData results = reader.ProcessBindingsFile();

            Assert.Equal(3, results.Methods.Count);

            Assert.Equal("FirstBinding", results.Methods.First());
            Assert.Equal("SingleInputBinding", results.Methods.ElementAt(1));
            Assert.Equal("MultiInputBinding", results.Methods.ElementAt(2));

            Assert.Equal("{\r\n" +
                "            //Example Comment\r\n" +
                "            Console.WriteLine(\"Example binding\");\r\n" +
                "        }",
                results.Bodies[results.Bodies.Keys.First()]);

            Assert.Equal("{\r\n" +
                "            //Comment\r\n" +
                "            Console.WriteLine($\"Binding has input of {input}\");\r\n" +
                "            return true;\r\n" +
                "        }",
                results.Bodies[results.Bodies.Keys.ElementAt(1)]);

            Assert.Equal("{\r\n" +
                "            //Another Comment\r\n" +
                "            Console.WriteLine($\"Inputs were string {stringInput}, char {charInput} and int {intInput}\");\r\n" +
                "        }",
                results.Bodies[results.Bodies.Keys.ElementAt(2)]);

            Assert.Equal("public void ", results.Modifiers[results.Modifiers.Keys.First()]);
            Assert.Equal("public bool ", results.Modifiers[results.Modifiers.Keys.ElementAt(1)]);
            Assert.Equal("public void ", results.Modifiers[results.Modifiers.Keys.ElementAt(2)]);

            Assert.Empty(results.Parameters["FirstBinding"]);
            Assert.Equal("string input", results.Parameters["SingleInputBinding"].Single());
            Assert.Equal("string stringInput", results.Parameters["MultiInputBinding"][0]);
            Assert.Equal("char charInput", results.Parameters["MultiInputBinding"][1]);
            Assert.Equal("int intInput", results.Parameters["MultiInputBinding"][2]);

            Assert.Equal("public void FirstBinding()", results.Bindings.Keys.First());
            Assert.Equal("public bool SingleInputBinding(string input)", results.Bindings.Keys.ElementAt(1));
            Assert.Equal("public void MultiInputBinding(string stringInput, char charInput, int intInput)", results.Bindings.Keys.ElementAt(2));

            Assert.Equal("[Given(@\"there is a first binding\")]", results.Bindings[results.Bindings.Keys.First()].Single());
            Assert.Equal("[When(@\"there is an input of '(.*)'\")]", results.Bindings[results.Bindings.Keys.ElementAt(1)].Single());
            Assert.Equal("[Then(@\"there are multiple inputs of '(*.)', '(a|b|c)', '(dddd)'\")]", results.Bindings[results.Bindings.Keys.ElementAt(2)][0]);
            Assert.Equal("[When(@\"there are inputs of '(*.)', '(a|b|c)', '(dddd)'\")]", results.Bindings[results.Bindings.Keys.ElementAt(2)][1]);

            Assert.Equal("public void FirstBinding()", results.GetMethodLine("FirstBinding"));
            Assert.Equal("public bool SingleInputBinding(string input)", results.GetMethodLine("SingleInputBinding"));
            Assert.Equal("public void MultiInputBinding(string stringInput, char charInput, int intInput)", results.GetMethodLine("MultiInputBinding"));
        }

        [Fact]
        public void ProcessBindingsFromRepositoryTest()
        {
            string content = File.ReadAllText(singleBindingFilePath);
            byte[] contentBytes = System.Text.Encoding.UTF8.GetBytes(content);
            string content64 = Convert.ToBase64String(contentBytes);
            var repoContent = new RepositoryContent(name: "DummyRepo", path: "path", sha: "dummySha0", size: content.Length, type: ContentType.File, downloadUrl: "dummyDownloadUrl", url: "dummyUrl", gitUrl: "dummyGitUrl", htmlUrl: "dummyHtmlUrl", encoding: "base64", encodedContent: content64, target: null, submoduleGitUrl: null);

            var actualData = reader.ProcessBindingsFileFromRepository(repoContent);
            reader.SetFilePath(singleBindingFilePath);
            var expectedData = reader.ProcessBindingsFile();
            Assert.Equal(expectedData.ConvertToString(), actualData.ConvertToString());
        }
    }
}