using System.Windows.Controls;
using PythonNodeModels;

namespace PythonNodeModelsWpf.Controls
{
    /// <summary>
    /// Interaction logic for EngineLabel.xaml
    /// </summary>
    public partial class EngineLabel : UserControl
    {
        private PythonNodeBase NodeModel { get; set; }
        public EngineLabel(PythonNodeModels.PythonNodeBase nodeModel)
        {
            InitializeComponent();
            NodeModel = nodeModel;
        }
    }
}
