using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

namespace DynamoTests.Elements
{
    public class InputNode : NodeBase
    {
        public InputNode(WindowsElement uiElemnt) : base(uiElemnt)
        {

        }

        public AppiumWebElement Output { get; set; }
        public AppiumWebElement ParameterEditor { get; set; }
    }
}
