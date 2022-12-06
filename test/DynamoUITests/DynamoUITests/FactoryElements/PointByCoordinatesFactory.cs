using DynamoTests.Elements;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using static DynamoTests.Utils.Enums;

namespace DynamoTests.FactoryElements
{
    public class PointByCoordinatesNodeFactory : NodeFactoryBase<PointByCoordinates>
    {
        private AppiumWebElement outPut;
        private AppiumWebElement inPut1;
        private AppiumWebElement inPut2;
        private bool nodeAutoComplete;

        public PointByCoordinatesNodeFactory(WindowsDriver<WindowsElement> dynamoSession)
            : base(dynamoSession)
        { }

        public PointByCoordinatesNodeFactory(WindowsDriver<WindowsElement> dynamoSession, IWebElement _elementFound)
            : base(dynamoSession)
        {
            elementFound = _elementFound;
        }

        public override PointByCoordinates Build()
        {
            SetNodeList();
            var result = (PointByCoordinates)BuildBase();
                         
            FindElementIn(ref inPut1, result.Node, "x", FindElementBy.Name);
            result.InPutX = inPut1;

            FindElementIn(ref inPut2, result.Node, "y", FindElementBy.Name);
            result.InPutY = inPut2;

            FindElementIn(ref outPut, result.Node, "Point", FindElementBy.Name);
            result.OutPutPoint = outPut;

            return result;
        }
    }
}
