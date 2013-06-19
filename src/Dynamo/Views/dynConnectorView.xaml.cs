using System;
using System.Diagnostics;
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
            Canvas.SetZIndex(this, 1);

            this.Loaded += new System.Windows.RoutedEventHandler(dynConnectorView_Loaded);
        }

        void dynConnectorView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //post load
            Debug.WriteLine("Connector loaded.");
        }

        void EndDot_OnMouseDown(object sender, MouseEventArgs e)
        {
          
        }

        private void Highlight(object sender, MouseEventArgs e)
        {
            if (DataContext is dynConnectorViewModel)
                ViewModel.HighlightCommand.Execute();
        }

        private void Unhighlight(object sender, MouseEventArgs e)
        {
            if (DataContext is dynConnectorViewModel)
                ViewModel.UnHighlightCommand.Execute();
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
