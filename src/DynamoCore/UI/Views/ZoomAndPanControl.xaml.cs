using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using Dynamo.ViewModels;

namespace Dynamo.Controls
{
    /// <summary>
    /// Interaction logic for ZoomAndPanControl.xaml
    /// </summary>
    public partial class ZoomAndPanControl : UserControl
    {
        public ZoomAndPanControl(WorkspaceViewModel workspaceViewModel)
        {
            InitializeComponent();
            this.DataContext = workspaceViewModel;
        }
    }
}