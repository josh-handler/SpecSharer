using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecSharer.CommandLineInterface
{
    public interface IConsole
    {
        public string? ReadLine();
        public void Write(string value);
        public void WriteLine(string value);
    }
}
