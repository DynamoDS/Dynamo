using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;

namespace DynamoTests.Elements
{
    public class NoteElement : ElementBase
    {
        public NoteElement(WindowsElement uiElement)
            : base(uiElement)
        { }

        public WindowsElement Note { get { return UiElemnt; } }

        public bool ContainsText(string expectedValue)
        {
            IWebElement noteTextElement = Note.FindElementByName(expectedValue);
            return noteTextElement != null ? noteTextElement.Text.Equals(expectedValue) : false;
        }

        public void SetText(WindowsDriver<WindowsElement> desktopSession, string text)
        {
            var actions = new Actions(desktopSession);
            actions.SendKeys(text);

            //Move outside the note and click
            actions.MoveByOffset(50,50).Click();
            actions.Build().Perform();
        }

    }
}
