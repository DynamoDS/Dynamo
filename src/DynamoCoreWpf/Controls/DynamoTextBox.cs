using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Dynamo.Controls;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.UI;
using Dynamo.Utilities;
using Dynamo.ViewModels;

using DynCmd = Dynamo.Models.DynamoModel;
using System.Windows.Controls.Primitives;

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

        private static Brush clear = new SolidColorBrush(System.Windows.Media.Color.FromArgb(100, 255, 255, 255));
        private static Brush highlighted = new SolidColorBrush(System.Windows.Media.Color.FromArgb(200, 255, 255, 255));

        private NodeViewModel nodeViewModel;
        private NodeViewModel NodeViewModel
        {
            get
            {
                if (this.nodeViewModel != null) return this.nodeViewModel;

                var f = WpfUtilities.FindUpVisualTree<NodeView>(this);
                if (f != null) this.nodeViewModel = f.ViewModel;

                return this.nodeViewModel;
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
            BorderThickness = new System.Windows.Thickness(1);
            GotFocus += OnGotFocus;
            LostFocus += OnLostFocus;
            LostKeyboardFocus += OnLostFocus;
            Padding = new System.Windows.Thickness(3);
            base.Text = initialText;
            this.Pending = false;
            Style = (Style)SharedDictionaryManager.DynamoModernDictionary["SZoomFadeTextBox"];
            MinHeight = 20;

            RequestReturnFocusToSearch += TryFocusSearch;
        }

        private void TryFocusSearch()
        {
            if (this.NodeViewModel == null) return;

            this.NodeViewModel.DynamoViewModel.ReturnFocusToSearch();
        }

        public void BindToProperty(System.Windows.Data.Binding binding)
        {
            this.SetBinding(TextBox.TextProperty, binding);
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

        protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
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
            if (this.Pending)
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
                            new DynCmd.UpdateModelValueCommand(
                                nvm.NodeModel.GUID, propName, this.Text));
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

        protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            // This method is overridden so that the base implementation will 
            // not be called (the base class commits changes once <Enter> key
            // is pressed, not something that a multi-line string edit box needs.
        }

        #endregion
    }

    public class DynamoSlider : Slider
    {
        NodeModel nodeModel;
        public DynamoSlider(NodeModel model)
        {
            nodeModel = model;
        }

        #region Event Handlers
        protected override void OnThumbDragStarted(System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            base.OnThumbDragStarted(e);
            nodeModel.Workspace.RecordModelForModification(nodeModel);
            (nodeModel as IBlockingModel).OnBlockingStarted(EventArgs.Empty);
        }

        protected override void OnThumbDragCompleted(System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            base.OnThumbDragCompleted(e);
            (nodeModel as IBlockingModel).OnBlockingEnded(EventArgs.Empty);
            nodeModel.RequiresRecalc = true;
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
            if (e.OriginalSource is System.Windows.Shapes.Rectangle)
                nodeModel.Workspace.RecordModelForModification(nodeModel);
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
            typeof(DynamoToolTip.Side), typeof(DynamoToolTip),
            new PropertyMetadata(DynamoToolTip.Side.Left));

        public enum Side
        {
            Left, Top, Right, Bottom
        }

        public DynamoToolTip()
        {
            this.Placement = PlacementMode.Custom;
            this.CustomPopupPlacementCallback = new CustomPopupPlacementCallback(PlacementCallback);
        }

        private CustomPopupPlacement[] PlacementCallback(Size popup, Size target, Point offset)
        {
            double x = 0, y = 0;
            double gap = Configurations.ToolTipTargetGapInPixels;
            PopupPrimaryAxis primaryAxis = PopupPrimaryAxis.None;

            switch (this.AttachmentSide)
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
}
