﻿using System.IO;
using System.Reflection;
using System.Windows;
using Dynamo.Wpf.Extensions;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    public class ViewExtensionTests : DynamoTestUIBase
    {
        private DummyViewExtension viewExtension = new DummyViewExtension();
        private ExtensionsSideBarViewExtension extensionsSideBarViewExtension = new ExtensionsSideBarViewExtension();
        private ExtensionsSideBarViewExtension extensionsSideBarViewExtensionNew = new ExtensionsSideBarViewExtension();

        [Test]
        public void OnWorkspaceChangedExtensionIsNotified()
        {
            RaiseLoadedEvent(this.View);
            var extensionManager = View.viewExtensionManager;
            extensionManager.Add(viewExtension);
           
            // Open first file.
            Open(@"core\CustomNodes\add.dyf");
            // Open next file.
            Open(@"core\CustomNodes\bar.dyf");

            Assert.AreEqual(2, viewExtension.Counter);
        }

        [Test]
        public void ExtensionWindowIsClosedWithDynamo()
        {
            var dummyExtension = new DummyViewExtension()
            {
                SetOwner = false
            };

            RaiseLoadedEvent(this.View);
            var extensionManager = View.viewExtensionManager;
            extensionManager.Add(dummyExtension);

            View.Close();

            Assert.IsTrue(dummyExtension.WindowClosed);
        }

        [Test]
        public void ExtensionsSideBarExtensionsTest()
        {
            RaiseLoadedEvent(this.View);

            var extensionManager = View.viewExtensionManager;

            var initialNum = View.ExtensionTabItems.Count;

            // Adding the first extension will add a tab in the extensions side bar
            extensionManager.Add(viewExtension);
            Assert.AreEqual(initialNum + 1, View.ExtensionTabItems.Count);

            // Adding the second extension will add another tab in the extensions side bar
            extensionManager.Add(extensionsSideBarViewExtension);
            Assert.AreEqual(initialNum + 2, View.ExtensionTabItems.Count);

            // Setting a different unique ID so as to add the extension to the extension manager. 
            // But since that extension is already added to the side bar, it won't be added again. 
            extensionsSideBarViewExtensionNew.UniqueId = "ExtensionsSideBarDummyIDNew";
            extensionManager.Add(extensionsSideBarViewExtensionNew);
            Assert.AreEqual(initialNum + 2, View.ExtensionTabItems.Count); 
        }

        [Test]
        public void ExtensionNotEnabledLoadingTest()
        {
            RaiseLoadedEvent(this.View);

            var extensionManager = View.viewExtensionManager;
            var loader = extensionManager.ExtensionLoader;
            var ext = new ViewExtensionDefinition();
            // By default, extensions are enabled.
            Assert.IsTrue(ext.IsEnabled);
            // Once we set it to false, loader will skip the loading and expect to return null.
            ext.IsEnabled = false;
            Assert.IsNull(loader.Load(ext));
        }

        [Test]
        public void ExtensionFromManifestNotEnabledLoadingTest()
        {
            RaiseLoadedEvent(this.View);

            var extensionManager = View.viewExtensionManager;
            var loader = extensionManager.ExtensionLoader;
            // Once IsEnabled set to false, loader will skip the loading and expect to return null.
            Assert.IsNull(loader.Load(Path.Combine(GetTestDirectory(ExecutingDirectory), @"DynamoCoreWpfTests\ViewExtensions\Sample Manifests\Sample_ViewExtensionDefinition.xml")));
        }

        public static void RaiseLoadedEvent(FrameworkElement element)
        {
            MethodInfo eventMethod = typeof(FrameworkElement).GetMethod("OnLoaded",
                BindingFlags.Instance | BindingFlags.NonPublic);

            RoutedEventArgs args = new RoutedEventArgs(FrameworkElement.LoadedEvent);

            eventMethod.Invoke(element, new object[] { args });
        }
    }

    public class DummyViewExtension : IViewExtension
    {
        public int Counter { get; set; }
        public bool SetOwner { get; set; } = true;
        public bool WindowClosed { get; set; }

        public string UniqueId
        {
            get { return "DummyID"; }
        }

        public string Name
        {
            get { return "DummyViewExtension"; }
        }

        public void Startup(ViewStartupParams viewStartupParams)
        {
            // Do nothing for now
        }

        public void Loaded(ViewLoadedParams p)
        {
            p.CurrentWorkspaceChanged += (ws) =>
            {
                Counter++;
            };

            var window = new Window();
            window.Closed += (sender, args) =>
            {
                WindowClosed = true;
            };
            if (SetOwner)
            {
                window.Owner = p.DynamoWindow;
            }

            p.AddToExtensionsSideBar(this, window);
        }

        public void Shutdown()
        {

        }

        public void Dispose()
        {

        }
    }

    // A sample view extension which will be added to the extensions side bar
    public class ExtensionsSideBarViewExtension : IViewExtension
    {
        public int Counter = 0;
        public string uID = "ExtensionsSideBarDummyID";

        public string UniqueId
        {
            set { uID = value; }
            get { return uID; }
        }

        public string Name
        {
            get { return "ExtensionsSideBarViewExtension"; }
        }

        public void Startup(ViewStartupParams viewStartupParams)
        {
            // Do nothing for now
        }

        public void Loaded(ViewLoadedParams p)
        {
            p.CurrentWorkspaceChanged += (ws) =>
            {
                Counter++;
            };

            var window = new Window
            {
                Owner = p.DynamoWindow
            };

            // Adding the extensiomn to the extensions side bar.
            p.AddToExtensionsSideBar(this,window);
        }

        public void Shutdown()
        {

        }

        public void Dispose()
        {

        }
    }
}
