using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dynamo.Commands;
using Dynamo.Controls;
using Dynamo.Search;
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
    /// Interaction logic for SearchView.xaml
    /// </summary>
    public partial class SearchView : UserControl
    {

        public SearchView( SearchViewModel viewModel )
        {
            this.DataContext = viewModel;
            InitializeComponent();
     
            this.PreviewKeyDown += viewModel.KeyHandler;

            SearchTextBox.IsVisibleChanged += delegate
                {
                    SearchTextBox.Focus();
                    DynamoCommands.SearchCmd.Execute(null);
                };
        }

        public void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ((TextBox)sender).Select(((TextBox)sender).Text.Length, 0);
            var binding = ((TextBox)sender).GetBindingExpression(TextBox.TextProperty);
            if (binding != null)
                binding.UpdateSource();
        }

    }
}
