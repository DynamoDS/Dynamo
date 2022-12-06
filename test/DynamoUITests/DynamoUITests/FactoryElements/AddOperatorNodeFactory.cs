using DynamoTests.Elements;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using static DynamoTests.Utils.Enums;

namespace DynamoTests.FactoryElements
{
    public class AddOperatorNodeFactory : NodeFactoryBase<AddOperatorNode>
    {
        private AppiumWebElement xInPut;
        private AppiumWebElement yInPut;
        private AppiumWebElement outPut;

        public AddOperatorNodeFactory(WindowsDriver<WindowsElement> dynamoSession)
            : base(dynamoSession)
        { }

        public AddOperatorNodeFactory(WindowsDriver<WindowsElement> dynamoSession, IWebElement _elementFound)
            : base(dynamoSession)
        {
            elementFound = _elementFound;
        }

        public override AddOperatorNode Build()
        {
            SetNodeList();
            var result = (AddOperatorNode)BuildBase();

            FindElementIn(ref xInPut, result.Node, "x", FindElementBy.Name);
            result.XInPut = xInPut;

            FindElementIn(ref yInPut, result.Node, "y", FindElementBy.Name);
            result.YInPut = yInPut;

            FindElementIn(ref outPut, result.Node, "var", FindElementBy.Name);
            result.OutPut = outPut;

            return result;
        }
    }
}
