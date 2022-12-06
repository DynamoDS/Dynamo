using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using System.Collections.Generic;

namespace DynamoTests.Elements
{
    public class CustomNode : NodeBase
    {
        public CustomNode(WindowsElement uiElemnt)
            : base(uiElemnt)
        { }

        /// <summary>
        /// Empty list if does not count with entry elements
        /// </summary>
        public List<AppiumWebElement> InputElements { get; set; }
        public List<AppiumWebElement> OutputElements { get; set; }
        
    }
}
