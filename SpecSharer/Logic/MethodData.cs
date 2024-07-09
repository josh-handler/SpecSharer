using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecSharer.Logic
{
    public class MethodData
    {
        string name = "";

        string body = "";

        string modifiers = "";

        List<string> parameters = [];

        List<string> bindings = [];

        public List<string> Parameters { get => parameters; set => parameters = value; }
        public string Modifiers { get => modifiers; set => modifiers = value; }
        public string Body { get => body; set => body = value; }
        public string Name { get => name; set => name = value; }
        public List<string> Bindings { get => bindings; set => bindings = value; }

        public string getMethodLine()
        {

            string parametersString = "";

            foreach (string param in parameters)
            {
                parametersString += param + ", ";
            }

            if (parametersString.Length != 0)
            {
                parametersString = parametersString.Remove(parametersString.Length - 2);
            }

            string line = $"{modifiers}{name}({parametersString})";
            return line;
        }
    }
}
