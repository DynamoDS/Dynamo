using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Prompts;
using Dynamo.Selection;
using Dynamo.UI;
using Dynamo.UI.Prompts;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using System.Windows.Media;
using DynCmd = Dynamo.ViewModels.DynamoViewModel;
using System.Windows.Threading;
using Dynamo.Core;

namespace Dynamo.Controls
{
    public partial class dynNodeView : IViewModelView<NodeViewModel>
    {
        public delegate void SetToolTipDelegate(string message);
        public delegate void UpdateLayoutDelegate(FrameworkElement el);

        private NodeViewModel viewModel;

        public dynNodeView TopControl
        {
            get { return topControl; }
        }

        public Grid ContentGrid
        {
            get { return inputGrid; }
        }

        public NodeViewModel ViewModel
        {
            get { return viewModel; }
            private set { viewModel = value; }
        }

        private DispatcherTimer toolTipDelayTimer;

        #region constructors

        public dynNodeView()
        {
            this.Resources.MergedDictionaries.Add(SharedDictionaryManager.DynamoModernDictionary);
            this.Resources.MergedDictionaries.Add(SharedDictionaryManager.DynamoColorsAndBrushesDictionary);
            this.Resources.MergedDictionaries.Add(SharedDictionaryManager.DataTemplatesDictionary);
            this.Resources.MergedDictionaries.Add(SharedDictionaryManager.DynamoConvertersDictionary);
            this.Resources.MergedDictionaries.Add(SharedDictionaryManager.PortsDictionary);

            InitializeComponent();

            this.Loaded += new RoutedEventHandler(OnNodeViewLoaded);
            inputGrid.Loaded += new RoutedEventHandler(inputGrid_Loaded);

            this.SizeChanged += OnSizeChanged;
            this.DataContextChanged += OnDataContextChanged;
            
            Canvas.SetZIndex(this, 1);
        }

        #endregion

        /// <summary>
        /// Called when the size of the node changes. Communicates changes down to the view model 
        /// then to the model.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void OnSizeChanged(object sender, EventArgs eventArgs)
        {
            if (ViewModel != null)
            {
                var size = new double[] { ActualWidth, ActualHeight };
                if (ViewModel.SetModelSizeCommand.CanExecute(size))
                {
                    Debug.WriteLine(string.Format("Updating {2} node size {0}:{1}", size[0], size[1], ViewModel.NodeLogic.GetType().ToString()));
                    ViewModel.SetModelSizeCommand.Execute(size);
                }
            }
        }

        /// <summary>
        /// This event handler is called soon as the NodeViewModel is bound to this 
        /// dynNodeView, which happens way before OnNodeViewLoaded event is sent. 
        /// There is a known bug in WPF 4.0 where DataContext becomes DisconnectedItem 
        /// when actions such as tab switching happens (that is when the View becomes 
        /// disconnected from the underlying ViewModel/Model that it was bound to). So 
        /// it is more reliable for dynNodeView to cache the NodeViewModel it is bound 
        /// to when it first becomes available, and refer to the cached value at a later
        /// time.
        /// </summary>
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // If this is the first time dynNodeView is bound to the NodeViewModel, 
            // cache the DataContext (i.e. NodeViewModel) locally and start 
            // referecing it from this point onwards. Note that this notification 
            // can be sent as a result of DataContext becoming DisconnectedItem too,
            // but ViewModel should not be updated in that case (hence the null-check).
            // 
            if (null == this.ViewModel)
                this.ViewModel = e.NewValue as NodeViewModel;
        }

        private void OnNodeViewLoaded(object sender, RoutedEventArgs e)
        {
            // We no longer cache the DataContext (NodeViewModel) here because 
            // OnNodeViewLoaded gets called at a much later time and we need the 
            // ViewModel to be valid earlier (e.g. OnSizeChanged is called before
            // OnNodeViewLoaded, and it needs ViewModel for size computation).
            // 
            // ViewModel = this.DataContext as NodeViewModel
            ViewModel.NodeLogic.DispatchedToUI += new DispatchedToUIThreadHandler(NodeLogic_DispatchedToUI);
            ViewModel.RequestShowNodeHelp += ViewModel_RequestShowNodeHelp;
            ViewModel.RequestShowNodeRename += new EventHandler(ViewModel_RequestShowNodeRename);
            ViewModel.RequestsSelection += new EventHandler(ViewModel_RequestsSelection);

            ViewModel.NodeLogic.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(NodeLogic_PropertyChanged);
            
        }

