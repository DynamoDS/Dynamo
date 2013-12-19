using Dynamo.UI.Commands;

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
        private DelegateCommand _setupCustomUiElementsCommand;
        private DelegateCommand _validateConnectionsCommand;
        private DelegateCommand _toggleIsVisibleCommand;
        private DelegateCommand _toggleIsUpstreamVisibleCommand;
        private DelegateCommand _renameCommand;
        private DelegateCommand _showTooltipCommand;
        private DelegateCommand _fadeOutTooltipCommand;
        private DelegateCommand _collapseTooltipCommand;
        private DelegateCommand showPreviewCommand;
        private DelegateCommand hidePreviewCommand;
        private DelegateCommand _setModelSizeCommand;
        private DelegateCommand _gotoWorkspaceCommand;

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

        public DelegateCommand SetupCustomUIElementsCommand
        {
            get {
                return _setupCustomUiElementsCommand
                    ?? (_setupCustomUiElementsCommand =
                        new DelegateCommand(SetupCustomUIElements, CanSetupCustomUIElements));
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

        public DelegateCommand ShowTooltipCommand
        {
            get
            {
                if (_showTooltipCommand == null)
                    _showTooltipCommand = new DelegateCommand(ShowTooltip, CanShowTooltip);
                return _showTooltipCommand;
            }
        }

        public DelegateCommand FadeOutTooltipCommand
        {
            get
            {
                if (_fadeOutTooltipCommand == null)
                    _fadeOutTooltipCommand = new DelegateCommand(FadeOutTooltip, CanFadeOutTooltip);
                return _fadeOutTooltipCommand;
            }
        }

        public DelegateCommand CollapseTooltipCommand
        {
            get
            {
                if (_collapseTooltipCommand == null)
                    _collapseTooltipCommand = new DelegateCommand(CollapseTooltip, CanCollapseTooltip);
                return _collapseTooltipCommand;
            }
        }

        public DelegateCommand ShowPreviewCommand
        {
            get
            {
                if (showPreviewCommand == null)
                    showPreviewCommand = new DelegateCommand(ShowPreview, CanShowPreview);
                return showPreviewCommand;
            }
        }

        public DelegateCommand HidePreviewCommand
        {
            get
            {
                if (hidePreviewCommand == null)
                    hidePreviewCommand = new DelegateCommand(HidePreview, CanHidePreview);
                return hidePreviewCommand;
            }
        }

        public DelegateCommand SetModelSizeCommand
        {
            get
            {
                if (_setModelSizeCommand == null)
                    _setModelSizeCommand = new DelegateCommand(SetModelSize, CanSetModelSize);
                return _setModelSizeCommand;
            }
        }

        public DelegateCommand GotoWorkspaceCommand
        {
            get
            {
                if (_gotoWorkspaceCommand == null)
                    _gotoWorkspaceCommand = new DelegateCommand(GotoWorkspace, CanGotoWorkspace);
                return _gotoWorkspaceCommand;
            }
        }
    }
}
