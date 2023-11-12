using System.IO;
using System.Windows;
using System.Windows.Forms;
using CoreNodeModels.Input;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Wpf;

namespace DSCore.File
{
    public class FilenameNodeViewCustomization : FileSystemBrowserNodeViewCustomization, INodeViewCustomization<Filename>
    {
        private Filename model;
        protected WorkspaceModel workspaceModel;

        public void CustomizeView(Filename nodeModel, Dynamo.Controls.NodeView nodeView)
        {
            base.CustomizeView(nodeModel, nodeView);
            this.model = nodeModel;
            workspaceModel = nodeView.ViewModel.WorkspaceViewModel.Model;
        }

        /// <summary>
        /// Handler for browse.. button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void readFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog
            {
                CheckFileExists = false,
                // if the recorded path is absolute path, find parent folder directly
                // if not, convert the relative path to absolute path first
                InitialDirectory = Utilities.IsAbsolutePath(model.Value) ? 
                                        Path.GetDirectoryName(model.Value) : 
                                        (string.IsNullOrEmpty(workspaceModel.FileName) ? string.Empty : 
                                        Path.GetDirectoryName(Utilities.MakeAbsolutePath(workspaceModel.FileName, model.Value)))
            };

            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                var filepath = openDialog.FileName;
                model.HintPath = filepath; //Update the hint path as full path.

                //Evaluate relative path to store as model.Value
                if (workspaceModel != null && !string.IsNullOrEmpty(workspaceModel.FileName))
                {
                    if (filepath == workspaceModel.FileName)
                        filepath = System.IO.Path.GetFileName(filepath);
                    else
                        filepath = Utilities.MakeRelativePath(workspaceModel.FileName, filepath);
                }
                model.Value = filepath; //This assignment will mark the node dirty and trigger evaluation.
            }
        }
    }
}