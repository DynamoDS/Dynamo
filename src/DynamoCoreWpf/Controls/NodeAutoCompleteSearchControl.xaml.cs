using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Graph.Workspaces;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.ViewModels;
using Res = Dynamo.Wpf.Properties.Resources;

namespace Dynamo.UI.Controls
{
    /// <summary>
    /// Interaction logic for AutoCompleteSearchControl.xaml
    /// Notice this control shares a lot of logic with InCanvasSearchControl for now
    /// But they will diverge eventually because of UI improvements to auto complete.
    /// </summary>
    [Obsolete("This class will be removed in a future version of Dynamo")]
    public partial class NodeAutoCompleteSearchControl : IDisposable
    {
        private double currentX;
        private ListBoxItem HighlightedItem;
        private ListBoxItem currentListBoxItem;
        private static NodeAutoCompleteSearchControl _controlInstance;

        // Prepare the autocomplete window and reuse it whenever possible.
        // Only a single instance of the window will be allowed at any given time.
        static internal void PrepareAndShowNodeAutoCompleteSearch(Window window, NodeAutoCompleteSearchViewModel viewModel)
        {
            // A new window will be created (replacing any existing one) whenever a new viewModel is provided.
            // Each workspace gets its own viewModel—for example, when opening a custom node alongside the current workspace.
            if (_controlInstance is null || !ReferenceEquals(_controlInstance.ViewModel, viewModel))
            {
                _controlInstance?.ResetNodeAutoCompleteSearch();
                _controlInstance = new NodeAutoCompleteSearchControl(window, viewModel);
            }

            // When a window is already open, adjust its position to the target port without repeating the full event subscription setup.
            if (_controlInstance?.IsVisible is true)
            {
                Analytics.TrackEvent(Actions.Open, Categories.NodeAutoCompleteOperations);
                if (_controlInstance?.ViewModel?.PortViewModel != null)
                {
                    _controlInstance.ViewModel.PortViewModel.Highlight = Visibility.Collapsed;
                    _controlInstance.ViewModel.PortViewModel?.SetupNodeAutoCompleteWindowPlacement(_controlInstance);
                }
                                
                _controlInstance?.ViewModel?.PopulateAutoCompleteCandidates();
            }
            else
            {
                _controlInstance?.OnShowNodeAutoCompleteSearch();
            }
        }

        /// <summary>
        /// Node AutoComplete Search ViewModel DataContext
        /// </summary>
        [Obsolete("This method will be removed in a future version of Dynamo")]
        public NodeAutoCompleteSearchViewModel ViewModel => DataContext as NodeAutoCompleteSearchViewModel;

        [Obsolete("This method will be removed in a future version of Dynamo")]
        public NodeAutoCompleteSearchControl()
        {
            InitializeComponent();
        }

        private NodeAutoCompleteSearchControl(Window window, NodeAutoCompleteSearchViewModel viewModel)
        {
            Owner = window;
            DataContext = viewModel;
            InitializeComponent();
            SubscribeToAppEvents();
        }

        //Unsubscribe from events and destroy the node autocomplete window.
        private void ResetNodeAutoCompleteSearch()
        {
            UnsubscribeFromAppEvents();
            OnHideNodeAutoCompleteSearch();
            Close();
            _controlInstance = null;
        }

        private void OnMainAppClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ResetNodeAutoCompleteSearch();
        }

        private void OnWorkspaceClosed()
        {
            ResetNodeAutoCompleteSearch();
        }

        void OnWorkspaceRemoved(WorkspaceModel workspace)
        {
            if (ViewModel?.PortViewModel?.NodeViewModel?.WorkspaceViewModel?.Model?.Guid == workspace?.Guid)
            {
                ResetNodeAutoCompleteSearch();
            }
        }
        
        // When triggered, they will result in the autocomplete window being destroyed.
        private void SubscribeToAppEvents()
        {
            if (string.IsNullOrEmpty(DynamoModel.HostAnalyticsInfo.HostName) && Application.Current != null)
            {
                if (Application.Current?.MainWindow != null)
                {
                    Application.Current.MainWindow.Closing += OnMainAppClosing;
                }
            }

            HomeWorkspaceModel.WorkspaceClosed += OnWorkspaceClosed; //Only hits for main workspace (does not hit for custom nodes workspace).
            ViewModel.dynamoViewModel.Model.WorkspaceHidden += OnWorkspaceRemoved; //De-activating current workspace (including custom node workspace).
            ViewModel.dynamoViewModel.Model.WorkspaceRemoveStarted  += OnWorkspaceRemoved; //Closing custom node workspace.
        }


