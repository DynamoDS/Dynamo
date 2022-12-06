using DynamoTests.Elements;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using static DynamoTests.Utils.Enums;

namespace DynamoTests.FactoryElements
{
    public class InputNodeFactory : NodeFactoryBase<InputNode>
    {
        private AppiumWebElement output;
        private AppiumWebElement parameterEditor;

        public InputNodeFactory(WindowsDriver<WindowsElement> dynamoSession, IWebElement _elementFound = null) 
            : base(dynamoSession)
        {
            if (_elementFound != null)
            {
                elementFound = _elementFound;
            }
        }

        public override InputNode Build()
        {
            SetNodeList();
            var result = (InputNode)BuildBase();

            FindElementIn(ref output, result.Node, "PortNameTextBox", FindElementBy.AccessibilityId);
            result.Output = output;

            FindElementIn(ref parameterEditor, result.Node, "ParameterEditor", FindElementBy.ClassName);
            result.ParameterEditor = parameterEditor;

            return result;
        }
    }
}