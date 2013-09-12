using Dynamo.UI.Commands;

namespace Dynamo.ViewModels
{
    public partial class WorkspaceViewModel
    {
        private DelegateCommand _hideCommand;
        private DelegateCommand _crossSelectCommand;
        private DelegateCommand _containSelectCommand;
        //private DelegateCommand _updateSelectedConnectorsCommand;
        private DelegateCommand _setCurrentOffsetCommand;
        private DelegateCommand _nodeFromSelectionCommand;
        private DelegateCommand _setZoomCommand;
        private DelegateCommand _zoomInCommand;
        private DelegateCommand _zoomOutCommand;
        private DelegateCommand _findByIdCommand;
        private DelegateCommand _alignSelectedCommand;
        private DelegateCommand _findNodesFromSelectionCommand;
        private DelegateCommand _selectAllCommand;

        public DelegateCommand SelectAllCommand
        {
            get
            {
                if(_selectAllCommand == null)
                    _selectAllCommand = new DelegateCommand(SelectAll, CanSelectAll);
                return _selectAllCommand;
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

        public DelegateCommand CrossSelectCommand
        {
            get
            {
                if(_crossSelectCommand == null)
                    _crossSelectCommand = new DelegateCommand(CrossingSelect, CanCrossSelect);

                return _crossSelectCommand;
            }
        }

        public DelegateCommand ContainSelectCommand
        {
            get
            {
                if(_containSelectCommand == null)
                    _containSelectCommand = new DelegateCommand(ContainSelect, CanContainSelect);

                return _containSelectCommand;
            }
        }

        //public DelegateCommand UpdateSelectedConnectorsCommand
        //{
        //    get
        //    {
        //        if(_updateSelectedConnectorsCommand == null)

        //        return _updateSelectedConnectorsCommand;
        //    }
        //}

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
    }
}
