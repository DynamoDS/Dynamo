using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;


namespace DynamoTests.Elements
{
   public class RangeNode : NodeBase
    {
        public RangeNode(WindowsElement uiElemnt)
          : base(uiElemnt)
        { }

        public AppiumWebElement InPutStart { get; set; }
        public AppiumWebElement InPutEnd { get; set; }
        public AppiumWebElement InPutStep { get; set; }
        public AppiumWebElement OutPutSeq { get; set; }


    }
}
