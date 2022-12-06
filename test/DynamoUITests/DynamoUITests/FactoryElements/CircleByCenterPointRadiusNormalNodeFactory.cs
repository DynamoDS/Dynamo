using DynamoTests.Elements;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using static DynamoTests.Utils.Enums;

namespace DynamoTests.FactoryElements
{
    public class CircleByCenterPointRadiusNormalNodeFactory : NodeFactoryBase<CircleByCenterPointRadiusNormalNode>
    {
        private AppiumWebElement outPut;
        private AppiumWebElement inPut1;
        private AppiumWebElement inPut2;
        private AppiumWebElement inPut3;

        public CircleByCenterPointRadiusNormalNodeFactory(WindowsDriver<WindowsElement> dynamoSession)
            : base(dynamoSession)
        { }

        public CircleByCenterPointRadiusNormalNodeFactory(WindowsDriver<WindowsElement> dynamoSession, IWebElement _elementFound)
            : base(dynamoSession)
        {
            elementFound = _elementFound;
        }

        public override CircleByCenterPointRadiusNormalNode Build()
        {
            SetNodeList();
            var result = (CircleByCenterPointRadiusNormalNode)BuildBase();

            FindElementIn(ref inPut1, result.Node, "centerPoint", FindElementBy.Name);
            result.InPutCenterPoint = inPut1;

            FindElementIn(ref inPut2, result.Node, "radius", FindElementBy.Name);
            result.InPutRadius = inPut2; 
            
            FindElementIn(ref inPut3, result.Node, "normal", FindElementBy.Name);
            result.InPutNormal = inPut3;

            FindElementIn(ref outPut, result.Node, "Circle", FindElementBy.Name);
            result.OutPutCircle = outPut;

            return result;
        }
    }
}
