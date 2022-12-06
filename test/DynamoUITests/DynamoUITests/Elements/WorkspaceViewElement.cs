using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

namespace DynamoTests.Elements
{
    public class WorkspaceViewElement : ElementBase
    {
        public WorkspaceViewElement(WindowsElement uiElement)
            : base(uiElement)
        { }

        public WindowsElement WorkspaceView { get { return UiElemnt; } }

        public AppiumWebElement ZoomInButton;
        public AppiumWebElement ZoomOutButton;
        public AppiumWebElement BackgroudViewCheck;
        public AppiumWebElement GraphViewCheck;
    }
}
