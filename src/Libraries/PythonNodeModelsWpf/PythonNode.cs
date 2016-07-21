using System.Windows.Controls;
using System.Windows.Input;

using Dynamo.Controls;
using Dynamo.ViewModels;
using Dynamo.Wpf;

using PythonNodeModels;

namespace PythonNodeModelsWpf
{
    public class PythonNodeViewCustomization : VariableInputNodeViewCustomization, INodeViewCustomization<PythonNode>
    {
        private DynamoViewModel dynamoViewModel;
        private PythonNode model;

        public void CustomizeView(PythonNode nodeModel, NodeView nodeView)
        {
            base.CustomizeView(nodeModel, nodeView);

            model = nodeModel;
            dynamoViewModel = nodeView.ViewModel.DynamoViewModel;

            var editWindowItem = new MenuItem { Header = PythonNodeModels.Properties.Resources.EditHeader, IsCheckable = false };
            nodeView.MainContextMenu.Items.Add(editWindowItem);
            editWindowItem.Click += delegate { EditScriptContent(); };
            nodeView.UpdateLayout();

            nodeView.MouseDown += view_MouseDown;
        }

        private void view_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                EditScriptContent();
                e.Handled = true;
            }
        }

        private void EditScriptContent()
        {
            using (var cmd = Dynamo.Logging.Analytics.TrackCommandEvent("PythonEdit"))
            {
                var editWindow = new ScriptEditorWindow(dynamoViewModel);
                editWindow.Initialize(model.GUID, "ScriptContent", model.Script);
                bool? acceptChanged = editWindow.ShowDialog();
                if (acceptChanged.HasValue && acceptChanged.Value)
                {
                    // Mark node for update
                    model.OnNodeModified();
                }
            }
        }
    }
}
