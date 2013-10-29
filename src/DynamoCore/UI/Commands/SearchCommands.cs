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

        private static DelegateCommand showLibItemInfoBubble;

        public static DelegateCommand ShowLibItemInfoBubbleCommand
        {
            get
            {
                if (showLibItemInfoBubble == null)
                    showLibItemInfoBubble = new DelegateCommand(_vm_search.ShowLibItemInfoBubble,
                        _vm_search.CanShowLibItemInfoBubble);
                return showLibItemInfoBubble;
            }
        }

        private static DelegateCommand hideLibItemInfoBubble;

        public static DelegateCommand HideLibItemInfoBubbleCommand
        {
            get
            {
                if (hideLibItemInfoBubble == null)
                    hideLibItemInfoBubble = new DelegateCommand(_vm_search.HideLibItemInfoBubble,
                        _vm_search.CanHideLibItemInfoBubble);
                return hideLibItemInfoBubble;
            }
        }
    }
}