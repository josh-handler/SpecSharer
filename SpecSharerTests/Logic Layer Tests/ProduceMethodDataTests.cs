using Microsoft.VisualStudio.TextManager.Interop;
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
        readonly string singleBindingFilePath = Path.Combine(""+Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\SingleBindingFile.cs");

        readonly string multiBindingFilePath = Path.Combine(""+Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\MultipleBindingFile.cs");

        private MethodReader reader;

        private readonly ITestOutputHelper testOutputHelper;

        public ProduceMethodDataTests(ITestOutputHelper testOutputHelper)
        {

            reader = new MethodReader();
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void ProduceSingleMethodData()
        {
            reader.SetFilePath(singleBindingFilePath);
            BindingsFileData fileData = reader.ProcessBindingsFile();
            List<MethodData> methods = fileData.ProduceAllMethodData();

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
        public void ProduceMultipleMethodDatas()
        {
            reader.SetFilePath(multiBindingFilePath);
            BindingsFileData fileData = reader.ProcessBindingsFile();
            List<MethodData> methods = fileData.ProduceAllMethodData();

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

        [Fact]
        public void BindingsFileDataConvertsToString()
        {
            reader.SetFilePath(multiBindingFilePath);

            string expectedOutput = "[Given(@\"there is a first binding\")]\r\n\tpublic void FirstBinding()\r\n\t{\r\n            //Example Comment\r\n            Console.WriteLine(\"Example binding\");\r\n        }\r\n\r\n[When(@\"there is an input of '(.*)'\")]\r\n\tpublic bool SingleInputBinding(string input)\r\n\t{\r\n            //Comment\r\n            Console.WriteLine($\"Binding has input of {input}\");\r\n            return true;\r\n        }\r\n\r\n[Then(@\"there are multiple inputs of '(*.)', '(a|b|c)', '(dddd)'\")]\r\n[When(@\"there are inputs of '(*.)', '(a|b|c)', '(dddd)'\")]\r\n\tpublic void MultiInputBinding(string stringInput, char charInput, int intInput)\r\n\t{\r\n            //Another Comment\r\n            Console.WriteLine($\"Inputs were string {stringInput}, char {charInput} and int {intInput}\");\r\n        }";

            BindingsFileData fileData = reader.ProcessBindingsFile();

            Assert.Equal(expectedOutput, fileData.ConvertToString());
        }

        [Fact]
        public void ProduceSpecificMethodDataTest()
        {
            reader.SetFilePath(multiBindingFilePath);
            BindingsFileData fileData = reader.ProcessBindingsFile();
            MethodData specificData = fileData.ProduceSpecificMethodData("MultiInputBinding");

            Assert.Equal("MultiInputBinding", specificData.Name);
            Assert.Equal("{\r\n            //Another Comment\r\n            Console.WriteLine($\"Inputs were string {stringInput}, char {charInput} and int {intInput}\");\r\n        }", specificData.Body);
            Assert.Equal("public void ", specificData.Modifiers);
            Assert.Contains("string stringInput", specificData.Parameters);
            Assert.Contains("char charInput", specificData.Parameters);
            Assert.Contains("int intInput", specificData.Parameters);
            Assert.Contains("[Then(@\"there are multiple inputs of '(*.)', '(a|b|c)', '(dddd)'\")]", specificData.Bindings);
            Assert.Contains("[When(@\"there are inputs of '(*.)', '(a|b|c)', '(dddd)'\")]", specificData.Bindings);
        }

        [Fact]
        public void RemoveSharedDataAllSharedTest()
        {
            reader.SetFilePath(multiBindingFilePath);
            BindingsFileData firstFileData = reader.ProcessBindingsFile();
            BindingsFileData secondFileData = reader.ProcessBindingsFile();

            Assert.Contains("FirstBinding", firstFileData.Methods);
            Assert.Contains("SingleInputBinding", firstFileData.Methods);
            Assert.Contains("MultiInputBinding", firstFileData.Methods);
            
            Assert.Contains("FirstBinding", secondFileData.Methods);
            Assert.Contains("SingleInputBinding", secondFileData.Methods);
            Assert.Contains("MultiInputBinding", secondFileData.Methods);

            Assert.NotSame(firstFileData, secondFileData);

            secondFileData.RemoveSharedData(firstFileData);

            Assert.Empty(secondFileData.Methods);
            Assert.Empty(secondFileData.Bindings);
            Assert.Empty(secondFileData.Parameters);
            Assert.Empty(secondFileData.Modifiers);
            Assert.Empty(secondFileData.Bodies);

            Assert.Contains("FirstBinding", firstFileData.Methods);
            Assert.Contains("SingleInputBinding", firstFileData.Methods);
            Assert.Contains("MultiInputBinding", firstFileData.Methods);
        }
        [Fact]
        public void RemoveSpecificMethodTest()
        {
            reader.SetFilePath(multiBindingFilePath);
            BindingsFileData fileData = reader.ProcessBindingsFile();

            Assert.Contains("MultiInputBinding", fileData.Methods);
            Assert.Contains("MultiInputBinding", fileData.Bodies.Keys);
            Assert.Contains("[Then(@\"there are multiple inputs of '(*.)', '(a|b|c)', '(dddd)'\")]", fileData.Bindings["public void MultiInputBinding(string stringInput, char charInput, int intInput)"]);
            Assert.Contains("[When(@\"there are inputs of '(*.)', '(a|b|c)', '(dddd)'\")]", fileData.Bindings["public void MultiInputBinding(string stringInput, char charInput, int intInput)"]);
            Assert.Contains("MultiInputBinding", fileData.Parameters.Keys);
            Assert.Contains("MultiInputBinding", fileData.Modifiers.Keys);

            fileData.RemoveSpecificMethodData("MultiInputBinding");

            Assert.DoesNotContain("MultiInputBinding", fileData.Methods);
            Assert.DoesNotContain("MultiInputBinding", fileData.Bodies.Keys);
            Assert.DoesNotContain("MultiInputBinding", fileData.Bindings.Keys);
            Assert.DoesNotContain("MultiInputBinding", fileData.Parameters.Keys);
            Assert.DoesNotContain("MultiInputBinding", fileData.Modifiers.Keys);
        }

        [Fact]
        public void RemoveSharedDataSomeSharedTest()
        {
            reader.SetFilePath(multiBindingFilePath);
            BindingsFileData fileData = reader.ProcessBindingsFile();
            BindingsFileData secondFileData = reader.ProcessBindingsFile();

            Assert.Contains("FirstBinding", fileData.Methods);
            Assert.Contains("SingleInputBinding", fileData.Methods);
            Assert.Contains("MultiInputBinding", fileData.Methods);

            Assert.Contains("FirstBinding", secondFileData.Methods);
            Assert.Contains("SingleInputBinding", secondFileData.Methods);
            Assert.Contains("MultiInputBinding", secondFileData.Methods);

            secondFileData.RemoveSpecificMethodData("MultiInputBinding");

            Assert.DoesNotContain("MultiInputBinding", secondFileData.Methods);
            Assert.DoesNotContain("MultiInputBinding", secondFileData.Bodies.Keys);
            Assert.DoesNotContain("public void MultiInputBinding(string stringInput, char charInput, int intInput)", secondFileData.Bindings.Keys);
            Assert.DoesNotContain("MultiInputBinding", secondFileData.Parameters.Keys);
            Assert.DoesNotContain("MultiInputBinding", secondFileData.Modifiers.Keys);

            fileData.RemoveSharedData(secondFileData);

            Assert.Contains("MultiInputBinding", fileData.Methods);
            Assert.Contains("MultiInputBinding", fileData.Bodies.Keys);
            Assert.Contains("public void MultiInputBinding(string stringInput, char charInput, int intInput)", fileData.Bindings.Keys);
            Assert.Contains("MultiInputBinding", fileData.Parameters.Keys);
            Assert.Contains("MultiInputBinding", fileData.Modifiers.Keys);

            Assert.DoesNotContain("FirstBinding", fileData.Methods);
            Assert.DoesNotContain("SingleInputBinding", fileData.Methods);
        }

    }
}
