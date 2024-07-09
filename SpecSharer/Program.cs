// See https://aka.ms/new-console-template for more information
using SpecSharer.CommandLineInterface;

Console.WriteLine("Welcome To SpecSharer!");

CommandLineInterfaceController controller = new CommandLineInterfaceController();

CommandReader commandReader = new();

Dictionary<string, string> argDict = new();

foreach (var arg in args)
{
    if (arg.Contains(':'))
    {
        string[] pair = arg.Split(':');
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
        $"{String.Join(' ', invalidKeys)}");
}

commandReader.Interpret(argDict);

foreach (string key in argDict.Keys)
{
    Console.WriteLine($"{key}: {argDict[key]}");
}
Console.ReadLine();
