using System.Windows;
using System.Windows.Controls;
using Dynamo.Controls;
using DSCoreNodesUI;
using Dynamo.UI.Commands;
using System.Xml;

namespace GeometryUIWpf.Controls
{
    /// <summary>
    /// Interaction logic for ExportAsSATControl.xaml
    /// </summary>
    public partial class ExportAsSATControl : UserControl
    {
        public ExportAsSATControl(DynamoConvert Model, NodeView nodeView)
        {
            InitializeComponent();
        }
    }
}
