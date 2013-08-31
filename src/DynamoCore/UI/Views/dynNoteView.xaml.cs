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

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Dynamo.Selection;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using DynamoCommands = Dynamo.UI.Commands.DynamoCommands;

namespace Dynamo.Nodes
{
    /// <summary>
    /// Interaction logic for dynNoteView.xaml
    /// </summary>
    public partial class dynNoteView : UserControl, IViewModelView<NoteViewModel>
    {
        
        public NoteViewModel ViewModel { get; set; }

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
            noteText.PreviewMouseDown += new MouseButtonEventHandler(noteText_PreviewMouseDown);

            this.Loaded += new RoutedEventHandler(dynNoteView_Loaded);
        }

        void dynNoteView_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel = this.DataContext as NoteViewModel;
            ViewModel.RequestsSelection += new EventHandler(ViewModel_RequestsSelection);
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
            ViewModel.SelectCommand.Execute(null);
        }

        private void editItem_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new dynEditWindow();

            editWindow.editText.TextChanged += delegate
                {
                    var expr = editWindow.editText.GetBindingExpression(TextBox.TextProperty);
                    if (expr != null)
                        expr.UpdateSource();
                };

            //setup a binding with the edit window's text field
            editWindow.editText.DataContext = DataContext as NoteViewModel;
            var bindingVal = new System.Windows.Data.Binding("Text")
            {
                Mode = BindingMode.TwoWay,
                Source = (DataContext as NoteViewModel),
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            editWindow.editText.SetBinding(TextBox.TextProperty, bindingVal);

            if (editWindow.ShowDialog() != true)
            {
                return;
            }

        }

        private void deleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
                dynSettings.Controller.DynamoViewModel.DeleteCommand.Execute(this.ViewModel.Model);
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
