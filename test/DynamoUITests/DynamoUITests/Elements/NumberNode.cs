using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

namespace DynamoTests.Elements
{
    public class NumberNode : NodeBase
    {
        public NumberNode(WindowsElement uiElemnt)
            : base(uiElemnt)
        { }

        public AppiumWebElement NumberText { get; set; }
        public AppiumWebElement OutPut { get; set; }

    }
}