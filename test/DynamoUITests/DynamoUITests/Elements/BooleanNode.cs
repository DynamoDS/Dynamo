using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

namespace DynamoTests.Elements
{
    public class BooleanNode : NodeBase
    {
        public BooleanNode(WindowsElement uiElemnt)
            : base(uiElemnt)
        { }
        public AppiumWebElement RbtTrue { get; set; }
        public AppiumWebElement RbtFalse { get; set; }
        public AppiumWebElement OutPt { get; set; }
    }
}
