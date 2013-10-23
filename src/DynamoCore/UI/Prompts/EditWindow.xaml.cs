using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Utilities;
using Dynamo.ViewModels;

namespace Dynamo.UI.Prompts
{
    /// <summary>
    /// Interaction logic for dynEditWindow.xaml
    /// </summary>
    public partial class EditWindow : Window
    {
        public EditWindow()
        {
            InitializeComponent();

            this.Owner = WPF.FindUpVisualTree<DynamoView>(this);
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            this.editText.Focus();

            // do not accept value if user closes 
            this.Closing += (sender, args) => this.DialogResult = false;
        }

        private void OkClick(object sender, RoutedEventArgs e)
        {
            var expr = editText.GetBindingExpression(TextBox.TextProperty);
            if (expr != null)
            {
                PreUpdateModel(expr.DataItem);
                expr.UpdateSource();
            }

            this.DialogResult = true;
        }

        private void PreUpdateModel(object dataItem)
        {
            // Attempt get to the data-bound model (if there's any).
            var nodeModel = dataItem as NodeModel;
            var noteModel = dataItem as NoteModel;
            if (null == nodeModel && (null == noteModel))
            {
                var nodeViewModel = dataItem as NodeViewModel;
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
                var models = new List<ModelBase>();
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
