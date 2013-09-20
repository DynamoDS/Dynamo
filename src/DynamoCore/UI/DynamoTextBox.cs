using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Dynamo.Utilities;

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
        }

        protected void UpdateDataSource()
        {
            var expr = GetBindingExpression(TextProperty);
            if (expr != null)
                expr.UpdateSource();

            if (OnChangeCommitted != null)
                OnChangeCommitted();

            Pending = false;
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
            UpdateDataSource();
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
}
