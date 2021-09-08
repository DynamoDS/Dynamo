using Dynamo.Extensions;
using Dynamo.Wpf.Extensions;
using System.Windows;
using System.Windows.Controls;

namespace Dynamo.DebugModes
{
    public class DebugModesViewExtension : IViewExtension
    {
        /// <summary>
        /// Extension Name
        /// </summary>
        public string Name
        {
            get
            {
                return "Debug Modes ViewExtension";
            }
        }

        /// <summary>
        /// GUID of the extension
        /// </summary>
        public string UniqueId
        {
            get
            {
                return "A6706BF5-11C2-458F-B7C7-B745A77EF7FD";
            }
        }

        private ReadyParams ReadyParams;

        /// <summary>
        /// Dispose function after extension is closed
        /// </summary>
        public void Dispose()
        {
        }

        public void Ready(ReadyParams readyParams)
        {
            ReadyParams = readyParams;
        }

        public void Shutdown()
        {
            this.Dispose();
        }

        public void Startup(ViewStartupParams viewLoadedParams)
        {

        }

        private MenuItem debugModesMenuItem;
        private DebugModesWindow debugModesWindow;

        public void Loaded(ViewLoadedParams viewLoadedParams)
        {
            debugModesWindow = new DebugModesWindow();
            debugModesWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            debugModesWindow.Owner = viewLoadedParams.DynamoWindow;

            // Adding a button in view menu to refresh and show manually
            debugModesMenuItem = new MenuItem { Header = "Debug Modes" };
            debugModesMenuItem.Click += (sender, args) =>
            {
                viewLoadedParams.AddToExtensionsSideBar(this, debugModesWindow);
            };
            viewLoadedParams.AddMenuItem(MenuBarType.View, debugModesMenuItem);
        }
    }
}
