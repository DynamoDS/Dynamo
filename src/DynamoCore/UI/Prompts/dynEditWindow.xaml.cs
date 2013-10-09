//Copyright 2013 Ian Keough

//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at

//http://www.apache.org/licenses/LICENSE-2.0

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Utilities;
using Dynamo.ViewModels;

namespace Dynamo.Nodes
{
    /// <summary>
    /// Interaction logic for dynEditWindow.xaml
    /// </summary>
    public partial class dynEditWindow : Window
    {
        public dynEditWindow()
        {
            InitializeComponent();
            //this.Owner = dynSettings.Bench;
            this.Owner = WPF.FindUpVisualTree<DynamoView>(this);
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            this.editText.Focus();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            var expr = editText.GetBindingExpression(TextBox.TextProperty);
            if (expr != null)
            {
                PreUpdateModel(expr.DataItem);
                expr.UpdateSource();
            }

            this.DialogResult = true;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            //cance = return false
            this.DialogResult = false;
        }

        private void PreUpdateModel(object dataItem)
        {
            // Attempt get to the data-bound model (if there's any).
            NodeModel nodeModel = dataItem as NodeModel;
            NoteModel noteModel = dataItem as NoteModel;
            if (null == nodeModel && (null == noteModel))
            {
                NodeViewModel nodeViewModel = dataItem as NodeViewModel;
                if (null != nodeViewModel)
                    nodeModel = nodeViewModel.NodeModel;
                else
                {
                    // TODO(Ben): We temporary do not handle NoteModel here 
                    // because NoteView actively update the data-bound "Text"
                    // property as user types, so when this method is called, 
                    // it will be too late to record the states before the 
                    // text change happened.
                    // 
                    // NoteViewModel noteViewModel = dataItem as NoteViewModel;
                    // if (null != noteViewModel)
                    //     noteModel = noteViewModel.Model;
                }
            }

            // If we do get a node/note, record it for undo.
            if (null != nodeModel || (null != noteModel))
            {
                List<ModelBase> models = new List<ModelBase>();
                if (null != nodeModel) models.Add(nodeModel);
                if (null != noteModel) models.Add(noteModel);

                DynamoModel dynamo = dynSettings.Controller.DynamoModel;
                dynamo.CurrentWorkspace.RecordModelsForModification(models);

                dynSettings.Controller.DynamoViewModel.UndoCommand.RaiseCanExecuteChanged();
                dynSettings.Controller.DynamoViewModel.RedoCommand.RaiseCanExecuteChanged();
            }
        }
    }
}
