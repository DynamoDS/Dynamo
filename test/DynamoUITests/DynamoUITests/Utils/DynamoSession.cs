using DynamoTests.DTO;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;
using System;
using System.IO;
using System.Reflection;
using System.Threading;

namespace DynamoTests.Utils
{
    public class DynamoSession
    {
        private static ConfigurationDTO windowsDriverSessionConfiguration;

        protected static WindowsDriver<WindowsElement> session;
        protected static WindowsDriver<WindowsElement> desktopSession;

        protected static Actions desktopActions;
        protected static Actions dynamoActions;

        protected static DynamoInteractions interactions;

        /// <summary>
        /// Setup a new dynamo session
        /// </summary>
        /// <param name="copyPackages">
        /// If true, populate the App data package folder and the Build-in package folder with some pre-installed packages
        /// </param>
        public static void Setup(bool copyPackages = true)
        {
            SetDynamoSessionConfiguration();

            if (copyPackages)
            {
                var DynamoSandboxPath = GetDynamoOptions().ToCapabilities().GetCapability("app") as string;
                var versionOfSandbox = AssemblyName.GetAssemblyName(DynamoSandboxPath).Version;
                var packagesPath = Path.Combine(ConfigurationHelper.DynamoDataRoute, @"Dynamo Core",
                    $"{versionOfSandbox.Major}.{versionOfSandbox.Minor}", @"Packages");

                // copy package.
                var source = new DirectoryInfo(Path.Combine(ConfigurationHelper.GetTestPackagesRoute, @"package"));
                var dest = new DirectoryInfo(Path.Combine(packagesPath, @"package"));
                ConfigurationHelper.CopyDirectory(source, dest);

                // Copy signed package to app data
                source = new DirectoryInfo(
                    Path.Combine(ConfigurationHelper.GetTestPackagesRoute, @"Signed Package"));
                dest = new DirectoryInfo(Path.Combine(packagesPath, @"Signed Package"));
                ConfigurationHelper.CopyDirectory(source, dest);

                // Copy signed package to built in packages
                var basePath = Path.GetDirectoryName(DynamoSandboxPath);
                var commonPackagesPath =
                    Path.Combine(basePath, @"Built-In Packages", @"Packages", @"Signed Package");
                dest = new DirectoryInfo(commonPackagesPath);
                ConfigurationHelper.CopyDirectory(source, dest);
            }

            if (session == null)
            {
                bool isDynamoFailOnLoad = false;

                try
                {
                    session = new WindowsDriver<WindowsElement>(
                        new Uri($"{windowsDriverSessionConfiguration.WindowsApplicationDriver.Url}:{windowsDriverSessionConfiguration.WindowsApplicationDriver.Port}"),
                        GetDynamoOptions(), TimeSpan.FromMinutes(1));
                    DynamoSleep(TimeSpan.FromSeconds(2));
                }
                catch (Exception)
                {
                    isDynamoFailOnLoad = true;
                    DynamoSleep(TimeSpan.FromSeconds(30));
                }

                desktopSession = new WindowsDriver<WindowsElement>(
                                new Uri($"{windowsDriverSessionConfiguration.WindowsApplicationDriver.Url}:{windowsDriverSessionConfiguration.WindowsApplicationDriver.Port}"),
                                GetDesktopOptions(), TimeSpan.FromMinutes(2));
                DynamoSleep(TimeSpan.FromSeconds(2));
                Assert.IsNotNull(desktopSession);
                ResetDesktopActions();

                if (session == null && isDynamoFailOnLoad)
                {
                    WindowsElement dynamoWindow = desktopSession.FindElementByName("Dynamo");
                    string dynamoTopLevelWindowHandle = dynamoTopLevelWindowHandle = (int.Parse(dynamoWindow.GetAttribute("NativeWindowHandle"))).ToString("x");

                    AppiumOptions appCapabilities = new AppiumOptions();
                    appCapabilities.AddAdditionalCapability("appTopLevelWindow", dynamoTopLevelWindowHandle);
                    session = new WindowsDriver<WindowsElement>(
                        new Uri($"{windowsDriverSessionConfiguration.WindowsApplicationDriver.Url}:{windowsDriverSessionConfiguration.WindowsApplicationDriver.Port}"),
                        appCapabilities, TimeSpan.FromMinutes(2));
                    DynamoSleep(TimeSpan.FromSeconds(2));
                }
                Assert.IsNotNull(session);

                // Set implicit timeout to 1.5 seconds to make element search to retry every 500 ms for at most five times
                session.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2.5);

                // Get focus 
                session.SwitchTo().Window(session.CurrentWindowHandle);
                
                ResetDynamoActions();
                SetDynamoInteractions();
            }
        }

        protected static void SetDynamoSessionConfiguration()
        {
            if (windowsDriverSessionConfiguration == null)
            {
                windowsDriverSessionConfiguration = ConfigurationHelper.GetConfiguration().GetSection("Configuration").Get<ConfigurationDTO>();
            }
        }

        protected static AppiumOptions GetDynamoOptions()
        {
            AppiumOptions options = new AppiumOptions();
            options.AddAdditionalCapability("deviceName", windowsDriverSessionConfiguration.AppiumOptions.DeviceName);
            options.AddAdditionalCapability("platformName", windowsDriverSessionConfiguration.AppiumOptions.PlatformName);
            options.AddAdditionalCapability("app", windowsDriverSessionConfiguration.AppId);
            return options;
        }

        private static AppiumOptions GetDesktopOptions()
        {
            AppiumOptions options = new AppiumOptions();
            options.AddAdditionalCapability("deviceName", windowsDriverSessionConfiguration.AppiumOptions.DeviceName);
            options.AddAdditionalCapability("platformName", windowsDriverSessionConfiguration.AppiumOptions.PlatformName);
            options.AddAdditionalCapability("app", windowsDriverSessionConfiguration.DesktopId);
            return options;
        }

        public static void TearDown()
        {
            // Close the application and delete the session
            if (desktopSession != null)
            {
                desktopSession.Dispose();
                desktopSession.Quit();
                desktopSession = null;
            }

            if (session != null)
            {
                interactions = null;
                CloseDynamo();
                session.Dispose();
                session.Quit();
                session = null;
            }
        }

        private static void CloseDynamo()
        {
            try
            {
                session.Close();
                DynamoSleep(TimeSpan.FromSeconds(1.5));
                string currentHandle = session.CurrentWindowHandle; // This should throw if the window is closed successfully
            }
            catch { }
        }

        private static void SetDynamoInteractions()
        {
            if (interactions == null)
            {
                interactions = new DynamoInteractions(session);
            }
        }

        internal static void ResetDynamoActions()
        {
            dynamoActions = new Actions(session);
        }

        internal static void ResetDesktopActions()
        {
            desktopActions = new Actions(desktopSession);
        }
      
        protected static void DynamoSleep(TimeSpan timeSpan)
        {
            Thread.Sleep(timeSpan);
        }

    }
}
