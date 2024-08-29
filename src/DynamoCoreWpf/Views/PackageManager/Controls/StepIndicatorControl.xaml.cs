using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Dynamo.UI;

namespace Dynamo.PackageManager.UI
{
    /// <summary>
    /// Interaction logic for StepIndicatorControl.xaml
    /// </summary>
    public partial class StepIndicatorControl : UserControl
    {
        private StepState _previousState;
        private bool _clickedDuringMouseOver;

        public StepIndicatorControl()
        {
            InitializeComponent();
            this.UpdateVisualState(State);
            this._clickedDuringMouseOver = false;
        }

        /// <summary>
        /// Title Dependency Property
        /// </summary>
        public static readonly DependencyProperty TitleProperty =
         DependencyProperty.Register("Title", typeof(string), typeof(StepIndicatorControl), new PropertyMetadata("Step"));

        /// <summary>
        /// Title property displayed on top of the step number
        /// </summary>
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        /// <summary>
        /// Step Number Dependency Property
        /// </summary>
        public static readonly DependencyProperty StepNumberProperty =
        DependencyProperty.Register("StepNumber", typeof(string), typeof(StepIndicatorControl),
            new PropertyMetadata("1", OnStepNumberChanged));

        /// <summary>
        /// Step Number property showing the number
        /// </summary>
        public string StepNumber
        {
            get { return (string)GetValue(StepNumberProperty); }
            set { SetValue(StepNumberProperty, value); }
        }

        private static void OnStepNumberChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (StepIndicatorControl)d;
            control.StepNumberText.Text = (string)e.NewValue;
        }

        /// <summary>
        /// Enum with the possible control states
        /// </summary>
        public enum StepState
        {
            Inactive,
            Active,
            Ok
        }

        /// <summary>
        /// Step State Dependency Property
        /// </summary>
        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register("State", typeof(StepState), typeof(StepIndicatorControl),
                new PropertyMetadata(StepState.Inactive, OnStateChanged));

        /// <summary>
        /// State Property accounting for the current state of the control
        /// </summary>
        public StepState State
        {
            get { return (StepState)GetValue(StateProperty); }
            set { SetValue(StateProperty, value); }
        }

        private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (StepIndicatorControl)d;
            var state = (StepState)e.NewValue;
            control.UpdateVisualState(state);
        }

        private void Ellipse_MouseEnter(object sender, MouseEventArgs e)
        {
            _previousState = State; // Save the current state
            VisualStateManager.GoToState(this, "MouseOver", true);
        }

        private void Ellipse_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!_clickedDuringMouseOver)
            {
                UpdateVisualState(_previousState);
            }
            _clickedDuringMouseOver = false;
        }

        private void Ellipse_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _clickedDuringMouseOver = true;

            if (State == StepState.Inactive)
            {
                State = StepState.Active; // Change to Active on click
            }
            else if (State == StepState.Active)
            {
                State = StepState.Ok; // Change to Ok on click
            }

            UpdateVisualState(State); // Update to the new state immediately
        }

        private void UpdateVisualState(StepState state)
        {
            switch (state)
            {
                case StepState.Active:
                    StepNumberText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#38abdf"));
                    StepNumberText.Visibility = Visibility.Visible;
                    OkIcon.Visibility = Visibility.Collapsed;
                    StepTitle.FontFamily = SharedDictionaryManager.DynamoModernDictionary["ArtifaktElementBold"] as FontFamily;
                    break;
                    
                case StepState.Inactive:
                    StepNumberText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#979797"));
                    StepNumberText.Visibility = Visibility.Visible;
                    OkIcon.Visibility = Visibility.Collapsed;
                    StepTitle.FontFamily = SharedDictionaryManager.DynamoModernDictionary["ArtifaktElementRegular"] as FontFamily;
                    break;

                case StepState.Ok:  
                    StepNumberText.Visibility = Visibility.Collapsed;
                    OkIcon.Visibility = Visibility.Visible;
                    StepTitle.FontFamily = SharedDictionaryManager.DynamoModernDictionary["ArtifaktElementRegular"] as FontFamily;
                    break;

            }

            var stateName = state.ToString();
            VisualStateManager.GoToState(this, stateName, true);
        }
    }
}
