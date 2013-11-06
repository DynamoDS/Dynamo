using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Dynamo.Models;
using Dynamo.UI;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using DynCmd = Dynamo.ViewModels.DynamoViewModel;

namespace Dynamo.Nodes
{
    //taken from http://stackoverflow.com/questions/660554/how-to-automatically-select-all-text-on-focus-in-wpf-textbox
    public class ClickSelectTextBox : TextBox
    {
        public ClickSelectTextBox()
        {
            AddHandler(PreviewMouseLeftButtonDownEvent,
                new MouseButtonEventHandler(SelectivelyIgnoreMouseButton), true);
            AddHandler(GotKeyboardFocusEvent,
                new RoutedEventHandler(SelectAllText), true);
            AddHandler(MouseDoubleClickEvent,
                new RoutedEventHandler(SelectAllText), true);
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
                var textBox = (TextBox)parent;
                if (!textBox.IsKeyboardFocusWithin)
                {
                    // If the text box is not yet focussed, give it the focus and
                    // stop further processing of this click event.
                    textBox.Focus();
                    e.Handled = true;
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
        public event Action OnChangeCommitted;

        private static Brush clear = new SolidColorBrush(System.Windows.Media.Color.FromArgb(100, 255, 255, 255));
        private static Brush highlighted = new SolidColorBrush(System.Windows.Media.Color.FromArgb(200, 255, 255, 255));

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
            base.Text = initialText;
            this.Pending = false;
            Style = (Style)SharedDictionaryManager.DynamoModernDictionary["SZoomFadeTextBox"];
            MinHeight = 20;
        }

        public void BindToProperty(System.Windows.Data.Binding binding)
        {
            binding.NotifyOnTargetUpdated = true;
            this.SetBinding(TextBox.TextProperty, binding);
            this.TargetUpdated += OnTargetUpdated;
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
                dynSettings.ReturnFocusToSearch();
            }
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            UpdateDataSource(true);
        }

        private void OnTargetUpdated(object sender, DataTransferEventArgs e)
        {
            // If "pending" is "false", then it means this event is being triggered 
            // by binding DynamoTextBox to the data source for the first time. This 
            // happens when DynamoTextBox is first bound to a NodeModel property, 
            // it does not mean there is an actual update on the text box by user.
            // In this case, we should not attempt to generate a command and record 
            // it.
            if (false == this.pending)
                return;

            var expr = GetBindingExpression(TextProperty);
            if (null != expr)
            {
                string propName = expr.ParentBinding.Path.Path;
                NodeModel nodeModel = GetBoundModel(expr.DataItem);

                dynSettings.Controller.DynamoViewModel.ExecuteCommand(
                    new DynCmd.UpdateModelValueCommand(
                        nodeModel.GUID, propName, this.Text));
            }
        }

        #endregion

        #region Private Class Helper Methods

        private void UpdateDataSource(bool recordForUndo)
        {
            if (this.Pending)
            {
                var expr = GetBindingExpression(TextProperty);
                if (expr != null)
                {
                    if (false != recordForUndo)
                        PreUpdateModel(GetBoundModel(expr.DataItem));

                    expr.UpdateSource();
                }

                if (OnChangeCommitted != null)
                    OnChangeCommitted();

                Pending = false;
            }
        }

        private void PreUpdateModel(NodeModel nodeModel)
        {
            if (null != nodeModel)
            {
                DynamoModel dynamo = dynSettings.Controller.DynamoModel;
                dynamo.CurrentWorkspace.RecordModelForModification(nodeModel);

                dynSettings.Controller.DynamoViewModel.UndoCommand.RaiseCanExecuteChanged();
                dynSettings.Controller.DynamoViewModel.RedoCommand.RaiseCanExecuteChanged();
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
            nodeModel.WorkSpace.RecordModelForModification(nodeModel);
        }
        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
            if (e.OriginalSource is System.Windows.Shapes.Rectangle)
                nodeModel.WorkSpace.RecordModelForModification(nodeModel);
        }
        #endregion
    }
}