        void NodeLogic_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ArgumentLacing":
                    ViewModel.SetLacingTypeCommand.RaiseCanExecuteChanged();
                    break;
            }
        }

        void ViewModel_RequestsSelection(object sender, EventArgs e)
        {
            if (!ViewModel.NodeLogic.IsSelected)
            {
                if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
                {
                    DynamoSelection.Instance.ClearSelection();
                }

                if (!DynamoSelection.Instance.Selection.Contains(ViewModel.NodeLogic))
                    DynamoSelection.Instance.Selection.Add(ViewModel.NodeLogic);
            }
            else
            {
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    DynamoSelection.Instance.Selection.Remove(ViewModel.NodeLogic);
                }
            }
        }

        void ViewModel_RequestShowNodeRename(object sender, EventArgs e)
        {
            var editWindow = new EditWindow
            {
                DataContext = ViewModel,
                Title = "Edit Node Name"
            };

            editWindow.Owner = Window.GetWindow(this);

            editWindow.BindToProperty(null, new Binding("NickName")
            {
                Mode = BindingMode.TwoWay,
                NotifyOnValidationError = false,
                Source = ViewModel,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            });

            editWindow.ShowDialog();
        }

        void ViewModel_RequestShowNodeHelp(object sender, NodeHelpEventArgs e)
        {
            if (e.Handled) return;

            e.Handled = true;

            var helpDialog = new NodeHelpPrompt(e.Model);
            helpDialog.Owner = Window.GetWindow(this);
            
            helpDialog.Show();

        }

        void NodeLogic_DispatchedToUI(object sender, UIDispatcherEventArgs e)
        {
            Dispatcher.Invoke(e.ActionToDispatch);
        }

        void inputGrid_Loaded(object sender, RoutedEventArgs e)
        {
            //once the input grid is loaded, send a command
            //to the view model, which will be pushed down
            //to the model to ask for types to load custom UI elements
            ViewModel.SetupCustomUIElementsCommand.Execute(this);
        }

        private Dictionary<UIElement, bool> enabledDict
            = new Dictionary<UIElement, bool>();

        internal void DisableInteraction()
        {
            enabledDict.Clear();

            foreach (UIElement e in inputGrid.Children)
            {
                enabledDict[e] = e.IsEnabled;

                e.IsEnabled = false;
            }

            //set the state using the view model's command
            if (ViewModel.SetStateCommand.CanExecute(ElementState.DEAD))
                ViewModel.SetStateCommand.Execute(ElementState.DEAD);
        }

        internal void EnableInteraction()
        {
            foreach (UIElement e in inputGrid.Children)
            {
                if (enabledDict.ContainsKey(e))
                    e.IsEnabled = enabledDict[e];
            }

            ViewModel.ValidateConnectionsCommand.Execute(null);
        }

        private void topControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            //e.Handled = true;
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            //set handled to avoid triggering key press
            //events on the workbench
            //e.Handled = true;
        }

        private void MainContextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {

        }

        private void topControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel == null) return;

            dynSettings.ReturnFocusToSearch();

            var view = WPF.FindUpVisualTree<DynamoView>(this);
            view.mainGrid.Focus();

            Guid nodeGuid = this.ViewModel.NodeModel.GUID;
            dynSettings.Controller.DynamoViewModel.ExecuteCommand(
                new DynCmd.SelectModelCommand(nodeGuid, Keyboard.Modifiers));

            if (e.ClickCount == 2)
            {
                if (ViewModel.GotoWorkspaceCommand.CanExecute(null))
                {
                    e.Handled = true;
                    ViewModel.GotoWorkspaceCommand.Execute(null);
                }
            }
        }

        private void topControl_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Debug.WriteLine("Node right selected.");
            e.Handled = true;
        }

        private void NickNameBlock_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (toolTipDelayTimer != null && toolTipDelayTimer.IsEnabled)
                toolTipDelayTimer.Stop();

            dynSettings.Controller.InfoBubbleViewModel.OnRequestAction(
                new InfoBubbleEventArgs(InfoBubbleEventArgs.Request.Hide));

            if (e.ClickCount == 2)
            {
                Debug.WriteLine("Nickname double clicked!");
                if (this.ViewModel != null && this.ViewModel.RenameCommand.CanExecute(null))
                {
                    this.ViewModel.RenameCommand.Execute(null);
                }
                e.Handled = true;
            }
        }

        private void NickNameBlock_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (toolTipDelayTimer != null && toolTipDelayTimer.IsEnabled)
                toolTipDelayTimer.Stop();

            SetupAndShowTooltip((UIElement)sender, InfoBubbleViewModel.Direction.Bottom);
        }

        private void NickNameBlock_OnMouseEnter(object sender, MouseEventArgs e)
        {
            SetupAndShowTooltip((UIElement)sender, InfoBubbleViewModel.Direction.Bottom);
        }

        private void NickNameBlock_OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (toolTipDelayTimer != null && toolTipDelayTimer.IsEnabled)
                toolTipDelayTimer.Stop();

            if (ViewModel != null)
                ViewModel.FadeOutTooltipCommand.Execute(null);
            else if (dynSettings.Controller != null)
                dynSettings.Controller.DynamoViewModel.FadeOutInfoBubble(null);
        }

        private void InputPort_OnMouseEnter(object sender, MouseEventArgs e)
        {
            SetupAndShowTooltip((UIElement)sender, InfoBubbleViewModel.Direction.Right);
        }

        private void InputPort_OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (toolTipDelayTimer != null && toolTipDelayTimer.IsEnabled)
                toolTipDelayTimer.Stop();

            if (ViewModel != null)
                ViewModel.FadeOutTooltipCommand.Execute(null);
            else if (dynSettings.Controller != null)
                dynSettings.Controller.DynamoViewModel.FadeOutInfoBubble(null);
        }

        private void InputPort_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (toolTipDelayTimer != null && toolTipDelayTimer.IsEnabled)
                toolTipDelayTimer.Stop();

            dynSettings.Controller.InfoBubbleViewModel.OnRequestAction(
                new InfoBubbleEventArgs(InfoBubbleEventArgs.Request.Hide));
        }

        private void OutputPort_OnMouseEnter(object sender, MouseEventArgs e)
        {
            SetupAndShowTooltip((UIElement)sender, InfoBubbleViewModel.Direction.Left);
        }

        private void OutputPort_OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (toolTipDelayTimer != null && toolTipDelayTimer.IsEnabled)
                toolTipDelayTimer.Stop();

            if (ViewModel != null)
                ViewModel.FadeOutTooltipCommand.Execute(null);
            else if (dynSettings.Controller != null)
                dynSettings.Controller.DynamoViewModel.FadeOutInfoBubble(null);
        }

        private void OutputPort_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (toolTipDelayTimer != null && toolTipDelayTimer.IsEnabled)
                toolTipDelayTimer.Stop();

            dynSettings.Controller.InfoBubbleViewModel.OnRequestAction(
                new InfoBubbleEventArgs(InfoBubbleEventArgs.Request.Hide));
        }

        private void PreviewArrow_MouseEnter(object sender, MouseEventArgs e)
        {
            UIElement uiElement = sender as UIElement;
            if (uiElement.Visibility == System.Windows.Visibility.Visible)
                ViewModel.ShowPreviewCommand.Execute(null);
        }

        private void SetupAndShowTooltip(UIElement sender, InfoBubbleViewModel.Direction direction)
        {
            string content = "";
            double actualWidth  = 0;
            double actualHeight = 0;

            switch (direction)
            {
                case InfoBubbleViewModel.Direction.Bottom:
                    TextBlock tb = sender as TextBlock;
                    content = ViewModel.Description;
                    if (string.IsNullOrWhiteSpace(content))
                        return;

                    actualWidth = tb.ActualWidth * dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.Zoom;
                    actualHeight = tb.ActualHeight * dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.Zoom;
                    break;

                case InfoBubbleViewModel.Direction.Left:
                case InfoBubbleViewModel.Direction.Right:
                    ContentPresenter nodePort = sender as ContentPresenter;
                    content = (nodePort.Content as PortViewModel).ToolTipContent;
                    if (string.IsNullOrWhiteSpace(content))
                        return;

                    actualWidth = nodePort.ActualWidth * dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.Zoom;
                    actualHeight = nodePort.ActualHeight * dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.Zoom;
                    break;
            }

            UIElement containingWorkspace = WPF.FindUpVisualTree<TabControl>(this);            

            InfoBubbleDataPacket data = new InfoBubbleDataPacket();
            data.Text       = content;
            data.Style      = InfoBubbleViewModel.Style.NodeTooltip;
            data.TopLeft    = sender.TranslatePoint(new Point(0, 0), containingWorkspace);
            data.BotRight   = new Point(data.TopLeft.X + actualWidth, data.TopLeft.Y + actualHeight);            
            data.ConnectingDirection = direction;

            StartDelayedTooltipFadeIn(data);
        }

        private void StartDelayedTooltipFadeIn(InfoBubbleDataPacket data)
        {
            if (toolTipDelayTimer == null)
            {
                toolTipDelayTimer = new DispatcherTimer();
                toolTipDelayTimer.Interval = TimeSpan.FromMilliseconds(Configurations.ToolTipFadeInDelayInMS);

                toolTipDelayTimer.Tick += delegate(object sender, EventArgs e)
                {
                    var timer = sender as DispatcherTimer;
                    timer.Stop(); // stop timer after one tick
                    ViewModel.ShowTooltipCommand.Execute((InfoBubbleDataPacket)timer.Tag);
                };
            }

            // Collapse any existing bubble before starting fade in
            if (ViewModel != null)
                ViewModel.HideTooltipCommand.Execute(null);
            else if (dynSettings.Controller != null)
                dynSettings.Controller.DynamoViewModel.HideInfoBubble(null);

            toolTipDelayTimer.Stop();
            toolTipDelayTimer.Tag = data;
            dynSettings.Controller.InfoBubbleViewModel.UpdateContentCommand.Execute(data);
            toolTipDelayTimer.Start();
        }
    }
}
