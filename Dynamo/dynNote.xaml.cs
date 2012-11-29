//Copyright 2012 Ian Keough

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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Dynamo.Utilities;
using Dynamo.Controls;

namespace Dynamo.Elements
{
    /// <summary>
    /// Interaction logic for dynNote.xaml
    /// </summary>
    public partial class dynNote : UserControl
    {
        public dynNote()
        {
            InitializeComponent();

            noteText.PreviewMouseDown += new MouseButtonEventHandler(noteText_PreviewMouseDown);
        }

        void noteText_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!dynElementSettings.SharedInstance.Bench.SelectedElements.Contains(this))
            {
                dynElementSettings.SharedInstance.Bench.SelectedElements.Add(this);
            }
        }

        private void editItem_Click(object sender, RoutedEventArgs e)
        {
            dynEditWindow editWindow = new dynEditWindow();

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
            var bench = dynElementSettings.SharedInstance.Bench;

            IdlePromise.ExecuteOnIdle(
               delegate
               {
                   bench.DeleteElement(this);
               },
               true
            );
        }
    }
}
