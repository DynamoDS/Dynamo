using DynamoTests.DTO;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Windows;

namespace DynamoTests.Elements
{
    public class GroupElement : ElementBase
    {
        public GroupElement(WindowsElement uiElement)
            : base(uiElement)
        { }

        public WindowsElement Group { get { return UiElemnt; } }

        public LocationDTO ClickLocation
        {
            get
            {
                return new LocationDTO
                {
                    X = Group.Size.Width / 2,
                    Y = Group.Size.Height - 2
                };
            }
        }

        public LocationDTO GroupTitleLocation
        {
            get
            {
                return new LocationDTO
                {
                    X = Group.Size.Width / 2,
                    Y = 10
                };
            }
        }

        public bool ContainsText(string expectedValue)
        {
            IWebElement watchNodeTextElement = Group.FindElementByName(expectedValue);
            return watchNodeTextElement.Text.Equals(expectedValue);
        }

    }
}
