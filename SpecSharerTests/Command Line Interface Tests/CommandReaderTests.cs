using SpecSharer.CommandLineInterface;
using System;
using System.Collections.Generic;
using System.Linq;
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

        Dictionary<string, string> validArgsDict = new Dictionary<string, string>{ 
            {"e","True" },
            {"p", "p" },
            {"help","True" },
        };

        Dictionary<string, string> invalidArgsDict = new Dictionary<string, string>{
            { "t","True" },
            {"paht", "p" },
        };

        Dictionary<string, string> mixedArgDict = new Dictionary<string, string>{
            { "t","True" },
            {"paht", "p" },
            {"e","True" },
            {"p", "p" },
            {"help","True" },
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
            Assert.Contains("t",invalidArgs);
            Assert.Contains("paht",invalidArgs);
        }

        [Fact]
        public void ValidateMixedCommands()
        {
            var invalidArgs = reader.ValidateArgs(mixedArgDict.Keys);
            Assert.Contains("t", invalidArgs);
            Assert.Contains("paht", invalidArgs);
        }

        [Fact]
        public void InterpretValidCommands()
        {
            reader.Interpret(validArgsDict);
        }
    }
}
