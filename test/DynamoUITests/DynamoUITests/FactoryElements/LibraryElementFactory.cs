using DynamoTests.Elements;
using OpenQA.Selenium.Appium.Windows;
using static DynamoTests.Utils.Enums;

namespace DynamoTests.FactoryElements
{
    public class LibraryElementFactory : FactoryBase<LibraryElement>
    {
        public LibraryElementFactory(WindowsDriver<WindowsElement> dynamoSession)
            : base (dynamoSession)
        { }

        public override LibraryElement Build()
        {
            FindElement(ref elementToFind, "LibraryView", FindElementBy.ClassName);
            return new LibraryElement(elementToFind);
        }
    }
}
