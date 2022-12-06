using DynamoTests.Elements;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using System;
using static DynamoTests.Utils.Enums;

namespace DynamoTests.FactoryElements
{
    public class PackageManagerElementFactory : FactoryBase<PackageManagerElement>
    {
        public PackageManagerElementFactory(WindowsDriver<WindowsElement> dynamoSession)
            : base(dynamoSession)
        { }

        public override PackageManagerElement Build()
        {
            FindElement(ref elementToFind, "Installed Packages", FindElementBy.Name);
            var result = new PackageManagerElement(elementToFind);

            return result;
        }
    }
}
