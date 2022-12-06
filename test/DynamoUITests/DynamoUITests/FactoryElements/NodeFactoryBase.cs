using DynamoTests.Elements;
using DynamoTests.Utils;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using static DynamoTests.Utils.Enums;

namespace DynamoTests.FactoryElements
{
    public abstract class NodeFactoryBase<T> : FactoryBase<T> where T : NodeBase
    {
        private List<WindowsElement> nodeList;
        private AppiumWebElement nameBlock;
        private AppiumWebElement previewControl;

        public NodeFactoryBase(WindowsDriver<WindowsElement> dynamoSession)
            : base(dynamoSession)
        { }

        internal void SetNodeList()
        {
            FindElements(ref nodeList, "topControl", FindElementBy.AccessibilityId);
            elementToFind = nodeList.LastOrDefault();
        }

        internal IElement BuildBase()
        {
            var baseNode = (T)Activator.CreateInstance(typeof(T), new object[] { elementFound ?? elementToFind });

            FindElementIn(ref nameBlock, baseNode.Node, "NameBlock", FindElementBy.AccessibilityId);                       
            baseNode.NameBlock = nameBlock;

            FactoryMoveTo(baseNode.Node, baseNode.Node.Center());
            //Wait for the PreviewControl
            FactorySleep(TimeSpan.FromSeconds(1.5));
            FindElementIn(ref previewControl, baseNode.Node, "PreviewControl", FindElementBy.ClassName);//PreviewControl or PreviewCompactView
            baseNode.PreviewControl = previewControl;

            return baseNode;
        }

    }
}
