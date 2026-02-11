using System;
using System.Windows;
using System.Windows.Controls;
using Dynamo.Models;
using Dynamo.ViewModels;

namespace Dynamo.UI.Controls
{
    /// <summary>
    /// Interaction logic for InPortContextMenu.xaml
    /// </summary>
    public partial class InPortContextMenu : UserControl
    {
        internal event Action<ShowHideFlags, PortViewModel> RequestShowPortContextMenu;

        public InPortContextMenu()
        {
            InitializeComponent();
            if (string.IsNullOrEmpty(DynamoModel.HostAnalyticsInfo.HostName) && Application.Current != null)
            {
                Application.Current.Deactivated += CurrentApplicationDeactivated;
            }
            Unloaded += InPortContextMenuControl_Unloaded;
        }

        /// <summary>
        /// Disposes of the event listener when the control is unloaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InPortContextMenuControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(DynamoModel.HostAnalyticsInfo.HostName) && Application.Current != null)
            {
                Application.Current.Deactivated -= CurrentApplicationDeactivated;
            }
            Unloaded -= InPortContextMenuControl_Unloaded;
        }
        
        /// <summary>
        /// Closes the popup when Dynamo is not the active application.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CurrentApplicationDeactivated(object sender, EventArgs e) => OnRequestShowPortContextMenu(ShowHideFlags.Hide);
        
        /// <summary>
        /// Requests to open the InPortContextMenu popup.
        /// </summary>
        /// <param name="flags"></param>
        private void OnRequestShowPortContextMenu(ShowHideFlags flags)
        {
            if (RequestShowPortContextMenu != null)
            {
                RequestShowPortContextMenu(flags, DataContext as PortViewModel);
            }
        }
    }
}
