using System;
using System.Windows.Controls;
using System.Windows.Input;
using Dynamo.Connectors;

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
        }

        void endDot_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Highlight(object sender, MouseEventArgs e)
        {
            ViewModel.HighlightCommand.Execute();
        }

        private void Unhighlight(object sender, MouseEventArgs e)
        {
            ViewModel.UnHighlightCommand.Execute();
        }

        public dynConnectorViewModel ViewModel
        {
            get { return (dynConnectorViewModel) DataContext; }
        }
    }
}
