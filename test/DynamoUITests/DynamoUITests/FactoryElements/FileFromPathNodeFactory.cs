using DynamoTests.Elements;
using DynamoTests.Utils;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using System.Collections.Generic;
using System.Linq;
using static DynamoTests.Utils.Enums;

namespace DynamoTests.FactoryElements
{
    public class FileFromPathNodeFactory : NodeFactoryBase<FileFromPathNode>
    {
        private List<AppiumWebElement> nodeElements;

        public FileFromPathNodeFactory(WindowsDriver<WindowsElement> dynamoSession)
           : base(dynamoSession)
        {
        }

        public override FileFromPathNode Build()
        {
            SetNodeList();
            var result = (FileFromPathNode)BuildBase();

            FindElementsIn(ref nodeElements, result.Node, "PortNameTextBox", FindElementBy.AccessibilityId);

            result.InPut = nodeElements.Where(x => x.Rect.X == result.Node.Rect.X).FirstOrDefault();
            result.OutPut = nodeElements.Where(x => x.Rect.X != result.Node.Rect.X).FirstOrDefault();

            return result;
        }
    }
}
