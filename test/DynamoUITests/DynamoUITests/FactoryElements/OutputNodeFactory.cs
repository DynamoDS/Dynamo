using DynamoTests.Elements;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using static DynamoTests.Utils.Enums;

namespace DynamoTests.FactoryElements
{
    public class OutputNodeFactory : NodeFactoryBase<OutputNode>
    {
        private AppiumWebElement input;
        private AppiumWebElement outputEditor;

        public OutputNodeFactory(WindowsDriver<WindowsElement> dynamoSession)
            : base(dynamoSession)
        {

        }

        public OutputNodeFactory(WindowsDriver<WindowsElement> dynamoSession, IWebElement _elementFound)
            : base(dynamoSession)
        {
            elementFound = _elementFound;
        }

        public override OutputNode Build()
        {
            SetNodeList();
            var result = (OutputNode)BuildBase();

            FindElementIn(ref input, result.Node, "PortNameTextBox", FindElementBy.AccessibilityId);
            result.Input = input;

            FindElementIn(ref outputEditor, result.Node, "OutputEditor", FindElementBy.ClassName);
            result.OutputEditor = outputEditor;

            return result;
        }
    }
}
