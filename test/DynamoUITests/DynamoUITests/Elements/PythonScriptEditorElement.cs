using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

namespace DynamoTests.Elements
{
    public class PythonScriptEditorElement : ElementBase
    {
        public WindowsElement Editor { get { return UiElemnt; } }

        public AppiumWebElement SaveButton { get; set; }
        public AppiumWebElement TextArea { get; set; }
        public AppiumWebElement RevertButton { get; set; }

        public PythonScriptEditorElement(WindowsElement uiElement) : base(uiElement)
        {

        }
    }
}
