using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Dynamo.Graph.Nodes;
using Dynamo.Wpf.Properties;

namespace Dynamo.UI.Controls
{
    /// <summary>
    /// The review popover for an input the AI Assistant changed: the input's name, its value before
    /// and after the change, "Undo this change" and "Next change ›". Opened by the input's port on
    /// hover and on keyboard focus; closes shortly after both the pointer and keyboard focus leave
    /// the port and the popover.
    /// </summary>
    internal sealed class AIChangePopover
    {
        // Long enough to move the pointer from the port onto the popover without it closing
        private static readonly TimeSpan CloseDelay = TimeSpan.FromMilliseconds(250);

        private static readonly SolidColorBrush outlineBrush = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["NodeAIChangedInputOutlineBrush"] as SolidColorBrush;
        private static readonly SolidColorBrush backgroundBrush = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["DarkGreyBrush"] as SolidColorBrush;
        private static readonly SolidColorBrush titleBrush = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["PrimaryCharcoal100Brush"] as SolidColorBrush;
        private static readonly SolidColorBrush labelBrush = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["PrimaryCharcoal300Brush"] as SolidColorBrush;
        private static readonly SolidColorBrush valueBrush = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["PrimaryCharcoal200Brush"] as SolidColorBrush;
        private static readonly FontFamily regularFont = SharedDictionaryManager.DynamoModernDictionary["ArtifaktElementRegular"] as FontFamily;
        private static readonly FontFamily boldFont = SharedDictionaryManager.DynamoModernDictionary["ArtifaktElementBold"] as FontFamily;
        private static readonly Style textButtonStyle = SharedDictionaryManager.DynamoModernDictionary["TextButtonStyle"] as Style;

        private readonly FrameworkElement target;
        private readonly Popup popup;
        private readonly Border content;
        private readonly DispatcherTimer closeTimer;

        /// <summary>Raised when "Undo this change" is clicked.</summary>
        internal event Action UndoRequested;

        /// <summary>Raised when "Next change ›" is clicked.</summary>
        internal event Action NextRequested;

        /// <summary>Raised when the popover opens.</summary>
        internal event Action Opened;

        /// <summary>Raised when the popover closes, however it was closed.</summary>
        internal event Action Closed;

        /// <param name="target">The changed input's port, which the popover opens above.</param>
        internal AIChangePopover(FrameworkElement target)
        {
            this.target = target;

            InputNameText = new TextBlock
            {
                Name = "aiChangeInputName",
                FontFamily = boldFont,
                FontSize = 14,
                Foreground = titleBrush,
                TextTrimming = TextTrimming.CharacterEllipsis
            };

            BeforeText = CreateValueText("aiChangeBefore");
            AfterText = CreateValueText("aiChangeAfter");

            var values = new Grid { Margin = new Thickness(0, 8, 0, 0) };
            values.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            values.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            values.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            values.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            AddValueRow(values, 0, Resources.AIChangePopoverBefore, BeforeText);
            AddValueRow(values, 1, Resources.AIChangePopoverAfter, AfterText);

            UndoButton = CreateButton("aiChangeUndo", Resources.AIChangePopoverUndo, HorizontalAlignment.Left);
            UndoButton.Click += (s, e) => UndoRequested?.Invoke();

            // Explains why the button is disabled, so it must show while disabled
            ToolTipService.SetShowOnDisabled(UndoButton, true);

            NextButton = CreateButton("aiChangeNext", Resources.AIChangePopoverNextChange, HorizontalAlignment.Right);
            NextButton.Click += (s, e) => NextRequested?.Invoke();

            var actions = new DockPanel { Margin = new Thickness(0, 12, 0, 0), LastChildFill = false };
            DockPanel.SetDock(UndoButton, Dock.Left);
            DockPanel.SetDock(NextButton, Dock.Right);
            actions.Children.Add(UndoButton);
            actions.Children.Add(NextButton);

            var stack = new StackPanel();
            stack.Children.Add(InputNameText);
            stack.Children.Add(values);
            stack.Children.Add(actions);

            content = new Border
            {
                Name = "aiChangePopover",
                MinWidth = 220,
                MaxWidth = 320,
                Padding = new Thickness(12),
                Background = backgroundBrush,
                BorderBrush = outlineBrush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Child = stack
            };

            // Tab stays inside the popover; Escape hands focus back to the port
            KeyboardNavigation.SetTabNavigation(content, KeyboardNavigationMode.Cycle);
            content.PreviewKeyDown += OnContentPreviewKeyDown;
            content.MouseLeave += (s, e) => CloseSoon();
            content.MouseEnter += (s, e) => closeTimer.Stop();
            content.LostKeyboardFocus += (s, e) => CloseSoon();

            popup = new Popup
            {
                Child = content,
                PlacementTarget = target,
                Placement = PlacementMode.Top,
                VerticalOffset = -4,
                AllowsTransparency = true,
                StaysOpen = true
            };
            popup.Opened += (s, e) => Opened?.Invoke();
            popup.Closed += (s, e) => Closed?.Invoke();

            closeTimer = new DispatcherTimer(DispatcherPriority.Input, target.Dispatcher) { Interval = CloseDelay };
            closeTimer.Tick += OnCloseTimerTick;
        }

