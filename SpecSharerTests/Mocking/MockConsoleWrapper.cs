using SpecSharer.CommandLineInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecSharerTests.Mocking
{
    public class MockConsoleWrapper : IConsole
    {
        StringBuilder output = new StringBuilder();
        Queue<string> input = new Queue<string>();
        public MockConsoleWrapper() { }
        public string? ReadLine()
        {
            return input.Dequeue();
        }

        public void Write(string value)
        {
            output.Append(value);
        }

        public void WriteLine(string value)
        {
            output.AppendLine(value);
        }

        public string GetConsoleOutput()
        {
            return output.ToString();
        }

        public void AddInput(string? userInput)
        {
            input.Enqueue(userInput);
        }
    }
}
