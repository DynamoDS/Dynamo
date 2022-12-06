using DynamoTests.Elements;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using static DynamoTests.Utils.Enums;

namespace DynamoTests.FactoryElements
{
    public class RangeNodeFactory : NodeFactoryBase<RangeNode>
    {
        private AppiumWebElement inPutStart;
        private AppiumWebElement inPutEnd;
        private AppiumWebElement inPutStep;
        private AppiumWebElement outPutSeq;

        public RangeNodeFactory(WindowsDriver<WindowsElement> dynamoSession)
           : base(dynamoSession)
        {           
        }
        public override RangeNode Build()
        {
            SetNodeList();
            var result = (RangeNode)BuildBase();

            FindElementIn(ref inPutStart, result.Node, "start", FindElementBy.Name);
            result.InPutStart = inPutStart;

            FindElementIn(ref inPutEnd, result.Node, "end", FindElementBy.Name);
            result.InPutEnd = inPutEnd;

            FindElementIn(ref inPutStep, result.Node, "step", FindElementBy.Name);
            result.InPutStep = inPutStep;

            FindElementIn(ref outPutSeq, result.Node, "list", FindElementBy.Name);
            result.OutPutSeq = outPutSeq;


            return result;
        }
    }
}
