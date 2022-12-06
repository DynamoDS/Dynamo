using DynamoTests.Elements;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using System.Collections.Generic;
using System.Linq;
using static DynamoTests.Utils.Enums;

namespace DynamoTests.FactoryElements
{
    public class BooleanNodeFactory : NodeFactoryBase<BooleanNode>
    {     
        private List<AppiumWebElement> nodeElements;
        private AppiumWebElement outPt;

        public BooleanNodeFactory(WindowsDriver<WindowsElement> dynamoSession)
          : base(dynamoSession)
        { }

        public override BooleanNode Build()
        {
            SetNodeList();
            var result = (BooleanNode)BuildBase();

            FindElementsIn(ref nodeElements, result.Node, "RadioButton", FindElementBy.ClassName);
            result.RbtTrue = nodeElements[0];
            result.RbtFalse = nodeElements[1];

            FindElementIn(ref outPt, result.Node, "PortNameTextBox", FindElementBy.AccessibilityId);
            result.OutPt = outPt;

            return result;
        }
    }
}
