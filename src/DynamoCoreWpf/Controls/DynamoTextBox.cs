using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Search;
using Dynamo.UI;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Thickness = System.Windows.Thickness;

namespace Dynamo.Nodes
{
    //taken from http://stackoverflow.com/questions/660554/how-to-automatically-select-all-text-on-focus-in-wpf-textbox
    public class ClickSelectTextBox : TextBox
    {
        protected bool selectAllWhenFocused = true;
        protected RoutedEventHandler focusHandler;

        public ClickSelectTextBox()
        {
            focusHandler = new RoutedEventHandler(SelectAllText);
            AddHandler(PreviewMouseLeftButtonDownEvent,
                new MouseButtonEventHandler(SelectivelyIgnoreMouseButton), true);
            AddHandler(GotKeyboardFocusEvent, focusHandler, true);
            AddHandler(MouseDoubleClickEvent, focusHandler, true);
        }

        private static void SelectivelyIgnoreMouseButton(
            object sender, MouseButtonEventArgs e)
        {
            // Find the TextBox
            DependencyObject parent = e.OriginalSource as UIElement;
            while (parent != null && !(parent is TextBox))
                parent = VisualTreeHelper.GetParent(parent);

            if (parent != null)
            {
                var textBox = parent as ClickSelectTextBox;
                if (textBox != null && (!textBox.IsKeyboardFocusWithin))
                {
                    // If the text box is not yet focussed, give it the focus and
                    // stop further processing of this click event.
                    textBox.Focus();
                    e.Handled = textBox.selectAllWhenFocused;
                }
            }
        }

        private static void SelectAllText(object sender, RoutedEventArgs e)
        {
            var textBox = e.OriginalSource as TextBox;
            if (textBox != null)
                textBox.SelectAll();
        }
    }

    public class DynamoTextBox : ClickSelectTextBox
    {
        public event RequestReturnFocusToSearchHandler RequestReturnFocusToSearch;
        public delegate void RequestReturnFocusToSearchHandler();
        protected void OnRequestReturnFocusToSearch()
        {
            if (RequestReturnFocusToSearch != null)
                RequestReturnFocusToSearch();
        }

        public event Action OnChangeCommitted;

        private static Brush clear = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255));
        private static Brush highlighted = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255));

        private NodeViewModel nodeViewModel;
        private NodeViewModel NodeViewModel
        {
            get
            {
                if (nodeViewModel != null) return nodeViewModel;

                var f = WpfUtilities.FindUpVisualTree<NodeView>(this);
                if (f != null) this.nodeViewModel = f.ViewModel;

                return nodeViewModel;
            }
        }

        #region Class Operational Methods

        public DynamoTextBox()
            : this(string.Empty)
        {
        }

        public DynamoTextBox(string initialText)
        {
            //turn off the border
            Background = clear;
            BorderThickness = new Thickness(1);
            GotFocus += OnGotFocus;
            LostFocus += OnLostFocus;
            LostKeyboardFocus += OnLostFocus;
            Padding = new Thickness(3);
            base.Text = initialText;
            Pending = false;
            Style = (Style)SharedDictionaryManager.DynamoModernDictionary["SZoomFadeTextBox"];
            MinHeight = 20;

            RequestReturnFocusToSearch += TryFocusSearch;
        }

        private void TryFocusSearch()
        {
            if (NodeViewModel == null) return;

            NodeViewModel.DynamoViewModel.ReturnFocusToSearch();
        }

        public void BindToProperty(Binding binding)
        {
            SetBinding(TextProperty, binding);
            UpdateDataSource(false);
        }

        #endregion

        #region Class Properties

        private bool pending;

        public bool Pending
        {
            get { return pending; }
            set
            {
                FontStyle = value ? FontStyles.Italic : FontStyles.Normal;
                pending = value;
            }
        }

        /// <summary>
        /// This property hides the base "TextBox.Text" property to remove the 
        /// ability to directly set its value (by-passing the undo recording 
        /// completely). For this reason, there is no setter for this property.
        /// </summary>
        new public string Text
        {
            get { return base.Text; }
            set
            {
                base.Text = value;
                UpdateDataSource(true);
            }
        }

        #endregion

        #region Class Event Handlers

        private void OnLostFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            Background = clear;
        }

        private void OnGotFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            Background = highlighted;
            SelectAll();
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            Pending = true;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Enter)
            {
                OnRequestReturnFocusToSearch();
            }
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            UpdateDataSource(true);
        }

        #endregion

        #region Private Class Helper Methods

        private void UpdateDataSource(bool recordForUndo)
        {
            if (Pending)
            {
                var expr = GetBindingExpression(TextProperty);

                var nvm = NodeViewModel;
                if (expr != null)
                {
                    // There are two ways in which the bound data source can be 
                    // updated: when it is first bound to the target (text box),
                    // and when it is explicitly updated by text box losing its 
                    // focus. In the first way, even though the bound data source 
                    // is updated, its actual value does not get changed. In this 
                    // case there is no need for undo recording. However, it is 
                    // deemed as a user commit when text box loses its focus, in 
                    // which case undo recording has to be done. It is in the 
                    // second case a command is being sent to actually update the 
                    // data source (also record the update for undo).

                    if (false == recordForUndo)
                        expr.UpdateSource();
                    else if (nvm != null)
                    {
                        string propName = expr.ParentBinding.Path.Path;
                        nvm.DynamoViewModel.ExecuteCommand(
                            new DynamoModel.UpdateModelValueCommand(
                                nodeViewModel.WorkspaceViewModel.Model.Guid,
                                nvm.NodeModel.GUID, propName, Text));
                    }
                }

                if (OnChangeCommitted != null)
                    OnChangeCommitted();

                Pending = false;
            }
        }

        private NodeModel GetBoundModel(object dataItem)
        {
            // Attempt get to the data-bound model (if there's any).
            NodeModel nodeModel = dataItem as NodeModel;
            if (null == nodeModel)
            {
                NodeViewModel nodeViewModel = dataItem as NodeViewModel;
                if (null != nodeViewModel)
                    nodeModel = nodeViewModel.NodeModel;
            }

            return nodeModel;
        }

        #endregion
    }

    public class StringTextBox : DynamoTextBox
    {

        #region Class Event Handlers

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            // This method is overridden so that the base implementation will 
            // not be called (the base class commits changes once <Enter> key
            // is pressed, not something that a multi-line string edit box needs.
        }

        #endregion
    }
}

