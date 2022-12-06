using DynamoTests.DTO;
using DynamoTests.Elements;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static DynamoTests.Utils.Enums;

namespace DynamoTests.FactoryElements
{
    public abstract class FactoryBase<T> : IDisposable where T : ElementBase
    {
        private WindowsDriver<WindowsElement> DynamoSession;

        protected WindowsElement elementToFind;
        protected WindowsElement menuItem;
        protected List<WindowsElement> elementsToFind;
        protected IWebElement elementFound;

        private Actions dynamoFactoryActions;

        public FactoryBase(WindowsDriver<WindowsElement> dynamoSession)
        {
            DynamoSession = dynamoSession;
        }

        public abstract T Build();

        internal void FindElement(ref WindowsElement windowsItem, string text, FindElementBy findElementBy)
        {
            windowsItem = null;

            try
            {
                switch (findElementBy)
                {
                    case FindElementBy.AccessibilityId:
                        windowsItem = DynamoSession.FindElementByAccessibilityId(text);
                        break;
                    case FindElementBy.ClassName:
                        windowsItem = DynamoSession.FindElementByClassName(text);
                        break;
                    case FindElementBy.Name:
                        windowsItem = DynamoSession.FindElementByName(text);
                        break;
                }
            }
            catch
            {
            }
        }

        internal void FindElements(ref List<WindowsElement> windowsItems, string text, FindElementBy findElementBy)
        {
            windowsItems = null;

            try
            {
                switch (findElementBy)
                {
                    case FindElementBy.AccessibilityId:
                        windowsItems = DynamoSession.FindElementsByAccessibilityId(text).ToList();
                        break;
                    case FindElementBy.ClassName:
                        windowsItems = DynamoSession.FindElementsByClassName(text).ToList();
                        break;
                    case FindElementBy.Name:
                        windowsItems = DynamoSession.FindElementsByName(text).ToList();
                        break;
                }
            }
            catch
            {
            }
        }

        internal void FindElementIn(ref AppiumWebElement elementItem, WindowsElement windowsItem, string text, FindElementBy findElementBy)
        {
            elementItem = null;

            try
            {
                switch (findElementBy)
                {
                    case FindElementBy.AccessibilityId:
                        elementItem = windowsItem.FindElementByAccessibilityId(text);
                        break;
                    case FindElementBy.ClassName:
                        elementItem = windowsItem.FindElementByClassName(text);
                        break;
                    case FindElementBy.Name:
                        elementItem = windowsItem.FindElementByName(text);
                        break;
                }
            }
            catch
            {
            }
        }

        internal void FindElementsIn(ref List<AppiumWebElement> elementItems, WindowsElement windowsItem, string text, FindElementBy findElementBy)
        {
            elementItems = null;

            try
            {
                switch (findElementBy)
                {
                    case FindElementBy.AccessibilityId:
                        elementItems = windowsItem.FindElementsByAccessibilityId(text).ToList();
                        break;
                    case FindElementBy.ClassName:
                        elementItems = windowsItem.FindElementsByClassName(text).ToList();
                        break;
                    case FindElementBy.Name:
                        elementItems = windowsItem.FindElementsByName(text).ToList();
                        break;
                }
            }
            catch
            {
            }
        }

        internal void FactoryMoveTo(IWebElement windowsElement, LocationDTO location = null)
        {
            dynamoFactoryActions = new Actions(DynamoSession);

            if (location == null)
            {
                location = new LocationDTO { X = 0, Y = 0 };
            }

            dynamoFactoryActions.MoveToElement(windowsElement, location.X, location.Y);
            dynamoFactoryActions.Build().Perform();
        }

        internal void FactoryClick()
        {
            dynamoFactoryActions = new Actions(DynamoSession);
            dynamoFactoryActions.Click();
            dynamoFactoryActions.Build().Perform();
        }

        internal void FactorySendKeys(string text)
        {
            dynamoFactoryActions = new Actions(DynamoSession);
            dynamoFactoryActions.SendKeys(text);
            dynamoFactoryActions.Build().Perform();
        }

        internal void FactorySleep(TimeSpan timeSpan)
        {
            Thread.Sleep(timeSpan);
        }

        public void Dispose()
        {
            DynamoSession = null;
        }

    }
}
