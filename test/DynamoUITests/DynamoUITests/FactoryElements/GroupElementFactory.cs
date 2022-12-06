using DynamoTests.Elements;
using OpenQA.Selenium.Appium.Windows;
using System.Linq;
using static DynamoTests.Utils.Enums;

namespace DynamoTests.FactoryElements
{
    public class GroupElementFactory : FactoryBase<GroupElement>
    {
        public GroupElementFactory(WindowsDriver<WindowsElement> dynamoSession)
            : base(dynamoSession)
        { }

        public override GroupElement Build()
        {
            FindElements(ref elementsToFind, "Group", FindElementBy.AccessibilityId);
            elementToFind = elementsToFind.LastOrDefault();
            return new GroupElement(elementToFind);
        }
    }
}
