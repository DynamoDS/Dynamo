using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Dynamo.Selection;
using Dynamo.UI;
using Dynamo.UI.Prompts;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using DynCmd = Dynamo.ViewModels.DynamoViewModel;

namespace Dynamo.Nodes
{
    /// <summary>
    /// Interaction logic for dynNoteView.xaml
    /// </summary>
    public partial class dynNoteView : IViewModelView<NoteViewModel>
    {
        public NoteViewModel ViewModel { get; private set; }

        public dynNoteView()
        {
            InitializeComponent();

            // for debugging purposes
            this.DataContextChanged += OnDataContextChanged;

            // update the size of the element when the text changes
            noteText.SizeChanged += (sender, args) =>
                {
                    if (ViewModel != null)
                        ViewModel.UpdateSizeFromView(noteText.ActualWidth, noteText.ActualHeight);
                };
            noteText.PreviewMouseDown += noteText_PreviewMouseDown;

            this.Loaded += dynNoteView_Loaded;
        }

        void dynNoteView_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel = this.DataContext as NoteViewModel;
            ViewModel.RequestsSelection += ViewModel_RequestsSelection;

            // NoteModel has default dimension of 100x100 which will not be ideal in 
            // most cases. Here we update the model according to the size of the view.
            // At this point the view (a TextBlock) would have already been updated 
            // with the bound data, so its size is up-to-date, here we make a call to 
            // update the corresponding model.
            // 
            ViewModel.UpdateSizeFromView(noteText.ActualWidth, noteText.ActualHeight);
        }

        void ViewModel_RequestsSelection(object sender, EventArgs e)
        {
            if (!ViewModel.Model.IsSelected)
            {
                if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
                {
                    DynamoSelection.Instance.ClearSelection();
                }

                if (!DynamoSelection.Instance.Selection.Contains(ViewModel.Model))
                {
                    DynamoSelection.Instance.Selection.Add(ViewModel.Model);
                }

            }
            else
            {
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    DynamoSelection.Instance.Selection.Remove(ViewModel.Model);
                }
            }
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            
        }

        void noteText_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Guid noteGuid = this.ViewModel.Model.GUID;
            dynSettings.Controller.DynamoViewModel.ExecuteCommand(
                new DynCmd.SelectModelCommand(noteGuid, Keyboard.Modifiers));
        }

        private void editItem_Click(object sender, RoutedEventArgs e)
        {
            // Setup a binding with the edit window's text field
            var editWindow = new EditWindow(true);
            editWindow.BindToProperty(DataContext, new Binding("Text")
            {
                Mode = BindingMode.TwoWay,
                Source = (DataContext as NoteViewModel),
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            });

            editWindow.ShowDialog();
        }

        private void deleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
                dynSettings.Controller.DynamoViewModel.DeleteCommand.Execute(null);
        }

        private void Note_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                editItem_Click(this, null);
                e.Handled = true;
            }
        }
    }
}
