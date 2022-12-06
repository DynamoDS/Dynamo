using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

namespace DynamoTests.Elements
{
    public class ImportLibraryElement : ElementBase
    {
        public ImportLibraryElement(WindowsElement uiElement)
            : base(uiElement)
        { }

        public WindowsElement ImportLibrary { get { return UiElemnt; } }

        public AppiumWebElement FileInput { get; set; }
        public AppiumWebElement OpenButton { get; set; }
        public AppiumWebElement CancelButton { get; set; }

    }
}
