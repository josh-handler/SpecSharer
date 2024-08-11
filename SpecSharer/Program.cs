// See https://aka.ms/new-console-template for more information
using SpecSharer.CommandLineInterface;
using SpecSharer.Data;
using SpecSharer.Logic;
using System;
Console.WriteLine("Welcome To SpecSharer!");

CommandReader commandReader = new(new GithubManager(), new ConsoleWrapper(), new MethodReader());

Dictionary<string, string> argDict = [];

if (args.Length == 0)
{
    throw new ArgumentException($"No arguments were provided. Please provide at least one argument. Use help or h to get help.");
}

//if (args.Length == 1 && args[0] == "SpecSharer")
//{ 
//    throw new ArgumentException($"No arguments were provided. Please provide at least one argument. Use help or h to get help.");
//}

foreach (var arg in args)
{
    if (arg.Contains(':'))
    {
        string[] pair = arg.Split(':', 2);
        argDict[pair[0]] = pair[1];
    }
    else 
    {
        argDict[arg] = true.ToString();
    }
}

List<string> invalidKeys = commandReader.ValidateArgs(argDict.Keys);

if(invalidKeys.Count > 0)
{
    throw new ArgumentException($"The following invalid arguments were received:{Environment.NewLine}" +
        $"{string.Join(' ', invalidKeys)}");
}

await commandReader.Interpret(argDict);

//Console.ReadLine();
