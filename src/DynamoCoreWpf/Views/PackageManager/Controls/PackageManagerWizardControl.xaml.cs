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
        private int currentStep;

        public PackageManagerWizardControl()
        {
            InitializeComponent();
            currentStep = 1;
        }

        private void StepIndicatorControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var indicator = sender as StepIndicatorControl;
            if (indicator != null)
            {
                int step;
                if (int.TryParse(indicator.StepNumber, out step))
                {
                    currentStep = step;
                    UpdateSteps();
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
                        if (step < currentStep)
                        {
                            indicator.State = StepIndicatorControl.StepState.Ok;
                        }
                        else if (step == currentStep)
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
