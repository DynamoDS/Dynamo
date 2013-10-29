using Dynamo.UI.Commands;

namespace Dynamo.ViewModels
{
    public partial class PortViewModel
    {
        private DelegateCommand _connectCommand;

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
