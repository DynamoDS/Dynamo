using System.Windows.Controls;
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
            DataContext = workspaceViewModel;
        }
    }
}