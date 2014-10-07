using Dynamo.UI.Commands;

namespace Dynamo.ViewModels
{
    partial class SearchViewModel
    {
        private DelegateCommand focusSearch;
        public DelegateCommand FocusSearchCommand
        {
            get
            {
                if (focusSearch == null)
                    focusSearch = new DelegateCommand(this.FocusSearch, this.CanFocusSearch);
                return focusSearch;
            }
        }

        private DelegateCommand search;
        public DelegateCommand SearchCommand
        {
            get
            {
                if (search == null)
                    search = new DelegateCommand(this.Search, this.CanSearch);
                return search;
            }
        }

        private DelegateCommand showSearch;
        public DelegateCommand ShowSearchCommand
        {
            get
            {
                if (showSearch == null)
                    showSearch = new DelegateCommand(this.ShowSearch, this.CanShowSearch);
                return showSearch;
            }
        }

        private DelegateCommand hideSearch;
        public DelegateCommand HideSearchCommand
        {
            get
            {
                if (hideSearch == null)
                    hideSearch = new DelegateCommand(this.HideSearch, this.CanHideSearch);
                return hideSearch;
            }
        }

    }
}