namespace Dynamo.UI.Controls
{
    /// <summary>
    /// This is a class designed to be used as a tool-tip for Library View, 
    /// Input/Output ports, and Node Caption. It replaces the default look 
    /// of the system tool-tip where it has a triangular side that points 
    /// to the corresponding "target" element. This tool-tip also aligns itself 
    /// to the center of its target, both vertically and horizontally depending 
    /// on its attachment side.
    /// </summary>
    /// 
    public class DynamoToolTip : ToolTip
    {
        public static readonly DependencyProperty AttachmentSideProperty =
            DependencyProperty.Register("AttachmentSide",
            typeof(Side), typeof(DynamoToolTip),
            new PropertyMetadata(Side.Left));

        public enum Side
        {
            Left, Top, Right, Bottom
        }

        public DynamoToolTip()
        {
            Placement = PlacementMode.Custom;
            CustomPopupPlacementCallback = new CustomPopupPlacementCallback(PlacementCallback);
        }

        private CustomPopupPlacement[] PlacementCallback(Size popup, Size target, Point offset)
        {
            double x = 0, y = 0;
            double gap = Configurations.ToolTipTargetGapInPixels;
            PopupPrimaryAxis primaryAxis = PopupPrimaryAxis.None;

            switch (AttachmentSide)
            {
                case Side.Left:
                    x = -(popup.Width + gap);
                    y = (target.Height - popup.Height) * 0.5;
                    primaryAxis = PopupPrimaryAxis.Horizontal;
                    break;

                case Side.Right:
                    x = target.Width + gap;
                    y = (target.Height - popup.Height) * 0.5;
                    primaryAxis = PopupPrimaryAxis.Horizontal;
                    break;

                case Side.Top:
                    x = (target.Width - popup.Width) * 0.5;
                    y = -(popup.Height + gap);
                    primaryAxis = PopupPrimaryAxis.Vertical;
                    break;

                case Side.Bottom:
                    x = (target.Width - popup.Width) * 0.5;
                    y = target.Height + gap;
                    primaryAxis = PopupPrimaryAxis.Vertical;
                    break;
            }

            return new CustomPopupPlacement[]
            {
                new CustomPopupPlacement()
                {
                    Point = new Point(x, y),
                    PrimaryAxis = primaryAxis
                }
            };
        }

        public Side AttachmentSide
        {
            get { return ((Side)GetValue(AttachmentSideProperty)); }
            set { SetValue(AttachmentSideProperty, value); }
        }
    }

    public class LibraryToolTipPopup : Popup
    {
        private ToolTipWindow tooltip = new ToolTipWindow();
        private readonly LibraryToolTipTimer toolTipTimer;
        private DynamoView mainDynamoWindow;

        public LibraryToolTipPopup()
        {
            this.Placement = PlacementMode.Custom;
            this.AllowsTransparency = true;
            this.CustomPopupPlacementCallback = PlacementCallback;
            this.DataContext = null;
            this.Child = tooltip;
            this.Loaded += LoadMainDynamoWindow;

            toolTipTimer = new LibraryToolTipTimer();
            toolTipTimer.TimerElapsed += (dataContext) =>
            {
                OpenCloseLibraryToolTipPopup(dataContext);
            };
        }

