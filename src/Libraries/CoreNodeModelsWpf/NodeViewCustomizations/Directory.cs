using System.Windows;
using System.Windows.Forms;
using CoreNodeModels.Input;
using DSCore.File;
using Dynamo.Controls;
using Dynamo.Wpf;

namespace CoreNodeModelsWpf.Nodes
{
    public class DirectoryNodeViewCustomization : FileSystemBrowserNodeViewCustomization, INodeViewCustomization<Directory>
    {
        private Directory model;

        public void CustomizeView(Directory model, NodeView nodeView)
        {
            base.CustomizeView(model, nodeView);
            this.model = model;
        }

        protected override void readFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new FolderBrowserDialog
            {
                ShowNewFolderButton = true
            };

            if (openDialog.ShowDialog() == DialogResult.OK)
                model.Value = openDialog.SelectedPath;
        }

    }
}