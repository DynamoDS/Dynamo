using DynamoTests.Elements;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using System;
using static DynamoTests.Utils.Enums;

namespace DynamoTests.FactoryElements
{
    public class PackageSearchElementFactory : FactoryBase<PackageSearchElement>
    {
        private AppiumWebElement searchTextField;
        private AppiumWebElement searchEditField;

        public PackageSearchElementFactory(WindowsDriver<WindowsElement> dynamoSession)
            : base(dynamoSession)
        { }

        public override PackageSearchElement Build()
        {
            FindElement(ref elementToFind, "Online Package Search", FindElementBy.Name);

            var result = new PackageSearchElement(elementToFind);

            FindElementIn(ref searchTextField, elementToFind, "Please wait...", FindElementBy.Name);
            result.SearchTextField = searchTextField;

            FindElementIn(ref searchEditField, elementToFind, "TextBlock", FindElementBy.ClassName);
            result.SearchEditField = searchEditField;

            return result;
        }
    }
}
