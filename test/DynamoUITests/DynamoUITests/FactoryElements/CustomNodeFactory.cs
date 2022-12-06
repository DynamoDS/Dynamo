using DynamoTests.Elements;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using System.Collections.Generic;
using System.Linq;
using static DynamoTests.Utils.Enums;

namespace DynamoTests.FactoryElements
{
    public class CustomNodeFactory : NodeFactoryBase<CustomNode>
    {
        private List<AppiumWebElement> customElements;

        public CustomNodeFactory(WindowsDriver<WindowsElement> dynamoSession)
            : base(dynamoSession)
        { }

        public CustomNodeFactory(WindowsDriver<WindowsElement> dynamoSession, IWebElement _elementFound)
            : base(dynamoSession)
        {
            elementFound = _elementFound;
        }

        public override CustomNode Build()
        {
            SetNodeList();
            var result = (CustomNode)BuildBase();

            FindElementsIn(ref customElements, result.Node, "PortNameTextBox", FindElementBy.AccessibilityId);

            List<AppiumWebElement> chevronElements = new List<AppiumWebElement>();
            FindElementsIn(ref chevronElements, result.Node, "Chevron", FindElementBy.AccessibilityId);

            var chevronElement = chevronElements.FirstOrDefault();
            //If Chevron symbol is found means that we have inputs (and by consequence outputs).
            if (chevronElement != null)
            {
                //Elements at the left side of the Chevron symbol are inputs and at the right side are outputs        
                result.InputElements = customElements.Where(x => x.Rect.X <= chevronElement.Rect.X).ToList();
                result.OutputElements = customElements.Where(x => x.Rect.X >= chevronElement.Rect.X).ToList();
            }
            else
            {
                result.InputElements = customElements.Where(x => x.Rect.X == result.Node.Rect.X).ToList();
                result.OutputElements = customElements.Where(x => x.Rect.X > result.Node.Rect.X).ToList();
            }

            return result;
        }
    }
}
