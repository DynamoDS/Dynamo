using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamoTests.Elements
{
    public class CustomNodePropertiesElement : ElementBase
    {
        public CustomNodePropertiesElement(WindowsElement uiElement)
            : base(uiElement)
        { }

        public WindowsElement CustomNodeWindow { get { return UiElemnt; } }
        public AppiumWebElement NameInput { get; set; }
        public AppiumWebElement DescriptionInput { get; set; }
        public AppiumWebElement CategoryBox { get; set; }
        public AppiumWebElement OkButton { get; set; }
        public AppiumWebElement CancelButton { get; set; }

    }
}
