using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

namespace DynamoTests.Elements
{
    public class CircleByCenterPointRadiusNode : NodeBase
    {
        public CircleByCenterPointRadiusNode(WindowsElement uiElemnt)
            : base(uiElemnt)
        { }

        public AppiumWebElement OutPutCircle { get; set; }
        public AppiumWebElement InPutCenterPoint { get; set; }
        public AppiumWebElement InPutRadius { get; set; }

    }
}