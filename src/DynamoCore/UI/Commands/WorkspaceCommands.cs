using System.Collections.Generic;
using Dynamo.Nodes;
using Dynamo.UI.Commands;
using Dynamo.Utilities;
using Dynamo.ViewModels;

namespace Dynamo.ViewModels
{
    public partial class WorkspaceViewModel
    {
        private DelegateCommand _hideCommand;
        private DelegateCommand _setCurrentOffsetCommand;
        private DelegateCommand _nodeFromSelectionCommand;
        private DelegateCommand _setZoomCommand;
        private DelegateCommand _zoomInCommand;
        private DelegateCommand _zoomOutCommand;
        private DelegateCommand _fitViewCommand;
        private DelegateCommand _resetFitViewToggleCommand;
        private DelegateCommand _togglePanCommand;
        private DelegateCommand _findByIdCommand;
        private DelegateCommand _alignSelectedCommand;
        private DelegateCommand _findNodesFromSelectionCommand;
        private DelegateCommand _selectAllCommand;
        private DelegateCommand _pauseVisualizationManagerUpdateCommand;
        private DelegateCommand _unpauseVisualizationManagerUpdateCommand;

        public DelegateCommand SelectAllCommand
        {
            get
            {
                if(_selectAllCommand == null)
                    _selectAllCommand = new DelegateCommand(SelectAll, CanSelectAll);
                return _selectAllCommand;
            }
        }

        private DelegateCommand _nodeToCodeCommand;
        public DelegateCommand NodeToCodeCommand
        {
            get
            {
                if (_nodeToCodeCommand == null)
                {
                    _nodeToCodeCommand = new DelegateCommand(_model.NodeToCode, _model.CanNodeToCode);
                }
                return _nodeToCodeCommand;
            }
        }


        public DelegateCommand HideCommand
        {
            get
            {
                if(_hideCommand == null)
                    _hideCommand = new DelegateCommand(Hide, CanHide);

                return _hideCommand;
            }
        }

        public DelegateCommand SetCurrentOffsetCommand
        {
            get
            {
                if(_setCurrentOffsetCommand == null)
                    _setCurrentOffsetCommand = new DelegateCommand(SetCurrentOffset, CanSetCurrentOffset);

                return _setCurrentOffsetCommand;
            }
        }

        public DelegateCommand NodeFromSelectionCommand
        {
            get
            {
                if(_nodeFromSelectionCommand == null)
                    _nodeFromSelectionCommand = new DelegateCommand(CreateNodeFromSelection, CanCreateNodeFromSelection);

                return _nodeFromSelectionCommand;
            }
        }

        public DelegateCommand SetZoomCommand
        {
            get
            {
                if(_setZoomCommand == null)
                    _setZoomCommand = new DelegateCommand(SetZoom, CanSetZoom);
                return _setZoomCommand;
            }

        }

        public DelegateCommand ZoomInCommand
        {
            get
            {
                if (_zoomInCommand == null)
                    _zoomInCommand = new DelegateCommand(ZoomIn, CanZoomIn);
                return _zoomInCommand;
            }

        }

        public DelegateCommand ZoomOutCommand
        {
            get
            {
                if (_zoomOutCommand == null)
                    _zoomOutCommand = new DelegateCommand(ZoomOut, CanZoomOut);
                return _zoomOutCommand;
            }

        }

        public DelegateCommand FitViewCommand
        {
            get
            {
                if (_fitViewCommand == null)
                    _fitViewCommand = new DelegateCommand(FitView, CanFitView);
                return _fitViewCommand;
            }
        }

        public DelegateCommand ResetFitViewToggleCommand
        {
            get
            {
                if (_resetFitViewToggleCommand == null)
                    _resetFitViewToggleCommand = new DelegateCommand(ResetFitViewToggle, CanResetFitViewToggle);
                return _resetFitViewToggleCommand;
            }
        }

        public DelegateCommand TogglePanCommand
        {
            get
            {
                if (_togglePanCommand == null)
                    _togglePanCommand = new DelegateCommand(TogglePan, CanTogglePan);
                return _togglePanCommand;
            }
        }

        public DelegateCommand FindByIdCommand
        {
            get
            {
                if(_findByIdCommand == null)
                    _findByIdCommand = new DelegateCommand(FindById, CanFindById);

                return _findByIdCommand;
            }
        }

        public DelegateCommand AlignSelectedCommand
        {
            get
            {
                if(_alignSelectedCommand == null)
                    _alignSelectedCommand = new DelegateCommand(AlignSelected, CanAlignSelected);

                return _alignSelectedCommand;
            }
        }

        public DelegateCommand FindNodesFromSelectionCommand
        {
            get
            {
                if(_findNodesFromSelectionCommand == null)
                    _findNodesFromSelectionCommand = new DelegateCommand(FindNodesFromSelection, CanFindNodesFromSelection);

                return _findNodesFromSelectionCommand;
            }
        }

        public DelegateCommand PauseVisualizationManagerCommand
        {
            get
            {
                if (_pauseVisualizationManagerUpdateCommand == null)
                    _pauseVisualizationManagerUpdateCommand = new DelegateCommand(PauseVisualizationManagerUpdates, CanPauseVisualizationManagerUpdates);

                return _pauseVisualizationManagerUpdateCommand;
            }
        }

        public DelegateCommand UnPauseVisualizationManagerCommand
        {
            get
            {
                if (_unpauseVisualizationManagerUpdateCommand == null)
                    _unpauseVisualizationManagerUpdateCommand = new DelegateCommand(UnPauseVisualizationManagerUpdates, CanUnPauseVisualizationManagerUpdates);

                return _unpauseVisualizationManagerUpdateCommand;
            }
        }
    }
}
