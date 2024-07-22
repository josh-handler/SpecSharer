// See https://aka.ms/new-console-template for more information
using SpecSharer.CommandLineInterface;

Console.WriteLine("Welcome To SpecSharer!");

CommandLineInterfaceController controller = new CommandLineInterfaceController();

CommandReader commandReader = new();

Dictionary<string, string> argDict = new();

if (args.Length == 0)
{
    throw new ArgumentException($"No arguments were provided. Please provide at least one argument. Use help or h to get help.");
}

//if (args.Length == 1 && args[0] == "SpecSharer")
//{ 
//    throw new ArgumentException($"No arguments were provided. Please provide at least one argument. Use help or h to get help.");
//}

string temp = "";

foreach (var arg in args)
{
    if (arg.Contains(':'))
    {
        string[] pair = arg.Split(':', 2);
        argDict[pair[0]] = pair[1];
        temp = pair[0];
    }
    else 
    {
        argDict[arg] = true.ToString();
        temp = arg;
    }
    Console.WriteLine($"{temp}-{argDict[temp]}");
}



List<string> invalidKeys = commandReader.ValidateArgs(argDict.Keys);

if(invalidKeys.Count > 0)
{
    throw new ArgumentException($"The following invalid arguments were received:{Environment.NewLine}" +
        $"{string.Join(' ', invalidKeys)}");
}

commandReader.Interpret(argDict);

//Console.ReadLine();
