using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dynamo.Graph;
using Dynamo.Graph.Annotations;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Notes;
using Dynamo.ViewModels;
using DynCmd = Dynamo.Models.DynamoModel;

namespace Dynamo.UI.Prompts
{
    /// <summary>
    /// Interaction logic for dynEditWindow.xaml
    /// </summary>
    public partial class EditWindow
    {
        /// <summary>
        /// Event analyses when a key has been typed on the edit window, 
        /// used to alter the behaviour of certain keys, 
        /// for example for adding bullet point support.         
        /// </summary>
        internal event EventHandler<KeyEventArgs> EditTextBoxPreviewKeyDown;
        private readonly DynamoViewModel dynamoViewModel;
        private bool CommitChangesOnReturn { get; set; }

        public EditWindow(DynamoViewModel dynamoViewModel,
            bool updateSourceOnTextChange = false, bool commitChangesOnReturn = false)
        {
            this.CommitChangesOnReturn = commitChangesOnReturn;

            InitializeComponent();
            this.dynamoViewModel = dynamoViewModel;

            Owner = dynamoViewModel.Owner;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;

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
            this.editText.PreviewKeyDown += EditText_PreviewKeyDown;
            this.Closed += EditWindow_Closed;
            this.ContentRendered += OnContentRendered;
        }

        // Centralize the window correctly after it is rendered
        private void OnContentRendered(object sender, EventArgs e)
        {
            // Unsubscribe immediately, we only call this once on initialization
            this.ContentRendered -= OnContentRendered;

            CenterWindowRelativeToOwner(); 
            editText.Focus(); 
        }

        // Centralize the window relative to another Window
        private void CenterWindowRelativeToOwner()
        {
            if (Owner != null)
            {
                this.Left = Owner.Left + (Owner.Width - this.ActualWidth) / 2;
                this.Top = Owner.Top + (Owner.Height - this.ActualHeight) / 2;
            }
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MinimizeButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_OnClick(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).Name.Equals("MaximizeButton"))
            {
                this.WindowState = WindowState.Maximized;
                ToggleButtons(true);
            }
            else
            {
                this.WindowState = WindowState.Normal;
                ToggleButtons(false);
            }
        }


        /// <summary>
        /// Toggles between the Maximize and Normalize buttons on the window
        /// </summary>
        /// <param name="toggle"></param>
        private void ToggleButtons(bool toggle)
        {
            if (toggle)
            {
                this.MaximizeButton.Visibility = Visibility.Collapsed;
                this.NormalizeButton.Visibility = Visibility.Visible;
            }
            else
            {
                this.MaximizeButton.Visibility = Visibility.Visible;
                this.NormalizeButton.Visibility = Visibility.Collapsed;
            }
        }

        private void EditText_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            EditTextBoxPreviewKeyDown?.Invoke(sender, e);
        }

        private void OnEditWindowPreviewKeyDown(object sender, KeyEventArgs e)
        {
           if(CommitChangesOnReturn && (e.Key == Key.Return || e.Key == Key.Enter))
           {
               UpdateNodeName();
               e.Handled = true;
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
            UpdateNodeName();
        }
        private void UpdateNodeName()
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

        /// <summary>
        /// Lets the user drag this window around with their left mouse button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;
            DragMove();
        }

        private void EditWindow_Closed(object sender, EventArgs e)
        {
            this.editText.PreviewKeyDown -= EditText_PreviewKeyDown;
            this.Closed -= EditWindow_Closed;
        }

        // ESC Button pressed triggers Window close        
        private void OnCloseExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }
    }
}
