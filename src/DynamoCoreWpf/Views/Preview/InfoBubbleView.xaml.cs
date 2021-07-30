using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Dynamo.Configuration;
using Dynamo.UI;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.UI;
using Dynamo.Wpf.Utilities;
using ProtoCore;
using InfoBubbleViewModel = Dynamo.ViewModels.InfoBubbleViewModel;

namespace Dynamo.Controls
{
    /// <summary>
    /// Interaction logic for PreviewInfoBubble.xaml
    /// </summary>
    public partial class InfoBubbleView : UserControl
    {
        #region Properties
        private InfoBubbleViewModel viewModel;

        public InfoBubbleViewModel ViewModel
        {
            get { return viewModel; }
            set
            {
                if (viewModel == null)
                {
                    viewModel = value;
                    viewModel.PropertyChanged += ViewModel_PropertyChanged;
                    viewModel.RequestAction += InfoBubbleRequestAction;
                }
            }
        }
        
        // When a NodeModel is removed, WPF places the NodeView into a "disconnected"
        // state (i.e. NodeView.DataContext becomes "DisconnectedItem") before 
        // eventually removing the view. This is the result of the host canvas being 
        // virtualized. This property is used by InfoBubbleView to determine if it should 
        // still continue to access the InfoBubbleViewModel that it is bound to.
        private bool IsDisconnected { get { return (this.ViewModel == null); } }

        #endregion
        
        /// <summary>
        /// Used to present useful/important information to user when the node is in Error or Warning state.
        /// </summary>
        public InfoBubbleView()
        {
            InitializeComponent();
            this.DataContextChanged += InfoBubbleView_DataContextChanged;
        }
        
