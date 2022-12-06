using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

namespace DynamoTests.Elements
{
    public class ShortcutToolbarElement : ElementBase
    {
        public ShortcutToolbarElement(WindowsElement uiElement) : base(uiElement)
        {

        }

        public WindowsElement ShortcutToolbar { get { return UiElemnt; } }

        public AppiumWebElement NewButton { get; set; }
        public AppiumWebElement OpenButton { get; set; }
        public AppiumWebElement SaveButton { get; set; }
        public AppiumWebElement UndoButton { get; set; }
        public AppiumWebElement RedoButton { get; set; }
    }
}
