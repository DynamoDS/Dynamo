using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Dynamo.Configuration;
using Dynamo.Graph.Nodes;
using Dynamo.ViewModels;

namespace Dynamo.Prompts
{
    /// <summary>
    /// Interaction logic for NodeHelpPrompt.xaml
    /// </summary>
    public partial class NodeHelpPrompt : Window
    {
        public NodeHelpPrompt(NodeModel node)
        {
            DataContext = node;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            InitializeComponent();
            
            if (node.IsCustomFunction)
            {
                // Hide the dictionary link if the node is a custom node
                DynamoDictionaryHeight.Height = new GridLength(0);
            }
        }

        private void OpenDynamoDictionary(object sender, MouseButtonEventArgs e)
        {
            var dynView = this.Owner.DataContext as DynamoViewModel;
            var node = this.DataContext as NodeModel;
            if (dynView != null && node != null)
            {
                node.DictionaryLink = node.ConstructDictionaryLinkFromLibrary(dynView.Model.LibraryServices);
                Process.Start(new ProcessStartInfo("explorer.exe", node.DictionaryLink));
            }
            else
            {
                Process.Start(new ProcessStartInfo("explorer.exe", Configurations.DynamoDictionary));
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            this.Owner.Focus(); //apply focus on the Dynamo window when the Help window is closed
        }
    }
}
