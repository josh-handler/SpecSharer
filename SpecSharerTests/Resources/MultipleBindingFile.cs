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

        [Given(@"there is a first binding")]
        public void FirstBinding()
        {
            //Example Comment
            Console.WriteLine("Example binding");
        }

        [When(@"there is an input of '(.*)'")]
        public void SingleInputBinding(string input)
        {
            //Comment
            Console.WriteLine($"Binding has input of {input}");
        }

        [Then(@"there are multiple inputs of '(*.)', '(a|b|c)', '(dddd)'")]
        public void MultiInputBinding(string stringInput, char charInput, int intInput)
        {
            //Another Comment
            Console.WriteLine($"Inputs were string {stringInput}, char {charInput} and int {intInput}");
        }


    }
}
