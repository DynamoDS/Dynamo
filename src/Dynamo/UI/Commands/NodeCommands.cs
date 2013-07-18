using Microsoft.Practices.Prism.Commands;

namespace Dynamo.ViewModels
{
    public partial class dynNodeViewModel
    {
        private DelegateCommand _deleteCommand1;
        private DelegateCommand<string> _setLacingTypeCommand;
        private DelegateCommand<object> _setStateCommand;
        private DelegateCommand _selectCommand;
        private DelegateCommand _showHelpCommand;
        private DelegateCommand _viewCustomNodeWorkspaceCommand;
        private DelegateCommand<object> _setLayoutCommand;
        private DelegateCommand<object> _setupCustomUiElementsCommand;
        private DelegateCommand _validateConnectionsCommand;
        private DelegateCommand _toggleIsVisibleCommand;
        private DelegateCommand _toggleIsUpstreamVisibleCommand;
        private DelegateCommand _renameCommand;

        public DelegateCommand RenameCommand
        {
            get
            {
                if(_renameCommand == null)
                    _renameCommand = new DelegateCommand(ShowRename, CanShowRename);
                return _renameCommand;
            }
        }

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

        public DelegateCommand<string> SetLacingTypeCommand
        {
            get
            {
                if(_setLacingTypeCommand == null)
                    _setLacingTypeCommand 
                        = new DelegateCommand<string>(SetLacingType, CanSetLacingType);

                return _setLacingTypeCommand;
            }
        }

        public DelegateCommand<object> SetStateCommand
        {
            get
            {
                if(_setStateCommand == null)
                    _setStateCommand = 
                        new DelegateCommand<object>(SetState, CanSetState);

                return _setStateCommand;
            }
        }

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

        public DelegateCommand<object> SetLayoutCommand
        {
            get
            {
                if(_setLayoutCommand == null)
                    _setLayoutCommand = 
                        new DelegateCommand<object>(SetLayout, CanSetLayout);

                return _setLayoutCommand;
            }
        }

        public DelegateCommand<object> SetupCustomUIElementsCommand
        {
            get
            {
                if(_setupCustomUiElementsCommand == null)
                    _setupCustomUiElementsCommand = 
                        new DelegateCommand<object>(SetupCustomUIElements, CanSetupCustomUIElements);

                return _setupCustomUiElementsCommand;
            }
        }

        public DelegateCommand ValidateConnectionsCommand
        {
            get
            {
                if(_validateConnectionsCommand == null)
                    _validateConnectionsCommand = 
                        new DelegateCommand(ValidateConnections, CanValidateConnections);

                return _validateConnectionsCommand;
            }
        }

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
    }
}
