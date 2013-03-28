using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Dynamo.Nodes.PackageManager;

namespace Dynamo.PackageManager
{
    /// <summary>
    /// Interaction logic for PackageManagerPublishUI.xaml
    /// </summary>
    public partial class PackageManagerPublishUI : UserControl
    {

        public PackageManagerPublishController Controller { get; internal set; }

        public PackageManagerPublishUI(PackageManagerPublishController controller)
        {
            this.Controller = controller;
            InitializeComponent();

            Binding binding = new Binding() { Source = controller, Path = new PropertyPath("DialogTitle") };
            BindingOperations.SetBinding(this.Title, TextBlock.TextProperty, binding);

        }

        public void Submit_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            Controller.Submit();
        }

    }
}
