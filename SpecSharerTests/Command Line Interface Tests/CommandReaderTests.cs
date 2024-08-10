using SpecSharer.CommandLineInterface;
using SpecSharer.CommandLineInterface.CliHelp;
using SpecSharer.Logic;
using SpecSharerTests.Mocking;
using SpecSharerTests.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace SpecSharerTests.Command_Line_Interface_Tests
{
    public class CommandReaderTests
    {
        CommandReader reader;
        MockConsoleWrapper console;
        MockGithubManager manager;

        private readonly ITestOutputHelper testOutputHelper;
        private static readonly string singleBindingFilePath = Path.Combine("" + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\SingleBindingFile.cs");
        private static readonly string multiBindingFilePath = Path.Combine(""+Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\MultipleBindingFile.cs");
        private static readonly string commandReaderNewFilePath = Path.Combine(""+Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\CommandReaderNewFile.cs");
        private static readonly string githubSingleBindingTestFilePath = "Data/SingleBindingTestFile";

        private static readonly string commandReaderTargetFile = Path.Combine("" + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\CommandReaderTargetFile.cs");

        private readonly Dictionary<string, string> validArgsDict = new Dictionary<string, string>{ 
            {"e","True" },
            {"p", multiBindingFilePath },
            {"help","True" },
        };

        private readonly Dictionary<string, string> invalidArgsDict = new Dictionary<string, string>{
            { "w","True" },
            {"paht", "p" },
        };

        private readonly Dictionary<string, string> mixedArgDict = new Dictionary<string, string>{
            { "w","True" },
            {"paht", "p" },
            {"e","True" },
            {"p", "p" },
            {"help","True" },
        };

        private readonly Dictionary<string, string> extractToConsoleArgDict = new Dictionary<string, string>{
            {"e","True" },
            {"p", multiBindingFilePath },
        };

        private readonly Dictionary<string, string> extractToLocalFileArgDict = new Dictionary<string, string>{
            {"e","True" },
            {"p", multiBindingFilePath },
            {"target", commandReaderNewFilePath }
        };

        Dictionary<string, string> extractToGithubArgDict = new Dictionary<string, string>{
            {"e","True" },
            {"p", singleBindingFilePath },
            {"target",  commandReaderTargetFile},
            {"g", "True"}
        };

        public CommandReaderTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
            console = new MockConsoleWrapper();
            manager = new MockGithubManager();
            reader = new CommandReader(manager, console, new MethodReader());
        }

        [Fact]
        public void ValidateValidCommands()
        {
            List<string> invalidArgs = reader.ValidateArgs(validArgsDict.Keys);
            Assert.Empty(invalidArgs);
        }

        [Fact]
        public void ValidateInvalidCommands()
        {
            List<string> invalidArgs = reader.ValidateArgs(invalidArgsDict.Keys);
            Assert.Contains("w",invalidArgs);
            Assert.Contains("paht",invalidArgs);
        }

        [Fact]
        public void ValidateMixedCommands()
        {
            List<string> invalidArgs = reader.ValidateArgs(mixedArgDict.Keys);
            Assert.Contains("w", invalidArgs);
            Assert.Contains("paht", invalidArgs);
        }

        [Fact]
        public async void InterpretValidHelpCommands()
        {
            string expected = HelpStringData.multipleArgumentsExplanationString + HelpStringData.generalHelpString;
            await reader.Interpret(validArgsDict);
            Assert.Equal(expected, console.GetConsoleOutput());
        }

        [Fact]
        public async void InterpretValidExtractAndNoPathCommands()
        {
            string expected = $"The following methods and associated bindings were extracted:{Environment.NewLine}Method Name:{Environment.NewLine}\tpublic void FirstBinding(){Environment.NewLine}Associated Bindings:{Environment.NewLine}\t[Given(@\"there is a first binding\")]{Environment.NewLine}Method Name:{Environment.NewLine}\tpublic bool SingleInputBinding(string input){Environment.NewLine}Associated Bindings:{Environment.NewLine}\t[When(@\"there is an input of '(.*)'\")]{Environment.NewLine}Method Name:{Environment.NewLine}\tpublic void MultiInputBinding(string stringInput, char charInput, int intInput){Environment.NewLine}Associated Bindings:{Environment.NewLine}\t[Then(@\"there are multiple inputs of '(*.)', '(a|b|c)', '(dddd)'\")]{Environment.NewLine}\t[When(@\"there are inputs of '(*.)', '(a|b|c)', '(dddd)'\")]{Environment.NewLine}";

            await reader.Interpret(extractToConsoleArgDict);
            Assert.Equal(expected, console.GetConsoleOutput());
        }

        [Fact]
        public async void InterpretValidExtractAndLocalPathCommands()
        {
            if(File.Exists(commandReaderNewFilePath))
            {
                File.Delete(commandReaderNewFilePath);
            }

            MethodReader methodReader = new MethodReader();
            methodReader.SetFilePath(multiBindingFilePath);
            string expected = methodReader.ProcessBindingsFile().ConvertToString();

            await reader.Interpret(extractToLocalFileArgDict);
            Assert.Equal(expected, File.ReadAllText(commandReaderNewFilePath));
            Assert.Equal($"Bindings have been succesfully stored on your local machine{Environment.NewLine}", console.GetConsoleOutput());

            File.Delete(commandReaderNewFilePath);
        }

        [Fact]
        public async void InterpretValidExtractAndGithubPathCommands()
        {
            //New file CommandReaderEditFile or similar
            string expected = "namespace SpecSharer.Data\r\n{\r\n    public class BindingsFile\r\n    {\r\n\r\n[Given(@\"there is a binding\")]\r\n\tpublic void Binding(string input)\r\n\t{\r\n            //Example Comment\r\n            Console.WriteLine(\"Example binding\");\r\n        }    }\r\n}";

            File.WriteAllText(commandReaderTargetFile, "namespace SpecSharer.Data{public class BindingsFile{}}/nTemporarily removed for testing");

            string beforeContent = File.ReadAllText(commandReaderTargetFile);

            await reader.Interpret(extractToGithubArgDict);

            string afterContent = File.ReadAllText(commandReaderTargetFile);

            testOutputHelper.WriteLine(afterContent);

            Assert.Equal("namespace SpecSharer.Data{public class BindingsFile{}}/nTemporarily removed for testing", beforeContent);
            Assert.Equal(expected, afterContent);
            Assert.Equal($"Attempting to store bindings in the GitHub Repository{Environment.NewLine}Bindings have been succesfully stored in the GitHub Repository{Environment.NewLine}", console.GetConsoleOutput());
        }
    }
}
