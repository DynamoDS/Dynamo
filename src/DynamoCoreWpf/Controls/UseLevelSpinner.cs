using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dynamo.UI.Controls
{
    /// <summary>
    /// UseLevelSpinner control 
    /// </summary>
    [DefaultProperty("Level"), DefaultEvent("LevelChanged")]
    public class UseLevelSpinner : Control
    {
        private const int MinimumLevel = 1;
        private const int MaximumLevel = 9;
        public static readonly DependencyProperty LevelProperty;
        public static readonly DependencyProperty IsReadOnlyProperty;
        public static readonly DependencyProperty KeepListStructureProperty;
        public static readonly RoutedEvent LevelChangedEvent;
        public static RoutedCommand IncreaseCommand;
        public static RoutedCommand DecreaseCommand;

        private TextBox _textBox;

        #region Properties
        /// <summary>
        /// Gets or sets the value assigned to the numeric up-down control.
        /// This is a dependency property.
        /// </summary>
        public int Level
        {
            get
            {
                return (int)GetValue(LevelProperty);
            }
            set
            {
                SetValue(LevelProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets a value that indicating whether the text can be changed by the use of the up or down buttons only.
        /// This is a dependency property.
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return (bool)GetValue(IsReadOnlyProperty);
            }

            set
            {
                SetValue(IsReadOnlyProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets a value that indicating whether the text can be changed by the use of the up or down buttons only.
        /// This is a dependency property.
        /// </summary>
        public bool KeepListStructure
        {
            get
            {
                return (bool)GetValue(KeepListStructureProperty);
            }

            set
            {
                SetValue(KeepListStructureProperty, value);
            }
        }

        /// <summary>
        /// Gets the current text content held by the text box.
        /// </summary>
        public string ContentText
        {
            get
            {
                if (_textBox != null)
                {
                    return _textBox.Text;
                }

                return null;
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the Level property changes.
        /// </summary>
        public event RoutedPropertyChangedEventHandler<int> LevelChanged
        {
            add { AddHandler(LevelChangedEvent, value); }

            remove { RemoveHandler(LevelChangedEvent, value); }
        }

        #endregion

        #region Constructors

        static UseLevelSpinner()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(UseLevelSpinner), new FrameworkPropertyMetadata(typeof(UseLevelSpinner)));

            InitializeCommands();

            LevelProperty = DependencyProperty.Register("Level", typeof(int), typeof(UseLevelSpinner),
                new FrameworkPropertyMetadata(2, OnLevelChanged));

            KeepListStructureProperty = DependencyProperty.Register("KeepListStructure", typeof(bool), typeof(UseLevelSpinner),
                new FrameworkPropertyMetadata(false, OnKeepListStructureChanged));

            IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(UseLevelSpinner),
                new FrameworkPropertyMetadata(false, OnIsReadOnlyChanged));

            LevelChangedEvent = EventManager.RegisterRoutedEvent("LevelChanged", RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<int>), typeof(UseLevelSpinner));

            EventManager.RegisterClassHandler(typeof(UseLevelSpinner),
                Mouse.MouseDownEvent, new MouseButtonEventHandler(OnMouseLeftButtonDown), true);
        }

        /// <summary>
        /// Initializes a new instance of UseLevelSpinner.
        /// </summary>
        public UseLevelSpinner()
            : base()
        {
        }

        #endregion

        private static void OnLevelChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            UseLevelSpinner control = (UseLevelSpinner)obj;

            int oldValue = (int)args.OldValue;
            int newValue = (int)args.NewValue;

            RoutedPropertyChangedEventArgs<int> e = new RoutedPropertyChangedEventArgs<int>(
                oldValue, newValue, LevelChangedEvent);

            control.OnLevelChanged(e);
            control.UpdateText();
        }

        private static void OnIsReadOnlyChanged(DependencyObject element, DependencyPropertyChangedEventArgs args)
        {
            UseLevelSpinner control = element as UseLevelSpinner;

            bool readOnly = (bool)args.NewValue;
            if (readOnly != control._textBox.IsReadOnly)
            {
                control._textBox.IsReadOnly = readOnly;
            }
        }

        private static void OnKeepListStructureChanged(DependencyObject element, DependencyPropertyChangedEventArgs args)
        {
            UseLevelSpinner control = element as UseLevelSpinner;
            control.UpdateText();
        }

        private static object CoerceValue(DependencyObject element, object value)
        {
            int newValue = (int)value;
            UseLevelSpinner control = (UseLevelSpinner)element;

            newValue = Math.Min(MaximumLevel, Math.Max(MinimumLevel, newValue));
            return newValue;
        }

        private static void InitializeCommands()
        {
            IncreaseCommand = new RoutedCommand("IncreaseCommand", typeof(UseLevelSpinner));
            CommandManager.RegisterClassCommandBinding(typeof(UseLevelSpinner), new CommandBinding(IncreaseCommand, OnIncreaseCommand));
            CommandManager.RegisterClassInputBinding(typeof(UseLevelSpinner), new InputBinding(IncreaseCommand, new KeyGesture(Key.Up)));

            DecreaseCommand = new RoutedCommand("DecreaseCommand", typeof(UseLevelSpinner));
            CommandManager.RegisterClassCommandBinding(typeof(UseLevelSpinner), new CommandBinding(DecreaseCommand, OnDecreaseCommand));
            CommandManager.RegisterClassInputBinding(typeof(UseLevelSpinner), new InputBinding(DecreaseCommand, new KeyGesture(Key.Down)));
        }

        private static void OnIncreaseCommand(object sender, ExecutedRoutedEventArgs e)
        {
            UseLevelSpinner control = sender as UseLevelSpinner;

            if (control != null)
            {
                control.OnIncrease();
            }
        }

        private static void OnDecreaseCommand(object sender, ExecutedRoutedEventArgs e)
        {
            UseLevelSpinner control = sender as UseLevelSpinner;

            if (control != null)
            {
                control.OnDecrease();
            }
        }

        private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            UseLevelSpinner control = (UseLevelSpinner)sender;

            // When someone click on a part in the NumericUpDown and it's not focusable
            // NumericUpDown needs to take the focus in order to process keyboard correctly
            if (!control.IsKeyboardFocusWithin)
            {
                e.Handled = control.Focus() || e.Handled;
            }
        }

        /// <summary>
        /// Called when the template's tree is generated.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_textBox != null)
            {
                _textBox.TextChanged -= new TextChangedEventHandler(OnTextBoxTextChanged);
                _textBox.PreviewKeyDown -= new KeyEventHandler(OnTextBoxPreviewKeyDown);
            }

            _textBox = (TextBox)base.GetTemplateChild("textbox");

            if (_textBox != null)
            {
                _textBox.TextChanged += new TextChangedEventHandler(OnTextBoxTextChanged);
                _textBox.PreviewKeyDown += new KeyEventHandler(OnTextBoxPreviewKeyDown);
                _textBox.IsReadOnly = false;
            }

            UpdateText();
        }

        /// <summary>
        /// Reports that the IsKeyboardFocusWithin property changed.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                OnGotFocus();
            }
            else
            {
                OnLostFocus();
            }
        }

        /// <summary>
        /// Handles the System.Windows.Input.Mouse.MouseWheel routed event.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            if (IsKeyboardFocusWithin)
            {
                if (e.Delta > 0)
                {
                    OnIncrease();
                }
                else
                {
                    OnDecrease();
                }
            }
        }

        /// <summary>
        /// Raises the LevelChanged event.
        /// </summary>
        /// <param name="args">Arguments associated with the LevelChanged event.</param>
        protected virtual void OnLevelChanged(RoutedPropertyChangedEventArgs<int> args)
        {
            RaiseEvent(args);
        }

        /// <summary>
        /// IncreaseCommand event handler.
        /// </summary>
        protected virtual void OnIncrease()
        {
            if (Level + 1 <= MaximumLevel)
            {
                Level += 1;
            }
        }

        /// <summary>
        /// DecreaseCommand event handler.
        /// </summary>
        protected virtual void OnDecrease()
        {
            if (Level - 1 >= MinimumLevel)
            {
                Level -= 1;
            }
        }

        private void OnGotFocus()
        {
            if (_textBox != null)
            {
                _textBox.Focus();
            }

            UpdateText();
        }

        private void OnLostFocus()
        {
            UpdateText();
        }

        private void OnTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            return;
        }

        private void OnTextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    OnIncrease();
                    break;

                case Key.Down:
                    OnDecrease();
                    break;

                default:
                    return;
            }

            e.Handled = true;
        }

        /// <summary>
        /// Displays level value. 
        /// </summary>
        internal void UpdateText()
        {
            string formattedValue = string.Format(KeepListStructure ? "@@L{0}" : "@L{0}", Level);
            if (_textBox != null)
            {
                _textBox.Text = formattedValue;
            }
        }
    }
}
