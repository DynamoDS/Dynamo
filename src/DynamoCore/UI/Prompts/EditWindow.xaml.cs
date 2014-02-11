using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.Models;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using DynCmd = Dynamo.ViewModels.DynamoViewModel;

namespace Dynamo.UI.Prompts
{
    /// <summary>
    /// Interaction logic for dynEditWindow.xaml
    /// </summary>
    public partial class EditWindow : Window
    {
        public EditWindow(bool updateSourceOnTextChange = false)
        {
            InitializeComponent();

            Owner = WPF.FindUpVisualTree<DynamoView>(this);
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            editText.Focus();

            // do not accept value if user closes 
            Closing += (sender, args) => DialogResult = false;

            if (false != updateSourceOnTextChange)
            {
                editText.TextChanged += delegate
                {
                    var expr = editText.GetBindingExpression(TextBox.TextProperty);
                    if (expr != null)
                        expr.UpdateSource();
                };
            }
        }

        public void BindToProperty(object dataContext, Binding binding)
        {
            if (null != dataContext)
                editText.DataContext = dataContext;

            editText.SetBinding(TextBox.TextProperty, binding);
        }

        private void OkClick(object sender, RoutedEventArgs e)
        {
            var expr = editText.GetBindingExpression(TextBox.TextProperty);
            if (expr != null)
            {
                ModelBase model = GetBoundModel(expr.DataItem);

                string propName = expr.ParentBinding.Path.Path;
                DynamoSettings.Controller.DynamoViewModel.ExecuteCommand(
                    new DynamoViewModel.UpdateModelValueCommand(
                        model.GUID, propName, editText.Text));
            }

            DialogResult = true;
        }

        private ModelBase GetBoundModel(object dataItem)
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
                    NoteViewModel noteViewModel = dataItem as NoteViewModel;
                    if (null != noteViewModel)
                        noteModel = noteViewModel.Model;
                }
            }

            if (null != nodeModel)
                return nodeModel;

            return noteModel;
        }
    }
}
