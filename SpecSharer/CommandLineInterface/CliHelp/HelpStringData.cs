using EnvDTE80;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecSharer.CommandLineInterface.CliHelp
{
    internal static class HelpStringData
    {
        private static readonly string versionNumber = "ALPHA";
        public static readonly string generalHelpString =
            $"Specsharer Version {versionNumber}{Environment.NewLine}" +
            $"Usage: specsharer [arguments]{Environment.NewLine}" +
            $"{Environment.NewLine}" +
            $"Extracts all methods and maps bindings to them from a C# Specflows bindings file.{Environment.NewLine}" +
            $"User then decides which bindings to keep, may add tags and stores/shares them.{Environment.NewLine}" +
            $"{Environment.NewLine}" +
            $"arguments:{Environment.NewLine}" +
            $" -h|--help         Show command line help.{Environment.NewLine}" +
            $" -e|--extract      Confirm that extraction is wanted in advance.{Environment.NewLine}" +
            $" -p|--path         Set the path to target bindings file.{Environment.NewLine}" +
            $"{Environment.NewLine}" +
            $"Run 'specsharer [argument] --help' for more information on a command.{Environment.NewLine}" +
            $"{Environment.NewLine}";


        public static readonly string multipleArgumentsExplanationString = $"To get help for a specific command run 'specsharer [argument] --help' with only one non-help argument.{Environment.NewLine}{Environment.NewLine}";

        private static readonly Dictionary<string, ArgumentHelpData> argumentHelpData = new()
            {
               { "p", new ArgumentHelpData(
                    "p",
                    "path",
                    "Path of file which will have its methods and bindings extracted.",
                    "specsharer -p:[path string] [other arguments]")},
                {"e", new ArgumentHelpData(
                    "e",
                    "extract",
                    $"Command to extract and present methods and bindings from file indicated by path command or indicated later in the process.",
                    "specsharer -e [other arguments]") }
        };

        public static string GenerateArgumentHelpString(string argument)
        {

            ArgumentHelpData data = argumentHelpData[argument.First().ToString()];

            return $"Description:{Environment.NewLine}" +
                $"  {data.Description}{Environment.NewLine}" +
                $"{Environment.NewLine}" +
                $"Usage:{Environment.NewLine}" +
                $"  {data.Usage}{Environment.NewLine}" +
                $"{Environment.NewLine}";
        }
    }
}
