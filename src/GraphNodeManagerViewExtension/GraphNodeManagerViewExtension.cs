using System;
using System.Windows;
using System.Windows.Controls;
using Dynamo.Logging;
using Dynamo.Wpf.Extensions;

namespace Dynamo.GraphNodeManager
{
    /// <summary>
    /// The GraphNodeManager is a visual layer that allows the user
    /// to quickly interrogate the current graph for information
    /// relevant to the Nodes and their States, Status and Issues
    /// </summary>
    public class GraphNodeManagerViewExtension : ViewExtensionBase, IViewExtension, ILogSource
    {
        #region Private Properties
        private ViewLoadedParams viewLoadedParamsReference;
        internal MenuItem graphNodeManagerMenuItem;

        public event Action<ILogMessage> MessageLogged;

        internal GraphNodeManagerViewModel ViewModel { get; private set; }
        internal GraphNodeManagerView ManagerView { get; private set; }
        #endregion

        #region Interface Implementation
        /// <summary>
        /// GUID of the extension
        /// </summary>
        public override string UniqueId => "F76F4274-537D-4782-B1E9-27E8FDE2186F";
        /// <summary>
        /// Extension Name
        /// </summary>
        public override string Name => Properties.Resources.ExtensionName;

        #endregion

        #region Add/Remove Extension 
        /// <summary>
        /// Add the View Extension to the list of Dynamo Extensions
        /// Will not initialize the ViewModel and View until checked
        /// </summary>
        /// <param name="viewLoadedParams"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public override void Loaded(ViewLoadedParams viewLoadedParams)
        {
            if (viewLoadedParams == null) throw new ArgumentNullException(nameof(viewLoadedParams));

            this.viewLoadedParamsReference = viewLoadedParams;
            
            // Add a button to Dynamo View menu to manually show the window
            this.graphNodeManagerMenuItem = new MenuItem { Header = Properties.Resources.MenuItemText, IsCheckable = true };
            this.graphNodeManagerMenuItem.Checked += MenuItemCheckHandler;
            this.graphNodeManagerMenuItem.Unchecked += MenuItemUnCheckedHandler;
            this.viewLoadedParamsReference.AddExtensionMenuItem(this.graphNodeManagerMenuItem);
        }

        /// <summary>
        ///  After user checks the Extension, add it to the side bar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemCheckHandler(object sender, RoutedEventArgs e)
        {
            AddToSidebar();
        }
        /// <summary>
        /// Close the Extension
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemUnCheckedHandler(object sender, RoutedEventArgs e)
        {
            this.Dispose();
            viewLoadedParamsReference.CloseExtensioninInSideBar(this);
        }

        /// <summary>
        /// Initialize the ViewModel and View
        /// </summary>
        private void AddToSidebar()
        {
            // initialise the ViewModel and View for the window
            this.ViewModel = new GraphNodeManagerViewModel(this.viewLoadedParamsReference, UniqueId, MessageLogged);
            this.ManagerView = new GraphNodeManagerView(this.ViewModel);

            if (this.ManagerView == null)
            {
                return;
            }

            this.ViewModel.GraphNodeManagerView = this.ManagerView;

            this.viewLoadedParamsReference?.AddToExtensionsSideBar(this, this.ManagerView);
        }

        public override void ReOpen()
        {
            AddToSidebar();
            this.graphNodeManagerMenuItem.IsChecked = true;
        }
        #endregion

        #region Dispose
        /// <summary>
        /// Tear down function
        /// </summary>
        public void Shutdown()
        {
            this.Dispose();
        }
        /// <summary>
        /// This model dispose method
        /// </summary>
        public override void Dispose()
        {
            this.ViewModel?.Dispose();
            this.ManagerView = null;
            this.ViewModel = null;
        }
        /// <summary>
        /// Uncheck if I get closed
        /// </summary>
        public override void Closed()
        {
            if (this.graphNodeManagerMenuItem != null)
            {
                this.graphNodeManagerMenuItem.IsChecked = false;
            }
        }
        #endregion
    }
}
