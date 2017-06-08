using Dynamo.UI.Commands;
using Newtonsoft.Json;

namespace Dynamo.ViewModels
{
    public partial class NoteViewModel
    {
        private DelegateCommand _selectCommand;
        [JsonIgnore]
        public DelegateCommand SelectCommand
        {
            get
            {
                if(_selectCommand == null)
                    _selectCommand = new DelegateCommand(Select, CanSelect);
                return _selectCommand;
            }
        }

        private DelegateCommand _createGroupCommand;
        [JsonIgnore]
        public DelegateCommand CreateGroupCommand
        {
            get
            {
                if (_createGroupCommand == null)
                    _createGroupCommand =
                        new DelegateCommand(CreateGroup, CanCreateGroup);

                return _createGroupCommand;
            }
        }

        private DelegateCommand _ungroupCommand;
        [JsonIgnore]
        public DelegateCommand UngroupCommand
        {
            get
            {
                if (_ungroupCommand == null)
                    _ungroupCommand =
                        new DelegateCommand(UngroupNote, CanUngroupNote);

                return _ungroupCommand;
            }
        }

        private DelegateCommand _addToGroupCommand;
        [JsonIgnore]
        public DelegateCommand AddToGroupCommand
        {
            get
            {
                if (_addToGroupCommand == null)
                    _addToGroupCommand =
                        new DelegateCommand(AddToGroup, CanAddToGroup);

                return _addToGroupCommand;
            }
        }
    }
}
