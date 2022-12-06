using DynamoTests.Elements;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using System;
using static DynamoTests.Utils.Enums;

namespace DynamoTests.FactoryElements
{
    public class SaveAsElementFactory : FactoryBase<SaveAsElement>
    {
        private AppiumWebElement fileInput;
        private AppiumWebElement saveButton;
        private AppiumWebElement cancelButton;

        public SaveAsElementFactory(WindowsDriver<WindowsElement> dynamoSession)
            : base(dynamoSession)
        { }
        public override SaveAsElement Build()
        {
            FindElement(ref elementToFind, "Save As", FindElementBy.Name);
            var result = new SaveAsElement(elementToFind);

            FindElementIn(ref fileInput, elementToFind, "File name:", FindElementBy.Name);
            result.FileInput = fileInput;

            FindElementIn(ref saveButton, elementToFind, "Save", FindElementBy.Name);
            result.SaveButton = saveButton;

            FindElementIn(ref cancelButton, elementToFind, "Cancel", FindElementBy.Name);
            result.CancelButton = cancelButton;

            return result;
        }

    }
}
