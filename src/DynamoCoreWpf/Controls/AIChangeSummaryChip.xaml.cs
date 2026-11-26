using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dynamo.ViewModels;

namespace Dynamo.UI.Controls
{
    /// <summary>
    /// Canvas chip summarizing the latest Autodesk Assistant turn.
    /// </summary>
    internal partial class AIChangeSummaryChip : UserControl
    {
        public AIChangeSummaryChip()
        {
            InitializeComponent();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            chip.DataContext = (e.NewValue as WorkspaceViewModel)?.AIChangeSummaryChip;
        }

        private void OnMouseButton(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}
