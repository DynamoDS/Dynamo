using Dynamo.Controls;
using Dynamo.UI.Commands;
using Dynamo.Utilities;
using Newtonsoft.Json;

namespace Dynamo.ViewModels
{
    public partial class NoteViewModel: IWorkspaceElement
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

        private DelegateCommand pinToNodeCommand;

        /// <summary>
        /// Command to pin the current note to a selected node
        /// </summary>
        [JsonIgnore]
        public DelegateCommand PinToNodeCommand
        {
            get
            {
                if (pinToNodeCommand == null)
                {
                    pinToNodeCommand = new DelegateCommand(PinToNode, CanPinToNode);
                } 
                return pinToNodeCommand;
            }
        }

        private DelegateCommand unpinFromNodeCommand;
        /// <summary>
        /// Command to unpin the pinned node (sets it to null)
        /// </summary>
        [JsonIgnore]
        public DelegateCommand UnpinFromNodeCommand
        {
            get
            {
                if (unpinFromNodeCommand == null)
                {
                    unpinFromNodeCommand = new DelegateCommand(UnpinFromNode, CanUnpinFromNode);
                }
                return unpinFromNodeCommand;
            }
        }

        /// <summary>
        /// This signifies if the note should be rendered
        /// </summary>
        [JsonIgnore]
        public bool IsVisibleInCanvas
        {
            get => isVisibleInCanvas;
            set
            {
                isVisibleInCanvas = value;
                RaisePropertyChanged(nameof(isVisibleInCanvas));
            }
        }
        private bool isVisibleInCanvas = false;

        public Rect2D Rect => Model.Rect;
    }
}
