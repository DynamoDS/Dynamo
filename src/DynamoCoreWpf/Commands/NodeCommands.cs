using Dynamo.UI.Commands;
using Newtonsoft.Json;

namespace Dynamo.ViewModels
{
    public partial class NodeViewModel
    {
        private DelegateCommand _deleteCommand1;
        private DelegateCommand _setLacingTypeCommand;
        private DelegateCommand _setStateCommand;
        private DelegateCommand _selectCommand;
        private DelegateCommand _showHelpCommand;
        private DelegateCommand _viewCustomNodeWorkspaceCommand;
        private DelegateCommand _validateConnectionsCommand;
        private DelegateCommand _toggleIsVisibleCommand;
        private DelegateCommand _toggleIsUpstreamVisibleCommand;
        private DelegateCommand _renameCommand;
        private DelegateCommand _setModelSizeCommand;
        private DelegateCommand _gotoWorkspaceCommand;
        private DelegateCommand _createGroupCommand;
        private DelegateCommand _ungroupCommand;
        private DelegateCommand _addToGroupCommand;
        private DelegateCommand _computeRunStateOfTheNodeCommand;

        [JsonIgnore]
        public DelegateCommand RenameCommand
        {
            get
            {
                if(_renameCommand == null)
                    _renameCommand = new DelegateCommand(ShowRename, CanShowRename);
                return _renameCommand;
            }
        }

        [JsonIgnore]
        public DelegateCommand DeleteCommand
        {
            get
            {
                if(_deleteCommand1 == null)
                    _deleteCommand1 = 
                        new DelegateCommand(DeleteNodeAndItsConnectors, CanDeleteNode);

                return _deleteCommand1;
            }
        }

        [JsonIgnore]
        public DelegateCommand SetLacingTypeCommand
        {
            get
            {
                if(_setLacingTypeCommand == null)
                    _setLacingTypeCommand 
                        = new DelegateCommand(SetLacingType, CanSetLacingType);

                return _setLacingTypeCommand;
            }
        }

        [JsonIgnore]
        public DelegateCommand SetStateCommand
        {
            get
            {
                if(_setStateCommand == null)
                    _setStateCommand = 
                        new DelegateCommand(SetState, CanSetState);

                return _setStateCommand;
            }
        }

        [JsonIgnore]
        public DelegateCommand SelectCommand
        {
            get
            {
                if(_selectCommand == null)
                    _selectCommand = 
                        new DelegateCommand(Select, CanSelect);

                return _selectCommand;
            }
        }

        [JsonIgnore]
        public DelegateCommand ShowHelpCommand
        {
            get
            {
                if(_showHelpCommand == null)
                    _showHelpCommand = 
                        new DelegateCommand(ShowHelp, CanShowHelp);

                return _showHelpCommand;
            }
        }

        [JsonIgnore]
        public DelegateCommand ViewCustomNodeWorkspaceCommand
        {
            get
            {
                if(_viewCustomNodeWorkspaceCommand == null)
                    _viewCustomNodeWorkspaceCommand = 
                        new DelegateCommand(ViewCustomNodeWorkspace, CanViewCustomNodeWorkspace);

                return _viewCustomNodeWorkspaceCommand;
            }
        }

        [JsonIgnore]
        public DelegateCommand ToggleIsVisibleCommand
        {
            get
            {
                if(_toggleIsVisibleCommand == null)
                    _toggleIsVisibleCommand = 
                        new DelegateCommand(ToggleIsVisible, CanVisibilityBeToggled);

                return _toggleIsVisibleCommand;
            }
        }

        [JsonIgnore]
        public DelegateCommand ToggleIsUpstreamVisibleCommand
        {
            get
            {
                if(_toggleIsUpstreamVisibleCommand == null)
                    _toggleIsUpstreamVisibleCommand = 
                        new DelegateCommand(ToggleIsUpstreamVisible, CanUpstreamVisibilityBeToggled);

                return _toggleIsUpstreamVisibleCommand;
            }
        }

        [JsonIgnore]
        public DelegateCommand SetModelSizeCommand
        {
            get
            {
                if (_setModelSizeCommand == null)
                    _setModelSizeCommand = new DelegateCommand(SetModelSize, CanSetModelSize);
                return _setModelSizeCommand;
            }
        }

        [JsonIgnore]
        public DelegateCommand GotoWorkspaceCommand
        {
            get
            {
                if (_gotoWorkspaceCommand == null)
                    _gotoWorkspaceCommand = new DelegateCommand(GotoWorkspace, CanGotoWorkspace);
                return _gotoWorkspaceCommand;
            }
        }

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

        [JsonIgnore]
        public DelegateCommand UngroupCommand
        {
            get
            {
                if (_ungroupCommand == null)
                    _ungroupCommand =
                        new DelegateCommand(UngroupNode, CanUngroupNode);

                return _ungroupCommand;
            }
        }

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

        [JsonIgnore]
        public DelegateCommand ToggleIsFrozenCommand
        {
            get
            {
                if (_computeRunStateOfTheNodeCommand == null)
                {
                    _computeRunStateOfTheNodeCommand = new DelegateCommand(ToggleIsFrozen, CanToggleIsFrozen);
                }

                return _computeRunStateOfTheNodeCommand;
            }
        }
    }
}
