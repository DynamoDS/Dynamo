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

        private void OutPortContextMenu_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext is HomeWorkspaceViewModel homeWorkspaceViewModel)
            {
                OutPortViewModel = homeWorkspaceViewModel.OutPortViewModel;
            }
        }

        private void BreakConnectionsButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (OutPortViewModel == null) return;
            OutPortViewModel.BreakConnectionsCommand.Execute(null);
        }

        private void ShowHideWiresButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (OutPortViewModel == null) return;
            OutPortViewModel.HideConnectionsCommand.Execute(null);
        }
    }
}
