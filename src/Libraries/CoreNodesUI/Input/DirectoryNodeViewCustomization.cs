using System.Windows;
using System.Windows.Forms;

using Dynamo.Wpf;

namespace DSCore.File
{
    public class DirectoryNodeViewCustomization : FileSystemBrowserNodeViewCustomization, INodeViewCustomization<Directory>
    {
        private Directory model;

        public void CustomizeView(Directory model, Dynamo.Controls.dynNodeView view)
        {
            this.model = model;
            base.CustomizeView(model, view);
        }

        protected override void readFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new FolderBrowserDialog
            {
                ShowNewFolderButton = true
            };

            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                model.Value = openDialog.SelectedPath;
            }
        }
    }
}