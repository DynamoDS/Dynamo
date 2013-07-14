using System.Windows;
using Dynamo.Controls;
using Dynamo.Nodes;
using Dynamo.Utilities;

namespace Dynamo.Prompts
{
    /// <summary>
    /// Interaction logic for NodeHelpPrompt.xaml
    /// </summary>
    public partial class NodeHelpPrompt : Window
    {
        public NodeHelpPrompt(dynNodeModel node)
        {
            this.DataContext = node;
            this.Owner = WPF.FindUpVisualTree<DynamoView>(this);
            //this.Owner = dynSettings.Bench;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            InitializeComponent();
        }
    }
}
