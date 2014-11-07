using Dynamo.UI.Commands;

namespace Dynamo.ViewModels
{
    public partial class PortViewModel
    {
        private DelegateCommand _connectCommand;
        private DelegateCommand _rectangleMouseEnterCommand;
        private DelegateCommand _rectangleMouseLeaveCommand;
        private DelegateCommand _rectangleMouseLeftButtonCommand;
        public DelegateCommand ConnectCommand
        {
            get
            {
                if(_connectCommand == null)
                    _connectCommand = new DelegateCommand(Connect, CanConnect);

                return _connectCommand;
            }
        }

        public DelegateCommand MouseEnterCommand
        {
            get
            {
                if (_rectangleMouseEnterCommand == null)
                    _rectangleMouseEnterCommand = new DelegateCommand(OnRectangleMouseEnter, CanConnect);

                return _rectangleMouseEnterCommand;
            }
        }

        public DelegateCommand MouseLeaveCommand
        {
            get
            {
                if (_rectangleMouseLeaveCommand == null)
                    _rectangleMouseLeaveCommand = new DelegateCommand(OnRectangleMouseLeave, CanConnect);

                return _rectangleMouseLeaveCommand;
            }
        }

        public DelegateCommand MouseLeftButtonDownCommand
        {
            get
            {
                if (_rectangleMouseLeftButtonCommand == null)
                    _rectangleMouseLeftButtonCommand = new DelegateCommand(OnRectangleMouseLeftButtonDown, CanConnect);

                return _rectangleMouseLeftButtonCommand;
            }
        }

    }
}
