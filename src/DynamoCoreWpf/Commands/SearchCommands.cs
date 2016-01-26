using Dynamo.UI.Commands;

namespace Dynamo.ViewModels
{
    partial class SearchViewModel
    {
        private DelegateCommand focusSearch;
        public DelegateCommand FocusSearchCommand
        {
            get { return focusSearch ?? (focusSearch = new DelegateCommand(FocusSearch, CanFocusSearch)); }
        }

        private DelegateCommand search;
        public DelegateCommand SearchCommand
        {
            get { return search ?? (search = new DelegateCommand(Search, CanSearch)); }
        }

        private DelegateCommand showSearch;
        public DelegateCommand ShowSearchCommand
        {
            get { return showSearch ?? (showSearch = new DelegateCommand(ShowSearch, CanShowSearch)); }
        }

        private DelegateCommand hideSearch;
        public DelegateCommand HideSearchCommand
        {
            get { return hideSearch ?? (hideSearch = new DelegateCommand(HideSearch, CanHideSearch)); }
        }

        public DelegateCommand ImportLibraryCommand
        {
            get { return dynamoViewModel.ImportLibraryCommand; }
        }


        public DelegateCommand ShowPackageManagerSearchCommand
        {
            get { return dynamoViewModel.ShowPackageManagerSearchCommand; }
        }

        private DelegateCommand toggleLayoutCommand;
        public DelegateCommand ToggleLayoutCommand
        {
            get { return toggleLayoutCommand ?? (toggleLayoutCommand = new DelegateCommand(ToggleLayout)); }
        }

        private DelegateCommand selectAllCategoriesCommand;
        public DelegateCommand SelectAllCategoriesCommand
        {
            get
            {
                return selectAllCategoriesCommand ??
                       (selectAllCategoriesCommand = new DelegateCommand(SelectAllCategories));
            }
        }        
    }
}
