using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
