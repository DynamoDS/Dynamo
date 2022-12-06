using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

namespace DynamoTests.Elements
{
    public class FileFromPathNode : NodeBase
    {
        public FileFromPathNode(WindowsElement uiElemnt)
           : base(uiElemnt)
        { }

        public AppiumWebElement OutPut { get; set; }
        public AppiumWebElement InPut { get; set; }
    }
}
