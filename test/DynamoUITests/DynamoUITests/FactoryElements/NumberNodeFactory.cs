using DynamoTests.Elements;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using static DynamoTests.Utils.Enums;

namespace DynamoTests.FactoryElements
{
    public class NumberNodeFactory : NodeFactoryBase<NumberNode>
    {
        private AppiumWebElement outPut;
        private AppiumWebElement textBox;

        public NumberNodeFactory(WindowsDriver<WindowsElement> dynamoSession)
            : base(dynamoSession)
        { }

        public NumberNodeFactory(WindowsDriver<WindowsElement> dynamoSession, IWebElement _elementFound)
            : base(dynamoSession)
        {
            elementFound = _elementFound;
        }

        public override NumberNode Build()
        {
            SetNodeList();
            var result = (NumberNode)BuildBase();

            FindElementIn(ref textBox, result.Node, "TextBox", FindElementBy.ClassName);
            result.NumberText = textBox;

            FindElementIn(ref outPut, result.Node, "PortNameTextBox", FindElementBy.AccessibilityId);
            result.OutPut = outPut;

            return result;
        }
    }
}
