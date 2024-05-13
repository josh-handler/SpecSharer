using EnvDTE;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SpecSharer.Logic
{
    //Create a method to find [Given(@"there is a binding")]
    public class MethodReader
    {

        private string filePath = "";

        private string bindingRegex = "\\[(Given|When|Then)(\\(@\".*\"\\))\\]";

        public string GetFilePath() { return filePath; }

        public bool SetFilePath(string path)
        {
            filePath = path;
            return File.Exists(filePath);
        }

    
        public Dictionary<string, string> MapMethodsToBindings(string path)
        {
            Dictionary<string, string> mappedMethodsAndBindings = new Dictionary<string, string>();
            string currentLine = "";
            //Chose the 'using' syntax to ensure the stream reader is disposed of promptly rather than waiting for the garbage collector
            using (StreamReader sr = new StreamReader(filePath)) 
            { 
                while (!sr.EndOfStream)
                {
                    currentLine = sr.ReadLine();
                    if (Regex.IsMatch(currentLine, bindingRegex))
                    {
                        currentLine = currentLine.Trim();
                        mappedMethodsAndBindings.Add(currentLine, getNextLineWithContent(sr));
                    }
                }
            }
            return mappedMethodsAndBindings;
        }

        private string getNextLineWithContent(StreamReader sr)
        {
            string contentLine = sr.ReadLine().Trim();

            while (contentLine.Length == 0)
            {
                contentLine = sr.ReadLine().Trim();
            }

            return contentLine;
        }

        public Dictionary<string, string> ExtractMethodsAndBodies()
        {
            Dictionary<string, string> output = new Dictionary<string, string>();

            string csFileContent = File.ReadAllText(filePath);
            SyntaxTree tree = CSharpSyntaxTree.ParseText(csFileContent);
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();
            NamespaceDeclarationSyntax nds = (NamespaceDeclarationSyntax)root.Members[0];
            ClassDeclarationSyntax cds = (ClassDeclarationSyntax)nds.Members[0];
            foreach (MemberDeclarationSyntax ds in cds.Members)
            {

                //Operate on methods only, ignore other syntax 
                if (ds is MethodDeclarationSyntax)
                {
                    MethodDeclarationSyntax mds = (MethodDeclarationSyntax)ds;
                    
                    //Method name
                    string methodName = ((SyntaxToken)mds.Identifier).ValueText;

                    //Method body (including curly braces)
                    string methodBody = mds.Body.ToString();

                    output.Add(methodName, methodBody);
                }
            }

            return output;
        }
    }
}

//string methodModifiers = "";

//foreach (SyntaxToken mod in mds.Modifiers.ToArray())
//{
//    methodModifiers = methodModifiers + mod.ValueText + " ";
//}
//methodModifiers = methodModifiers + mds.ReturnType.GetText().ToString();
//methodName = methodModifiers + methodName;

//var parameters = new List<string>();

//foreach (var n in mds.ParameterList.Parameters)
//{
//    var parameterName = n.Identifier.Text;
//    parameters.Add(parameterName);
//}