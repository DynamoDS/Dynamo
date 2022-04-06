using System.Windows.Controls;
using CoreNodeModels;
using Dynamo.Controls;

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
