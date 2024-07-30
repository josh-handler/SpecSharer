using SpecSharer.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace SpecSharerTests.Logic_Layer_Tests
{
    public class UpdateMethodsAndBindingsTests
    {
        readonly string singleBindingFilePath = Path.Combine(""+Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\SingleBindingFile.cs");
        readonly string singleBindingFileWithSmallChangesPath = Path.Combine(""+Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\SingleBindingFileWithSmallChanges.cs");

        readonly string multiBindingFilePath = Path.Combine(""+Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\MultipleBindingFile.cs");
        readonly string multiBindingFileWithChangesPath = Path.Combine(""+Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\MultipleBindingFileWithChanges.cs");

        private MethodReader reader;

        private readonly ITestOutputHelper testOutputHelper;

        public UpdateMethodsAndBindingsTests(ITestOutputHelper testOutputHelper)
        {

            reader = new MethodReader();
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void UpdateMethodAndBindingsTest()
        {
            reader.SetFilePath(singleBindingFileWithSmallChangesPath);
            BindingsFileData oldData = reader.ProcessBindingsFile();

            reader.SetFilePath(singleBindingFilePath);
            BindingsFileData newData = reader.ProcessBindingsFile();

            oldData.UpdateMethodAndBindings("Binding", newData);

            Assert.Equal(newData.ConvertToString(), oldData.ConvertToString());
        }

        [Fact]
        public void UpdateAllMethodsAndBindingsTest()
        {
            string expectedOutput = "[Given(@\"there is the first binding\")]\r\n\tpublic void AFirstBinding()\r\n\t{\r\n            Console.Write(\"Text Here\");\r\n        }\r\n\r\n[When(@\"there is an input of '(.*)'\")]\r\n\tpublic bool SingleInputBinding(string input)\r\n\t{\r\n            //Comment\r\n            Console.WriteLine($\"Binding has input of {input}\");\r\n            return true;\r\n        }\r\n\r\n[Then(@\"there are multiple inputs of '(*.)', '(a|b|c)', '(dddd)'\")]\r\n[When(@\"there are inputs of '(*.)', '(a|b|c)', '(dddd)'\")]\r\n\tpublic void MultiInputBinding(string stringInput, char charInput, int intInput)\r\n\t{\r\n            //Another Comment\r\n            Console.WriteLine($\"Inputs were string {stringInput}, char {charInput} and int {intInput}\");\r\n        }";

            reader.SetFilePath(multiBindingFileWithChangesPath);
            BindingsFileData oldData = reader.ProcessBindingsFile();
            
            reader.SetFilePath(multiBindingFilePath);
            BindingsFileData newData = reader.ProcessBindingsFile();

            oldData.UpdateAllUpdatedMethodsAndBindings(newData);

            Assert.Equal(expectedOutput, oldData.ConvertToString());
        }
    }
}
