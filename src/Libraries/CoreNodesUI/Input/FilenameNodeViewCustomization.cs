using System.Windows;
using System.Windows.Forms;

using Dynamo.Wpf;

namespace DSCore.File
{
    public class FilenameNodeViewCustomization : FileSystemBrowserNodeViewCustomization, INodeViewCustomization<Filename>
    {
        private Filename model;

        public void CustomizeView(Filename model, Dynamo.Controls.dynNodeView view)
        {
            this.model = model;
            base.CustomizeView(model, view); 
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