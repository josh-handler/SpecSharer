using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
namespace SpecSharerTests.Resources
{
    public class SingleBindingFile
    {

        [Given(@"there is the binding")]
        internal void Binding(int input)
        {
            //An Example Comment
            Console.Write("Example binding");
        }
    }
}
