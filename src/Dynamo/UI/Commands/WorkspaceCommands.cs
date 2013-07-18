
using Microsoft.Practices.Prism.Commands;

namespace Dynamo
{
    public partial class dynWorkspaceViewModel
    {
        private DelegateCommand<object> _hideCommand;
        private DelegateCommand<object> _crossSelectCommand;
        private DelegateCommand<object> _containSelectCommand;
        //private DelegateCommand _updateSelectedConnectorsCommand;
        private DelegateCommand<object> _setCurrentOffsetCommand;
        private DelegateCommand _nodeFromSelectionCommand;
        private DelegateCommand<object> _setZoomCommand;
        private DelegateCommand<object> _findByIdCommand;
        private DelegateCommand<string> _alignSelectedCommand;
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

        public DelegateCommand<object> HideCommand
        {
            get
            {
                if(_hideCommand == null)
                    _hideCommand = new DelegateCommand<object>(Hide, CanHide);

                return _hideCommand;
            }
        }

        public DelegateCommand<object> CrossSelectCommand
        {
            get
            {
                if(_crossSelectCommand == null)
                    _crossSelectCommand = new DelegateCommand<object>(CrossingSelect, CanCrossSelect);

                return _crossSelectCommand;
            }
        }

        public DelegateCommand<object> ContainSelectCommand
        {
            get
            {
                if(_containSelectCommand == null)
                    _containSelectCommand = new DelegateCommand<object>(ContainSelect, CanContainSelect);

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

        public DelegateCommand<object> SetCurrentOffsetCommand
        {
            get
            {
                if(_setCurrentOffsetCommand == null)
                    _setCurrentOffsetCommand = new DelegateCommand<object>(SetCurrentOffset, CanSetCurrentOffset);

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

        public DelegateCommand<object> SetZoomCommand
        {
            get
            {
                if(_setZoomCommand == null)
                    _setZoomCommand = new DelegateCommand<object>(SetZoom, CanSetZoom);
                return _setZoomCommand;
            }

        }

        public DelegateCommand<object> FindByIdCommand
        {
            get
            {
                if(_findByIdCommand == null)
                    _findByIdCommand = new DelegateCommand<object>(FindById, CanFindById);

                return _findByIdCommand;
            }
        }

        public DelegateCommand<string> AlignSelectedCommand
        {
            get
            {
                if(_alignSelectedCommand == null)
                    _alignSelectedCommand = new DelegateCommand<string>(AlignSelected, CanAlignSelected);

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
