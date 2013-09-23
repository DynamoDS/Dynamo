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

using Dynamo.Search.Regions;
using Dynamo.Utilities;
using Dynamo.ViewModels;

namespace Dynamo.UI.Commands
{
    public static partial class DynamoCommands
    {
        private static SearchViewModel _vm_search =
            dynSettings.Controller.SearchViewModel;

        private static DelegateCommand focusSearch;
        public static DelegateCommand FocusSearchCommand
        {
            get
            {
                if (focusSearch == null)
                    focusSearch = new DelegateCommand(_vm_search.FocusSearch, _vm_search.CanFocusSearch);
                return focusSearch;
            }
        }

        private static DelegateCommand search;
        public static DelegateCommand SearchCommand
        {
            get
            {
                if (search == null)
                    search = new DelegateCommand(_vm_search.Search, _vm_search.CanSearch);
                return search;
            }
        }

        private static DelegateCommand showSearch;
        public static DelegateCommand ShowSearchCommand
        {
            get
            {
                if (showSearch == null)
                    showSearch = new DelegateCommand(_vm_search.ShowSearch, _vm_search.CanShowSearch);
                return showSearch;
            }
        }

        private static DelegateCommand hideSearch;
        public static DelegateCommand HideSearchCommand
        {
            get
            {
                if (hideSearch == null)
                    hideSearch = new DelegateCommand(_vm_search.HideSearch, _vm_search.CanHideSearch);
                return hideSearch;
            }
        }

        private static DelegateCommand showLibItemPopup;
        public static DelegateCommand ShowLibItemPopupCommand
        {
            get
            {
                if (showLibItemPopup == null)
                    showLibItemPopup = new DelegateCommand(_vm_search.ShowLibItemPopup, _vm_search.CanShowLibItemPopup);
                return showLibItemPopup;
            }
        }

        private static DelegateCommand hideLibItemPopup;
        public static DelegateCommand HideLibItemPopupCommand
        {
            get
            {
                if (hideLibItemPopup == null)
                    hideLibItemPopup = new DelegateCommand(_vm_search.HideLibItemPopup, _vm_search.CanHideLibItemPopup);
                return hideLibItemPopup;
            }
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

namespace Dynamo.ViewModels
{
    public partial class SearchViewModel
    {
        /// <summary>
        ///     Regions property
        /// </summary>
        /// <value>
        ///     Specifies different regions to search over.  The command toggles whether searching
        ///     over that field or not.
        /// </value>
        public ObservableDictionary<string, RegionBase> Regions { get; set; }
    }
}