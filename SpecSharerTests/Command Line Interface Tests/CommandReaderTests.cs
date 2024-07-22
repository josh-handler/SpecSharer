using SpecSharer.CommandLineInterface;
using SpecSharer.CommandLineInterface.CliHelp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace SpecSharerTests.Command_Line_Interface_Tests
{
    public class CommandReaderTests
    {
        CommandReader reader;
        private readonly ITestOutputHelper _testOutputHelper;
        readonly static string multiBindingFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\MultipleBindingFile.cs");

        Dictionary<string, string> validArgsDict = new Dictionary<string, string>{ 
            {"e","True" },
            {"p", multiBindingFilePath },
            {"help","True" },
        };

        Dictionary<string, string> invalidArgsDict = new Dictionary<string, string>{
            { "w","True" },
            {"paht", "p" },
        };

        Dictionary<string, string> mixedArgDict = new Dictionary<string, string>{
            { "w","True" },
            {"paht", "p" },
            {"e","True" },
            {"p", "p" },
            {"help","True" },
        };

        Dictionary<string, string> targetArgsDict = new Dictionary<string, string>{
            {"e","True" },
            {"p", multiBindingFilePath },
        };

        public CommandReaderTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            reader = new CommandReader();
        }

        [Fact]
        public void ValidateValidCommands()
        {
            var invalidArgs = reader.ValidateArgs(validArgsDict.Keys);
            Assert.Empty(invalidArgs);
        }

        [Fact]
        public void ValidateInvalidCommands()
        {
            var invalidArgs = reader.ValidateArgs(invalidArgsDict.Keys);
            Assert.Contains("w",invalidArgs);
            Assert.Contains("paht",invalidArgs);
        }

        [Fact]
        public void ValidateMixedCommands()
        {
            var invalidArgs = reader.ValidateArgs(mixedArgDict.Keys);
            Assert.Contains("w", invalidArgs);
            Assert.Contains("paht", invalidArgs);
        }

        [Fact]
        public void InterpretValidHelpCommands()
        {
            string expected = HelpStringData.multipleArgumentsExplanationString + HelpStringData.generalHelpString;

            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);
                reader.Interpret(validArgsDict);
                Assert.Equal(expected, sw.ToString());
            }
        }

        [Fact]
        public void InterpretValidExtractAndPathCommands()
        {
            string expected = $"The following methods and associated bindings were extracted:{Environment.NewLine}Method Name:{Environment.NewLine}\tpublic void FirstBinding(){Environment.NewLine}Associated Bindings:{Environment.NewLine}\t[Given(@\"there is a first binding\")]{Environment.NewLine}Method Name:{Environment.NewLine}\tpublic bool SingleInputBinding(string input){Environment.NewLine}Associated Bindings:{Environment.NewLine}\t[When(@\"there is an input of '(.*)'\")]{Environment.NewLine}Method Name:{Environment.NewLine}\tpublic void MultiInputBinding(string stringInput, char charInput, int intInput){Environment.NewLine}Associated Bindings:{Environment.NewLine}\t[Then(@\"there are multiple inputs of '(*.)', '(a|b|c)', '(dddd)'\")]{Environment.NewLine}\t[When(@\"there are inputs of '(*.)', '(a|b|c)', '(dddd)'\")]{Environment.NewLine}";

            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);
                reader.Interpret(targetArgsDict);
                Assert.Equal(expected, sw.ToString());
            }
        }
    }
}