        private void InfoBubbleWindowUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                switch (ViewModel.InfoBubbleState)
                {
                    case InfoBubbleViewModel.State.Minimized:
                        mainGrid.Visibility = Visibility.Collapsed;
                        mainGrid.Opacity = 0;
                        break;
                    case InfoBubbleViewModel.State.Pinned:
                        mainGrid.Visibility = Visibility.Visible;
                        mainGrid.Opacity = Configurations.MaxOpacity;
                       UpdatePosition();
                        break;
                }
            }
        }
        
        private void InfoBubbleView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ViewModel = e.NewValue as InfoBubbleViewModel;
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // The fix for the following issue was previously performed in 
            // NodeModel. This is shifted over to infobubble to centralize 
            // the issue until code restructuring is completed.
            //
            // This is a temporarily measure, it work by dispatching the 
            // work to UI thread when info bubble UI values need to be 
            // modified by background evaluation thread.
            // To completely solve this, changes affecting UI values should be 
            // restructured into UI Binding in order for things to be thread 
            // safe. 
            // The above mentioned issue is being documented in:
            //
            //      http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-847
            //
            Action propertyChanged = (() =>
            {
                switch (e.PropertyName)
                {
                    case "Content":
                        UpdatePosition();
                        break;

                    case "TargetTopLeft":
                    case "TargetBotRight":
                        UpdatePosition();
                        break;

                    case "ConnectingDirection":
                        UpdatePosition();
                        break;

                    case "InfoBubbleState":
                        UpdatePosition();
                        HandleInfoBubbleStateChanged(ViewModel.InfoBubbleState);
                        break;

                    case "InfoBubbleStyle":
                        break;
                }
            });

            if (this.ViewModel.DynamoViewModel.UIDispatcher != null &&
                this.ViewModel.DynamoViewModel.UIDispatcher != null)
            {
                if (this.ViewModel.DynamoViewModel.UIDispatcher.CheckAccess())
                    propertyChanged();
                else
                    this.ViewModel.DynamoViewModel.UIDispatcher.BeginInvoke(propertyChanged);
            }
        }
       
        #region Update Position

        private void UpdatePosition()
        {
            nodeInformationalStateDockPanel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double estimatedHeight = nodeInformationalStateDockPanel.DesiredSize.Height;
            double estimatedWidth = nodeInformationalStateDockPanel.DesiredSize.Width;

            nodeInformationalStateDockPanel.Margin = GetMargin_Error(estimatedHeight, estimatedWidth);
        }

        private System.Windows.Thickness GetMargin_Error(double estimatedHeight, double estimatedWidth)
        {
            System.Windows.Thickness margin = new System.Windows.Thickness();
            double nodeWidth = ViewModel.TargetBotRight.X - ViewModel.TargetTopLeft.X;
            margin.Top = -(estimatedHeight) + ViewModel.TargetTopLeft.Y;
            margin.Left = -((estimatedWidth - nodeWidth) / 2) + ViewModel.TargetTopLeft.X;
            return margin;
        }

        #endregion
        
        private void InfoBubbleRequestAction(object sender, InfoBubbleEventArgs e)
        {
            switch (e.RequestType)
            {
                case InfoBubbleEventArgs.Request.Show:
                    ShowInfoBubble();
                    break;
                case InfoBubbleEventArgs.Request.Hide:
                    HideInfoBubble();
                    break;
            }
        }

        private void HandleInfoBubbleStateChanged(InfoBubbleViewModel.State state)
        {
            switch (state)
            {
                case InfoBubbleViewModel.State.Minimized:
                    // Changing to Minimized
                    this.HideInfoBubble();
                    break;

                case InfoBubbleViewModel.State.Pinned:
                    // Changing to Pinned
                    this.ShowInfoBubble();
                    break;
            }
        }

        private void ShowInfoBubble()
        {
            if (mainGrid.Visibility == Visibility.Collapsed)
            {
                mainGrid.Visibility = Visibility.Visible;
                // Run animation and skip it to end state i.e. MaxOpacity
            }
        }

        // Hide bubble instantly
        private void HideInfoBubble()
        {
            if (mainGrid.Visibility == Visibility.Visible)
            {
                mainGrid.Visibility = Visibility.Collapsed;
            }
        }
        
        private void ErrorsBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;
            if (ViewModel.NodeErrorsVisibilityState == InfoBubbleViewModel.NodeMessageVisibility.Icon)
            {
                ViewModel.NodeErrorsVisibilityState = InfoBubbleViewModel.NodeMessageVisibility.CollapseMessages;
            }
            else
            {
                ViewModel.NodeErrorsVisibilityState = InfoBubbleViewModel.NodeMessageVisibility.Icon;
            }
            ViewModel.NodeWarningsShowLessMessageVisible = false;
        }
        
        private void WarningsBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;
            if (ViewModel.NodeWarningsVisibilityState == InfoBubbleViewModel.NodeMessageVisibility.Icon)
            {
                ViewModel.NodeWarningsVisibilityState = InfoBubbleViewModel.NodeMessageVisibility.CollapseMessages;
            }
            else
            {
                ViewModel.NodeWarningsVisibilityState = InfoBubbleViewModel.NodeMessageVisibility.Icon;
            }
            ViewModel.NodeWarningsShowLessMessageVisible = false;
        }
        
        private void InfoBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;
            if (ViewModel.NodeInfoVisibilityState == InfoBubbleViewModel.NodeMessageVisibility.Icon)
            {
                ViewModel.NodeInfoVisibilityState = InfoBubbleViewModel.NodeMessageVisibility.CollapseMessages;
            }
            else
            {
                ViewModel.NodeInfoVisibilityState = InfoBubbleViewModel.NodeMessageVisibility.Icon;
            }
            ViewModel.NodeWarningsShowLessMessageVisible = false;
        }
        
        private void ShowAllErrorsButton_Click(object sender, RoutedEventArgs e)
        {
            // If we're already expanded, this button collapses the border
            if (ViewModel.NodeErrorsVisibilityState == InfoBubbleViewModel.NodeMessageVisibility.ShowAllMessages)
            {
                ViewModel.NodeErrorsVisibilityState = InfoBubbleViewModel.NodeMessageVisibility.CollapseMessages;
                ViewModel.NodeErrorsShowLessMessageVisible = false;
            }
            // Otherwise it expands the border
            else
            {
                ViewModel.NodeErrorsVisibilityState = InfoBubbleViewModel.NodeMessageVisibility.ShowAllMessages;
                ViewModel.NodeErrorsShowLessMessageVisible = true;
            }
        }

        private void ShowAllWarningsButton_Click(object sender, RoutedEventArgs e)
        {
            // If we're already expanded, this button collapses the border
            if (ViewModel.NodeWarningsVisibilityState == InfoBubbleViewModel.NodeMessageVisibility.ShowAllMessages)
            {
                ViewModel.NodeWarningsVisibilityState = InfoBubbleViewModel.NodeMessageVisibility.CollapseMessages;
                ViewModel.NodeWarningsShowLessMessageVisible = false;
            }
            // Otherwise it expands the border
            else
            {
                ViewModel.NodeWarningsVisibilityState = InfoBubbleViewModel.NodeMessageVisibility.ShowAllMessages;
                ViewModel.NodeWarningsShowLessMessageVisible = true;
            }
        }

        private void ShowAllInfoButton_Click(object sender, RoutedEventArgs e)
        {
            // If we're already expanded, this button collapses the border
            if (ViewModel.NodeInfoVisibilityState == InfoBubbleViewModel.NodeMessageVisibility.ShowAllMessages)
            {
                ViewModel.NodeInfoVisibilityState = InfoBubbleViewModel.NodeMessageVisibility.CollapseMessages;
                ViewModel.NodeInfoShowLessMessageVisible = false;
            }
            // Otherwise it expands the border
            else
            {
                ViewModel.NodeInfoVisibilityState = InfoBubbleViewModel.NodeMessageVisibility.ShowAllMessages;
                ViewModel.NodeInfoShowLessMessageVisible = true;
            }
        }
        
        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }
        
        private void DismissAllInfoButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (InfoBubbleDataPacket infoBubbleDataPacket in ViewModel.NodeInfoToDisplay)
            {
                ViewModel.DismissedMessages.Add(infoBubbleDataPacket);
            }
            ViewModel.RefreshNodeInformationalStateDisplay();
        }

        private void DismissAllWarningsButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (InfoBubbleDataPacket infoBubbleDataPacket in ViewModel.NodeWarningsToDisplay)
            {
                ViewModel.DismissedMessages.Add(infoBubbleDataPacket);
            }
            ViewModel.RefreshNodeInformationalStateDisplay();
        }

        private void DismissAllErrorsButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (InfoBubbleDataPacket infoBubbleDataPacket in ViewModel.NodeErrorsToDisplay)
            {
                ViewModel.DismissedMessages.Add(infoBubbleDataPacket);
            }
            ViewModel.RefreshNodeInformationalStateDisplay();
        }
    }
}