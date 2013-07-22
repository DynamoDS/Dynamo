using System.Windows.Controls;
using System.Windows.Input;
using Dynamo.ViewModels;

namespace Dynamo.Nodes.Views
{
    /// <summary>
    /// Interaction logic for dynConnectorView.xaml
    /// </summary>
    public partial class dynConnectorView : UserControl, IViewModelView<dynConnectorViewModel>
    {
       
        public dynConnectorView()
        {
            InitializeComponent();
            Dynamo.Controls.DragCanvas.SetCanBeDragged(this, false);
            Canvas.SetZIndex(this, 1);
        }

        void EndDot_OnMouseDown(object sender, MouseEventArgs e)
        {
          
        }

        private void Highlight(object sender, MouseEventArgs e)
        {
            if (DataContext is dynConnectorViewModel)
                ViewModel.HighlightCommand.Execute(null);
        }

        private void Unhighlight(object sender, MouseEventArgs e)
        {
            if (DataContext is dynConnectorViewModel)
                ViewModel.UnHighlightCommand.Execute(null);
        }

        public dynConnectorViewModel ViewModel
        {
            get { return (dynConnectorViewModel) DataContext; }
        }

        void Connector_OnMouseDown(object sender, MouseEventArgs e)
        {
           // Let the click pass through the connector
        }

    }
}
