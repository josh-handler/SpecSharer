using SpecSharer.CommandLineInterface;
using SpecSharer.CommandLineInterface.CliHelp;
using SpecSharer.Logic;
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

        readonly string resourcesFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\");

        readonly string singleBindingFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\SingleBindingFile.cs");

        readonly string multiBindingFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\MultipleBindingFile.cs");

        readonly string invalidFilePath = "Not A Path";

        Dictionary<string, string> argsDict = new Dictionary<string, string>{
            {"e","True" },
            {"p", "p" },
            {"help","True" },
        };

        private readonly ITestOutputHelper _testOutputHelper;

        public CommandLineInterfaceControllerTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            controller = new CommandLineInterfaceController();
            if(Console.IsOutputRedirected)
            {
                var standardOutput = new StreamWriter(Console.OpenStandardOutput());
                Console.SetOut(standardOutput);
            }
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
            Console.SetIn(new StringReader(singleBindingFilePath));
            string results = controller.RequestPath(true);
            Assert.Equal(singleBindingFilePath, results);
        }

        [Fact]
        public void RequestNullPath()
        {
            StringReader nullReader = new StringReader("Pre-Null Text");
            nullReader.ReadLine();
            Console.SetIn(nullReader);
            string results = controller.RequestPath(true);
            Assert.Equal("", results);
        }

        [Fact]
        public void SetValidFilePathFromInput()
        {
            Console.SetIn(new StringReader(singleBindingFilePath));
            controller.SetFilePathFromInput();
            string results = controller.GetPath();
            Assert.Equal(singleBindingFilePath, results);
        }

        [Fact]
        public void SetInvalidThenValidFilePathFromInput()
        {
            string[] responsesArray = [invalidFilePath, singleBindingFilePath];
            string responsesString = String.Join(Environment.NewLine, responsesArray);
            StringReader sr = new StringReader(responsesString);
            Console.SetIn(sr);
            controller.SetFilePathFromInput();
            string results = controller.GetPath();
            Assert.Equal(singleBindingFilePath, results);
        }

        //[Fact]
        //public void ConfirmProcessFile()
        //{

        //}

        [Fact]
        public void GiveGeneralHelp()
        {
            Dictionary<string, string> justHelpArgDict = new Dictionary<string, string> { { "help", "True" } };

            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);
                controller.GiveHelp(justHelpArgDict);
                Assert.Equal(HelpStringData.generalHelpString, sw.ToString());
            }

        }

        [Fact]
        public void GiveHelpForArguments()
        {
            Dictionary<string, string> helpArgsDict = new Dictionary<string, string> { { "help", "True" }, {"path", "True"} };

            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);
                controller.GiveHelp(helpArgsDict);
                Assert.Equal(HelpStringData.GenerateArgumentHelpString("p"), sw.ToString());
            }
        }

        [Fact]
        public void GiveHelpForTooManyArguments()
        {
            string expected = HelpStringData.multipleArgumentsExplanationString + HelpStringData.generalHelpString;

            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);
                controller.GiveHelp(argsDict);
                
                Assert.Equal(expected, sw.ToString());
            }
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

            using (StringWriter sw = new StringWriter())
            {
                _testOutputHelper.WriteLine(sw.ToString());
                Console.SetOut(sw);
                BindingsFileData data = controller.ProcessFileAtPath(singleBindingFilePath);
                controller.DisplayPassedBindings(data);
                Assert.Equal(expected, sw.ToString());
            }
        }

        [Fact]
        public void DisplayPassedBindingsTestMultiBindingFile()
        {
            string expected = $"Method Name:{Environment.NewLine}\tpublic void FirstBinding(){Environment.NewLine}Associated Bindings:{Environment.NewLine}\t[Given(@\"there is a first binding\")]{Environment.NewLine}Method Name:{Environment.NewLine}\tpublic bool SingleInputBinding(string input){Environment.NewLine}Associated Bindings:{Environment.NewLine}\t[When(@\"there is an input of '(.*)'\")]{Environment.NewLine}Method Name:{Environment.NewLine}\tpublic void MultiInputBinding(string stringInput, char charInput, int intInput){Environment.NewLine}Associated Bindings:{Environment.NewLine}\t[Then(@\"there are multiple inputs of '(*.)', '(a|b|c)', '(dddd)'\")]{Environment.NewLine}\t[When(@\"there are inputs of '(*.)', '(a|b|c)', '(dddd)'\")]{Environment.NewLine}";

            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);
                BindingsFileData data = controller.ProcessFileAtPath(multiBindingFilePath);
                controller.DisplayPassedBindings(data);
                Assert.Equal(expected, sw.ToString());
            }
        }

        [Fact]
        public void DisplayExtractedBindingsTestMultiBindingFile()
        {
            string expected = $"The following methods and associated bindings were extracted:{Environment.NewLine}Method Name:{Environment.NewLine}\tpublic void FirstBinding(){Environment.NewLine}Associated Bindings:{Environment.NewLine}\t[Given(@\"there is a first binding\")]{Environment.NewLine}Method Name:{Environment.NewLine}\tpublic bool SingleInputBinding(string input){Environment.NewLine}Associated Bindings:{Environment.NewLine}\t[When(@\"there is an input of '(.*)'\")]{Environment.NewLine}Method Name:{Environment.NewLine}\tpublic void MultiInputBinding(string stringInput, char charInput, int intInput){Environment.NewLine}Associated Bindings:{Environment.NewLine}\t[Then(@\"there are multiple inputs of '(*.)', '(a|b|c)', '(dddd)'\")]{Environment.NewLine}\t[When(@\"there are inputs of '(*.)', '(a|b|c)', '(dddd)'\")]{Environment.NewLine}";

            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);
                BindingsFileData data = controller.ProcessFileAtPath(multiBindingFilePath);
                controller.DisplayExtractedBindings();
                Assert.Equal(expected, sw.ToString());
            }
        }

        [Fact]
        public void VerifyTargetFile()
        {
            controller.VerifyTarget(singleBindingFilePath);
        }

        [Fact]
        public void VerifyTargetFolder()
        {
            controller.VerifyTarget(resourcesFolder);
        }

    }
}