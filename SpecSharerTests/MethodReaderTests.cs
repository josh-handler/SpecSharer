using SpecSharer.Logic;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace SpecSharerTests
{
    public class MethodReaderTests
    {
        readonly string singleBindingFilePath = "C:\\Users\\jshimans\\source\\repos\\SpecSharer\\SpecSharerTests\\Resources\\SingleBindingFile.cs";

        readonly string multiBindingFilePath = "C:\\Users\\jshimans\\source\\repos\\SpecSharer\\SpecSharerTests\\Resources\\MultipleBindingFile.cs";

        readonly string invalidFilePath = "Not A Path";

        private MethodReader reader;

        private readonly ITestOutputHelper _testOutputHelper;

        public MethodReaderTests(ITestOutputHelper testOutputHelper)
        {

            reader = new MethodReader();
            _testOutputHelper = testOutputHelper;
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
        public void MapMethodsToBindingsSingleBindingTest()
        {
            reader.SetFilePath(singleBindingFilePath);
            Dictionary<String, String> results = reader.MapMethodsToBindings(singleBindingFilePath);

            Assert.Single(results.Keys);

            Assert.Equal("[Given(@\"there is a binding\")]", results.Keys.Single());

            Assert.Equal("public void Binding()", results["[Given(@\"there is a binding\")]"]);
        }
        [Fact]
        public void ExtractMethodsAndBodiesSingleBindingTest()
        {
            reader.SetFilePath(singleBindingFilePath);
            Dictionary<String, String> results = reader.ExtractMethodsAndBodies();

            Assert.Single(results.Keys);

            Assert.Equal("Binding", results.Keys.Single());

            Assert.Equal("{\r\n" +
                "            //Example Comment\r\n" +
                "            Console.WriteLine(\"Example binding\");\r\n" +
                "        }",
                results[results.Keys.Single()]);
        }

        [Fact]
        public void MapMethodsToBindingsMultiBindingTest()
        {
            reader.SetFilePath(multiBindingFilePath);
            Dictionary<String, String> results = reader.MapMethodsToBindings(singleBindingFilePath);

            Assert.Equal(3, results.Keys.Count);

            Assert.Equal("[Given(@\"there is a first binding\")]", results.Keys.First());

            Assert.Equal("[When(@\"there is an input of '(.*)'\")]", results.Keys.ElementAt(1));

            Assert.Equal("[Then(@\"there are multiple inputs of '(*.)', '(a|b|c)', '(dddd)'\")]", results.Keys.ElementAt(2));

            Assert.Equal("public void FirstBinding()", results[results.Keys.First()]);

            Assert.Equal("public void SingleInputBinding(string input)", results[results.Keys.ElementAt(1)]);

            Assert.Equal("public void MultiInputBinding(string stringInput, char charInput, int intInput)", results[results.Keys.ElementAt(2)]);
        }

        [Fact]
        public void ExtractMethodsAndBodiesMultiBindingTest()
        {
            reader.SetFilePath(multiBindingFilePath);
            Dictionary<String, String> results = reader.ExtractMethodsAndBodies();
            foreach (string key in results.Keys)
            {
                _testOutputHelper.WriteLine(key);
                _testOutputHelper.WriteLine(results[key]);
                _testOutputHelper.WriteLine("");
            }

            Assert.Equal(3, results.Keys.Count);

            Assert.Equal("FirstBinding", results.Keys.First());

            Assert.Equal("SingleInputBinding", results.Keys.ElementAt(1));

            Assert.Equal("MultiInputBinding", results.Keys.ElementAt(2));

            Assert.Equal("{\r\n" +
                "            //Example Comment\r\n" +
                "            Console.WriteLine(\"Example binding\");\r\n" +
                "        }",
                results[results.Keys.First()]);

            Assert.Equal("{\r\n" +
                "            //Comment\r\n" +
                "            Console.WriteLine($\"Binding has input of {input}\");\r\n" +
                "        }",
                results[results.Keys.ElementAt(1)]);

            Assert.Equal("{\r\n" +
                "            //Another Comment\r\n" +
                "            Console.WriteLine($\"Inputs were string {stringInput}, char {charInput} and int {intInput}\");\r\n" +
                "        }",
                results[results.Keys.ElementAt(2)]);
        }
    }
}