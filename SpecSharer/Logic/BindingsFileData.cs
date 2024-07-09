using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecSharer.Logic
{
    public class BindingsFileData
    {

        List<string> methods = [];

        Dictionary<string, string> bodies = [];

        Dictionary<string, string> modifiers = [];

        Dictionary<string, List<string>> parameters = [];

        Dictionary<string, List<string>> bindings = [];


        public BindingsFileData() { }

        public Dictionary<string, List<string>> Parameters { get => parameters; set => parameters = value; }
        public Dictionary<string, string> Modifiers { get => modifiers; set => modifiers = value; }
        public Dictionary<string, string> Bodies { get => bodies; set => bodies = value; }
        public List<string> Methods { get => methods; set => methods = value; }
        public Dictionary<string, List<string>> Bindings { get => bindings; set => bindings = value; }

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

        public List<MethodData> produceMethodData()
        {
            List<MethodData> result = new List<MethodData>();
            string method = "";

            for(int i = 0; i < methods.Count; i++)
            {
                method = methods[i];
                result.Add(new MethodData());
                result[i].Name = method;
                result[i].Body = bodies[method];
                result[i].Modifiers = modifiers[method];
                result[i].Parameters = parameters[method];
                result[i].Bindings = bindings[this.getMethodLine(method)];
            }

            return result;
        }
    }
}
