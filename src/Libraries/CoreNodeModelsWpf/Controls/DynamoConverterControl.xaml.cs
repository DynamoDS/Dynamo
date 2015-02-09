using System.Windows;
using System.Windows.Controls;
using Dynamo.Controls;
using DSCoreNodesUI;
using Dynamo.UI.Commands;

namespace Dynamo.Wpf.Controls
{
    /// <summary>
    /// Interaction logic for ConverterControl.xaml
    /// </summary>
    public partial class DynamoConverterControl : UserControl
    {
        public DynamoConverterControl(DynamoConvert Model, NodeView nodeView)
        {
            InitializeComponent();
        }

        private void OnDirectionButtonClick(object sender, RoutedEventArgs e)
        {
            var dataContext = this.DataContext as DynamoConvert;
            if (dataContext != null)
            {
                var temp = dataContext.SelectedFromConversion;
                dataContext.SelectedFromConversion = dataContext.SelectedToConversion;
                dataContext.SelectedToConversion = temp;
            }
        }
    }
}
