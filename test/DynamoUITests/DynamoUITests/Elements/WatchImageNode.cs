using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

namespace DynamoTests.Elements
{
    public class WatchImageNode : NodeBase
    {
        public WatchImageNode(WindowsElement uiElemnt)
           : base(uiElemnt)
        { }

        public AppiumWebElement OutPut { get; set; }
        public AppiumWebElement InPut { get; set; }
        public AppiumWebElement Image { get; set; }

    }
}
