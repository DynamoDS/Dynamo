using System;
using System.Windows;
using System.Windows.Controls;
<<<<<<< HEAD
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
=======
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

>>>>>>> Sitrus2
using Dynamo.Controls;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Search;
using Dynamo.UI;
using Dynamo.UI.Views;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using DynCmd = Dynamo.Models.DynamoModel;
<<<<<<< HEAD
using System.Windows.Controls.Primitives;
using Dynamo.Core;
using Thickness = System.Windows.Thickness;
=======
>>>>>>> Sitrus2

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

    public class DynamoSlider : Slider
    {
        readonly NodeModel nodeModel;
        private readonly UndoRedoRecorder recorder;

        public DynamoSlider(NodeModel model, UndoRedoRecorder undoRecorder)
        {
            nodeModel = model;
            recorder = undoRecorder;
        }

        #region Event Handlers
        protected override void OnThumbDragStarted(DragStartedEventArgs e)
        {
            base.OnThumbDragStarted(e);
            WorkspaceModel.RecordModelForModification(nodeModel, recorder);
        }

        protected override void OnThumbDragCompleted(DragCompletedEventArgs e)
        {
            base.OnThumbDragCompleted(e);
            nodeModel.OnAstUpdated();
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
            if (e.OriginalSource is Rectangle)
                WorkspaceModel.RecordModelForModification(nodeModel, recorder);
        }
        #endregion
    }

    public class CodeNodeTextBox : DynamoTextBox
    {

        bool shift, enter;
        public CodeNodeTextBox(string s)
            : base(s)
        {
            shift = enter = false;

            //Remove the select all when focused feature
            RemoveHandler(GotKeyboardFocusEvent, focusHandler);

            //Allow for event processing after textbook has been focused to
            //help set the Caret position
            selectAllWhenFocused = false;

            //Set style for Watermark
            this.SetResourceReference(TextBox.StyleProperty, "CodeBlockNodeTextBox");
            this.Tag = "Your code goes here";
        }


        /// <summary>
        /// To allow users to remove focus by pressing Shift Enter. Uses two bools (shift / enter)
        /// and sets them when pressed/released
        /// </summary>
        #region Key Press Event Handlers
        protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                shift = true;
            }
            else if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                enter = true;
            }
            else if (e.Key == Key.Escape)
            {
                HandleEscape();
            }
            if (shift == true && enter == true)
            {
                OnRequestReturnFocusToSearch();
                shift = enter = false;
            }
        }
        protected override void OnPreviewKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                shift = false;
            }
            else if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                enter = false;
            }
        }
        #endregion

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            e.Handled = true; //hide base
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnLostFocus(RoutedEventArgs e)
        {
            Pending = true;
            base.OnLostFocus(e);
        }

        private void HandleEscape()
        {
            var text = this.Text;
            var cb = DataContext as CodeBlockNodeModel;

            if (cb == null || cb.Code != null && text.Equals(cb.Code))
                OnRequestReturnFocusToSearch();
            else
                (this as TextBox).Text = (DataContext as CodeBlockNodeModel).Code;
        }
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
        private DispatcherTimer dispatcherTimer = new DispatcherTimer();
        private DispatcherTimer showTimer = new DispatcherTimer();
        private object nextDataContext;

        public LibraryToolTipPopup()
        {
            this.Placement = PlacementMode.Custom;
            this.AllowsTransparency = true;
            this.CustomPopupPlacementCallback = PlacementCallback;
            this.Child = tooltip;
            this.dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            this.dispatcherTimer.Tick += CloseLibraryToolTipPopup;
            this.showTimer.Interval = new TimeSpan(0, 0, 0, 0, 60);
            this.showTimer.Tick += OpenLibraryToolTipPopup;
            this.Loaded += LoadMainDynamoWindow;
        }

        // We should load main window after Popup has been initialized.
        // If we try to load it before, we will get null.
        private void LoadMainDynamoWindow(object sender, RoutedEventArgs e)
        {
            var mainDynamoWindow = WpfUtilities.FindUpVisualTree<DynamoView>(this);

            // When Dynamo window goes behind another app, the tool-tip should be hidden right 
            // away. We cannot use CloseLibraryToolTipPopup because it only hides the tool-tip 
            // window after a pause.
            mainDynamoWindow.Deactivated += (Sender, args) =>
            {
                this.DataContext = null;
            };
        }

        public void SetDataContext(object dataContext, bool closeImmediately = false)
        {
            if (dataContext == null)
            {
                if (closeImmediately)
                {
                    CloseLibraryToolTipPopup(null, null);
                    return;
                }
                showTimer.Stop();
                dispatcherTimer.Start();
                return;
            }
            dispatcherTimer.Stop();
            nextDataContext = dataContext;
            showTimer.Start();
        }

        private void OpenLibraryToolTipPopup(object sender, EventArgs e)
        {
            this.DataContext = nextDataContext;

            // This line is needed to change position of Popup.
            // As position changed PlacementCallback is called and
            // Popup placed correctly.            
            HorizontalOffset++;

            // Moving tooltip back.
            HorizontalOffset--;

            showTimer.Stop();
        }

        private void CloseLibraryToolTipPopup(object sender, EventArgs e)
        {
            if (!this.IsMouseOver)
                this.DataContext = null;
        }

        private CustomPopupPlacement[] PlacementCallback(Size popup, Size target, Point offset)
        {
            double gap = Configurations.ToolTipTargetGapInPixels;
            var dynamoWindow = WpfUtilities.FindUpVisualTree<DynamoView>(this.PlacementTarget);
            Point targetLocation = this.PlacementTarget
                .TransformToAncestor(dynamoWindow)
                .Transform(new Point(0, 0));

            // Count width.
            double x = 0;
            x = WpfUtilities.FindUpVisualTree<SearchView>(this.PlacementTarget).ActualWidth
                + gap * 2 + targetLocation.X * (-1);

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
    }
}
