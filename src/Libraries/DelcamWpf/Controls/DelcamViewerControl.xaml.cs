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

using Dynamo.Controls;

using DynamoDelcam;

namespace Dynamo.Wpf.Controls
{
    /// <summary>
    /// Interaction logic for DelcamViewerControl.xaml
    /// </summary>
    public partial class DelcamViewerControl : UserControl
    {
        public DelcamViewerControl(DelcamViewer Model, NodeView nodeView)
        {
            InitializeComponent();



        }
    }
}
