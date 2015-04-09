using System.Windows;
using System.Windows.Controls;
using Dynamo.Controls;
using DSCoreNodesUI;

using Dynamo.Nodes;
using Dynamo.UI.Commands;
using System.Xml;

using GeometryUI;


namespace Dynamo.Wpf.Controls
{
    /// <summary>
    /// Interaction logic for ExportAsSATControl.xaml
    /// </summary>
    public partial class ExportWithUnitsControl : UserControl
    {
        public ExportWithUnitsControl(ExportWithUnits Model, NodeView nodeView)
        {
            InitializeComponent();
        }
    }
}
