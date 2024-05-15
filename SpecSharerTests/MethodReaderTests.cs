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
        public void ProcessBindingsFileSingleBindingTest()
        {
            reader.SetFilePath(singleBindingFilePath);
            BindingsData results = reader.processBindingsFile();

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
            Assert.Equal("[Given(@\"there is a binding\")]", results.Bindings.Single().Key);
            Assert.Equal("public void Binding(string input)", results.Bindings.Single().Value);

            Assert.Equal("public void Binding(string input)", results.getMethodLine("Binding"));

        }

        [Fact]
        public void MapMethodsToBindingsMultiBindingTest()
        {
            reader.SetFilePath(multiBindingFilePath);
            BindingsData results = reader.processBindingsFile();

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


            Assert.Equal("[Given(@\"there is a first binding\")]", results.Bindings.Keys.First());
            Assert.Equal("[When(@\"there is an input of '(.*)'\")]", results.Bindings.Keys.ElementAt(1));
            Assert.Equal("[Then(@\"there are multiple inputs of '(*.)', '(a|b|c)', '(dddd)'\")]", results.Bindings.Keys.ElementAt(2));

            Assert.Equal("public void FirstBinding()", results.Bindings[results.Bindings.Keys.First()]);
            Assert.Equal("public bool SingleInputBinding(string input)", results.Bindings[results.Bindings.Keys.ElementAt(1)]);
            Assert.Equal("public void MultiInputBinding(string stringInput, char charInput, int intInput)", results.Bindings[results.Bindings.Keys.ElementAt(2)]);

            Assert.Equal("public void FirstBinding()", results.getMethodLine("FirstBinding"));
            Assert.Equal("public bool SingleInputBinding(string input)", results.getMethodLine("SingleInputBinding"));
            Assert.Equal("public void MultiInputBinding(string stringInput, char charInput, int intInput)", results.getMethodLine("MultiInputBinding"));
        }
    }
}