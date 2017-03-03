using System.Windows;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
using System.Diagnostics;
using Dynamo.Configuration;
using System.Windows.Input;
using System.Windows.Controls;
using System;

namespace Dynamo.Prompts
{
    /// <summary>
    /// Interaction logic for NodeHelpPrompt.xaml
    /// </summary>
    public partial class NodeHelpPrompt : Window
    {
        public NodeHelpPrompt(NodeModel node)
        {
            this.DataContext = node;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            InitializeComponent();

            Type type = node.GetType();
            if (type.Namespace == "Dynamo.Graph.Nodes.CustomNodes")
            {
                // Hide the dictionary link if the node is a custom node
                DynamoDictionaryHeight.Height = new GridLength(0);
            }
        }

        private void OpenDynamoDictionary(object sender, MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo("explorer.exe", ((TextBlock)sender).Tag.ToString()));
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            this.Owner.Focus(); //apply focus on the Dynamo window when the Help window is closed
        }
    }
}
