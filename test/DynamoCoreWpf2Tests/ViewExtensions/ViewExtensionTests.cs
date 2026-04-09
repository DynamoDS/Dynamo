using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Dynamo.Engine;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.Wpf.Extensions;
using Dynamo.Wpf.UI.GuidedTour;
using DynamoCoreWpfTests.Utility;
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
            this.View.WindowState = WindowState.Maximized;
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

            var initialNum = ViewModel.SideBarTabItems.Count;

            // Adding the first extension will add a tab in the extensions side bar
            extensionManager.Add(viewExtension);
            Assert.AreEqual(initialNum + 1, ViewModel.SideBarTabItems.Count);

            // Adding the second extension will add another tab in the extensions side bar
            extensionManager.Add(extensionsSideBarViewExtension);
            Assert.AreEqual(initialNum + 2, ViewModel.SideBarTabItems.Count);

            // Setting a different unique ID so as to add the extension to the extension manager. 
            // But since that extension is already added to the side bar, it won't be added again. 
            extensionsSideBarViewExtensionNew.UniqueId = "ExtensionsSideBarDummyIDNew";
            extensionManager.Add(extensionsSideBarViewExtensionNew);
            Assert.AreEqual(initialNum + 2, ViewModel.SideBarTabItems.Count); 
        }

        [Test]
        public void CloseViewExtensionTest()
        {
            RaiseLoadedEvent(this.View);

            var extensionManager = View.viewExtensionManager;

            var initialNum = ViewModel.SideBarTabItems.Count;

            // Adding a dummy extension will add a new tab in the extensions side bar
            extensionManager.Add(viewExtension);
            Assert.AreEqual(initialNum + 1, ViewModel.SideBarTabItems.Count);

            var loadedParams = new ViewLoadedParams(View, ViewModel);

            // Closing the view extension using the CloseExtensioninInSideBar API should close the view extension.
            loadedParams.CloseExtensioninInSideBar(this.viewExtension);
            Assert.AreEqual(initialNum, ViewModel.SideBarTabItems.Count);
        }

        [Test]
        public void ExtensionSideBarIsUncollapsedOnActivation()
        {
            RaiseLoadedEvent(this.View);

            // Add extension
            View.viewExtensionManager.Add(viewExtension);
            
            // Extension bar is shown
            Assert.IsFalse(View.ExtensionsCollapsed);

            // Collapse extension bar
            View.ToggleExtensionBarCollapseStatus();
            Assert.IsTrue(View.ExtensionsCollapsed);

            // Simulate the extension activating the tab content again.
            // The content here should not matter because it won't be used.
            View.AddOrFocusExtensionControl(viewExtension, new UserControl());

            // Extension bar is shown
            Assert.IsFalse(View.ExtensionsCollapsed);
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
            Assert.IsNull(loader.Load(Path.Combine(GetTestDirectory(ExecutingDirectory), @"DynamoCoreWpf2Tests\ViewExtensions\Sample Manifests\Sample_ViewExtensionDefinition.xml")));
        }

        [Test]
        public void ExtensionUndockRedock()
        {
            RaiseLoadedEvent(this.View);

            // Add extension
            View.viewExtensionManager.Add(viewExtension);

            // Extension bar is shown
            Assert.AreEqual(1, ViewModel.SideBarTabItems.Count);
            Assert.IsFalse(View.ExtensionsCollapsed);

            // The content is in the extension tab
            var content = ViewModel.SideBarTabItems[0].Content as TextBlock;
            Assert.IsNotNull(content);
            Assert.AreEqual("Dummy", content.Text);

            // Undock extension 
            View.UndockExtension(viewExtension.Name);

            // Extension is no longer in the side bar (now collapsed)
            Assert.AreEqual(0, ViewModel.SideBarTabItems.Count);
            Assert.IsTrue(View.ExtensionsCollapsed);
            // Extension is in a window now
            Assert.AreEqual(1, View.ExtensionWindows.Count);

            // The content is in the extension window
            content = View.ExtensionWindows[viewExtension.Name].ExtensionContent.Content as TextBlock;
            Assert.IsNotNull(content);
            Assert.AreEqual("Dummy", content.Text);

            // Dock the window
            var window = View.ExtensionWindows[viewExtension.Name];
            window.DockRequested = true;
            window.Close();

            // Extension is in the sidebar again and the window is gone
            Assert.AreEqual(1, ViewModel.SideBarTabItems.Count);
            Assert.IsFalse(View.ExtensionsCollapsed);
            Assert.AreEqual(0, View.ExtensionWindows.Count);

            // The content is in the extension tab
            content = ViewModel.SideBarTabItems[0].Content as TextBlock;
            Assert.IsNotNull(content);
            Assert.AreEqual("Dummy", content.Text);
        }

        [Test]
        public void ExtensionUndockClose()
        {
            RaiseLoadedEvent(this.View);

            // Add extension
            View.viewExtensionManager.Add(viewExtension);

            // Extension bar is shown
            Assert.AreEqual(1, ViewModel.SideBarTabItems.Count);
            Assert.IsFalse(View.ExtensionsCollapsed);

            // The content is in the extension tab
            var content = ViewModel.SideBarTabItems[0].Content as TextBlock;
            Assert.IsNotNull(content);
            Assert.AreEqual("Dummy", content.Text);

            // Undock extension 
            View.UndockExtension(viewExtension.Name);

            // Extension is no longer in the side bar (now collapsed)
            Assert.AreEqual(0, ViewModel.SideBarTabItems.Count);
            Assert.IsTrue(View.ExtensionsCollapsed);
            // Extension is in a window now
            Assert.AreEqual(1, View.ExtensionWindows.Count);

            // The content is in the extension window
            content = View.ExtensionWindows[viewExtension.Name].ExtensionContent.Content as TextBlock;
            Assert.IsNotNull(content);
            Assert.AreEqual("Dummy", content.Text);

            // Close the window without docking
            var window = View.ExtensionWindows[viewExtension.Name];
            window.Close();

            // Extension is not in the sidebar nor as a window
            Assert.AreEqual(0, ViewModel.SideBarTabItems.Count);
            Assert.IsTrue(View.ExtensionsCollapsed);
            Assert.AreEqual(0, View.ExtensionWindows.Count);
        }

        [Test]
        public void ExtensionDockAndUndockWithRandomGUID()
        {
            RaiseLoadedEvent(this.View);

            // Add extension, default state is docked immediately
            View.viewExtensionManager.Add(extensionsSideBarViewExtension);

            // Extension bar is shown
            Assert.AreEqual(1, ViewModel.SideBarTabItems.Count);
            Assert.IsFalse(View.ExtensionsCollapsed);

            // Undock extension which return a new UniqueId, this should not crash Dynamo
            extensionsSideBarViewExtension.UniqueId = "NewRandomGuid1";
            Assert.DoesNotThrow(() => View.UndockExtension(extensionsSideBarViewExtension.Name));
        }

        [Test]
        public void ExtensionCannotBeAddedAsBothWindowAndTab()
        {
            RaiseLoadedEvent(this.View);

            // Add extension
            View.viewExtensionManager.Add(viewExtension);

            // Extension bar is shown
            Assert.AreEqual(1, ViewModel.SideBarTabItems.Count);
            Assert.IsFalse(View.ExtensionsCollapsed);

            // Undock extension 
            View.UndockExtension(viewExtension.Name);

            // Extension is no longer in the side bar (now collapsed)
            Assert.AreEqual(0, ViewModel.SideBarTabItems.Count);
            Assert.IsTrue(View.ExtensionsCollapsed);
            // Extension is in a window now
            Assert.AreEqual(1, View.ExtensionWindows.Count);

            // Attempt to open the extension in the side bar when it's open as a window
            View.AddOrFocusExtensionControl(viewExtension, viewExtension.Content);

            // Extension is not added to the sidebar
            Assert.AreEqual(0, ViewModel.SideBarTabItems.Count);
            Assert.IsTrue(View.ExtensionsCollapsed);
        }

        [Test]
        public void ExtensionLocationIsRemembered()
        {
            RaiseLoadedEvent(this.View);

            // Add extension
            View.viewExtensionManager.Add(viewExtension);

            // Extension bar is shown
            Assert.AreEqual(1, ViewModel.SideBarTabItems.Count);
            Assert.IsFalse(View.ExtensionsCollapsed);

            // Undock extension 
            View.UndockExtension(viewExtension.Name);

            // Extension is no longer in the side bar (now collapsed)
            Assert.AreEqual(0, ViewModel.SideBarTabItems.Count);
            Assert.IsTrue(View.ExtensionsCollapsed);
            // Extension is in a window now
            Assert.AreEqual(1, View.ExtensionWindows.Count);

            // Close the window without docking
            var window = View.ExtensionWindows[viewExtension.Name];
            window.Close();

            // Extension is not in the sidebar nor as a window
            Assert.AreEqual(0, ViewModel.SideBarTabItems.Count);
            Assert.IsTrue(View.ExtensionsCollapsed);
            Assert.AreEqual(0, View.ExtensionWindows.Count);

            // Re-open the extension
            View.AddOrFocusExtensionControl(viewExtension, viewExtension.Content);

            // Extension is remembered to be opened as a window
            Assert.AreEqual(0, ViewModel.SideBarTabItems.Count);
            Assert.IsTrue(View.ExtensionsCollapsed);
            Assert.AreEqual(1, View.ExtensionWindows.Count);

            // Dock the extension to the sidebar
            window = View.ExtensionWindows[viewExtension.Name];
            window.DockRequested = true;
            window.Close();

            // Extension is in the sidebar now
            Assert.AreEqual(1, ViewModel.SideBarTabItems.Count);
            Assert.IsFalse(View.ExtensionsCollapsed);
            Assert.AreEqual(0, View.ExtensionWindows.Count);

            // Close the extension tab in the sidebar
            View.CloseExtensionControl(viewExtension);

            // Extension is closed
            Assert.AreEqual(0, ViewModel.SideBarTabItems.Count);
            Assert.IsTrue(View.ExtensionsCollapsed);
            Assert.AreEqual(0, View.ExtensionWindows.Count);

            // Re-open the extension again
            View.AddOrFocusExtensionControl(viewExtension, viewExtension.Content);

            // Extension is remembered to be opened in the sidebar
            Assert.AreEqual(1, ViewModel.SideBarTabItems.Count);
            Assert.IsFalse(View.ExtensionsCollapsed);
            Assert.AreEqual(0, View.ExtensionWindows.Count);
        }

        public static void RaiseLoadedEvent(FrameworkElement element)
        {
            MethodInfo eventMethod = typeof(FrameworkElement).GetMethod("OnLoaded",
                BindingFlags.Instance | BindingFlags.NonPublic);

            RoutedEventArgs args = new RoutedEventArgs(FrameworkElement.LoadedEvent);

            eventMethod.Invoke(element, new object[] { args });
        }

        /// <summary>
        /// Test if the number of nodes displayed in the extension is equal to current number of nodes
        /// </summary>
        [Test]
        public void TestGraphEvaluationEvents()
        {
            RaiseLoadedEvent(this.View);
            var extensionManager = View.viewExtensionManager;
            extensionManager.Add(viewExtension);

            EngineController controller = null;
            int counter = 0;

            void startedEvent(HomeWorkspaceModel hwm, EventArgs args)
            {
                counter++;
                Assert.IsTrue(hwm.GraphRunInProgress);

                Assert.IsNotNull(hwm.EngineController);
                Assert.IsFalse(hwm.EngineController.IsDisposed);

                controller = hwm.EngineController;
                // Test that we do not get into an infinite loop
                hwm.RequestRun();
            };

            void finishedEvent(HomeWorkspaceModel hwm, EventArgs args)
            {
                counter++;

                Assert.IsFalse(hwm.GraphRunInProgress);
                Assert.AreEqual(controller, hwm.EngineController);
            };

            viewExtension.OnOpenEvaluationStarted += startedEvent;
            viewExtension.OnOpenEvaluationEnded += finishedEvent;

            // Open first file.
            Open(@"core\node2code\numberRange.dyn");

            Assert.AreEqual(2, counter);

            viewExtension.OnOpenEvaluationStarted -= startedEvent;
            viewExtension.OnOpenEvaluationEnded -= finishedEvent;
        }


        [Test]
        public void LaunchTourClosesSidePanelViewExtensions()
        {
            var initialTabsOpen = ViewModel.SideBarTabItems.OfType<TabItem>()
                .Count(tab => tab.Tag is IViewExtension);

            var dockedExtension = new GuidedTourSidePanelTestViewExtension();
            var added = View.AddOrFocusExtensionControl(dockedExtension, new UserControl());
            Assert.IsTrue(added);

            var hasAddedExtensionTab = ViewModel.SideBarTabItems.OfType<TabItem>().Any(tab =>
                tab.Tag is IViewExtension extension &&
                extension.UniqueId == dockedExtension.UniqueId);
            Assert.IsTrue(hasAddedExtensionTab);

            var guidesManager = new GuidesManager(View, ViewModel);
            Assert.DoesNotThrow(() => guidesManager.CloseAllViewExtensions(View));

            hasAddedExtensionTab = ViewModel.SideBarTabItems.OfType<TabItem>().Any(tab =>
                tab.Tag is IViewExtension extension &&
                extension.UniqueId == dockedExtension.UniqueId);
            Assert.IsFalse(hasAddedExtensionTab);

            var finalTabsOpen = ViewModel.SideBarTabItems.OfType<TabItem>()
                .Count(tab => tab.Tag is IViewExtension);
            Assert.LessOrEqual(finalTabsOpen, initialTabsOpen);
        }

        [Test]
        public void CloseAllViewExtensions_ClosesOwnerOwnedExtensionWindows()
        {
            var extension = new GuidedTourSidePanelTestViewExtension();
            View.viewExtensionManager.Add(extension);

            Window ownerOwnedWindow = null;
            try
            {
                ownerOwnedWindow = new Window
                {
                    Owner = View,
                    Tag = extension,
                    Title = "OwnerOwnedExtensionWindow",
                    Content = new TextBlock { Text = "OwnerOwnedExtensionWindow" }
                };
                ownerOwnedWindow.Show();
                Assert.IsTrue(ownerOwnedWindow.IsVisible);

                var guidesManager = new GuidesManager(View, ViewModel);
                Assert.DoesNotThrow(() => guidesManager.CloseAllViewExtensions(View));
                DispatcherUtil.DoEvents();

                Assert.IsFalse(ownerOwnedWindow.IsVisible);
            }
            finally
            {
                if (ownerOwnedWindow != null && ownerOwnedWindow.IsVisible)
                {
                    ownerOwnedWindow.Close();
                }
            }
        }
    }

    public class DummyViewExtension : IViewExtension
    {
        public int Counter { get; private set; }
        public HomeWorkspaceModel CurrentHWM;
        public ViewLoadedParams Params;
        public bool SetOwner { get; set; } = true;
        public bool WindowClosed { get; private set; }
        public Window Content { get; private set; }

        public event Action<HomeWorkspaceModel, EventArgs> OnOpenEvaluationStarted;
        public event Action<HomeWorkspaceModel, EventArgs> OnOpenEvaluationEnded;

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

        private void OnEvalStarted(object sender, EventArgs args)
        {
            OnOpenEvaluationStarted?.Invoke(Params.CurrentWorkspaceModel as HomeWorkspaceModel, args);
        }

        private void OnEvalEnded(object sender, EvaluationCompletedEventArgs args)
        {
            OnOpenEvaluationEnded?.Invoke(Params.CurrentWorkspaceModel as HomeWorkspaceModel, args);
        }

        private void workpaceChangedHandler(IWorkspaceModel ws) {
            if (ws is HomeWorkspaceModel hwm)
            {
                hwm.EvaluationStarted += OnEvalStarted;
                hwm.EvaluationCompleted += OnEvalEnded;
            }
            
            Counter++;
        }

        public void Loaded(ViewLoadedParams p)
        { 
            Params = p;
            Params.CurrentWorkspaceChanged += workpaceChangedHandler;

            var window = new Window();
            window.Content = new TextBlock() { Text = "Dummy" };
            window.Closed += (sender, args) =>
            {
                WindowClosed = true;
            };
            if (SetOwner)
            {
                window.Owner = p.DynamoWindow;
            }
            Content = window;

            p.AddToExtensionsSideBar(this, window);
        }

        public void Shutdown()
        {

        }

        public void Dispose()
        {
            if (Params != null)
            {
                if (Params.CurrentWorkspaceModel is HomeWorkspaceModel hwm)
                {
                    hwm.EvaluationStarted -= OnEvalStarted;
                    hwm.EvaluationCompleted -= OnEvalEnded;
                }

                Params.CurrentWorkspaceChanged -= workpaceChangedHandler;
            }
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

    internal class GuidedTourSidePanelTestViewExtension : IViewExtension
    {
        public string UniqueId { get; } = Guid.NewGuid().ToString("N");
        public string Name => $"GuidedTourSidePanelTestViewExtension_{UniqueId}";

        public void Startup(ViewStartupParams viewStartupParams)
        {
        }

        public void Loaded(ViewLoadedParams p)
        {
        }

        public void Shutdown()
        {
        }

        public void Dispose()
        {
        }
    }
}
