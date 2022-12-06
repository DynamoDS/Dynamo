using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

namespace DynamoTests.Elements
{
    public class NodeBase : ElementBase
    {
        public NodeBase(WindowsElement uiElement)
            : base(uiElement)
        { }

        public WindowsElement Node { get { return UiElemnt; } }
        public AppiumWebElement NameBlock { get; set; }

        //Null if the node does not have preview
        public AppiumWebElement PreviewControl { get; set; }
    }
}

