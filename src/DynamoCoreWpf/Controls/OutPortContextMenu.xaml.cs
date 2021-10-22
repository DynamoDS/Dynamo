using System;
using System.Windows;
using System.Windows.Controls;
using Dynamo.ViewModels;
using Dynamo.Wpf.ViewModels.Core;

namespace Dynamo.UI.Controls
{
    /// <summary>
    /// Interaction logic for InPortContextMenu.xaml
    /// </summary>
    public partial class OutPortContextMenu : UserControl
    {
        internal event Action<ShowHideFlags> RequestShowOutPortContextMenu;

        /// <summary>
        /// A reference to the PortViewModel which the user is interacting with.
        /// </summary>
        public OutPortViewModel OutPortViewModel { get; set; }

        public OutPortContextMenu()
        {
            InitializeComponent();

            if (Application.Current != null) Application.Current.Deactivated += CurrentApplicationDeactivated;
            Unloaded += OutPortContextMenuControl_Unloaded;
        }

        private void OutPortContextMenuControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (Application.Current != null)
            {
                Application.Current.Deactivated -= CurrentApplicationDeactivated;
            }
        }
        private void CurrentApplicationDeactivated(object sender, EventArgs e) => OnRequestShowOutPortContextMenu(ShowHideFlags.Hide);
        private void OnRequestShowOutPortContextMenu(ShowHideFlags flags)
        {
            if (RequestShowOutPortContextMenu != null)
            {
                RequestShowOutPortContextMenu(flags);
            }
        }
    }
}
