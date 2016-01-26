using System.Windows;
using System.Windows.Controls;
using Dynamo.Controls;
using CoreNodeModels;
using Dynamo.UI.Commands;

namespace CoreNodeModelsWpf.Controls
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
    }
}
