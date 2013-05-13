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

using System;
using System.Windows;
using System.Windows.Input;
using Dynamo.Search;
using Dynamo.Utilities;
using Dynamo.Controls;

namespace Dynamo.Commands
{

    public static partial class DynamoCommands
    {
        private static FocusSearchCommand focusSearch;
        public static FocusSearchCommand FocusSearch
        {
            get
            {
                if (focusSearch == null)
                    focusSearch = new FocusSearchCommand();
                return focusSearch;
            }
        }

        private static SearchCommand search;
        public static SearchCommand Search
        {
            get
            {
                if (search == null)
                    search = new SearchCommand();
                return search;
            }
        }

        private static ShowSearchCommand showSearch;
        public static ShowSearchCommand ShowSearch
        {
            get
            {
                if (showSearch == null)
                    showSearch = new ShowSearchCommand();
                return showSearch;
            }
        }

        private static HideSearchCommand hideSearch;
        public static HideSearchCommand HideSearch
        {
            get
            {
                if (hideSearch == null)
                    hideSearch = new HideSearchCommand();
                return hideSearch;
            }
        }
    }

    public class SearchCommand : ICommand
    {
        public void Execute(object parameters)
        {
            dynSettings.Controller.SearchViewModel.SearchAndUpdateResults();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameters)
        {
            return true;
        }
    }

    public class HideSearchCommand : ICommand
    {

        public void Execute(object parameters)
        {
            if (dynSettings.Controller.PackageManagerLoginViewModel.Visible == Visibility.Visible)
            {
                dynSettings.Controller.PackageManagerLoginViewModel.Visible = Visibility.Collapsed;
                return;
            }

            if (dynSettings.Controller.PackageManagerPublishViewModel.Visible == Visibility.Visible)
            {
                dynSettings.Controller.PackageManagerPublishViewModel.Visible = Visibility.Collapsed;
                return;
            }

            dynSettings.Controller.SearchViewModel.Visible = Visibility.Collapsed;

        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameters)
        {
            return (dynSettings.Controller.SearchViewModel.Visible == Visibility.Visible);
        }
    }

    public class FocusSearchCommand : ICommand {

        public void Execute(object parameters)
        {
            ShowSearchCommand.search.SearchTextBox.Focus();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameters)
        {
            return true;
        }
    }

    public class ShowSearchCommand : ICommand
    {
        public static SearchView search;

        public void Execute(object parameters)
        {

            search = new SearchView(dynSettings.Controller.SearchViewModel); 
                
            dynSettings.Bench.sidebarGrid.Children.Add(search);
            dynSettings.Controller.SearchViewModel.Visible = Visibility.Visible;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameters)
        {
            return (dynSettings.Controller.SearchViewModel.Visible != Visibility.Visible);
        }
    }
}
