﻿using System;
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
            AddHandler(
                PreviewMouseLeftButtonDownEvent,
                new MouseButtonEventHandler(SelectivelyIgnoreMouseButton), true);
            AddHandler(
                GotKeyboardFocusEvent,
                new RoutedEventHandler(SelectAllText), true);
            AddHandler(
                MouseDoubleClickEvent,
                new RoutedEventHandler(SelectAllText), true);
        }

        private static void SelectivelyIgnoreMouseButton(
            object sender,
            MouseButtonEventArgs e)
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

    public class dynTextBox : ClickSelectTextBox
    {
        public event Action OnChangeCommitted;

        private static Brush clear = new SolidColorBrush(System.Windows.Media.Color.FromArgb(100, 255, 255, 255));
        private static Brush highlighted = new SolidColorBrush(System.Windows.Media.Color.FromArgb(200, 255, 255, 255));

        public dynTextBox()
        {
            //turn off the border
            Background = clear;
            BorderThickness = new Thickness(1);
            GotFocus += OnGotFocus;
            LostFocus += OnLostFocus;
            LostKeyboardFocus += OnLostFocus;
        }

        private void OnLostFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            Background = clear;
        }

        private void OnGotFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            Background = highlighted;
            SelectAll();
        }

        private bool numeric;
        public bool IsNumeric
        {
            get { return numeric; }
            set
            {
                numeric = value;
                if (value && Text.Length > 0)
                {
                    Text = dynSettings.RemoveChars(
                        Text,
                        Text.ToCharArray()
                            .Where(c => !char.IsDigit(c) && c != '-' && c != '.')
                            .Select(c => c.ToString())
                        );
                }
            }
        }

        private bool pending;
        public bool Pending
        {
            get { return pending; }
            set
            {
                if (value)
                {
                    FontStyle = FontStyles.Italic;
                }
                else
                {
                    FontStyle = FontStyles.Normal;
                }
                pending = value;
            }
        }

        public void Commit()
        {
            var expr = GetBindingExpression(TextProperty);
            if (expr != null)
                expr.UpdateSource();

            if (OnChangeCommitted != null)
            {
                OnChangeCommitted();
            }
            Pending = false;

            //dynSettings.Bench.mainGrid.Focus();
        }

        new public string Text
        {
            get { return base.Text; }
            set
            {
                base.Text = value;
                Commit();
            }
        }

        private bool shouldCommit()
        {
            return !dynSettings.Controller.DynamoViewModel.DynamicRunEnabled;
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            Pending = true;

            if (IsNumeric)
            {
                var p = CaretIndex;

                //base.Text = dynSettings.RemoveChars(
                //   Text,
                //   Text.ToCharArray()
                //      .Where(c => !char.IsDigit(c) && c != '-' && c != '.')
                //      .Select(c => c.ToString())
                //);

                CaretIndex = p;
            }
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
            Commit();
        }
    }
}