using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Dynamo.Graph.Nodes;
using Dynamo.Wpf.Properties;

namespace Dynamo.UI.Controls
{
    /// <summary>
    /// The review popover for an input the AI Assistant changed (Figma 8:304): who changed the node,
    /// the input's name, its value before and after the change, "Undo this change" and "Next
    /// change ›". Opened by the input's port on hover and on keyboard focus; closes shortly after
    /// both the pointer and keyboard focus leave the port and the popover.
    /// </summary>
    internal sealed class AIChangePopover
    {
        // Long enough to move the pointer from the port onto the popover without it closing
        private static readonly TimeSpan CloseDelay = TimeSpan.FromMilliseconds(250);

        // Smallest space between "Undo this change" and "Next change ›"
        internal const double ActionsGap = 16;

        // Layout from Figma 8:304
        private const double Padding = 16;
        private const double PopoverMinWidth = 340;
        private const double FontSize = 14;
        private const double IconSize = 18;

        // Widest the title, name and values get before they trim or wrap. The cap is on the text,
        // not the popover, so it never squeezes the buttons.
        private const double TextMaxWidth = PopoverMinWidth - 2 * Padding;

        private static readonly SolidColorBrush backgroundBrush = GetBrush("AIChangePopoverBackgroundBrush");
        private static readonly SolidColorBrush borderBrush = GetBrush("AIChangePopoverBorderBrush");
        private static readonly SolidColorBrush dividerBrush = GetBrush("AIChangePopoverDividerBrush");
        private static readonly SolidColorBrush primaryTextBrush = GetBrush("AIChangePopoverPrimaryTextBrush");
        private static readonly SolidColorBrush secondaryTextBrush = GetBrush("AIChangePopoverSecondaryTextBrush");
        private static readonly SolidColorBrush undoBrush = GetBrush("AIChangePopoverUndoBrush");
        private static readonly SolidColorBrush nextBrush = GetBrush("AIChangePopoverActionBrush");
        private static readonly SolidColorBrush disabledBrush = GetBrush("AIChangePopoverDisabledActionBrush");
        private static readonly FontFamily regularFont = SharedDictionaryManager.DynamoModernDictionary["ArtifaktElementRegular"] as FontFamily;
        private static readonly FontFamily boldFont = SharedDictionaryManager.DynamoModernDictionary["ArtifaktElementBold"] as FontFamily;
        private static readonly BitmapImage iconSource = new BitmapImage(new Uri("pack://application:,,,/DynamoCoreWpf;component/UI/Images/ai-icon-wrapper-96px.png"));
        private static readonly Style actionButtonStyle = CreateActionButtonStyle();

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

            // Header: the sparkle and "Edited by Autodesk Assistant"
            var icon = new Image
            {
                Source = iconSource,
                Width = IconSize,
                Height = IconSize,
                Stretch = Stretch.Uniform,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            };
            RenderOptions.SetBitmapScalingMode(icon, BitmapScalingMode.HighQuality);

            TitleText = CreateText("aiChangeTitle", boldFont, primaryTextBrush);
            TitleText.TextTrimming = TextTrimming.CharacterEllipsis;
            TitleText.VerticalAlignment = VerticalAlignment.Center;

            var header = new DockPanel { MaxWidth = TextMaxWidth, HorizontalAlignment = HorizontalAlignment.Left };
            DockPanel.SetDock(icon, Dock.Left);
            header.Children.Add(icon);
            header.Children.Add(TitleText);

            InputNameText = CreateText("aiChangeInputName", regularFont, secondaryTextBrush);
            InputNameText.TextTrimming = TextTrimming.CharacterEllipsis;
            InputNameText.MaxWidth = TextMaxWidth;
            InputNameText.HorizontalAlignment = HorizontalAlignment.Left;
            InputNameText.Margin = new Thickness(0, 12, 0, 4);

            // Before is struck through in grey; After is the current value, in bold
            BeforeText = CreateValueText("aiChangeBefore", regularFont, secondaryTextBrush);
            AfterText = CreateValueText("aiChangeAfter", boldFont, primaryTextBrush);

            var values = new Grid { MaxWidth = TextMaxWidth, HorizontalAlignment = HorizontalAlignment.Left };
            values.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            values.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            values.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            values.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            AddValueRow(values, 0, Resources.AIChangePopoverBefore, BeforeText);
            AddValueRow(values, 1, Resources.AIChangePopoverAfter, AfterText);

