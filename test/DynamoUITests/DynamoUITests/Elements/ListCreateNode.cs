using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using System.Collections.Generic;

namespace DynamoTests.Elements
{
    public class ListCreateNode : NodeBase
    {
        public ListCreateNode(WindowsElement uiElemnt)
        : base(uiElemnt)
        { }

        public List<AppiumWebElement> itemsList { get; set; }
        public AppiumWebElement ButtonPlus { get; set; }
        public AppiumWebElement ButtonLess { get; set; }
        public AppiumWebElement OutPutList { get; set; }

    }
}
