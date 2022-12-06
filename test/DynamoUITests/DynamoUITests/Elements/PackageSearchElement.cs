using System;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

namespace DynamoTests.Elements
{
    public class PackageSearchElement : ElementBase
    {
        public PackageSearchElement(WindowsElement uiElement)
            : base(uiElement)
        { }

        public WindowsElement PackageSearch { get { return UiElemnt; } }

        public AppiumWebElement SearchEditField { get; set; }
        public AppiumWebElement SearchTextField { get; set; }
        public AppiumWebElement DownloadButton { get { return PackageSearch.FindElementByAccessibilityId("installButton"); } }
        public AppiumWebElement ClearDownloadsButton { get { return PackageSearch.FindElementByAccessibilityId("clearSearchTextBox"); } }

        private const int numberOfAttempts = 10;

        public void WaitForSync()
        {
            int attempts = 0;
            while (IsSyncing() && attempts != numberOfAttempts)
            {
                attempts++;
                Thread.Sleep(TimeSpan.FromSeconds(2));
            }
        }

        public void WaitForDownload()
        {
            int attempts = 0;
            while (IsPackageDownloading() && attempts != numberOfAttempts)
            {
                attempts++;
                Thread.Sleep(TimeSpan.FromSeconds(2));
            }
        }

        public void WaitForLoad()
        {
            int attempts = 0;
            while (IsPackageLoading() && attempts != numberOfAttempts)
            {
                attempts++;
                Thread.Sleep(TimeSpan.FromSeconds(2));
            }
        }

        private bool IsSyncing()
        {
            try
            {
                return PackageSearch.FindElementByName("Syncing with server...") != null;
            }
            catch (WebDriverException)
            {
                return false;
            }
        }

        private bool IsPackageDownloading()
        {
            try
            {
                return PackageSearch.FindElementByName("Installed") == null;
            }
            catch (WebDriverException)
            {
                return true;
            }
        }

        private bool IsPackageLoading()
        {
            try
            {
                return PackageSearch.FindElementByAccessibilityId("installButton") == null;
            }
            catch (WebDriverException)
            {
                return true;
            }
        }
    }
}
