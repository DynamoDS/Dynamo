using DynamoTests.Elements;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using static DynamoTests.Utils.Enums;

namespace DynamoTests.FactoryElements
{
    public class CircleByCenterPointRadiusNodeFactory : NodeFactoryBase<CircleByCenterPointRadiusNode>
    {
        private AppiumWebElement outPut;
        private AppiumWebElement inPut1;
        private AppiumWebElement inPut2;

        public CircleByCenterPointRadiusNodeFactory(WindowsDriver<WindowsElement> dynamoSession)
            : base(dynamoSession)
        { }

        public CircleByCenterPointRadiusNodeFactory(WindowsDriver<WindowsElement> dynamoSession, IWebElement _elementFound)
            : base(dynamoSession)
        {
            elementFound = _elementFound;
        }

        public override CircleByCenterPointRadiusNode Build()
        {
            SetNodeList();
            var result = (CircleByCenterPointRadiusNode)BuildBase();

            FindElementIn(ref inPut1, result.Node, "centerPoint", FindElementBy.Name);
            result.InPutCenterPoint = inPut1;

            FindElementIn(ref inPut2, result.Node, "radius", FindElementBy.Name);
            result.InPutRadius = inPut2;

            FindElementIn(ref outPut, result.Node, "Circle", FindElementBy.Name);
            result.OutPutCircle = outPut;

            return result;
        }
    }
}
