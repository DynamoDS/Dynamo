using DynamoTests.Elements;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using System.Collections.Generic;
using static DynamoTests.Utils.Enums;

namespace DynamoTests.FactoryElements
{
    public class ShortcutToolbarElementFactory : FactoryBase<ShortcutToolbarElement>
    {
        public ShortcutToolbarElementFactory(WindowsDriver<WindowsElement> dynamoSession) : base(dynamoSession)
        {

        }

        public override ShortcutToolbarElement Build()
        {
            FindElement(ref elementToFind, "ShortcutToolbar", FindElementBy.AccessibilityId);

            List<AppiumWebElement> toolbarButtons = null;
            FindElementsIn(ref toolbarButtons, elementToFind, "Button", FindElementBy.ClassName);

            return new ShortcutToolbarElement(elementToFind)
            {
                NewButton = toolbarButtons[0],
                OpenButton = toolbarButtons[1],
                SaveButton = toolbarButtons[2],
                UndoButton = toolbarButtons[3],
                RedoButton = toolbarButtons[4]
            };
        }
    }
}
