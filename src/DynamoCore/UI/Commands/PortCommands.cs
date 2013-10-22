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
    }
}
