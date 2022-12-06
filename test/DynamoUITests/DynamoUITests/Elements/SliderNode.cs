using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

namespace DynamoTests.Elements
{
   public class SliderNode : NodeBase
    {
        public SliderNode(WindowsElement uiElemnt)
         : base(uiElemnt)
        { }

        public AppiumWebElement TextBoxGetTheMovementStep { get; set; }
        public AppiumWebElement TextBoxMinValue { get; set; }
        public AppiumWebElement TextBoxMaxValue { get; set; }
        public AppiumWebElement TextBoxStep { get; set; }
        public AppiumWebElement ThummBar { get; set; }
        public AppiumWebElement OutPut { get; set; }
        public AppiumWebElement HeaderSiteExpander { get; set; }


    }
}
