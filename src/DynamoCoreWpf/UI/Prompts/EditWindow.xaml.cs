using System.Windows;
using System.Windows.Controls;

using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.ViewModels;
using DynCmd = Dynamo.Models.DynamoModel;

namespace Dynamo.UI.Prompts
{
    /// <summary>
    /// Interaction logic for dynEditWindow.xaml
    /// </summary>
    public partial class EditWindow
    {
        private readonly DynamoViewModel dynamoViewModel;

        public EditWindow(DynamoViewModel dynamoViewModel,
            bool updateSourceOnTextChange = false)
        {
            InitializeComponent();
            this.dynamoViewModel = dynamoViewModel;

            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;          
            this.editText.Focus();
            
            // do not accept value if user closes 
            this.Closing += (sender, args) => this.DialogResult = false;

            if (false != updateSourceOnTextChange)
            {
                this.editText.TextChanged += delegate
                {
                    var expr = editText.GetBindingExpression(TextBox.TextProperty);
                    if (expr != null)
                        expr.UpdateSource();
                };
            }
        }

        public void BindToProperty(object dataContext, System.Windows.Data.Binding binding)
        {
            if (null != dataContext)
                editText.DataContext = dataContext;

            editText.SetBinding(TextBox.TextProperty, binding);
            editText.SelectAll();
        }

        private void OkClick(object sender, RoutedEventArgs e)
        {
            var expr = editText.GetBindingExpression(TextBox.TextProperty);
            if (expr != null)
            {
                ModelBase model = GetBoundModel(expr.DataItem);
                string propName = expr.ParentBinding.Path.Path;

                dynamoViewModel.ExecuteCommand(
                    new DynCmd.UpdateModelValueCommand(
                        System.Guid.Empty, model.GUID, propName, editText.Text));
            }
           
            this.DialogResult = true;
        }

        private ModelBase GetBoundModel(object dataItem)
        {
            // Attempt get to the data-bound model (if there's any).
            var nodeModel = dataItem as NodeModel;
            var noteModel = dataItem as NoteModel;
            var annotationModel = dataItem as AnnotationModel; 
            if (null == nodeModel && (null == noteModel) && (null == annotationModel))
            {
                var nodeViewModel = dataItem as NodeViewModel;
                if (null != nodeViewModel)
                    nodeModel = nodeViewModel.NodeModel;
                else
                {
                    NoteViewModel noteViewModel = dataItem as NoteViewModel;
                    if (null != noteViewModel)
                        noteModel = noteViewModel.Model;
                    else
                    {
                        AnnotationViewModel annotationViewModel = dataItem as AnnotationViewModel;
                        if (null != annotationViewModel)
                            annotationModel = annotationViewModel.AnnotationModel;
                    }
                }
            }

            if (null != nodeModel)
                return nodeModel;
            else if (null != noteModel)
                return noteModel;
            return annotationModel;
        }
    }
}
