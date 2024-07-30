using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.CodeAnalysis.CSharp.SyntaxTokenParser;

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

        public string GetMethodLine(string method)
        {

            string parametersString = "";

            foreach (string param in parameters[method])
            {
                parametersString += param + ", ";
            }

            if (parametersString.Length != 0)
            {
                parametersString = parametersString.Remove(parametersString.Length - 2);
            }

            string line = $"{modifiers[method]}{method}({parametersString})";
            return line;
        }

        public List<MethodData> ProduceAllMethodData()
        {
            List<MethodData> result = new List<MethodData>();
            string method = "";

            for (int i = 0; i < methods.Count; i++)
            {
                method = methods[i];
                result.Add(new MethodData());
                result[i].Name = method;
                result[i].Body = bodies[method];
                result[i].Modifiers = modifiers[method];
                result[i].Parameters = parameters[method];
                result[i].Bindings = bindings[this.GetMethodLine(method)];
            }

            return result;
        }

        public MethodData ProduceSpecificMethodData(string methodName)
        {
            MethodData result = new MethodData();

            if (!Methods.Contains(methodName))
            {
                throw new KeyNotFoundException($"The Bindings File Data does not include a {methodName} method");
            }

            result.Name = methodName;
            result.Body = bodies[methodName];
            result.Modifiers = modifiers[methodName];
            result.Parameters = parameters[methodName];
            result.Bindings = bindings[GetMethodLine(methodName)];

            return result;
        }

        public string ConvertToString()
        {
            StringBuilder sb = new StringBuilder();
            List<MethodData> methodDataList = this.ProduceAllMethodData();

            if(methodDataList.Count == 0)
            {
                return "";
            }

            foreach (MethodData method in methodDataList)
            {
                sb.AppendLine();
                sb.AppendLine();
                foreach (string bindings in method.Bindings)
                {
                    sb.AppendLine(bindings);
                }

                sb.AppendLine($"\t{method.getMethodLine()}");
                sb.Append($"\t{method.Body}");
            }

            sb.Remove(0, Environment.NewLine.Length * 2);

            return sb.ToString();
        }

        public void UpdateMethodAndBindings(string methodName, BindingsFileData newerBindingsFileData)
        {
            if (!Methods.Contains(methodName))
            {
                throw new KeyNotFoundException($"The Bindings File Data does not include a {methodName} method");
            }

            MethodData updateMethodData = newerBindingsFileData.ProduceSpecificMethodData(methodName);

            Bodies[methodName] = updateMethodData.Body;
            Modifiers[methodName] = updateMethodData.Modifiers;
            Parameters[methodName] = updateMethodData.Parameters;
            Bindings[this.GetMethodLine(methodName)] = updateMethodData.Bindings;
        }

        public void UpdateAllUpdatedMethodsAndBindings(BindingsFileData newerBindingsFileData)
        {
            foreach(string methodName in newerBindingsFileData.Methods)
            {
                if(!Methods.Contains(methodName))
                {
                    continue;
                }

                UpdateMethodAndBindings(methodName, newerBindingsFileData);
            }
        }

        public void RemoveSharedData(BindingsFileData otherData)
        {
            foreach (string methodName in otherData.Methods)
            {
                RemoveSpecificMethodData(methodName);
            }
        }

        public void RemoveSpecificMethodData(string methodName)
        {
            Bindings.Remove(GetMethodLine(methodName));
            Bodies.Remove(methodName);
            Modifiers.Remove(methodName);
            Parameters.Remove(methodName);
            Methods.Remove(methodName);
        }
    }
}
