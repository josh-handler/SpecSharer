using SpecSharer.Logic;
using SpecSharer.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace SpecSharerTests
{
    public class FileStorerTests
    {
        readonly string multiBindingFilePath = Path.Combine(""+Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\MultipleBindingFile.cs");

        readonly string newTargetFilePath = Path.Combine(""+Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\TargetFile.txt");

        readonly string existingTargetFilePath = Path.Combine(""+Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\ExistingTargetFile.txt");

        private readonly string singleBindingFilePath = Path.Combine("" + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\SingleBindingFile.cs");

        private MethodReader reader;
        private FileStorer storer;

        private readonly ITestOutputHelper testOutputHelper;

        public FileStorerTests(ITestOutputHelper testOutputHelper)
        {

            reader = new MethodReader();
            storer = new FileStorer();
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void ProduceStringForStorage()
        {
            string expected = $"namespace SpecSharerLocalStorage{Environment.NewLine}{{{Environment.NewLine}    public class TargetFile{Environment.NewLine}    {{{Environment.NewLine}{Environment.NewLine}[Given(@\"there is a binding\")]{Environment.NewLine}\tpublic void Binding(string input){Environment.NewLine}\t{{{Environment.NewLine}            //Example Comment{Environment.NewLine}            Console.WriteLine(\"Example binding\");{Environment.NewLine}        }}    }}{Environment.NewLine}}}";

            reader.SetFilePath(singleBindingFilePath);
            BindingsFileData fileData = reader.ProcessBindingsFile();

            string actual = storer.ProduceStringForStorage(fileData, newTargetFilePath);

            Assert.Equal(expected, actual);

        }

        [Fact]
        public void UpdateBindingsDataWithLocalData()
        {
            string expected = $"[Given(@\"there is a binding\")]{Environment.NewLine}\tpublic void Binding(string input){Environment.NewLine}\t{{{Environment.NewLine}            //Example Comment{Environment.NewLine}            Console.WriteLine(\"Example binding\");{Environment.NewLine}        }}{Environment.NewLine}{Environment.NewLine}[Given(@\"there is a first binding\")]{Environment.NewLine}\tpublic void FirstBinding(){Environment.NewLine}\t{{{Environment.NewLine}            //Example Comment{Environment.NewLine}            Console.WriteLine(\"Example binding\");{Environment.NewLine}        }}{Environment.NewLine}{Environment.NewLine}[When(@\"there is an input of '(.*)'\")]{Environment.NewLine}\tpublic bool SingleInputBinding(string input){Environment.NewLine}\t{{{Environment.NewLine}            //Comment{Environment.NewLine}            Console.WriteLine($\"Binding has input of {{input}}\");{Environment.NewLine}            return true;{Environment.NewLine}        }}{Environment.NewLine}{Environment.NewLine}[Then(@\"there are multiple inputs of '(*.)', '(a|b|c)', '(dddd)'\")]{Environment.NewLine}[When(@\"there are inputs of '(*.)', '(a|b|c)', '(dddd)'\")]{Environment.NewLine}\tpublic void MultiInputBinding(string stringInput, char charInput, int intInput){Environment.NewLine}\t{{{Environment.NewLine}            //Another Comment{Environment.NewLine}            Console.WriteLine($\"Inputs were string {{stringInput}}, char {{charInput}} and int {{intInput}}\");{Environment.NewLine}        }}";

            reader.SetFilePath(multiBindingFilePath);
            BindingsFileData fileData = reader.ProcessBindingsFile();

            BindingsFileData updatedData = storer.UpdateBindingsDataWithExistingContent(singleBindingFilePath, fileData, reader);

            string actual = updatedData.ConvertToString();
            testOutputHelper.WriteLine(actual);
            Assert.Equal(expected, actual);

        }

        [Fact]
        public void StoreBindingInNewFile()
        {
            string expected = $"namespace SpecSharerLocalStorage{Environment.NewLine}{{{Environment.NewLine}    public class TargetFile{Environment.NewLine}    {{{Environment.NewLine}{Environment.NewLine}[Given(@\"there is a first binding\")]{Environment.NewLine}\tpublic void FirstBinding(){Environment.NewLine}\t{{{Environment.NewLine}            //Example Comment{Environment.NewLine}            Console.WriteLine(\"Example binding\");{Environment.NewLine}        }}{Environment.NewLine}{Environment.NewLine}[When(@\"there is an input of '(.*)'\")]{Environment.NewLine}\tpublic bool SingleInputBinding(string input){Environment.NewLine}\t{{{Environment.NewLine}            //Comment{Environment.NewLine}            Console.WriteLine($\"Binding has input of {{input}}\");{Environment.NewLine}            return true;{Environment.NewLine}        }}{Environment.NewLine}{Environment.NewLine}[Then(@\"there are multiple inputs of '(*.)', '(a|b|c)', '(dddd)'\")]{Environment.NewLine}[When(@\"there are inputs of '(*.)', '(a|b|c)', '(dddd)'\")]{Environment.NewLine}\tpublic void MultiInputBinding(string stringInput, char charInput, int intInput){Environment.NewLine}\t{{{Environment.NewLine}            //Another Comment{Environment.NewLine}            Console.WriteLine($\"Inputs were string {{stringInput}}, char {{charInput}} and int {{intInput}}\");{Environment.NewLine}        }}    }}{Environment.NewLine}}}";

            File.Delete(newTargetFilePath);

            reader.SetFilePath(multiBindingFilePath);
            BindingsFileData fileData = reader.ProcessBindingsFile();
            storer.StoreBindings(newTargetFilePath, fileData, true, reader);

            string result = File.ReadAllText(newTargetFilePath);

            File.Delete(newTargetFilePath);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void StoreBindingInExistingFile()
        {
            string expectedText = $"namespace SpecSharerLocalStorage{Environment.NewLine}{{{Environment.NewLine}    public class ExistingTargetFile{Environment.NewLine}    {{{Environment.NewLine}{Environment.NewLine}[Then(@\"a target Exists\")]{Environment.NewLine}\tpublic void Target(string input){Environment.NewLine}\t{{{Environment.NewLine}            //Existing Comment{Environment.NewLine}            Console.WriteLine(\"Simple Method\");{Environment.NewLine}        }}{Environment.NewLine}{Environment.NewLine}[Given(@\"there is a first binding\")]{Environment.NewLine}\tpublic void FirstBinding(){Environment.NewLine}\t{{{Environment.NewLine}            //Example Comment{Environment.NewLine}            Console.WriteLine(\"Example binding\");{Environment.NewLine}        }}{Environment.NewLine}{Environment.NewLine}[When(@\"there is an input of '(.*)'\")]{Environment.NewLine}\tpublic bool SingleInputBinding(string input){Environment.NewLine}\t{{{Environment.NewLine}            //Comment{Environment.NewLine}            Console.WriteLine($\"Binding has input of {{input}}\");{Environment.NewLine}            return true;{Environment.NewLine}        }}{Environment.NewLine}{Environment.NewLine}[Then(@\"there are multiple inputs of '(*.)', '(a|b|c)', '(dddd)'\")]{Environment.NewLine}[When(@\"there are inputs of '(*.)', '(a|b|c)', '(dddd)'\")]{Environment.NewLine}\tpublic void MultiInputBinding(string stringInput, char charInput, int intInput){Environment.NewLine}\t{{{Environment.NewLine}            //Another Comment{Environment.NewLine}            Console.WriteLine($\"Inputs were string {{stringInput}}, char {{charInput}} and int {{intInput}}\");{Environment.NewLine}        }}    }}{Environment.NewLine}}}";

            reader.SetFilePath(multiBindingFilePath);
            BindingsFileData? fileData = reader.ProcessBindingsFile();
            storer.StoreBindings(existingTargetFilePath, fileData, true, reader);

            Assert.Equal(expectedText, File.ReadAllText(existingTargetFilePath));
        }
    }
}
