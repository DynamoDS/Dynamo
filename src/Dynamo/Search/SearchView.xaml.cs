using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using Dynamo.Commands;
using TextBox = System.Windows.Controls.TextBox;
using UserControl = System.Windows.Controls.UserControl;
using System.Windows.Media;

//Copyright © Autodesk, Inc. 2012. All rights reserved.
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

namespace Dynamo.Search
{
    /// <summary>
    ///     Interaction logic for SearchView.xaml
    /// </summary>
    public partial class SearchView : UserControl
    {
        public SearchView(SearchViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();

            PreviewKeyDown += viewModel.KeyHandler;

            SearchTextBox.IsVisibleChanged += delegate
                {
                    SearchTextBox.SelectAll();
                    SearchTextBox.Focus();
                    DynamoCommands.SearchCmd.Execute(null);
                };

            SearchTextBox.GotKeyboardFocus += delegate
            {
                if (SearchTextBox.Text == "Search...")
                {
                    SearchTextBox.Text = "";
                }

                SearchTextBox.Foreground = Brushes.White;
            };

            SearchTextBox.LostKeyboardFocus += delegate
            {
                if (SearchTextBox.Text == "")
                {
                    SearchTextBox.Text = "Search...";
                    SearchTextBox.Foreground = Brushes.Gray;
                }
            };
        }

        public void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ((TextBox) sender).Select(((TextBox) sender).Text.Length, 0);
            BindingExpression binding = ((TextBox) sender).GetBindingExpression(TextBox.TextProperty);
            if (binding != null)
                binding.UpdateSource();
        }

        public void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
           ((SearchViewModel) DataContext).ExecuteSelected();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            ((SearchViewModel) DataContext).RemoveLastPartOfSearchText();
        }
    }
}