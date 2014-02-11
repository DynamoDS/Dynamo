using System.Windows;
using Dynamo.Models;

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
        }
    }
}
