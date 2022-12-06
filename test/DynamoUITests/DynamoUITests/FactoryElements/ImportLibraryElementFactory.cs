using DynamoTests.Elements;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using System;
using static DynamoTests.Utils.Enums;

namespace DynamoTests.FactoryElements
{
    public class ImportLibraryElementFactory : FactoryBase<ImportLibraryElement>
    {
        private AppiumWebElement fileInput;
        private AppiumWebElement openButton;
        private AppiumWebElement cancelButton;

        public ImportLibraryElementFactory(WindowsDriver<WindowsElement> dynamoSession)
            : base(dynamoSession)
        { }

        public override ImportLibraryElement Build()
        {
            FindElement(ref elementToFind, "Import Library", FindElementBy.Name);
            var result = new ImportLibraryElement(elementToFind);

            FindElementIn(ref fileInput, elementToFind, "File name:", FindElementBy.Name);
            result.FileInput = fileInput;

            FindElementIn(ref openButton, elementToFind, "Open", FindElementBy.Name);
            result.OpenButton = openButton;

            FindElementIn(ref cancelButton, elementToFind, "Cancel", FindElementBy.Name);
            result.CancelButton = cancelButton;

            return result;
        }
    }
}
