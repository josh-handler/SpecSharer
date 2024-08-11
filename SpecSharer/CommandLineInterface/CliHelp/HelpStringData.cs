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
            $"SpecSharer Version {versionNumber}{Environment.NewLine}" +
            $"Usage: SpecSharer [arguments]{Environment.NewLine}" +
            $"{Environment.NewLine}" +
            $"Extracts all methods and maps bindings to them from a C# Specflows bindings file.{Environment.NewLine}" +
            $"User then decides which bindings to keep, may add tags and stores/shares them.{Environment.NewLine}" +
            $"{Environment.NewLine}" +
            $"arguments:{Environment.NewLine}" +
            $" h|help         Show command line help. Use alone or with one other command to get help for that command.{Environment.NewLine}" +
            $" p|path         The path to the file or files that you want extracted. If local this should be a full path to a single file. If on Github this should be a relative path to either a file or directory.{Environment.NewLine}" +
            $"t|target         The path to the file where extracted bindings will be stored. If the file does not exist one will be created. If local this should be a full path to a single file. If on Github this should be a relative path to a file. If this is not included a summary of results will be printed to console but nothing will be stored.{Environment.NewLine}" +
            $"g|github         Command to store in or retrieve from with Github. Do not include if a local operation is desired.{Environment.NewLine}" +
            $"r|retrieve         Command to retrieve from Github instead of storing. Does nothing if g|github is not set.{Environment.NewLine}" +
            $"{Environment.NewLine}" +
            $"Run 'SpecSharer [argument] --help' for more information on a command.{Environment.NewLine}" +
            $"{Environment.NewLine}";


        public static readonly string multipleArgumentsExplanationString = $"To get help for a specific command run 'SpecSharer [argument] --help' with only one non-help argument.{Environment.NewLine}{Environment.NewLine}";

        private static readonly Dictionary<string, ArgumentHelpData> argumentHelpData = new()
            {
                { "p", new ArgumentHelpData(
                    "p",
                    "path",
                    "The path to the file or files that you want extracted. If local this should be a full path to a single file. If on Github this should be a relative path to either a file or directory",
                    "SpecSharer p:[path string] [other arguments]")},
                {"g", new ArgumentHelpData(
                    "g",
                    "github",
                    $"Command to store in or retrieve from with Github. Do not include if a local operation is desired.",
                    "SpecSharer g [other arguments]") },
                {"t", new ArgumentHelpData(
                    "t",
                    "taret",
                    "The path to the file where extracted bindings will be stored. If the file does not exist one will be created. If local this should be a full path to a single file. If on Github this should be a relative path to a file. If this is not included a summary of results will be printed to console but nothing will be stored.",
                    "SpecSharer t:[target path string] [other arguments]") },
                {"r", new ArgumentHelpData(
                    "r",
                    "retrieve",
                    " Command to retrieve from Github instead of storing. Does nothing if g|github is not set.",
                    "SpecSharer r [other arguments]") }
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
