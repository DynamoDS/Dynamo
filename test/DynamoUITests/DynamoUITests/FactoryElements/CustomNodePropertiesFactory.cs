using DynamoTests.Elements;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using System.Linq;
using static DynamoTests.Utils.Enums;

namespace DynamoTests.FactoryElements
{
    public class CustomNodePropertiesFactory : FactoryBase<CustomNodePropertiesElement>
    {
        private AppiumWebElement nameInput;
        private AppiumWebElement descriptionInput;
        private AppiumWebElement categoryBox;
        private AppiumWebElement okButton;
        private AppiumWebElement cancelButton;

        public CustomNodePropertiesFactory(WindowsDriver<WindowsElement> dynamoSession)
            : base(dynamoSession)
        { }

        public override CustomNodePropertiesElement Build()
        {
            FindElements(ref elementsToFind, "Window", FindElementBy.ClassName);
            elementToFind = elementsToFind.FirstOrDefault(e => e.Text == "Custom Node Properties");

            var result = new CustomNodePropertiesElement(elementToFind);

            FindElementIn(ref nameInput, elementToFind, "nameBox", FindElementBy.AccessibilityId);
            result.NameInput = nameInput;

            FindElementIn(ref descriptionInput, elementToFind, "DescriptionInput", FindElementBy.AccessibilityId);
            result.DescriptionInput = descriptionInput;

            FindElementIn(ref categoryBox, elementToFind, "categoryBox", FindElementBy.AccessibilityId);
            result.CategoryBox = categoryBox;

            FindElementIn(ref okButton, elementToFind, "okButton", FindElementBy.AccessibilityId);
            result.OkButton = okButton;

            FindElementIn(ref cancelButton, elementToFind, "cancelButton", FindElementBy.AccessibilityId);
            result.CancelButton = cancelButton;

            return result;
        }
    }
}
