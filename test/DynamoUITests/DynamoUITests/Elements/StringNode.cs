using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

namespace DynamoTests.Elements
{
   public class StringNode : NodeBase
    {
        public StringNode(WindowsElement uiElemnt)
          : base(uiElemnt)
        { }

        public AppiumWebElement OutPut { get; set; }
        public AppiumWebElement TextBox { get; set; }

    }
}