        private void UnsubscribeFromAppEvents()
        {
            if (string.IsNullOrEmpty(DynamoModel.HostAnalyticsInfo.HostName) && Application.Current != null)
            {
                if (Application.Current?.MainWindow != null)
                {
                    Application.Current.MainWindow.Closing -= OnMainAppClosing;
                }
            }

            HomeWorkspaceModel.WorkspaceClosed -= OnWorkspaceClosed;
            ViewModel.dynamoViewModel.Model.WorkspaceHidden -= OnWorkspaceRemoved;
            ViewModel.dynamoViewModel.Model.WorkspaceRemoveStarted -= OnWorkspaceRemoved;
        }

        //Hide the window and unsubscribe from model events.
        //Note that the window is not destroyed, it is just hidden so that it can be reused.
        internal void OnHideNodeAutoCompleteSearch()
        {            
            ViewModel.ParentNodeRemoved -= OnParentNodeRemoved;
            ViewModel.OnNodeAutoCompleteWindowClosed();
            Hide();
        }

        internal void OnShowNodeAutoCompleteSearch()
        {
            Analytics.TrackEvent(Actions.Open, Categories.NodeAutoCompleteOperations);

            if (ViewModel != null)
            {
                ViewModel.ParentNodeRemoved += OnParentNodeRemoved;
                ViewModel.PortViewModel?.SetupNodeAutoCompleteWindowPlacement(this);
                ViewModel.OnNodeAutoCompleteWindowOpened();
            }

            Show();

            // Visibility of textbox changed, but text box has not been initialized(rendered) yet.
            // Call asynchronously focus, when textbox will be ready.
            ViewModel.ResetAutoCompleteSearchViewState();
            Dispatcher.BeginInvoke(new Action(() =>
            {
                SearchTextBox?.Focus();
                ViewModel?.PopulateAutoCompleteCandidates();
            }), DispatcherPriority.Loaded);
        }


        private void OnSearchTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            BindingExpression binding = ((TextBox)sender).GetBindingExpression(TextBox.TextProperty);
            if (binding != null)
                binding.UpdateSource();

