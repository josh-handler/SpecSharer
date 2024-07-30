using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecSharer.CommandLineInterface
{
    public class ConsoleWrapper : IConsole
    {
        public void Write(string value)
        {
            Console.Write(value);
        }

        public void WriteLine(string value)
        {
            Console.WriteLine(value);
        }

        public string? ReadLine()
        {
            return Console.ReadLine();
        }
    }
}
