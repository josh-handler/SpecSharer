using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.RegularExpressions;

namespace SpecSharer.Logic
{
    public class MethodReader
    {

        private string filePath = "";

        private string bindingRegex = "\\[(Given|When|Then)(\\(@\".*\"\\))\\]";

        public MethodReader() { }


        public string GetFilePath() { return filePath; }

        public bool SetFilePath(string path)
        {
            filePath = path;
            return File.Exists(filePath);
        }

        public BindingsFileData ProcessBindingsFile()
        {
            BindingsFileData data = new BindingsFileData();

            string csFileContent = File.ReadAllText(filePath);

            Dictionary<string, string> bodies;
            Dictionary<string, string> modifiers;
            Dictionary<string, List<string>> parameters;

            ExtractMethodsAndBodies(csFileContent, out bodies, out modifiers, out parameters);
            data.Methods = bodies.Keys.ToList();
            data.Bodies = bodies;
            data.Modifiers = modifiers;
            data.Parameters = parameters;

            data.Bindings = MapMethodsToBindings(csFileContent);

            return data;
        }

        private Dictionary<string, List<string>> MapMethodsToBindings(string csFileContent)
        {
            Dictionary<string, List<string>> mappedMethodsAndBindings = new Dictionary<string, List<string>>();
            List<string> bindingsList = new List<string>();
            string currentLine = "";
            bool readyForMethod = false;
            //Chose the 'using' syntax to ensure the stream reader is disposed of promptly rather than waiting for the garbage collector
            using (StringReader sr = new StringReader(csFileContent))
            {
                while (sr.Peek() != -1)
                {
                    currentLine = getNextLineWithContent(sr);
                    if (Regex.IsMatch(currentLine, bindingRegex))
                    {
                        bindingsList.Add(currentLine);
                        readyForMethod = true;
                        
                    }else if (readyForMethod)
                    {
                        mappedMethodsAndBindings.Add(currentLine, bindingsList);
                        readyForMethod = false;
                        bindingsList = new List<string>();
                    }
                }
            }
            return mappedMethodsAndBindings;
        }

        private string getNextLineWithContent(StringReader sr)
        {
            string contentLine = "";
            string? untrimmedLine;
            while (sr.Peek() != -1 && contentLine.Length == 0)
            {
                untrimmedLine = sr.ReadLine();
                if(untrimmedLine != null)
                {
                    contentLine = untrimmedLine.Trim();

                }
            }
            return contentLine;
        }

        private void ExtractMethodsAndBodies(string csFileContent, out Dictionary<string, string> bodies,
            out Dictionary<string, string> modifiers, out Dictionary<string, List<string>> parameters)
        {
            bodies = new Dictionary<string, string>();
            modifiers = new Dictionary<string, string>();
            parameters = new Dictionary<string, List<string>>();

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
                    string methodBody = "";

                    //Method body (including curly braces)
                    if (mds.Body != null)
                    {
                        methodBody = mds.Body.ToString();
                    }

                    bodies.Add(methodName, methodBody);

                    modifiers.Add(methodName, ExtractModifiers(mds));

                    parameters.Add(methodName, ExtractParameters(mds));
                }
            }
        }

        private string ExtractModifiers(MethodDeclarationSyntax mds)
        {
            string methodModifiers = "";
            foreach (SyntaxToken mod in mds.Modifiers.ToArray())
            {
                methodModifiers = methodModifiers + mod.ValueText + " ";
            }
            methodModifiers = methodModifiers + mds.ReturnType.GetText().ToString();
            return methodModifiers;
        }

        private List<string> ExtractParameters(MethodDeclarationSyntax mds)
        {
            List<string> parameters = new List<string>();
            string parameterType = "";

            foreach (ParameterSyntax ps in mds.ParameterList.Parameters)
            {
                string parameterName = ps.Identifier.Text;
                if (ps.Type != null)
                {
                    parameterType = ps.Type.GetText().ToString();
                }

                parameters.Add($"{parameterType}{parameterName}");
            }
            return parameters;
        }
    }
}
