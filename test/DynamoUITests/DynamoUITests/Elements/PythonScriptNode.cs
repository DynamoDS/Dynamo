using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

namespace DynamoTests.Elements
{
    public class PythonScriptNode: NodeBase
    {
        public AppiumWebElement Output { get; set; }
        public AppiumWebElement Input { get; set; }

        public PythonScriptNode(WindowsElement uiElemnt) : base(uiElemnt)
        {

        }
    }
}
