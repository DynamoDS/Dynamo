using Dynamo.UI.Commands;

namespace Dynamo.ViewModels
{
    public partial class PortViewModel
    {
        //private DelegateCommand _setCenterCommand;
        private DelegateCommand _connectCommand;
        private DelegateCommand _highlightCommand;
        private DelegateCommand _unHighlightCommand;

        //public DelegateCommand SetCenterCommand
        //{
        //    get { return _setCenterCommand; }
        //}

        public DelegateCommand ConnectCommand
        {
            get
            {
                if(_connectCommand == null)
                    _connectCommand = new DelegateCommand(Connect, CanConnect);

                return _connectCommand;
            }
        }

        public DelegateCommand HighlightCommand
        {
            get
            {
                if(_highlightCommand == null)
                    _highlightCommand = new DelegateCommand(Highlight, CanHighlight);

                return _highlightCommand;
            }
        }

        public DelegateCommand UnHighlightCommand
        {
            get
            {
                if(_unHighlightCommand == null)
                    _unHighlightCommand = new DelegateCommand(UnHighlight, CanUnHighlight);

                return _unHighlightCommand;
            }
        }
    }
}
