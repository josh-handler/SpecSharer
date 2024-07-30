using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
namespace SpecSharerTests.Resources
{
    public class MultipleBindingFile
    {

        [Given(@"there is the first binding")]
        public void AFirstBinding()
        {
            Console.Write("Text Here");
        }

        [Given(@"there is an thing of '(.*)'")]

        public int SingleInputBinding(int teger)
        {
            //Comment
            Console.WriteLine($"Binding has input of {teger}");
            return int + 1;
        }

        [Given(@"there are multiple inputs of '(*.)', '(a|b|c)', '(dddd)'")]

        [When(@"there are ways of '(*.)', '(a|b|c)', '(dddd)'")]
        public void MultiInputBinding(string stringInput, char charInput, int intInput)
        {
            //Another Comment
            Console.WriteLine($"Inputs were string {stringInput}, char {charInput} and int {intInput}");
        }


    }
}
