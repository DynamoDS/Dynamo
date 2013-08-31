using Dynamo.Nodes;
using Dynamo.ViewModels;

namespace Dynamo.Controls
{
    /// <summary>
    /// Interaction logic for WatchControl.xaml
    /// </summary>
    public partial class WatchView : IViewModelView<NodeViewModel>
    {
        System.Windows.Point _rightMousePoint;

        public WatchView()
        {
            InitializeComponent();

            MouseRightButtonUp += new System.Windows.Input.MouseButtonEventHandler(view_MouseRightButtonUp);
            PreviewMouseRightButtonDown += new System.Windows.Input.MouseButtonEventHandler(view_PreviewMouseRightButtonDown);
        }

        void view_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _rightMousePoint = e.GetPosition(topControl);
        }

        void view_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //if the mouse has moved, and this is a right click, we assume 
            // rotation. handle the event so we don't show the context menu
            // if the user wants the contextual menu they can click on the
            // node sidebar or top bar
            if (e.GetPosition(topControl) != _rightMousePoint)
            {
                e.Handled = true;
            }
        }

        public NodeViewModel ViewModel
        {
            get
            {
                if (this.DataContext is NodeViewModel)
                    return (NodeViewModel)this.DataContext;
                else
                    return null;
            }
        }
    }
}
