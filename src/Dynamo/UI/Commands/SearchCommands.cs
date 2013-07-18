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
using System.Windows.Input;
using Dynamo.Utilities;
using Microsoft.Practices.Prism.Commands;

namespace Dynamo.Commands
{

    public static partial class DynamoCommands
    {
        private static DelegateCommand focusSearch;
        public static DelegateCommand FocusSearchCommand
        {
            get
            {
                if (focusSearch == null)
                    focusSearch = new DelegateCommand(FocusSearch, CanFocusSearch);
                return focusSearch;
            }
        }

        private static DelegateCommand search;
        public static DelegateCommand SearchCommand
        {
            get
            {
                if (search == null)
                    search = new DelegateCommand(Search, CanSearch);
                return search;
            }
        }

        private static DelegateCommand showSearch;
        public static DelegateCommand ShowSearchCommand
        {
            get
            {
                if (showSearch == null)
                    showSearch = new DelegateCommand(ShowSearch, CanShowSearch);
                return showSearch;
            }
        }

        private static DelegateCommand hideSearch;
        public static DelegateCommand HideSearchCommand
        {
            get
            {
                if (hideSearch == null)
                    hideSearch = new DelegateCommand(HideSearch, CanHideSearch);
                return hideSearch;
            }
        }

        private static void Search()
        {
            dynSettings.Controller.SearchViewModel.SearchAndUpdateResults();
        }

        private static bool CanSearch()
        {
            return true;
        }

        private static void HideSearch()
        {
            dynSettings.Controller.PackageManagerPublishViewModel.Visible = false;
            dynSettings.Controller.PackageManagerLoginViewModel.Visible = false;
            dynSettings.Controller.SearchViewModel.Visible = false;
        }

        private static bool CanHideSearch()
        {
            if (dynSettings.Controller.SearchViewModel.Visible == true)
                return true;
            return false;
        }

        private static void ShowSearch()
        {
            dynSettings.Controller.SearchViewModel.Visible = true;
        }

        private static bool CanShowSearch()
        {
            if (dynSettings.Controller.SearchViewModel.Visible == false)
                return true;
            return false;
        }

        private static void FocusSearch()
        {
            dynSettings.Controller.SearchViewModel.OnRequestFocusSearch(dynSettings.Controller.DynamoViewModel, EventArgs.Empty);
        }

        private static bool CanFocusSearch()
        {
            return true;
        }
    }

    //public class SearchCommand : ICommand
    //{
    //    public void Execute(object parameters)
    //    {
    //        dynSettings.Controller.SearchViewModel.SearchAndUpdateResults();
    //    }

    //    public event EventHandler CanExecuteChanged
    //    {
    //        add { CommandManager.RequerySuggested += value; }
    //        remove { CommandManager.RequerySuggested -= value; }
    //    }

    //    public bool CanExecute(object parameters)
    //    {
    //        return true;
    //    }
    //}

    //public class HideSearchCommand : ICommand
    //{

    //    public void Execute(object parameters)
    //    {
    //        if (dynSettings.Controller.PackageManagerLoginViewModel.Visible == Visibility.Visible)
    //        {
    //            dynSettings.Controller.PackageManagerLoginViewModel.Visible = Visibility.Collapsed;
    //            return;
    //        }

    //        if (dynSettings.Controller.PackageManagerPublishViewModel.Visible == Visibility.Visible)
    //        {
    //            dynSettings.Controller.PackageManagerPublishViewModel.Visible = Visibility.Collapsed;
    //            return;
    //        }

    //        dynSettings.Controller.SearchViewModel.Visible = Visibility.Collapsed;

    //    }

    //    public event EventHandler CanExecuteChanged
    //    {
    //        add { CommandManager.RequerySuggested += value; }
    //        remove { CommandManager.RequerySuggested -= value; }
    //    }

    //    public bool CanExecute(object parameters)
    //    {
    //        return (dynSettings.Controller.SearchViewModel.Visible == Visibility.Visible);
    //    }
    //}

    //public class FocusSearchCommand : ICommand {

    //    public void Execute(object parameters)
    //    {
    //        ShowSearchCommand.search.SearchTextBox.Focus();
    //    }

    //    public event EventHandler CanExecuteChanged
    //    {
    //        add { CommandManager.RequerySuggested += value; }
    //        remove { CommandManager.RequerySuggested -= value; }
    //    }

    //    public bool CanExecute(object parameters)
    //    {
    //        return true;
    //    }
    //}

    //public class ShowSearchCommand : ICommand
    //{
    //    public static SearchView search;

    //    public void Execute(object parameters)
    //    {

    //        search = new SearchView(dynSettings.Controller.SearchViewModel);

    //        dynSettings.Bench.sidebarGrid.Children.Add(search);
    //        dynSettings.Controller.SearchViewModel.Visible = Visibility.Visible;
    //    }

    //    public event EventHandler CanExecuteChanged
    //    {
    //        add { CommandManager.RequerySuggested += value; }
    //        remove { CommandManager.RequerySuggested -= value; }
    //    }

    //    public bool CanExecute(object parameters)
    //    {
    //        return (dynSettings.Controller.SearchViewModel.Visible != Visibility.Visible);
    //    }
    //}
}
