using DynamoTests.Elements;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using System.Collections.Generic;
using System.Linq;
using static DynamoTests.Utils.Enums;

namespace DynamoTests.FactoryElements
{
    public class WatchNodeFactory : NodeFactoryBase<WatchNode>
    {
        private List<AppiumWebElement> nodeElements;

        public WatchNodeFactory(WindowsDriver<WindowsElement> dynamoSession)
            : base(dynamoSession)
        {

        }

        public WatchNodeFactory(WindowsDriver<WindowsElement> dynamoSession, IWebElement _elementFound)
            : base(dynamoSession)
        {
            elementFound = _elementFound;
        }

        public override WatchNode Build()
        {
            SetNodeList();
            var result = (WatchNode)BuildBase();

            FindElementsIn(ref nodeElements, result.Node, "PortNameTextBox", FindElementBy.AccessibilityId);

            //FindElementsIn get the ports in different order, then in order to know which are input and outputs we need to check the position over the X axis (horizontally)
            if (nodeElements.Count() == 2)
            {
                if(nodeElements[0].Rect.X < nodeElements[1].Rect.X)
                {
                    result.InPut = nodeElements[0];
                    result.OutPut = nodeElements[1];
                }
                else
                {
                    result.InPut = nodeElements[1];
                    result.OutPut = nodeElements[0];
                }
                
            }

            return result;
        }
    }
}
