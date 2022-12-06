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
    public class CodeBlockNodeFactory : NodeFactoryBase<CodeBlockNode>
    {
        private string CodeBlockText;

        private AppiumWebElement innerTextEditor;
        private List<AppiumWebElement> codeBlockElements;
        private List<AppiumWebElement> inPutElements;
        private List<AppiumWebElement> outPutElements;


        public CodeBlockNodeFactory(string codeBlockText, WindowsDriver<WindowsElement> dynamoSession)
            : base(dynamoSession)
        {
            CodeBlockText = codeBlockText;
        }

        public override CodeBlockNode Build()
        {
            SetNodeList();
            var result = (CodeBlockNode)BuildBase();

            FindElementIn(ref innerTextEditor, result.Node, "InnerTextEditor", FindElementBy.AccessibilityId);
            result.CodeBlockEditor = innerTextEditor;

            FactoryMoveTo(result.CodeBlockEditor, result.CodeBlockEditor.Center());
            FactoryClick();
            Tools.SetClipboard(CodeBlockText);
            FactorySendKeys(Keys.Control + "v" + Keys.Control);
                        
            FactoryMoveTo(result.NameBlock, result.NameBlock.Center());
            FactoryClick();

            FindElementsIn(ref codeBlockElements, result.Node, "PortNameTextBox", FindElementBy.AccessibilityId);

            inPutElements = codeBlockElements.Where(x => x.Rect.X == result.Node.Rect.X).ToList();
            outPutElements = codeBlockElements.Where(x => x.Rect.X > result.Node.Rect.X).ToList();

            result.InPutElements = inPutElements;
            result.OutPuts = outPutElements;

            return result;
        }
        
    }
}
