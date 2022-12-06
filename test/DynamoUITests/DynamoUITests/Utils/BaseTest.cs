using DynamoTests.DTO;
using DynamoTests.Elements;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static DynamoTests.Utils.Enums;

namespace DynamoTests.Utils
{

    public abstract class BaseTest : DynamoSession
    {
        protected static WindowsElement startPage;

        protected static WorkspaceViewElement workspaceViewElement;
        protected static LibraryElement libraryElement;

        protected static List<WindowsElement> tabItem;
        protected static WindowsElement confimationDialog;

        protected ShortcutToolbarElement shortcutToolbar;

        #region .: Test configuration :.


        [OneTimeSetUp]
        public void Clear()
        {
            StartDynamo(true);
        }

        protected void StartDynamo(bool clear)
        {
            if (clear)
            {
                DeleteAppData();
                DeleteBuiltInPackages();
            }

            Setup(clear);

            WindowMaximize();

            try
            {
                FindElement(ref startPage, "StartPageView", FindElementBy.ClassName);
                if (startPage.Displayed)
                {
                    startPage.Click();
                }
                DynamoSleep(TimeSpan.FromSeconds(1));
                Assert.IsTrue(startPage.Enabled);
            }
            catch (Exception)
            {
                session.SwitchTo().Window(session.CurrentWindowHandle);
                FindElement(ref startPage, "StartPageView", FindElementBy.ClassName);
                if (startPage.Displayed)
                {
                    startPage.Click();
                }
                DynamoSleep(TimeSpan.FromSeconds(1));
                Assert.IsTrue(startPage.Enabled);
            }
        }

        private void DeleteAppData()
        {
            if (Tools.DynamoAppDataExist())
            {
                // This will delete the cache data of Dynamo and the selected/unselected options in the menus (like Settings) will be reset
                // Downloaded packages will also be deleted
                Tools.DeleteDynamoData();
            }
        }

        private void DeleteBuiltInPackages()
        {
            SetDynamoSessionConfiguration();
            var dynamoSandboxPath = GetDynamoOptions().ToCapabilities().GetCapability("app") as string;
            var buitInPackagesPath = Path.Combine(Path.GetDirectoryName(dynamoSandboxPath), @"Built-In Packages");
            if (Directory.Exists(buitInPackagesPath))
            {
                Tools.DeleteDir(buitInPackagesPath);
            }
        }

        private void WindowMaximize()
        {
            var windowList = session.FindElementsByClassName("Window");

            if (windowList.Count() > 1)
            {
                var agreementWindow = windowList.Where(t => t.Text == "Agreement to collect usability data on your use of Dynamo").FirstOrDefault();

                if (agreementWindow != null)
                {
                    agreementWindow.SendKeys(Keys.Alt + Keys.F4 + Keys.Alt);
                    DynamoSleep(TimeSpan.FromSeconds(1));
                }
            }

            windowList = session.FindElementsByClassName("Window");

            if (windowList.Count() > 1)
            {
                var agreementWindow = windowList.Where(t => t.Text == "Agreement to collect usability data on your use of Dynamo").FirstOrDefault();

                if (agreementWindow != null)
                {
                    interactions.MoveTo(agreementWindow, new LocationDTO { X = agreementWindow.Size.Width - 8, Y = 8 });
                    interactions.Click();
                    DynamoSleep(TimeSpan.FromSeconds(1));
                }
            }

            try
            {
                session.Manage().Window.Maximize();
                DynamoSleep(TimeSpan.FromSeconds(1));
            }
            catch (Exception)
            {
                session.SwitchTo().Window(session.CurrentWindowHandle);
                session.Manage().Window.Maximize();
                DynamoSleep(TimeSpan.FromSeconds(1));
            }
        }

        private void OnFirstExecute()
        {
            try
            {
                DynamoSleep(TimeSpan.FromSeconds(3));
                var okButton = session.FindElementByAccessibilityId("okButton");
                if (okButton != null)
                {
                    interactions.MoveTo(okButton, okButton.Center());
                    okButton.Click();
                }
            }
            catch (WebDriverException)
            { }
        }

        [OneTimeTearDown]
        public static void ClassCleanup()
        {
            StopDynamo();
        }

