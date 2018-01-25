﻿using System.Windows.Controls;
using System.Windows.Input;

using Dynamo.Controls;
using Dynamo.ViewModels;
using Dynamo.Wpf;
using Dynamo.Wpf.Windows;

using PythonNodeModels;
using System;
using System.Windows;

namespace PythonNodeModelsWpf
{
    public class PythonNodeViewCustomization : VariableInputNodeViewCustomization, INodeViewCustomization<PythonNode>
    {
        private DynamoViewModel dynamoViewModel;
        private PythonNode model;
        private NodeView view;
        private ScriptEditorWindow editWindow;
        private ModelessChildWindow.WindowRect editorWindowRect;

        public void CustomizeView(PythonNode nodeModel, NodeView nodeView)
        {
            base.CustomizeView(nodeModel, nodeView);

            model = nodeModel;
            view = nodeView;
            dynamoViewModel = nodeView.ViewModel.DynamoViewModel;

            var editWindowItem = new MenuItem { Header = PythonNodeModels.Properties.Resources.EditHeader, IsCheckable = false };
            nodeView.MainContextMenu.Items.Add(editWindowItem);
            editWindowItem.Click += delegate { EditScriptContent(); };
            nodeView.UpdateLayout();

            nodeView.MouseDown += view_MouseDown;
            nodeModel.DeletionStarted += NodeModel_DeletionStarted;
            nodeModel.Disposed += NodeModel_Disposed;
        }

        private void NodeModel_Disposed(Dynamo.Graph.ModelBase obj)
        {
            if (editWindow != null)
            {
                editWindow.Close();
            }
        }

        private void NodeModel_DeletionStarted(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (editWindow != null)
            {
                var res = MessageBox.Show(
                    String.Format(
                        PythonNodeModels.Properties.Resources.DeletingPythonNodeWithOpenEditorMessage, 
                        this.model.Name),
                    PythonNodeModels.Properties.Resources.DeletingPythonNodeWithOpenEditorTitle,
                    MessageBoxButton.OKCancel, 
                    MessageBoxImage.Question);

                if (res == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        private void view_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                EditScriptContent();
                e.Handled = true;
            }
        }

        public void editWindow_Closed(object sender, EventArgs e)
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
                    editWindow = new ScriptEditorWindow(dynamoViewModel, model, view, ref editorWindowRect);
                    editWindow.Initialize(model.GUID, "ScriptContent", model.Script);
                    editWindow.Closed += editWindow_Closed;
                    editWindow.Show();
                }
            }
        }
    }
}