            var divider = new Border
            {
                Name = "aiChangeDivider",
                Height = 1,
                Background = dividerBrush,
                Margin = new Thickness(0, 12, 0, 0)
            };

            UndoButton = CreateButton("aiChangeUndo", Resources.AIChangePopoverUndo, boldFont, undoBrush, HorizontalAlignment.Left);
            UndoButton.Click += (s, e) => UndoRequested?.Invoke();
            UndoButton.GotKeyboardFocus += OnActionGotKeyboardFocus;

            // Explains why the button is disabled, so it must show while disabled
            ToolTipService.SetShowOnDisabled(UndoButton, true);

            NextButton = CreateButton("aiChangeNext", Resources.AIChangePopoverNextChange, regularFont, nextBrush, HorizontalAlignment.Right);
            NextButton.Click += (s, e) => NextRequested?.Invoke();
            NextButton.GotKeyboardFocus += OnActionGotKeyboardFocus;

            // Undo on the left, Next on the right, never closer than ActionsGap. The popover widens
            // to fit both buttons, so long (translated) labels cannot run into each other.
            var actions = new Grid { Name = "aiChangeActions", Margin = new Thickness(0, 12, 0, 0) };
            actions.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            actions.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star), MinWidth = ActionsGap });
            actions.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Grid.SetColumn(UndoButton, 0);
            Grid.SetColumn(NextButton, 2);
            actions.Children.Add(UndoButton);
            actions.Children.Add(NextButton);

            var stack = new StackPanel();
            stack.Children.Add(header);
            stack.Children.Add(InputNameText);
            stack.Children.Add(values);
            stack.Children.Add(divider);
            stack.Children.Add(actions);

            content = new Border
            {
                Name = "aiChangePopover",
                MinWidth = PopoverMinWidth,
                Padding = new Thickness(Padding),
                Background = backgroundBrush,
                BorderBrush = borderBrush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Child = stack,
                // Room for the shadow inside the popup
                Margin = new Thickness(8, 4, 8, 12),
                Effect = new DropShadowEffect
                {
                    BlurRadius = 12,
                    ShadowDepth = 2,
                    Direction = 270,
                    Opacity = 0.15
                }
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
                // The margin around the content is shadow, so the visible edge keeps a 4px gap
                HorizontalOffset = -8,
                VerticalOffset = 8,
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

        /// <summary>Who changed the node, for example "Edited by Autodesk Assistant". Exposed for tests.</summary>
        internal TextBlock TitleText { get; }

        /// <summary>"Input" and the changed input's name. Exposed for tests.</summary>
        internal TextBlock InputNameText { get; }

        /// <summary>The input's value before the change. Exposed for tests.</summary>
        internal TextBlock BeforeText { get; }

        /// <summary>The input's value after the change. Exposed for tests.</summary>
        internal TextBlock AfterText { get; }

        /// <summary>"Undo this change". Exposed for tests.</summary>
        internal Button UndoButton { get; }

        /// <summary>"Next change ›". Exposed for tests.</summary>
        internal Button NextButton { get; }

        /// <summary>The popover's visible surface. Exposed for tests.</summary>
        internal Border Content => content;

        /// <summary>
        /// Fills in the popover and opens it.
        /// </summary>
        /// <param name="title">Who changed the node, as on its AI badge's tooltip.</param>
        /// <param name="inputName">The changed input's name, as shown on its port.</param>
        /// <param name="change">The change to show.</param>
        /// <param name="canUndo">Whether undo would revert just the turn that made the change.</param>
        /// <param name="hasNext">Whether the turn changed another input to move on to.</param>
        internal void Show(string title, string inputName, AIInputChange change, bool canUndo, bool hasNext)
        {
            closeTimer.Stop();

            TitleText.Text = title;
            InputNameText.Text = string.Format(Resources.AIChangePopoverInputName, inputName);
            BeforeText.Text = change.Before ?? Resources.AIChangePopoverNoValue;
            AfterText.Text = change.After ?? Resources.AIChangePopoverNoValue;

            // Only a real old value is struck through; "None" is not a value that was replaced
            BeforeText.TextDecorations = change.Before == null ? null : TextDecorations.Strikethrough;

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
                    // Focusing the button raises GotKeyboardFocus, which asks Narrator to read it.
                    // The popup stays inactive, so this does not pull activation off DynamoView.
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

        private static SolidColorBrush GetBrush(string key)
        {
            return SharedDictionaryManager.DynamoColorsAndBrushesDictionary[key] as SolidColorBrush;
        }

        private static TextBlock CreateText(string name, FontFamily font, Brush foreground)
        {
            return new TextBlock
            {
                Name = name,
                FontFamily = font,
                FontSize = FontSize,
                Foreground = foreground
            };
        }

        private static TextBlock CreateValueText(string name, FontFamily font, Brush foreground)
        {
            var text = CreateText(name, font, foreground);
            text.TextWrapping = TextWrapping.Wrap;
            text.TextTrimming = TextTrimming.CharacterEllipsis;
            // Three lines; longer values are cut off
            text.MaxHeight = 60;
            text.Margin = new Thickness(0, 4, 0, 4);
            return text;
        }

        private static void AddValueRow(Grid grid, int row, string label, TextBlock value)
        {
            var labelText = CreateText(null, regularFont, secondaryTextBrush);
            labelText.Text = label;
            labelText.Margin = new Thickness(0, 4, 24, 4);

            Grid.SetRow(labelText, row);
            Grid.SetRow(value, row);
            Grid.SetColumn(value, 1);
            grid.Children.Add(labelText);
            grid.Children.Add(value);
        }

        private static Button CreateButton(string name, string text, FontFamily font, Brush foreground, HorizontalAlignment alignment)
        {
            return new Button
            {
                Name = name,
                Content = text,
                Style = actionButtonStyle,
                FontFamily = font,
                FontSize = FontSize,
                Foreground = foreground,
                HorizontalAlignment = alignment,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        /// <summary>
        /// The popover stays inactive, so Narrator does not follow focus into it. One notification
        /// is the sentence it speaks for this button.
        /// </summary>
        private static void OnActionGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!ReferenceEquals(e.OriginalSource, sender))
            {
                return;
            }

            var button = (Button)sender;
            AccessibleAnnouncement.Speak(button, button.Content as string);
        }

        /// <summary>
        /// A text-only button for the light popover: its own colour, underlined on hover, grey
        /// when disabled. The shared TextButtonStyle turns white on hover, which would vanish here.
        /// </summary>
        private static Style CreateActionButtonStyle()
        {
            var textFactory = new FrameworkElementFactory(typeof(TextBlock), "text");
            textFactory.SetValue(TextBlock.TextProperty, new TemplateBindingExtension(ContentControl.ContentProperty));
            textFactory.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);

            // A transparent background makes the whole label, not just the glyphs, clickable
            var borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.SetValue(Border.BackgroundProperty, Brushes.Transparent);
            borderFactory.AppendChild(textFactory);

            var template = new ControlTemplate(typeof(Button)) { VisualTree = borderFactory };

            var hover = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true };
            hover.Setters.Add(new Setter(TextBlock.TextDecorationsProperty, TextDecorations.Underline, "text"));
            template.Triggers.Add(hover);

            var disabled = new Trigger { Property = UIElement.IsEnabledProperty, Value = false };
            disabled.Setters.Add(new Setter(TextBlock.ForegroundProperty, disabledBrush, "text"));
            disabled.Setters.Add(new Setter(TextBlock.TextDecorationsProperty, null, "text"));
            template.Triggers.Add(disabled);

            var style = new Style(typeof(Button));
            style.Setters.Add(new Setter(Control.TemplateProperty, template));
            style.Setters.Add(new Setter(FrameworkElement.CursorProperty, Cursors.Hand));
            style.Seal();
            return style;
        }
    }

    /// <summary>
    /// Narrator does not follow keyboard focus into the graph, or into a popup that stays
    /// inactive. A notification is the sentence it will speak. The sparkle keeps its name for
    /// the automation peer, and this notification is what Narrator actually says.
    /// </summary>
    internal static class AccessibleAnnouncement
    {
        internal static void Speak(UIElement element, string message)
        {
            if (element == null || string.IsNullOrEmpty(message))
            {
                return;
            }

            var peer = UIElementAutomationPeer.FromElement(element)
                ?? UIElementAutomationPeer.CreatePeerForElement(element);
            peer?.RaiseNotificationEvent(
                AutomationNotificationKind.Other,
                AutomationNotificationProcessing.ImportantMostRecent,
                message,
                "Dynamo");
        }
    }
}