        protected static void StopDynamo()
        {
            try
            {
                FindElements(ref tabItem, "TabItem", FindElementBy.ClassName);
                CloseTab(tabItem[0]);
            }
            catch (Exception)
            { }

            startPage = null;
            workspaceViewElement = null;
            libraryElement = null;

            TearDown();
        }

        internal static void CloseTab(WindowsElement _tabItem)
        {
            if (_tabItem != null && _tabItem.Displayed)
            {
                AppiumWebElement cancelButon = null;
                FindElementIn(ref cancelButon, _tabItem, "Button", FindElementBy.ClassName);
                MoveTo(cancelButon, cancelButon.Center());
                dynamoActions.Click();
                ExecuteActions();
                CloseDialog();
            }
        }

        private static void CloseDialog()
        {
            FindElement(ref confimationDialog, "Confirmation", FindElementBy.Name);

            if (confimationDialog != null)
            {
                AppiumWebElement confimationNoButton = null;
                FindElementIn(ref confimationNoButton, confimationDialog, "No", FindElementBy.Name);
                MoveTo(confimationNoButton, confimationNoButton.Center());
                dynamoActions.Click();
                ExecuteActions();
            }
        }

         protected void OpenFile(string path)
        {
            if(shortcutToolbar == null)
            {
                interactions.MakeShortcutToolbar(ref shortcutToolbar);

            }
            interactions.MoveTo(shortcutToolbar.OpenButton, shortcutToolbar.OpenButton.Center());
            interactions.Click();

            CloseDialog();

            Tools.SetClipboard(path);
            DynamoSleep(TimeSpan.FromSeconds(2));
            interactions.SendKeys(Keys.Control + "v" + Keys.Control);

            interactions.SendKeys(Keys.Enter);
        }

        #endregion .: Test configuration :.

        public static void FindElement(ref WindowsElement windowsItem, string text, FindElementBy findElementBy)
        {
            windowsItem = null;

            try
            {
                switch (findElementBy)
                {
                    case FindElementBy.AccessibilityId:
                        windowsItem = session.FindElementByAccessibilityId(text);
                        break;
                    case FindElementBy.ClassName:
                        windowsItem = session.FindElementByClassName(text);
                        break;
                    case FindElementBy.Name:
                        windowsItem = session.FindElementByName(text);
                        break;
                }
            }
            catch 
            {
            }
        }

        public static void FindElements(ref List<WindowsElement> windowsItems, string text, FindElementBy findElementBy)
        {
            windowsItems = null;

            try
            {
                switch (findElementBy)
                {
                    case FindElementBy.AccessibilityId:
                        windowsItems = session.FindElementsByAccessibilityId(text).ToList();
                        break;
                    case FindElementBy.ClassName:
                        windowsItems = session.FindElementsByClassName(text).ToList();
                        break;
                    case FindElementBy.Name:
                        windowsItems = session.FindElementsByName(text).ToList();
                        break;
                }
            }
            catch 
            {
            }
        }

        public static void FindElementIn(ref AppiumWebElement elementItem, WindowsElement windowsItem, string text, FindElementBy findElementBy)
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

        public static void FindElementsIn(ref List<AppiumWebElement> elementItems, WindowsElement windowsItem, string text, FindElementBy findElementBy)
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

        private static void MoveTo(IWebElement windowsElement, LocationDTO location = null)
        {
            if (location == null)
            {
                location = new LocationDTO { X = 0, Y = 0 };
            }

            dynamoActions.MoveToElement(windowsElement, location.X, location.Y);
        }

        public static void CheckElement(IWebElement element)
        {
            MoveTo(element, element.Center());
            dynamoActions.Click();
            ExecuteActions();
            DynamoSleep(TimeSpan.FromSeconds(.5));
        }

        public static void LinkElements(IWebElement from, IWebElement to)
        {
            MoveTo(from, from.Center());
            dynamoActions.Click();
            ExecuteActions();
            MoveTo(to, to.Center());
            dynamoActions.Click();
            ExecuteActions();
        }

        public static void ExecuteActions()
        {
            dynamoActions.Build().Perform();
            ResetDynamoActions();
        }

        public static void ExecuteDesktopActions()
        {
            desktopActions.Build().Perform();
            ResetDesktopActions();
        }

    }
}
