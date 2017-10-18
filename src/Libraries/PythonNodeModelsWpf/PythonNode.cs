using System.Windows.Controls;
using System.Windows.Input;

using Dynamo.Controls;
using Dynamo.ViewModels;
using Dynamo.Wpf;

using PythonNodeModels;
using System;

namespace PythonNodeModelsWpf
{
    public class PythonNodeViewCustomization : VariableInputNodeViewCustomization, INodeViewCustomization<PythonNode>
    {
        private DynamoViewModel dynamoViewModel;
        private PythonNode model;
        private ScriptEditorWindow editWindow;

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

        public void view_EditWindowClosed(object sender, EventArgs e)
        {
            editWindow = null;
        }

        private void EditScriptContent()
        {
            using (var cmd = Dynamo.Logging.Analytics.TrackCommandEvent("PythonEdit"))
            {
                if (editWindow != null)
                {
                    editWindow.Activate();
                }
                else
                {
                    editWindow = new ScriptEditorWindow(dynamoViewModel, model);
                    editWindow.Initialize(model.GUID, "ScriptContent", model.Script);
                    editWindow.Closed += this.view_EditWindowClosed;
                    System.Windows.Application.Current.MainWindow.Closing += delegate { Application_Exit(); };
                    editWindow.Show();
                }
            }
        }

        private void Application_Exit()
        {
            if (editWindow != null)
            {
                editWindow.Close();
            }
        }
    }
}
