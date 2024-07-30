using SpecSharer.CommandLineInterface;
using SpecSharer.CommandLineInterface.CliHelp;
using SpecSharer.Data;
using SpecSharer.Logic;
using SpecSharerTests.Mocking;
using SpecSharerTests.Resources;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.PortableExecutable;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace SpecSharerTests
{
    public class CommandLineInterfaceControllerTests
    {
        CommandLineInterfaceController controller;
        MockConsoleWrapper console;
        MockGithubManager manager;

        readonly string resourcesFolder = Path.Combine("" + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\");

        readonly string singleBindingFilePath = Path.Combine("" + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\SingleBindingFile.cs");

        readonly string multiBindingFilePath = Path.Combine("" + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\MultipleBindingFile.cs");

        readonly string newCliTestTargetFilePath = Path.Combine("" + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\CliTargetFile.txt");

        readonly string invalidFilePath = "Not A Path";

        Dictionary<string, string> argsDict = new Dictionary<string, string>{
            {"e","True" },
            {"p", "p" },
            {"help","True" },
        };

        private readonly ITestOutputHelper testOutputHelper;

        public CommandLineInterfaceControllerTests(ITestOutputHelper testOutputHelper)
        {
            console = new MockConsoleWrapper();
            manager = new MockGithubManager();
            this.testOutputHelper = testOutputHelper;
            controller = new CommandLineInterfaceController(manager, console);
        }

        [Fact]
        public void VerifyInvalidPath()
        {
            bool outcome = controller.VerifyPath(invalidFilePath);
            Assert.False(outcome);
        }

        [Fact]
        public void VerifyValidPath()
        {
            bool outcome = controller.VerifyPath(singleBindingFilePath);
            Assert.True(outcome);
        }

        [Fact]
        public void RequestNonNullPath()
        {
            console.AddInput(singleBindingFilePath);
            string results = controller.RequestPath(true);
            Assert.Equal(singleBindingFilePath, results);
        }

        [Fact]
        public void RequestNullPath()
        {
            console.AddInput(null);
            string results = controller.RequestPath(true);
            Assert.Equal("", results);
        }

        [Fact]
        public void SetValidFilePathFromInput()
        {
            console.AddInput(singleBindingFilePath);
            controller.SetFilePathFromInput();
            string results = controller.GetPath();
            Assert.Equal(singleBindingFilePath, results);
        }

        [Fact]
        public void SetInvalidThenValidFilePathFromInput()
        {
            console.AddInput(invalidFilePath);
            console.AddInput(singleBindingFilePath);
            controller.SetFilePathFromInput();
            string results = controller.GetPath();
            Assert.Equal(singleBindingFilePath, results);
        }

        [Fact]
        public void GiveGeneralHelp()
        {
            Dictionary<string, string> justHelpArgDict = new Dictionary<string, string> { { "help", "True" } };
            controller.GiveHelp(justHelpArgDict);
            Assert.Equal(HelpStringData.generalHelpString, console.GetConsoleOutput());
        }

        [Fact]
        public void GiveHelpForArguments()
        {
            Dictionary<string, string> helpArgsDict = new Dictionary<string, string> { { "help", "True" }, { "path", "True" } };
            controller.GiveHelp(helpArgsDict);
            Assert.Equal(HelpStringData.GenerateArgumentHelpString("p"), console.GetConsoleOutput());
        }

        [Fact]
        public void GiveHelpForTooManyArguments()
        {
            string expected = HelpStringData.multipleArgumentsExplanationString + HelpStringData.generalHelpString;
            controller.GiveHelp(argsDict);
            Assert.Equal(expected, console.GetConsoleOutput());
        }

        [Fact]
        public void GiveHelpNoHelpCommand()
        {
            Dictionary<string, string> justHelpArgDict = new Dictionary<string, string> { { "p", "True" } };

            bool exceptionThrown = false;
            try
            {
                controller.GiveHelp(justHelpArgDict);
            }
            catch (UnreachableException ex)
            {
                Assert.Equal("You have reached the help function without an argument requesting help.", ex.Message);
                exceptionThrown = true;
            }

            Assert.True(exceptionThrown);
        }

        [Fact]
        public void DisplayPassedBindingsTestSingleBindingFile()
        {
            string expected = $"Method Name:{Environment.NewLine}\tpublic void Binding(string input){Environment.NewLine}Associated Bindings:{Environment.NewLine}\t[Given(@\"there is a binding\")]{Environment.NewLine}";
            BindingsFileData data = controller.ProcessFileAtPath(singleBindingFilePath);
            controller.DisplayPassedBindings(data);
            Assert.Equal(expected, console.GetConsoleOutput());
        }

        [Fact]
        public void DisplayPassedBindingsTestMultiBindingFile()
        {
            string expected = $"Method Name:{Environment.NewLine}\tpublic void FirstBinding(){Environment.NewLine}Associated Bindings:{Environment.NewLine}\t[Given(@\"there is a first binding\")]{Environment.NewLine}Method Name:{Environment.NewLine}\tpublic bool SingleInputBinding(string input){Environment.NewLine}Associated Bindings:{Environment.NewLine}\t[When(@\"there is an input of '(.*)'\")]{Environment.NewLine}Method Name:{Environment.NewLine}\tpublic void MultiInputBinding(string stringInput, char charInput, int intInput){Environment.NewLine}Associated Bindings:{Environment.NewLine}\t[Then(@\"there are multiple inputs of '(*.)', '(a|b|c)', '(dddd)'\")]{Environment.NewLine}\t[When(@\"there are inputs of '(*.)', '(a|b|c)', '(dddd)'\")]{Environment.NewLine}";
            BindingsFileData data = controller.ProcessFileAtPath(multiBindingFilePath);
            controller.DisplayPassedBindings(data);
            Assert.Equal(expected, console.GetConsoleOutput());
        }

        [Fact]
        public void DisplayExtractedBindingsTestMultiBindingFile()
        {
            string expected = $"The following methods and associated bindings were extracted:{Environment.NewLine}Method Name:{Environment.NewLine}\tpublic void FirstBinding(){Environment.NewLine}Associated Bindings:{Environment.NewLine}\t[Given(@\"there is a first binding\")]{Environment.NewLine}Method Name:{Environment.NewLine}\tpublic bool SingleInputBinding(string input){Environment.NewLine}Associated Bindings:{Environment.NewLine}\t[When(@\"there is an input of '(.*)'\")]{Environment.NewLine}Method Name:{Environment.NewLine}\tpublic void MultiInputBinding(string stringInput, char charInput, int intInput){Environment.NewLine}Associated Bindings:{Environment.NewLine}\t[Then(@\"there are multiple inputs of '(*.)', '(a|b|c)', '(dddd)'\")]{Environment.NewLine}\t[When(@\"there are inputs of '(*.)', '(a|b|c)', '(dddd)'\")]{Environment.NewLine}";
            controller.ProcessFileAtPath(multiBindingFilePath);
            controller.DisplayExtractedBindings();
            Assert.Equal(expected, console.GetConsoleOutput());

        }

        [Fact]
        public void VerifyTargetFileTest()
        {
            bool result = controller.VerifyLocalTarget(singleBindingFilePath);
            Assert.True(result);
        }

        [Fact]
        public void VerifyTargetFolderTest()
        {
            bool result = controller.VerifyLocalTarget(resourcesFolder);
            Assert.True(result);
        }

        [Fact]
        public void StoreExtractedBindingsLocally()
        {
            File.Delete(newCliTestTargetFilePath);

            string expectedConsoleString = $"Bindings have been succesfully stored on your local machine{Environment.NewLine}";
            string expectedFileString = $"[Given(@\"there is a binding\")]{Environment.NewLine}\tpublic void Binding(string input){Environment.NewLine}\t{{{Environment.NewLine}            //Example Comment{Environment.NewLine}            Console.WriteLine(\"Example binding\");{Environment.NewLine}        }}";
            BindingsFileData data = controller.ProcessFileAtPath(singleBindingFilePath);
            controller.ExtractedBindings = data;
            controller.StoreExtractedBindingsLocally(newCliTestTargetFilePath);

            string result = File.ReadAllText(newCliTestTargetFilePath);

            File.Delete(newCliTestTargetFilePath);
            Assert.Equal(expectedConsoleString, console.GetConsoleOutput());
            Assert.Equal(expectedFileString, result);
        }

        [Fact]
        public async void StoreExtractedBindingsInGithubTest()
        {
            string expected = $"namespace SpecSharer.Data{Environment.NewLine}{{{Environment.NewLine}    public class BindingsFile{Environment.NewLine}    {{{Environment.NewLine}{Environment.NewLine}[Given(@\"there is a first binding\")]{Environment.NewLine}\tpublic void FirstBinding(){Environment.NewLine}\t{{{Environment.NewLine}            //Example Comment{Environment.NewLine}            Console.WriteLine(\"Example binding\");{Environment.NewLine}        }}{Environment.NewLine}{Environment.NewLine}[When(@\"there is an input of '(.*)'\")]{Environment.NewLine}\tpublic bool SingleInputBinding(string input){Environment.NewLine}\t{{{Environment.NewLine}            //Comment{Environment.NewLine}            Console.WriteLine($\"Binding has input of {{input}}\");{Environment.NewLine}            return true;{Environment.NewLine}        }}{Environment.NewLine}{Environment.NewLine}[Then(@\"there are multiple inputs of '(*.)', '(a|b|c)', '(dddd)'\")]{Environment.NewLine}[When(@\"there are inputs of '(*.)', '(a|b|c)', '(dddd)'\")]{Environment.NewLine}\tpublic void MultiInputBinding(string stringInput, char charInput, int intInput){Environment.NewLine}\t{{{Environment.NewLine}            //Another Comment{Environment.NewLine}            Console.WriteLine($\"Inputs were string {{stringInput}}, char {{charInput}} and int {{intInput}}\");{Environment.NewLine}        }}    }}{Environment.NewLine}}}";

            File.Delete(newCliTestTargetFilePath);

            MethodReader reader = new MethodReader();
            reader.SetFilePath(multiBindingFilePath);
            BindingsFileData data = reader.ProcessBindingsFile();

            controller.ExtractedBindings = data;
            await controller.StoreExtractedBindingsInGithub(newCliTestTargetFilePath);
            string actual = File.ReadAllText(newCliTestTargetFilePath);
            Assert.Equal(expected, actual);
            File.Delete(newCliTestTargetFilePath);
            Assert.Equal($"Attempting to store bindings in the GitHub Repository{Environment.NewLine}Bindings have been succesfully stored in the GitHub Repository{Environment.NewLine}", console.GetConsoleOutput());

        }

        [Fact]
        public async void RetrieveBindingsFromGithubTest()
        {            
            MethodReader reader = new();
            reader.SetFilePath(singleBindingFilePath);

            await controller.RetrieveBindingsFromGithub(singleBindingFilePath);

            Assert.Single(controller.RetreivedBindings);
            Assert.Equal(reader.ProcessBindingsFile().ConvertToString(), controller.RetreivedBindings[0].ConvertToString());
            Assert.Equal($"Attempting to retrieve bindings from the GitHub Repository{Environment.NewLine}Bindings have been succesfully retrieved from the GitHub Repository{Environment.NewLine}", console.GetConsoleOutput());
        }
        [Fact]
        public void DisplayRetrievedBindingsTest()
        {
            string expected = $"The following methods and associated bindings were retreived:{Environment.NewLine}Method Name:{Environment.NewLine}\tpublic void FirstBinding(){Environment.NewLine}Associated Bindings:{Environment.NewLine}\t[Given(@\"there is a first binding\")]{Environment.NewLine}Method Name:{Environment.NewLine}\tpublic bool SingleInputBinding(string input){Environment.NewLine}Associated Bindings:{Environment.NewLine}\t[When(@\"there is an input of '(.*)'\")]{Environment.NewLine}Method Name:{Environment.NewLine}\tpublic void MultiInputBinding(string stringInput, char charInput, int intInput){Environment.NewLine}Associated Bindings:{Environment.NewLine}\t[Then(@\"there are multiple inputs of '(*.)', '(a|b|c)', '(dddd)'\")]{Environment.NewLine}\t[When(@\"there are inputs of '(*.)', '(a|b|c)', '(dddd)'\")]{Environment.NewLine}Method Name:{Environment.NewLine}\tpublic void Binding(string input){Environment.NewLine}Associated Bindings:{Environment.NewLine}\t[Given(@\"there is a binding\")]{Environment.NewLine}";

            List<BindingsFileData> data = [];
            data.Add(controller.ProcessFileAtPath(multiBindingFilePath));
            data.Add(controller.ProcessFileAtPath(singleBindingFilePath));
            controller.RetreivedBindings = data;
            controller.DisplayRetrievedBindings();
            Assert.Equal(expected, console.GetConsoleOutput());
        }
        [Fact]
        public void StoreRetrievedBindingsTest() 
        {
            string expectedConsoleString = $"Bindings have been succesfully stored on your local machine{Environment.NewLine}";
            string expectedFileString = $"[Given(@\"there is a first binding\")]{Environment.NewLine}\tpublic void FirstBinding(){Environment.NewLine}\t{{{Environment.NewLine}            //Example Comment{Environment.NewLine}            Console.WriteLine(\"Example binding\");{Environment.NewLine}        }}{Environment.NewLine}{Environment.NewLine}[When(@\"there is an input of '(.*)'\")]{Environment.NewLine}\tpublic bool SingleInputBinding(string input){Environment.NewLine}\t{{{Environment.NewLine}            //Comment{Environment.NewLine}            Console.WriteLine($\"Binding has input of {{input}}\");{Environment.NewLine}            return true;{Environment.NewLine}        }}{Environment.NewLine}{Environment.NewLine}[Then(@\"there are multiple inputs of '(*.)', '(a|b|c)', '(dddd)'\")]{Environment.NewLine}[When(@\"there are inputs of '(*.)', '(a|b|c)', '(dddd)'\")]{Environment.NewLine}\tpublic void MultiInputBinding(string stringInput, char charInput, int intInput){Environment.NewLine}\t{{{Environment.NewLine}            //Another Comment{Environment.NewLine}            Console.WriteLine($\"Inputs were string {{stringInput}}, char {{charInput}} and int {{intInput}}\");{Environment.NewLine}        }}{Environment.NewLine}{Environment.NewLine}[Given(@\"there is a binding\")]{Environment.NewLine}\tpublic void Binding(string input){Environment.NewLine}\t{{{Environment.NewLine}            //Example Comment{Environment.NewLine}            Console.WriteLine(\"Example binding\");{Environment.NewLine}        }}";

            List<BindingsFileData> data = [];
            data.Add(controller.ProcessFileAtPath(multiBindingFilePath));
            data.Add(controller.ProcessFileAtPath(singleBindingFilePath));
            controller.RetreivedBindings = data;
            controller.StoreRetrievedBindingsLocally(newCliTestTargetFilePath);

            string result = File.ReadAllText(newCliTestTargetFilePath);

            File.Delete(newCliTestTargetFilePath);

            Assert.Equal(expectedConsoleString, console.GetConsoleOutput());
            Assert.Equal(expectedFileString, result);
        }
    }
}