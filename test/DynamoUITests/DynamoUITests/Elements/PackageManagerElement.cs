using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

namespace DynamoTests.Elements
{
    public class PackageManagerElement : ElementBase
    {
        public PackageManagerElement(WindowsElement uiElement)
            : base(uiElement)
        { }

        public WindowsElement PackageManager { get { return UiElemnt; } }

        public IList<AppiumWebElement> MoreOptionsButtons { get { return PackageManager.FindElementsByAccessibilityId("MoreButton").ToList(); } }

        internal bool FindPackage(string name)
        {
            try
            {
                return PackageManager.FindElementByName(name) != null;
            }
            catch (WebDriverException)
            {
                return false;
            }
        }

    }
}
