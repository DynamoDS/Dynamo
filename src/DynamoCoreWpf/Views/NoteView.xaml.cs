using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Dynamo.Selection;
using Dynamo.UI;
using Dynamo.UI.Prompts;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using DynCmd = Dynamo.Models.DynamoModel;

namespace Dynamo.Nodes
{
    public partial class NoteView : IViewModelView<NoteViewModel>
    {
        public NoteViewModel ViewModel { get; private set; }

        public NoteView()
        {
            InitializeComponent();

            // update the size of the element when the text changes
            noteText.SizeChanged += (sender, args) =>
                {
                    if (ViewModel != null)
                        ViewModel.UpdateSizeFromView(noteText.ActualWidth, noteText.ActualHeight);
                };
            noteText.PreviewMouseDown += OnNoteTextPreviewMouseDown;

            Loaded += OnNoteViewLoaded;
            Unloaded += OnNoteViewUnloaded;
        }

        void OnNoteViewLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel = this.DataContext as NoteViewModel;
            ViewModel.RequestsSelection += OnViewModelRequestsSelection;

            // NoteModel has default dimension of 100x100 which will not be ideal in 
            // most cases. Here we update the model according to the size of the view.
            // At this point the view (a TextBlock) would have already been updated 
            // with the bound data, so its size is up-to-date, here we make a call to 
            // update the corresponding model.
            // 
            ViewModel.UpdateSizeFromView(noteText.ActualWidth, noteText.ActualHeight);
        }

        void OnNoteViewUnloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.RequestsSelection -= OnViewModelRequestsSelection;
        }

        void OnViewModelRequestsSelection(object sender, EventArgs e)
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

        void OnNoteTextPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Guid noteGuid = this.ViewModel.Model.GUID;
            ViewModel.WorkspaceViewModel.DynamoViewModel.ExecuteCommand(
                new DynCmd.SelectModelCommand(noteGuid, Keyboard.Modifiers.AsDynamoType()));
        }

        private void OnEditItemClick(object sender, RoutedEventArgs e)
        {
            // Setup a binding with the edit window's text field
            var dynamoViewModel = ViewModel.WorkspaceViewModel.DynamoViewModel;
            var editWindow = new EditWindow(dynamoViewModel, true);
            editWindow.BindToProperty(DataContext, new Binding("Text")
            {
                Mode = BindingMode.TwoWay,
                Source = (DataContext as NoteViewModel),
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            });

            editWindow.ShowDialog();
        }

        private void OnDeleteItemClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
                ViewModel.WorkspaceViewModel.DynamoViewModel.DeleteCommand.Execute(null);
        }

        private void OnNoteMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                OnEditItemClick(this, null);
                e.Handled = true;
            }
        }
    }
}
