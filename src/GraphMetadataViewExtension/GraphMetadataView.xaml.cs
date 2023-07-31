using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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

        /// <summary>
        /// This enables moving to next textbox by pressing tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        private void TextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var textBox = sender as TextBox;
            if (Uri.TryCreate(textBox.Text, UriKind.Absolute, out Uri uri))
            {
                try
                {
                    Process.Start(new ProcessStartInfo(uri.ToString()) { UseShellExecute = true });
                }
                catch
                {
                    return;
                }
            }
        }
    }
}
