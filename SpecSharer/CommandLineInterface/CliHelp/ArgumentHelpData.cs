using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecSharer.CommandLineInterface.CliHelp
{
    internal class ArgumentHelpData
    {
        private string argumentShort;
        private string argumentLong;
        private string description;
        private string usage;
        private Dictionary<string, string> argumentDescriptionsDict;

        public ArgumentHelpData(string argumentShort, string argumentLong, string description, string usage, Dictionary<string, string> argumentDescriptionsDict)
        {
            this.argumentShort = argumentShort;
            this.argumentLong = argumentLong;
            this.description = description;
            this.usage = usage;
            this.argumentDescriptionsDict = argumentDescriptionsDict;
        }

        public ArgumentHelpData(string argumentShort, string argumentLong, string description, string usage)
        {
            this.argumentShort = argumentShort;
            this.argumentLong = argumentLong;
            this.description = description;
            this.usage = usage;
            this.argumentDescriptionsDict = new Dictionary<string, string>();
        }

        public string ArgumentShort { get => argumentShort; set => argumentShort = value; }
        public string ArgumentLong { get => argumentLong; set => argumentLong = value; }
        public string Description { get => description; set => description = value; }
        public string Usage { get => usage; set => usage = value; }
        public Dictionary<string, string> ArgumentDescriptionsDict { get => argumentDescriptionsDict; set => argumentDescriptionsDict = value; }

    }
}