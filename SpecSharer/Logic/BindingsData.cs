using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecSharer.Logic
{
    public class BindingsData
    {

        List<string> methods = [];

        Dictionary<string, string> bodies = [];

        Dictionary<string, string> modifiers = [];

        Dictionary<string, List<string>> parameters = [];

        Dictionary<string, string> bindings = [];


        public BindingsData() { }

        public Dictionary<string, List<string>> Parameters { get => parameters; set => parameters = value; }
        public Dictionary<string, string> Modifiers { get => modifiers; set => modifiers = value; }
        public Dictionary<string, string> Bodies { get => bodies; set => bodies = value; }
        public List<string> Methods { get => methods; set => methods = value; }
        public Dictionary<string, string> Bindings { get => bindings; set => bindings = value; }

        public string getMethodLine(string method) {

            string parametersString = "";

            foreach (string param in parameters[method])
            {
                parametersString += param + ", ";
            }

            if(parametersString.Length != 0)
            {
                parametersString = parametersString.Remove(parametersString.Length - 2);
            }

            string line = $"{modifiers[method]}{method}({parametersString})";
            return line;
        }

    }
}
