using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Dynamo.Controls;
using Dynamo.Search.Regions;
using Dynamo.ViewModels;
using TextBox = System.Windows.Controls.TextBox;
using UserControl = System.Windows.Controls.UserControl;
using System.Windows.Media;
using Dynamo.Utilities;
using DynamoCommands = Dynamo.UI.Commands.DynamoCommands;

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
        private SearchViewModel _viewModel;

        public SearchView()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(SearchView_Loaded);

            SearchTextBox.IsVisibleChanged += delegate
            {
                if (SearchTextBox.IsVisible)
                {
                    DynamoCommands.SearchCommand.Execute(null);
                    Keyboard.Focus(this.SearchTextBox);
                    var view = WPF.FindUpVisualTree<DynamoView>(this);
                    SearchTextBox.InputBindings.AddRange(view.InputBindings);
                }
            };
        }

        void SearchView_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = _viewModel = dynSettings.Controller.SearchViewModel;

            PreviewKeyDown += KeyHandler;

            dynSettings.Controller.SearchViewModel.RequestFocusSearch += new EventHandler(SearchViewModel_RequestFocusSearch);
            dynSettings.Controller.SearchViewModel.RequestReturnFocusToSearch += new EventHandler(SearchViewModel_RequestReturnFocusToSearch);

            //setup the regions on the view model
            _viewModel.Regions = new ObservableDictionary<string, RegionBase>();
            //Regions.Add("Include Nodes from Package Manager", DynamoCommands.PackageManagerRegionCommand );
            var region = new RevitAPIRegion(SearchViewModel.RevitAPIRegionExecute, SearchViewModel.RevitAPIRegionCanExecute);
            region.RaiseCanExecuteChanged();
            _viewModel.Regions.Add("Include Experimental Revit API Nodes", region);

        }

        /// <summary>
        ///     A KeyHandler method used by SearchView, increments decrements and executes based on input.
        /// </summary>
        /// <param name="sender">Originating object for the KeyHandler </param>
        /// <param name="e">Parameters describing the key push</param>
        public void KeyHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                _viewModel.ExecuteSelected();
            }
            else if (e.Key == Key.Tab)
            {
                _viewModel.PopulateSearchTextWithSelectedResult();
            }
            else if (e.Key == Key.Down)
            {
                _viewModel.SelectNext();
            }
            else if (e.Key == Key.Up)
            {
                _viewModel.SelectPrevious();
            }
        }

        void SearchViewModel_RequestReturnFocusToSearch(object sender, EventArgs e)
        {
            Keyboard.Focus(SearchTextBox);
        }

        void SearchViewModel_RequestFocusSearch(object sender, EventArgs e)
        {
            SearchTextBox.Focus();
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

        public void ListBoxItem_Click(object sender, RoutedEventArgs e)
        {
            ((ListBoxItem) sender).IsSelected = true;
            Keyboard.Focus(this.SearchTextBox);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            ((SearchViewModel) DataContext).RemoveLastPartOfSearchText();
            Keyboard.Focus(this.SearchTextBox);
        }

        public void ibtnServiceController_MouseLeftButtonUp(object sender, RoutedEventArgs e)
        {
            //RegionMenu.PlacementTarget = (UIElement) sender;
            //RegionMenu.IsOpen = true;
        }

        private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine(sender);
        }
    }
} ;