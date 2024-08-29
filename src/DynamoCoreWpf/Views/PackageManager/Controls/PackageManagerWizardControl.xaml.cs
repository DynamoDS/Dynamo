using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dynamo.PackageManager.UI
{
    /// <summary>
    /// Interaction logic for PackageManagerWizardControl.xaml
    /// </summary>
    public partial class PackageManagerWizardControl : UserControl
    {
        private bool isUpdatingStep;

        /// <summary>
        /// Current Step Dependency Property
        /// </summary>
        public static readonly DependencyProperty CurrentStepProperty =
            DependencyProperty.Register(
                nameof(CurrentStep),
                typeof(int),
                typeof(PackageManagerWizardControl),
                new PropertyMetadata(1, OnCurrentStepChanged));

        /// <summary>
        /// Current Step of the wizard control
        /// </summary>
        public int CurrentStep
        {
            get { return (int)GetValue(CurrentStepProperty); }
            set
            {
                // Check if we are already in an update to prevent infinite loop
                if (!isUpdatingStep)
                {
                    SetValue(CurrentStepProperty, value);
                }
            }
        }

        /// <summary>
        /// Allows to subscribe to the event triggered when the step has changed
        /// </summary>
        public event EventHandler<int> StepChanged;

        public PackageManagerWizardControl()
        {
            InitializeComponent();
            CurrentStep = 1;
        }

        private static void OnCurrentStepChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (PackageManagerWizardControl)d;

            // Prevent re-entrant updates
            control.isUpdatingStep = true;  

            try
            {
                control.UpdateSteps();
                control.StepChanged?.Invoke(control, control.CurrentStep);
            }
            finally
            {
                control.isUpdatingStep = false; 
            }
        }

        private void StepIndicatorControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var indicator = sender as StepIndicatorControl;
            if (indicator != null)
            {
                int step;
                if (int.TryParse(indicator.StepNumber, out step))
                {
                    CurrentStep = step;
                    UpdateSteps();

                    StepChanged?.Invoke(this, CurrentStep);
                }
            }
        }

        private void UpdateSteps()
        {
            UpdateStepStateRecursive(this);
        }

        private void UpdateStepStateRecursive(DependencyObject parent)
        {
            if (parent == null)
                return;

            foreach (object child in LogicalTreeHelper.GetChildren(parent))
            {
                if (child is StepIndicatorControl indicator)
                {
                    int step;
                    if (int.TryParse(indicator.StepNumber, out step))
                    {
                        if (step < CurrentStep)
                        {
                            indicator.State = StepIndicatorControl.StepState.Ok;
                        }
                        else if (step == CurrentStep)
                        {
                            indicator.State = StepIndicatorControl.StepState.Active;
                        }
                        else
                        {
                            indicator.State = StepIndicatorControl.StepState.Inactive;
                        }
                    }
                }
                else if (child is DependencyObject dependencyChild)
                {
                    UpdateStepStateRecursive(dependencyChild);
                }
            }
        }
    }
}
