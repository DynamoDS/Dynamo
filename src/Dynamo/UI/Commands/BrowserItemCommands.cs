using Dynamo.UI.Commands;

namespace Dynamo.Search
{
    public partial class BrowserItem
    {
        public DelegateCommand _toggleIsExpanded;
        public DelegateCommand ToggleIsExpanded
        {
            get
            {
                if (_toggleIsExpanded == null)
                    _toggleIsExpanded = new DelegateCommand(ToggleIsExpandedExecute, CanToggleIsExpandedCanExecute);
                return _toggleIsExpanded;
            }
        }
    }
}