        // We should load main window after Popup has been initialized.
        // If we try to load it before, we will get null.
        private void LoadMainDynamoWindow(object sender, RoutedEventArgs e)
        {
            mainDynamoWindow = WpfUtilities.FindUpVisualTree<DynamoView>(this);
            if (mainDynamoWindow == null)
                return;

            // When Dynamo window goes behind another app, the tool-tip should be hidden right 
            // away. We cannot use CloseLibraryToolTipPopup because it only hides the tool-tip 
            // window after a pause.
            mainDynamoWindow.Deactivated += (Sender, args) =>
            {
                this.DataContext = null;
                IsOpen = false;
            };
        }

        public void SetDataContext(object dataContext, bool closeImmediately = false)
        {
            // If Dynamo window is not active, we should not show as well as hide tooltip or do any other staff.
            if (mainDynamoWindow == null || !mainDynamoWindow.IsActive) return;

            if (closeImmediately)
            {
                CloseLibraryToolTipPopup();
                return;
            }

            toolTipTimer.Start(dataContext, 150);
        }

        private void OpenCloseLibraryToolTipPopup(object dataContext)
        {
            IsOpen = dataContext != null || this.IsMouseOver;
            if (dataContext != null)
                this.DataContext = dataContext;

            // This line is needed to change position of Popup.
            // As position changed PlacementCallback is called and
            // Popup placed correctly.            
            HorizontalOffset++;

            // Moving tooltip back.
            HorizontalOffset--;
        }

        private void CloseLibraryToolTipPopup()
        {
            this.DataContext = null;
            IsOpen = false;
            toolTipTimer.Stop();
        }

        private CustomPopupPlacement[] PlacementCallback(Size popup, Size target, Point offset)
        {
           // http://stackoverflow.com/questions/1918877/how-can-i-get-the-dpi-in-wpf
            // MAGN 7397 Library tooltip popup is offset over library items on highres monitors (retina and >96 dpi)
            //Youtrack http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7397
            PresentationSource source = PresentationSource.FromVisual(this);
            double xfactor = 1.0;
            if (source != null)
            {
                xfactor = source.CompositionTarget.TransformToDevice.M11;
            }
           
            double gap = Configurations.ToolTipTargetGapInPixels;
            var dynamoWindow = WpfUtilities.FindUpVisualTree<DynamoView>(this.PlacementTarget);
            if (dynamoWindow == null)
            {
                SetDataContext(null, true);
                return null;
            }
            Point targetLocation = this.PlacementTarget
                .TransformToAncestor(dynamoWindow)
                .Transform(new Point(0, 0));

            // Count width.
            // multiplying by xfactor scales the placement point of the library UI tooltip to the correct location
            //otherwise direct pixel coordinates are off by this factor due to screen dpi.
            double x = 0;
            x = (WpfUtilities.FindUpVisualTree<SearchView>(this.PlacementTarget).ActualWidth
                + gap * 2 + targetLocation.X * (-1)) * xfactor;

            // Count height.
            var availableHeight = dynamoWindow.ActualHeight - popup.Height
                - (targetLocation.Y + Configurations.NodeButtonHeight);

            double y = 0;
            if (availableHeight < Configurations.BottomPanelHeight)
                y = availableHeight - (Configurations.BottomPanelHeight + gap * 4);

            return new CustomPopupPlacement[]
            {
                new CustomPopupPlacement()
                {
                    Point = new Point(x, y),
                    PrimaryAxis = PopupPrimaryAxis.Horizontal
                }
            };
        }

        private class LibraryToolTipTimer
        {
            private readonly DispatcherTimer dispatcherTimer;

            internal LibraryToolTipTimer()
            {
                dispatcherTimer = new DispatcherTimer();
                dispatcherTimer.Tick += (sender, args) =>
                {
                    // Send the data context to caller.
                    this.OnTimerElapsed(dispatcherTimer.Tag);
                };

            }
            internal void Start(object dataContext, int milliseconds)
            {
                dispatcherTimer.Stop();
                dispatcherTimer.Tag = dataContext;
                dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, milliseconds);
                dispatcherTimer.Start();
            }

            internal void Stop()
            {
                dispatcherTimer.Stop();
                dispatcherTimer.Tag = null;
            }

            internal event Action<object> TimerElapsed;
            private void OnTimerElapsed(object dataContext)
            {
                this.Stop();

                var handler = TimerElapsed;
                if (handler != null)
                    handler(dataContext);
            }
        }
    }
}
