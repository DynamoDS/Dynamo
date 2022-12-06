using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

namespace DynamoTests.Elements
{
    public class MultiplyOperatorNode : NodeBase
    {
        public MultiplyOperatorNode(WindowsElement uiElemnt) : base(uiElemnt)
        {

        }

        public AppiumWebElement XInPut { get; set; }
        public AppiumWebElement YInPut { get; set; }
        public AppiumWebElement OutPut { get; set; }
    }
}
