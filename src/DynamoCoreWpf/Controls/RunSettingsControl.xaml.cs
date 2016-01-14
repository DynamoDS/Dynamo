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

        private void RunButton_OnClick(object sender, RoutedEventArgs e)
        {
            var dynamoView = WpfUtilities.FindUpVisualTree<DynamoView>(this);
            var dynamoVm = dynamoView.DataContext as DynamoViewModel;
            dynamoVm.OnRequestReturnFocusToView();
        }
    }
}
