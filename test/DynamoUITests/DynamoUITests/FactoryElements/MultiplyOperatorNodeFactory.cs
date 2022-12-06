using DynamoTests.Elements;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using static DynamoTests.Utils.Enums;

namespace DynamoTests.FactoryElements
{
    public class MultiplyOperatorNodeFactory : NodeFactoryBase<MultiplyOperatorNode>
    {
        private AppiumWebElement xInPut;
        private AppiumWebElement yInPut;
        private AppiumWebElement outPut;

        public MultiplyOperatorNodeFactory(WindowsDriver<WindowsElement> dynamoSession, IWebElement _elementFound = null)
            : base(dynamoSession)
        {
            if (_elementFound != null)
            {
                elementFound = _elementFound;
            }
        }

        public override MultiplyOperatorNode Build()
        {
            SetNodeList();
            var result = (MultiplyOperatorNode)BuildBase();

            FindElementIn(ref xInPut, result.Node, "x", FindElementBy.Name);
            result.XInPut = xInPut;

            FindElementIn(ref yInPut, result.Node, "y", FindElementBy.Name);
            result.YInPut = yInPut;

            FindElementIn(ref outPut, result.Node, "number", FindElementBy.Name);
            result.OutPut = outPut;

            return result;
        }
    }
}
