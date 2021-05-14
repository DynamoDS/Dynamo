using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dynamo.GraphMetadata
{
    /// <summary>
    /// Interaction logic for GraphMetadataView.xaml
    /// </summary>
    public partial class GraphMetadataView : UserControl
    {
        public GraphMetadataView()
        {
            InitializeComponent();
        }

        private void StackPanel_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                TraversalRequest tRequest = new TraversalRequest(FocusNavigationDirection.Next);
                UIElement keyboardFocus = Keyboard.FocusedElement as UIElement;

                if (keyboardFocus != null)
                {
                    keyboardFocus.MoveFocus(tRequest);
                }

                e.Handled = true;
            }
        }
    }
}
