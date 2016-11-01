using Dynamo.Extensions;
using Dynamo.Wpf.Extensions;
using NUnit.Framework;
using System;
using System.Reflection;
using System.Windows;

namespace DynamoCoreWpfTests
{
    public class ViewExtensionTests : DynamoTestUIBase
    {
        private DummyViewExtension viewExtension = new DummyViewExtension();

        [Test]
        public void OnWorkspaceChangedExtensionIsNotified()
        {
            var extensionManager = View.viewExtensionManager;
            extensionManager.Add(viewExtension);
            
            RaiseLoadedEvent(View);

            // Open first file.
            Open(@"core\CustomNodes\add.dyf");
            // Open next file.
            Open(@"core\CustomNodes\bar.dyf");

            Assert.AreEqual(2, viewExtension.Counter);

        }

        private static void RaiseLoadedEvent(FrameworkElement element)
        {
            MethodInfo eventMethod = typeof(FrameworkElement).GetMethod("OnLoaded",
                BindingFlags.Instance | BindingFlags.NonPublic);

            RoutedEventArgs args = new RoutedEventArgs(FrameworkElement.LoadedEvent);

            eventMethod.Invoke(element, new object[] { args });
        }
    }

    public class DummyViewExtension : IViewExtension
    {
        public int Counter = 0;

        public string UniqueId
        {
            get { return "DummyID"; }
        }

        public string Name
        {
            get { return "DummyViewExtension"; }
        }

        public void Startup(StartupParams p)
        {

        }

        public void Loaded(ViewLoadedParams p)
        {
            p.CurrentWorkspaceChanged += (ws) =>
            {
                Counter++;
            };
        }

        public void Shutdown()
        {

        }

        public void Dispose()
        {

        }
    }
}
