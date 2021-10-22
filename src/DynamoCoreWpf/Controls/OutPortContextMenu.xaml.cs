using System;
using System.Windows;
using System.Windows.Controls;
using Dynamo.ViewModels;

namespace Dynamo.UI.Controls
{
    /// <summary>
    /// Interaction logic for InPortContextMenu.xaml
    /// </summary>
    public partial class OutPortContextMenu : UserControl
    {
        internal event Action<ShowHideFlags> RequestShowOutPortContextMenu;

        public OutPortContextMenu()
        {
            InitializeComponent();

            if (Application.Current != null) Application.Current.Deactivated += CurrentApplicationDeactivated;
            Unloaded += OutPortContextMenuControl_Unloaded;
        }

        /// <summary>
        /// Disposes of the event listener when the control is unloaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OutPortContextMenuControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (Application.Current != null)
            {
                Application.Current.Deactivated -= CurrentApplicationDeactivated;
            }
        }

        /// <summary>
        /// Closes the popup when Dynamo is not the active application.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CurrentApplicationDeactivated(object sender, EventArgs e) => OnRequestShowOutPortContextMenu(ShowHideFlags.Hide);

        /// <summary>
        /// Requests to open the OutPortContextMenu popup.
        /// </summary>
        /// <param name="flags"></param>
        private void OnRequestShowOutPortContextMenu(ShowHideFlags flags)
        {
            if (RequestShowOutPortContextMenu != null)
            {
                RequestShowOutPortContextMenu(flags);
            }
        }
    }
}
