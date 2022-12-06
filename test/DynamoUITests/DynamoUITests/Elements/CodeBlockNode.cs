using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using System.Collections.Generic;

namespace DynamoTests.Elements
{
    public class CodeBlockNode : NodeBase
    {
        public CodeBlockNode(WindowsElement uiElemnt)
           : base(uiElemnt)
        { }

        public AppiumWebElement CodeBlockEditor { get; set; }
        public List<AppiumWebElement> OutPuts { get; set; }    
        /// <summary>
        /// Empty list if does not count with entry elements
        /// </summary>
        public List<AppiumWebElement> InPutElements { get; set; }
        
    }
}
