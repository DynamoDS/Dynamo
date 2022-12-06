using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

namespace DynamoTests.Elements
{
    public class PointByCoordinates : NodeBase
    {
        public PointByCoordinates(WindowsElement uiElemnt)
            : base(uiElemnt)
        { }

        public AppiumWebElement OutPutPoint { get; set; }
        public AppiumWebElement InPutX { get; set; }
        public AppiumWebElement InPutY { get; set; }

    }
}