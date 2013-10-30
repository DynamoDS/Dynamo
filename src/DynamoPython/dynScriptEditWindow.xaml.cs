using System.Windows;
using System.Windows.Media;
using Dynamo.Controls;
using Dynamo.Utilities;

namespace Dynamo.Nodes
{
    /// <summary>
    /// Interaction logic for dynScriptEditWindow.xaml
    /// </summary>
    public partial class dynScriptEditWindow : Window
    {
        public dynScriptEditWindow()
        {
            InitializeComponent();
            //this.Owner = dynSettings.Bench;
            var view = FindUpVisualTree<DynamoView>(this);
            this.Owner = view;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            //cance = return false
            this.DialogResult = false;
        }

        // walk up the visual tree to find object of type T, starting from initial object
        public static T FindUpVisualTree<T>(DependencyObject initial) where T : DependencyObject
        {
            DependencyObject current = initial;

            while (current != null && current.GetType() != typeof(T))
            {
                current = VisualTreeHelper.GetParent(current);
            }
            return current as T;
        }
    }
}
