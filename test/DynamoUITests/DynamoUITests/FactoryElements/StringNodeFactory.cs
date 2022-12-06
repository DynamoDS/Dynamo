using DynamoTests.Elements;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using static DynamoTests.Utils.Enums;

namespace DynamoTests.FactoryElements
{
    public class StringNodeFactory : NodeFactoryBase<StringNode>
    {
        private AppiumWebElement outPut;
        private AppiumWebElement textBox;

        public StringNodeFactory(WindowsDriver<WindowsElement> dynamoSession)
            : base(dynamoSession)
        { }

        public override StringNode Build()
        {
            SetNodeList();
            var result = (StringNode)BuildBase();

            FindElementIn(ref textBox, result.Node, "TextBox", FindElementBy.ClassName);
            result.TextBox = textBox;

            FindElementIn(ref outPut, result.Node, "PortNameTextBox", FindElementBy.AccessibilityId);
            result.OutPut = outPut;

            return result;
        }

    }
}
