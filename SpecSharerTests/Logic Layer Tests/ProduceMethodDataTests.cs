using SpecSharer.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace SpecSharerTests
{
    public class ProduceMethodDataTests
    {
        readonly string singleBindingFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\SingleBindingFile.cs");

        readonly string multiBindingFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\MultipleBindingFile.cs");

        private MethodReader reader;

        private readonly ITestOutputHelper _testOutputHelper;

        public ProduceMethodDataTests(ITestOutputHelper testOutputHelper)
        {

            reader = new MethodReader();
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void produceSingleMethodData()
        {
            reader.SetFilePath(singleBindingFilePath);
            BindingsFileData fileData = reader.ProcessBindingsFile();
            List<MethodData> methods = fileData.produceMethodData();

            Assert.Single(methods);
            Assert.Equal("Binding", methods.Single().Name);
            Assert.Equal("{\r\n" +
                "            //Example Comment\r\n" +
                "            Console.WriteLine(\"Example binding\");\r\n" +
                "        }", methods.Single().Body);
            Assert.Equal("public void ", methods.Single().Modifiers);
            Assert.Equal("string input", methods.Single().Parameters.Single());
            Assert.Equal("[Given(@\"there is a binding\")]", methods.Single().Bindings.Single());
            Assert.Equal("public void Binding(string input)", methods.Single().getMethodLine());
        }

        [Fact]
        public void produceMultipleMethodDatas()
        {
            reader.SetFilePath(multiBindingFilePath);
            BindingsFileData fileData = reader.ProcessBindingsFile();
            List<MethodData> methods = fileData.produceMethodData();

            Assert.Equal(3, methods.Count);

            Assert.Equal("FirstBinding", methods[0].Name);
            Assert.Equal("SingleInputBinding", methods[1].Name);
            Assert.Equal("MultiInputBinding", methods[2].Name);

            Assert.Equal("{\r\n" +
                "            //Example Comment\r\n" +
                "            Console.WriteLine(\"Example binding\");\r\n" +
                "        }",
                methods[0].Body);

            Assert.Equal("{\r\n" +
                "            //Comment\r\n" +
                "            Console.WriteLine($\"Binding has input of {input}\");\r\n" +
                "            return true;\r\n" +
                "        }",
                methods[1].Body);

            Assert.Equal("{\r\n" +
                "            //Another Comment\r\n" +
                "            Console.WriteLine($\"Inputs were string {stringInput}, char {charInput} and int {intInput}\");\r\n" +
                "        }",
                methods[2].Body);

            Assert.Equal("public void ", methods[0].Modifiers);
            Assert.Equal("public bool ", methods[1].Modifiers);
            Assert.Equal("public void ", methods[2].Modifiers);

            Assert.Empty(methods[0].Parameters);
            Assert.Equal("string input", methods[1].Parameters.Single());
            Assert.Equal("string stringInput", methods[2].Parameters[0]);
            Assert.Equal("char charInput", methods[2].Parameters[1]);
            Assert.Equal("int intInput", methods[2].Parameters[2]);

            Assert.Equal("[Given(@\"there is a first binding\")]", methods[0].Bindings.Single());
            Assert.Equal("[When(@\"there is an input of '(.*)'\")]", methods[1].Bindings.Single());
            Assert.Equal("[Then(@\"there are multiple inputs of '(*.)', '(a|b|c)', '(dddd)'\")]", methods[2].Bindings[0]);
            Assert.Equal("[When(@\"there are inputs of '(*.)', '(a|b|c)', '(dddd)'\")]", methods[2].Bindings[1]);

            Assert.Equal("public void FirstBinding()", methods[0].getMethodLine());
            Assert.Equal("public bool SingleInputBinding(string input)", methods[1].getMethodLine());
            Assert.Equal("public void MultiInputBinding(string stringInput, char charInput, int intInput)", methods[2].getMethodLine());
        }
    }
}
