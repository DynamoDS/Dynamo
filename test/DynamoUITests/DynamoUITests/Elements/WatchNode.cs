using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

namespace DynamoTests.Elements
{
    public class WatchNode : NodeBase
    {
        public WatchNode(WindowsElement uiElemnt)
            : base(uiElemnt)
        { }

        public AppiumWebElement InPut { get; set; }
        public AppiumWebElement OutPut { get; set; }

        public bool ContainsText(string expectedValue)
        {
            bool isTooEarly = false;
            IWebElement watchNodeTextElement = null;

            try
            {
                watchNodeTextElement = Node.FindElementByName(expectedValue);
            }
            catch
            {
                isTooEarly = true;
            }

            if (isTooEarly)
            {
                try
                {
                    System.Threading.Thread.Sleep(2000);
                    watchNodeTextElement = Node.FindElementByName(expectedValue);
                }
                catch
                {
                }
            }

            return watchNodeTextElement != null ? watchNodeTextElement.Text.Equals(expectedValue) : false;
        }
    }
}
