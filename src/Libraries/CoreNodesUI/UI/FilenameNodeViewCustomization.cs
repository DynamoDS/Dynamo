using System.Windows;
using System.Windows.Forms;

using Dynamo.Wpf;

namespace DSCore.File
{
    public class FilenameNodeViewCustomization : FileSystemBrowserNodeViewCustomization, INodeViewCustomization<Filename>
    {
        private Filename model;

        public void CustomizeView(Filename nodeModel, Dynamo.Controls.NodeView nodeView)
        {
            base.CustomizeView(nodeModel, nodeView);
            this.model = nodeModel;
        }

        protected override void readFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog
            {
                CheckFileExists = false
            };

            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                model.Value = openDialog.FileName;
            }
        }
    }
}