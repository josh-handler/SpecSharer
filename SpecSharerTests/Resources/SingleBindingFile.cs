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

        [Given(@"there is a binding")]
        public void Binding(string input)
        {
            //Example Comment
            Console.WriteLine("Example binding");
        }
    }
}
