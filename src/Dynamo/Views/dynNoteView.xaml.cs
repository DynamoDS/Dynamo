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
using System.Windows.Input;
using Dynamo.Utilities;
using Dynamo.Selection;

namespace Dynamo.Nodes
{
    /// <summary>
    /// Interaction logic for dynNoteView.xaml
    /// </summary>
    public partial class dynNoteView : UserControl, IViewModelView<dynNoteViewModel>
    {
        public dynNoteViewModel ViewModel
        {
            get { return (dynNoteViewModel)DataContext; }
        }

        public dynNoteView()
        {
            InitializeComponent();

            // update the size of the element when the text changes
            noteText.SizeChanged += (sender, args) => ViewModel.UpdateSizeFromView(noteText.ActualWidth, noteText.ActualHeight);
            noteText.PreviewMouseDown += new MouseButtonEventHandler(noteText_PreviewMouseDown);
        }

        void noteText_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ViewModel.SelectCommand.Execute();
        }

        private void editItem_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new dynEditWindow();
            
            //set the text of the edit window to begin
            editWindow.editText.Text = noteText.Text;

            if (editWindow.ShowDialog() != true)
            {
                return;
            }

            //set the value from the text in the box
            noteText.Text = editWindow.editText.Text;
        }

        private void deleteItem_Click(object sender, RoutedEventArgs e)
        {
            dynSettings.Controller.DynamoViewModel.DeleteCommand.Execute(this);
        }

    }
}
