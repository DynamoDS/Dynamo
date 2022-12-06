using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

namespace DynamoTests.Elements
{
    public class SaveAsElement : ElementBase
    {
        public SaveAsElement(WindowsElement uiElement)
           : base(uiElement)
        { }

        public AppiumWebElement FileInput { get; set; }
        public AppiumWebElement SaveButton { get; set; }
        public AppiumWebElement CancelButton { get; set; }
    }
}
