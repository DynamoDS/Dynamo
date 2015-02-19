using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Dynamo.Controls;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.ViewModels;

namespace Dynamo.Wpf.Controls
{
    /// <summary>
    /// Interaction logic for RunManagerControl.xaml
    /// </summary>
    public partial class RunSettingsControl : UserControl
    {
        private RunSettingsViewModel vm;

        public RunSettingsControl()
        {
            InitializeComponent();
            Loaded += RunSettingsControl_Loaded;
        }

        void RunSettingsControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            vm = DataContext as RunSettingsViewModel;

            EnumComboBox.SelectionChanged += EnumComboBox_OnSelectionChanged;
        }

        /// <summary>
        /// Use the KeyDown event handler to set the binding to update
        /// when the enter key is pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UIElement_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            var bindingExpr = RunPeriodTextBox.GetBindingExpression(TextBox.TextProperty);
            bindingExpr.UpdateSource();
        }

        /// <summary>
        /// Use the SelectionChanged event handler to trigger a run
        /// depending on what type of run is set.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EnumComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.RunTypeChangedRunCommand.Execute(null);
        }

        private void RunButton_OnClick(object sender, RoutedEventArgs e)
        {
            var dynamoView = WpfUtilities.FindUpVisualTree<DynamoView>(this);
            var dynamoVm = dynamoView.DataContext as DynamoViewModel;
            dynamoVm.ReturnFocusToSearch();
        }
    }
}