        /// <summary>True while the popover is shown.</summary>
        internal bool IsOpen => popup.IsOpen;

        /// <summary>
        /// True while Escape hands keyboard focus back to the port, which should not reopen the
        /// popover.
        /// </summary>
        internal bool IsReturningFocus { get; private set; }

        /// <summary>The changed input's name. Exposed for tests.</summary>
        internal TextBlock InputNameText { get; }

        /// <summary>The input's value before the change. Exposed for tests.</summary>
        internal TextBlock BeforeText { get; }

        /// <summary>The input's value after the change. Exposed for tests.</summary>
        internal TextBlock AfterText { get; }

        /// <summary>"Undo this change". Exposed for tests.</summary>
        internal Button UndoButton { get; }

        /// <summary>"Next change ›". Exposed for tests.</summary>
        internal Button NextButton { get; }

        /// <summary>
        /// Fills in the popover and opens it.
        /// </summary>
        /// <param name="inputName">The changed input's name, as shown on its port.</param>
        /// <param name="change">The change to show.</param>
        /// <param name="canUndo">Whether undo would revert just the turn that made the change.</param>
        /// <param name="hasNext">Whether the turn changed another input to move on to.</param>
        internal void Show(string inputName, AIInputChange change, bool canUndo, bool hasNext)
        {
            closeTimer.Stop();

            InputNameText.Text = inputName;
            BeforeText.Text = change.Before ?? Resources.AIChangePopoverNoValue;
            AfterText.Text = change.After ?? Resources.AIChangePopoverNoValue;

            UpdateActions(canUndo, hasNext);

            popup.IsOpen = true;
        }

        /// <summary>
        /// Updates "Undo this change" and "Next change ›" while the popover stays open, for example
        /// after an edit puts a later step on top of the undo stack.
        /// </summary>
        /// <param name="canUndo">Whether undo would revert just the turn that made the change.</param>
        /// <param name="hasNext">Whether the turn changed another input to move on to.</param>
        internal void UpdateActions(bool canUndo, bool hasNext)
        {
            UndoButton.IsEnabled = canUndo;
            UndoButton.ToolTip = canUndo ? Resources.AIChangePopoverUndoToolTip : Resources.AIChangePopoverUndoUnavailable;
            NextButton.IsEnabled = hasNext;
        }

        /// <summary>
        /// Moves keyboard focus into the open popover: to "Next change ›" when asked and possible,
        /// so repeated Enter walks through the turn's changes, otherwise to its first enabled button.
        /// </summary>
        /// <param name="preferNext">Focus "Next change ›" if it is enabled.</param>
        internal void FocusContent(bool preferNext)
        {
            if (!popup.IsOpen) return;

            // The popover's content is only focusable once it has been laid out
            content.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!popup.IsOpen) return;

                var button = preferNext && NextButton.IsEnabled ? NextButton
                    : UndoButton.IsEnabled ? UndoButton
                    : NextButton.IsEnabled ? NextButton
                    : null;

                if (button != null)
                {
                    Keyboard.Focus(button);
                }
            }), DispatcherPriority.Input);
        }

        /// <summary>
        /// Closes the popover after a short delay, unless the pointer or keyboard focus comes back
        /// to the port or the popover in the meantime.
        /// </summary>
        internal void CloseSoon()
        {
            if (!popup.IsOpen) return;

            closeTimer.Stop();
            closeTimer.Start();
        }

        /// <summary>Closes the popover now.</summary>
        internal void Close()
        {
            closeTimer.Stop();
            popup.IsOpen = false;
        }

        private void OnCloseTimerTick(object sender, EventArgs e)
        {
            closeTimer.Stop();

            if (target.IsMouseOver || content.IsMouseOver
                || target.IsKeyboardFocusWithin || content.IsKeyboardFocusWithin)
            {
                return;
            }

            popup.IsOpen = false;
        }

        private void OnContentPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape) return;

            Close();

            IsReturningFocus = true;
            try
            {
                Keyboard.Focus(target);
            }
            finally
            {
                IsReturningFocus = false;
            }

            e.Handled = true;
        }

        private static TextBlock CreateValueText(string name)
        {
            return new TextBlock
            {
                Name = name,
                FontFamily = regularFont,
                FontSize = 12,
                Foreground = valueBrush,
                TextWrapping = TextWrapping.Wrap,
                TextTrimming = TextTrimming.CharacterEllipsis,
                // Three lines; longer values are cut off
                MaxHeight = 54,
                Margin = new Thickness(0, 2, 0, 2)
            };
        }

        private static void AddValueRow(Grid grid, int row, string label, TextBlock value)
        {
            var labelText = new TextBlock
            {
                Text = label,
                FontFamily = regularFont,
                FontSize = 12,
                Foreground = labelBrush,
                Margin = new Thickness(0, 2, 12, 2)
            };

            Grid.SetRow(labelText, row);
            Grid.SetRow(value, row);
            Grid.SetColumn(value, 1);
            grid.Children.Add(labelText);
            grid.Children.Add(value);
        }

        private static Button CreateButton(string name, string text, HorizontalAlignment alignment)
        {
            return new Button
            {
                Name = name,
                Content = text,
                Style = textButtonStyle,
                HorizontalAlignment = alignment,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0)
            };
        }
    }
}
