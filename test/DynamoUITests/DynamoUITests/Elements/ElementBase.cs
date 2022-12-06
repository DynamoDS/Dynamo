using OpenQA.Selenium.Appium.Windows;

namespace DynamoTests.Elements
{
    public abstract class ElementBase : IElement
    {
        protected readonly WindowsElement UiElemnt;

        public ElementBase(WindowsElement uiElemnt)
        {
            if (uiElemnt != null)
            {
                UiElemnt = uiElemnt;
            }
        }
        
    }
}
