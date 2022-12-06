using DynamoTests.Elements;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using System.Collections.Generic;
using static DynamoTests.Utils.Enums;

namespace DynamoTests.FactoryElements
{
    public class PythonScriptNodeFactory : NodeFactoryBase<PythonScriptNode>
    {
        private List<AppiumWebElement> nodeElements;

        public PythonScriptNodeFactory(WindowsDriver<WindowsElement> dynamoSession) : base(dynamoSession)
        {

        }

        public PythonScriptNodeFactory(WindowsDriver<WindowsElement> dynamoSession, IWebElement _elementFound) : base(dynamoSession)
        {
            if (_elementFound != null)
            {
                elementFound = _elementFound;
            }
        }

        public override PythonScriptNode Build()
        {
            SetNodeList();
            var result = (PythonScriptNode)BuildBase();

            FindElementsIn(ref nodeElements, result.Node, "PortNameTextBox", FindElementBy.AccessibilityId);

            if (nodeElements.Count == 2)
            {
                result.Input = nodeElements[0];
                result.Output = nodeElements[1];
            }

            return result;
        }
    }
}
