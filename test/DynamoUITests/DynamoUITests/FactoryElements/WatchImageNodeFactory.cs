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
    public class WatchImageNodeFactory : NodeFactoryBase<WatchImageNode>
    {
        private List<AppiumWebElement> nodeElements;
        private AppiumWebElement image;

        public WatchImageNodeFactory(WindowsDriver<WindowsElement> dynamoSession)
          : base(dynamoSession)
        {
        }
        public override WatchImageNode Build()
        {
            SetNodeList();
            var result = (WatchImageNode)BuildBase();
            FindElementsIn(ref nodeElements, result.Node, "PortNameTextBox", FindElementBy.AccessibilityId);
            result.InPut = nodeElements.Where(x => x.Rect.X == result.Node.Rect.X).FirstOrDefault();
            result.OutPut = nodeElements.Where(x => x.Rect.X != result.Node.Rect.X).FirstOrDefault();
            FindElementIn(ref image, result.Node, "image1", FindElementBy.AccessibilityId);
            result.Image = image;
            return result;
        }
    }
}
