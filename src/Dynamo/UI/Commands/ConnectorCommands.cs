
using Microsoft.Practices.Prism.Commands;

namespace Dynamo.Connectors
{
    public partial class dynConnectorViewModel
    {
        private DelegateCommand<object> _connectCommand;
        private DelegateCommand _redrawCommand;
        private DelegateCommand _highlightCommand;
        private DelegateCommand _unHighlightCommand;

        public DelegateCommand<object> ConnectCommand
        {
            get
            {
                if(_connectCommand == null)
                    _connectCommand = new DelegateCommand<object>(Connect, CanConnect);

                return _connectCommand;
            }
        }

        public DelegateCommand RedrawCommand
        {
            get
            {
                if(_redrawCommand == null)
                    _redrawCommand = new DelegateCommand(Redraw, CanRedraw);

                return _redrawCommand;
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
                    _unHighlightCommand = new DelegateCommand(Unhighlight, CanUnHighlight);

                return _unHighlightCommand;
            }
        }
    }
}
