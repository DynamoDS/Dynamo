using DynamoTests.Elements;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using static DynamoTests.Utils.Enums;

namespace DynamoTests.FactoryElements
{
    public class SphereByCenterPointRadiusNodeFactory : NodeFactoryBase<SphereByCenterPointRadiusNode>
    {
        private AppiumWebElement outPut;
        private AppiumWebElement inPut1;
        private AppiumWebElement inPut2;

        public SphereByCenterPointRadiusNodeFactory(WindowsDriver<WindowsElement> dynamoSession)
            : base(dynamoSession)
        { }

        public SphereByCenterPointRadiusNodeFactory(WindowsDriver<WindowsElement> dynamoSession, IWebElement _elementFound)
            : base(dynamoSession)
        {
            elementFound = _elementFound;
        }

        public override SphereByCenterPointRadiusNode Build()
        {
            SetNodeList();
            var result = (SphereByCenterPointRadiusNode)BuildBase();

            FindElementIn(ref inPut1, result.Node, "centerPoint", FindElementBy.Name);
            result.InPutCenterPoint = inPut1;

            FindElementIn(ref inPut2, result.Node, "radius", FindElementBy.Name);
            result.InPutRadius = inPut2;

            FindElementIn(ref outPut, result.Node, "Sphere", FindElementBy.Name);
            result.OutputSphere = outPut;

            return result;
        }
    }
}
