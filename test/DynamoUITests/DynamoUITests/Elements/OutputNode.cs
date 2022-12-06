using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamoTests.Elements
{
    public class OutputNode : NodeBase
    {
        public OutputNode(WindowsElement uiElemnt)
            : base(uiElemnt)
        { }

        public AppiumWebElement Input { get; set; }
        public AppiumWebElement OutputEditor { get; set; }

    }
}
