using Dynamo.UI.Commands;

namespace Dynamo.ViewModels
{
    public partial class NoteViewModel
    {
        private DelegateCommand _selectCommand;
        public DelegateCommand SelectCommand
        {
            get
            {
                if(_selectCommand == null)
                    _selectCommand = new DelegateCommand(Select, CanSelect);
                return _selectCommand;
            }
        }
    }
}
