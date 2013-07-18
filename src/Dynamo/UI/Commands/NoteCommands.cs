using Microsoft.Practices.Prism.Commands;

namespace Dynamo.Nodes
{
    public partial class dynNoteViewModel
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
