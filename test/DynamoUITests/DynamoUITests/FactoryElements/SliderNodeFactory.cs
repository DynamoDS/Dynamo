using DynamoTests.DTO;
using DynamoTests.Elements;
using DynamoTests.Utils;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using System.Collections.Generic;
using System.Linq;
using static DynamoTests.Utils.Enums;

namespace DynamoTests.FactoryElements
{
    public class SliderNodeFactory : NodeFactoryBase<SliderNode>
    {
        private int MinValue;
        private int MaxValue;
        private int Step;

        private List<AppiumWebElement> nodeElements;
        private AppiumWebElement textBoxMinTb;
        private AppiumWebElement textBoxMaxTb;
        private AppiumWebElement textBoxStepTb;
        private AppiumWebElement thumbs;
        private AppiumWebElement headerSiteExpander;

        public SliderNodeFactory(int minValue, int maxValue, int step, WindowsDriver<WindowsElement> dynamoSession)
            : base(dynamoSession)
        {
            this.MinValue = minValue;
            this.MaxValue = maxValue;
            this.Step= step;
        }

        public override SliderNode Build()
        {
            SetNodeList();
            var result = (SliderNode)BuildBase();

            FindElementIn(ref headerSiteExpander, result.Node, "HeaderSite", FindElementBy.AccessibilityId);
            result.HeaderSiteExpander = headerSiteExpander;
                       
            FactoryMoveTo(headerSiteExpander, new LocationDTO{
            Y = headerSiteExpander.Center().Y,
            X = (headerSiteExpander.Center().X - (headerSiteExpander.Size.Width / 2)) + 5});

            FactoryClick();

            FindElementIn(ref textBoxMinTb, result.Node, "MinTb", FindElementBy.AccessibilityId);
            result.TextBoxMinValue = textBoxMinTb;

            FactoryMoveTo(result.TextBoxMinValue, result.TextBoxMinValue.Center());
            FactoryClick();
            Tools.SetClipboard(MinValue.ToString());
            FactorySendKeys(Keys.Control + "v" + Keys.Control);

            FindElementIn(ref textBoxMaxTb, result.Node, "MaxTb", FindElementBy.AccessibilityId);
            result.TextBoxMaxValue = textBoxMaxTb;

            FactoryMoveTo(result.TextBoxMaxValue, result.TextBoxMaxValue.Center());
            FactoryClick();
            Tools.SetClipboard(MaxValue.ToString());
            FactorySendKeys(Keys.Control + "v" + Keys.Control);

            FindElementIn(ref textBoxStepTb, result.Node, "StepTb", FindElementBy.AccessibilityId);
            result.TextBoxStep = textBoxStepTb;

            FactoryMoveTo(result.TextBoxStep, result.TextBoxStep.Center());
            FactoryClick();
            Tools.SetClipboard(Step.ToString());
            FactorySendKeys(Keys.Control + "v" + Keys.Control);

            FindElementIn(ref thumbs, result.Node, "Thumb", FindElementBy.AccessibilityId);
            result.ThummBar = thumbs;
                        
            FindElementsIn(ref nodeElements, result.Node, "PortNameTextBox", FindElementBy.AccessibilityId);

            if (nodeElements.Count() != 0)
            {                
                result.OutPut = nodeElements[0];
            }

            return result;
        }
    }
}
