using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

namespace DynamoTests.Elements
{
    public class SphereByCenterPointRadiusNode : NodeBase
    {
        public SphereByCenterPointRadiusNode(WindowsElement uiElemnt)
            : base(uiElemnt)
        { }

        public AppiumWebElement OutputSphere { get; set; }
        public AppiumWebElement InPutCenterPoint { get; set; }
        public AppiumWebElement InPutRadius { get; set; }
    }
}
