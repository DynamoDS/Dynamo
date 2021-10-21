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
    public partial class InPortContextMenu : UserControl
    {
        internal event Action<ShowHideFlags> RequestShowInPortContextMenu;

        /// <summary>
        /// A reference to the PortViewModel which the user is interacting with.
        /// </summary>
        public InPortViewModel InPortViewModel { get; set; }

        public InPortContextMenu()
        {
            InitializeComponent();

            if (Application.Current != null) Application.Current.Deactivated += CurrentApplicationDeactivated;
            Unloaded += InPortContextMenuControl_Unloaded;
        }
        private void InPortContextMenu_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext is HomeWorkspaceViewModel homeWorkspaceViewModel)
            {
                InPortViewModel = homeWorkspaceViewModel.InPortViewModel;
            }
        }

        private void InPortContextMenuControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (Application.Current != null)
            {
                Application.Current.Deactivated -= CurrentApplicationDeactivated;
            }
        }
        private void CurrentApplicationDeactivated(object sender, EventArgs e) => OnRequestShowInPortContextMenu(ShowHideFlags.Hide);
        private void OnRequestShowInPortContextMenu(ShowHideFlags flags)
        {
            if (RequestShowInPortContextMenu != null)
            {
                RequestShowInPortContextMenu(flags);
            }
        }
        
        private void UseLevel_OnChecked(object sender, RoutedEventArgs e)
        {
            if (InPortViewModel == null) return;
            InPortViewModel.UseLevelsCommand.Execute(true);
        }
        private void UseLevel_OnUnchecked(object sender, RoutedEventArgs e)
        {
            if (InPortViewModel == null) return;
            InPortViewModel.UseLevelsCommand.Execute(false);
        }
        private void KeepListStructure_OnChecked(object sender, RoutedEventArgs e)
        {
            if (InPortViewModel == null) return;
            InPortViewModel.KeepListStructureCommand.Execute(true);
        }
        private void KeepListStructure_OnUnchecked(object sender, RoutedEventArgs e)
        {
            if (InPortViewModel == null) return;
            InPortViewModel.KeepListStructureCommand.Execute(false);
        }
        private void UseDefaultValue_OnChecked(object sender, RoutedEventArgs e)
        {
            if (InPortViewModel == null) return;
            InPortViewModel.UsingDefaultValue = true;
        }
        private void UseDefaultValue_OnUnchecked(object sender, RoutedEventArgs e)
        {
            if (InPortViewModel == null) return;
            InPortViewModel.UsingDefaultValue = false;
        }
    }
}
