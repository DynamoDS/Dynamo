using DynamoTests.Elements;
using OpenQA.Selenium;
using System.Linq;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using static DynamoTests.Utils.Enums;
using System.Collections.Generic;

namespace DynamoTests.FactoryElements
{
    public class ListCreateFactory : NodeFactoryBase<ListCreateNode>
    {
        private List<AppiumWebElement> itemsList;
        private AppiumWebElement buttonPlus;
        private AppiumWebElement buttonLess;
        private AppiumWebElement outPutList;

        public ListCreateFactory(WindowsDriver<WindowsElement> dynamoSession)
           : base(dynamoSession)
        {
        }
        public override ListCreateNode Build()
        {
            SetNodeList();
            var result = (ListCreateNode)BuildBase();

            FindElementIn(ref buttonPlus, result.Node, "+", FindElementBy.Name);
            result.ButtonPlus = buttonPlus;

            for (var i = 0; i < 2; i++)
            {
                result.ButtonPlus.Click();
            }

            FindElementIn(ref buttonLess, result.Node, "-", FindElementBy.Name);
            result.ButtonLess = buttonLess;

            FindElementIn(ref outPutList, result.Node, "list", FindElementBy.Name);
            result.OutPutList = outPutList;

            FindElementsIn(ref itemsList, result.Node, "PortNameTextBox", FindElementBy.AccessibilityId);
            result.itemsList = itemsList.Where(x=> x.TagName != "list").ToList();

            






            return result;
        }

    }
}