            // Search the filtered results to match the user input.
            if (ViewModel != null) 
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    ViewModel.SearchAutoCompleteCandidates(SearchTextBox.Text);
                }), DispatcherPriority.Loaded);
            }
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is ListBoxItem listBoxItem) || e.OriginalSource is Thumb) return;

            ExecuteSearchElement(listBoxItem);
            OnHideNodeAutoCompleteSearch();
            e.Handled = true;
        }

        private void ExecuteSearchElement(ListBoxItem listBoxItem)
        {
            var searchElement = listBoxItem.DataContext as NodeSearchElementViewModel;
            if (searchElement != null)
            {
                searchElement.Position = ViewModel.InCanvasSearchPosition;
                PortViewModel port = ViewModel.PortViewModel;
                if (searchElement.CreateAndConnectCommand.CanExecute(port.PortModel))
                {
                    searchElement.CreateAndConnectCommand.Execute(port.PortModel);
                    var selectedNodeName = (searchElement.Model is Search.SearchElements.ZeroTouchSearchElement) ?
                                                searchElement.Model.CreationName :
                                                // Same as NameTypeId.ToStrng() format
                                                string.Format("{0}, {1}", searchElement.Model.CreationName, searchElement.Assembly.Split('\\').Last().Split('.').First());
                    var originalNodeName = (port.NodeViewModel.NodeModel is DSFunctionBase) ?
                                                port.NodeViewModel.NodeModel.CreationName :
                                                string.Format("{0}, {1}", port.NodeViewModel.NodeModel.GetType().FullName, port.NodeViewModel.NodeModel.GetType().Assembly.GetName().Name) ;
                    var searchElementInfo = ViewModel.IsDisplayingMLRecommendation ?
                        selectedNodeName + " " + port.PortModel.Index.ToString() + " " + port.PortName + " " + originalNodeName + " " +
                        searchElement.Model.AutoCompletionNodeElementInfo.PortToConnect.ToString() + " " +
                        searchElement.AutoCompletionNodeMachineLearningInfo.ConfidenceScore.ToString() + " "  +  ViewModel.ServiceVersion
                        : selectedNodeName;

                    Analytics.TrackEvent(
                    Dynamo.Logging.Actions.Select,
                    Dynamo.Logging.Categories.NodeAutoCompleteOperations,
                    searchElementInfo);
                }
            }
        }        

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!(sender is FrameworkElement fromSender)) return;
            currentX = e.GetPosition(MembersListBox).X;

            DisplayOrHideConfidenceTooltip(sender);
        }

        private void DisplayOrHideConfidenceTooltip(object sender)
        {
            currentListBoxItem = sender as ListBoxItem;
            if (currentX <= 35)
            {
                confidenceToolTip.PlacementTarget = currentListBoxItem;
                confidenceToolTip.Placement = PlacementMode.Bottom;
                confidenceToolTip.IsOpen = true;
            }
            else
            {
                confidenceToolTip.IsOpen = false;
                confidenceToolTip.PlacementTarget = null;
            }
        }

        private void OnMouseEnter(object sender, MouseEventArgs e)
        {
            if (!(sender is FrameworkElement fromSender)) return;

            if (HighlightedItem != null)
            {
                HighlightedItem.IsSelected = false;
            }
            
            toolTipPopup.DataContext = fromSender.DataContext;
            toolTipPopup.IsOpen = true;
            confidenceToolTip.IsOpen = false;

            DisplayOrHideConfidenceTooltip(sender);

            dynamic currentNodeSearchElement = currentListBoxItem.DataContext;
            var scoreFormatter = new Dynamo.Controls.ConfidenceScoreFormattingConverter();
            var score = scoreFormatter.Convert(currentNodeSearchElement.AutoCompletionNodeMachineLearningInfo.ConfidenceScore, null, null, CultureInfo.CurrentCulture);
            confidenceScoreTitle.Text = $"{Res.ConfidenceToolTipTitle}: {score}%";
        }

        private void onCloseConfidenceToolTip(object sender, RoutedEventArgs e)
        {
            confidenceToolTip.IsOpen = false;
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (!(sender is FrameworkElement)) return;

            HighlightedItem.IsSelected = true;
            toolTipPopup.DataContext = null;
            toolTipPopup.IsOpen = false;
        }

        private void onConfidenceToolTipLearnMoreClicked(object sender, MouseButtonEventArgs e)
        {
            confidenceToolTip.IsOpen = false;
            ViewModel.dynamoViewModel.OpenDocumentationLinkCommand.Execute(new OpenDocumentationLinkEventArgs(new Uri(Res.NodeAutocompleteDocumentationUriString, UriKind.Relative)));
        }

        //Removes nodeautocomplete menu when the associated parent node is removed.
        private void OnParentNodeRemoved(NodeModel node)
        {
            NodeModel parent_node = ViewModel?.PortViewModel?.PortModel?.Owner;
            if (ReferenceEquals(node,parent_node))
            {
                OnHideNodeAutoCompleteSearch();
            }
        }

        private void OnMembersListBoxUpdated(object sender, DataTransferEventArgs e)
        {
            var membersListBox = sender as ListBox;
            // As soon as listbox renders, select first member.
            membersListBox.ItemContainerGenerator.StatusChanged += OnMembersListBoxIcgStatusChanged;
        }

        private void OnMembersListBoxIcgStatusChanged(object sender, EventArgs e)
        {
            if (MembersListBox.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
            {
                MembersListBox.ItemContainerGenerator.StatusChanged -= OnMembersListBoxIcgStatusChanged;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    var scrollViewer = MembersListBox.ChildOfType<ScrollViewer>();
                    scrollViewer.ScrollToTop();

                    UpdateHighlightedItem(GetListItemByIndex(MembersListBox, 0));
                }),
                    DispatcherPriority.Loaded);
            }
        }

        private void UpdateHighlightedItem(ListBoxItem newItem)
        {
            if (HighlightedItem == newItem)
                return;

            // Unselect old value.
            if (HighlightedItem != null)
                HighlightedItem.IsSelected = false;

            HighlightedItem = newItem;

            // Select new value.
            if (HighlightedItem != null)
            {
                HighlightedItem.IsSelected = true;
                HighlightedItem.BringIntoView();
            }
        }

        private ListBoxItem GetListItemByIndex(ListBox parent, int index)
        {
            if (parent.Equals(null)) return null;

            var generator = parent.ItemContainerGenerator;
            if ((index >= 0) && (index < parent.Items.Count))
                return generator.ContainerFromIndex(index) as ListBoxItem;

            return null;
        }

        private void OnInCanvasSearchKeyDown(object sender, KeyEventArgs e)
        {
            var key = e.Key;

            int index;
            var members = MembersListBox.Items.Cast<NodeSearchElementViewModel>();
            NodeSearchElementViewModel highlightedMember = null;
            if (HighlightedItem != null)
                highlightedMember = HighlightedItem.DataContext as NodeSearchElementViewModel;

            switch (key)
            {
                case Key.Escape:
                    OnHideNodeAutoCompleteSearch();
                    break;
                case Key.Enter:
                    if (HighlightedItem != null)
                    {
                        ExecuteSearchElement(HighlightedItem);
                        OnHideNodeAutoCompleteSearch();
                    }
                    break;
                case Key.Up:
                    index = MoveToNextMember(false, members, highlightedMember);
                    UpdateHighlightedItem(GetListItemByIndex(MembersListBox, index));
                    break;
                case Key.Down:
                    index = MoveToNextMember(true, members, highlightedMember);
                    UpdateHighlightedItem(GetListItemByIndex(MembersListBox, index));
                    break;
            }
        }

        internal int MoveToNextMember(bool moveForward,
            IEnumerable<NodeSearchElementViewModel> members, NodeSearchElementViewModel selectedMember)
        {
            int selectedMemberIndex = -1;
            for (int i = 0; i < members.Count(); i++)
            {
                var member = members.ElementAt(i);
                if (member.Equals(selectedMember))
                {
                    selectedMemberIndex = i;
                    break;
                }
            }

            int nextselectedMemberIndex = selectedMemberIndex;
            if (moveForward)
                nextselectedMemberIndex++;
            else
                nextselectedMemberIndex--;

            if (nextselectedMemberIndex < 0 || (nextselectedMemberIndex >= members.Count()))
                return selectedMemberIndex;

            return nextselectedMemberIndex;
        }

        private void OnMembersListBoxMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var listBox = sender as FrameworkElement;
            if (listBox == null)
                return;

            var scrollViewer = listBox.ChildOfType<ScrollViewer>();
            if (scrollViewer == null)
                return;

            // Make delta less to achieve smooth scrolling and not jump over other elements.
            var delta = e.Delta / 100;
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - delta);
            // do not propagate to child items with scrollable content
            e.Handled = true;
        }

        private void OnMoreInfoClicked(object sender, RoutedEventArgs e)
        {
            ViewModel.dynamoViewModel.OpenDocumentationLinkCommand.Execute(new OpenDocumentationLinkEventArgs(new Uri(Dynamo.Wpf.Properties.Resources.NodeAutocompleteDocumentationUriString, UriKind.Relative)));
        }

        internal void CloseAutoCompleteWindow(object sender, RoutedEventArgs e)
        {
            OnHideNodeAutoCompleteSearch();
        }

        /// <summary>
        /// A common method to handle the suggestions Button being clicked
        /// </summary>
        private void DisplaySuggestions(object sender, RoutedEventArgs e)
        {
            var cm = this.SuggestionsContextMenu;
            cm.PlacementTarget = sender as Button;
            cm.IsOpen = true;
        }

        private void ShowLowConfidenceResults(object sender, RoutedEventArgs e)
        {
            ViewModel.ShowLowConfidenceResults();
            //Tracking Analytics when the Low Confidence combobox (located in the Autocomplete popup)  is clicked
            Analytics.TrackEvent(
                    Actions.Expanded,
                    Categories.NodeAutoCompleteOperations,
                    "LowConfidenceResults",
                    ViewModel.dynamoViewModel.Model.PreferenceSettings.MLRecommendationConfidenceLevel);
        }

        private void OnSuggestion_Click(object sender, RoutedEventArgs e)
        {
            MenuItem selectedSuggestion = sender as MenuItem;
            if (selectedSuggestion.Name.Contains(nameof(Models.NodeAutocompleteSuggestion.MLRecommendation)))
            {
                if(ViewModel.IsMLAutocompleteTOUApproved)
                {
                    ViewModel.dynamoViewModel.PreferenceSettings.DefaultNodeAutocompleteSuggestion = Models.NodeAutocompleteSuggestion.MLRecommendation;
                    Analytics.TrackEvent(Actions.Switch, Categories.Preferences, nameof(NodeAutocompleteSuggestion.MLRecommendation));
                }
                else
                {
                    ViewModel.dynamoViewModel.MainGuideManager.CreateRealTimeInfoWindow(Res.NotificationToAgreeMLNodeautocompleteTOU);
                    // Do nothing for now, do not report analytics since the switch did not happen
                }
            }
            else
            {
                ViewModel.dynamoViewModel.PreferenceSettings.DefaultNodeAutocompleteSuggestion = Models.NodeAutocompleteSuggestion.ObjectType;
                Analytics.TrackEvent(Actions.Switch, Categories.Preferences, nameof(NodeAutocompleteSuggestion.ObjectType));
            }
            ViewModel.PopulateAutoCompleteCandidates();
        }

        /// <summary>
        /// Dispose the control
        /// </summary>
        [Obsolete("This method will be removed in a future version of Dynamo")]
        public void Dispose()
        {
            ResetNodeAutoCompleteSearch();
        }
    }
}
