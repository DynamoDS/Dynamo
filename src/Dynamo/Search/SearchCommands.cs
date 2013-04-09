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

namespace Dynamo.Commands
{

    public static partial class DynamoCommands
    {

        private static SearchCommand searchCmd;
        public static SearchCommand SearchCmd
        {
            get
            {
                if (searchCmd == null)
                    searchCmd = new SearchCommand();
                return searchCmd;
            }
        }

        private static ShowSearchCommand showSearchCmd;
        public static ShowSearchCommand ShowSearchCmd
        {
            get
            {
                if (showSearchCmd == null)
                    showSearchCmd = new ShowSearchCommand();
                return showSearchCmd;
            }
        }

        private static HideSearchCommand hideSearchCmd;
        public static HideSearchCommand HideSearchCmd
        {
            get
            {
                if (hideSearchCmd == null)
                    hideSearchCmd = new HideSearchCommand();
                return hideSearchCmd;
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
    
    public class HideSearchCommand : ICommand {

        public void Execute(object parameters)
        {
            dynSettings.Controller.SearchViewModel.Visible = Visibility.Collapsed;
            dynSettings.Controller.PackageManagerLoginViewModel.Visible = Visibility.Collapsed;
            dynSettings.Controller.PackageManagerPublishViewModel.Visible = Visibility.Collapsed;
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

    public class ShowSearchCommand : ICommand
    {
        private SearchView search;
        private static bool init = false;

        public void Execute(object parameters)
        {
            if (!init)
            {
                search = new SearchView(dynSettings.Controller.SearchViewModel);
                dynSettings.Bench.outerCanvas.Children.Add(search);
                init = true;
            }

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
