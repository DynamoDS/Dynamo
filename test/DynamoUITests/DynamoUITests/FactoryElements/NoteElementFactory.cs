using DynamoTests.Elements;
using OpenQA.Selenium.Appium.Windows;
using System.Linq;
using static DynamoTests.Utils.Enums;

namespace DynamoTests.FactoryElements
{
    public class NoteElementFactory : FactoryBase<NoteElement>
    {
        public NoteElementFactory(WindowsDriver<WindowsElement> dynamoSession)
            : base(dynamoSession)
        { }

        public override NoteElement Build()
        {
            FindElements(ref elementsToFind, "NoteView", FindElementBy.ClassName);
            elementToFind = elementsToFind.LastOrDefault();
            return new NoteElement(elementToFind);
        }
    }
}
