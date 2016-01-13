using System.Linq;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.Selection;
using Dynamo.UI.Commands;
using Dynamo.Utilities;
using Dynamo.Wpf.Properties;

namespace Dynamo.ViewModels
{
    public partial class WorkspaceViewModel
    {
        #region Private Delegate Command Data Members

        private DelegateCommand _hideCommand;
        private DelegateCommand _setCurrentOffsetCommand;
        private DelegateCommand _nodeFromSelectionCommand;
        private DelegateCommand _setZoomCommand;
        private DelegateCommand _resetFitViewToggleCommand;
        private DelegateCommand _findByIdCommand;
        private DelegateCommand _alignSelectedCommand;
        private DelegateCommand _setArgumentLacingCommand;
        private DelegateCommand _findNodesFromSelectionCommand;
        private DelegateCommand _selectAllCommand;
        private DelegateCommand _graphAutoLayoutCommand;
        private DelegateCommand _pauseVisualizationManagerUpdateCommand;
        private DelegateCommand _unpauseVisualizationManagerUpdateCommand;
        private DelegateCommand _showHideAllGeometryPreviewCommand;
        private DelegateCommand _showHideAllUpstreamPreviewCommand;
        private DelegateCommand _showInCanvasSearchCommand;
        private DelegateCommand _pasteCommand;
        private DelegateCommand _computeRunStateCommand;

        #endregion

        #region Public Delegate Commands

        public DelegateCommand CopyCommand
        {
            get { return DynamoViewModel.CopyCommand; }
        }

        public DelegateCommand PasteCommand
        {
            get { return _pasteCommand ?? (_pasteCommand = new DelegateCommand(Paste, DynamoViewModel.CanPaste)); }
        }

        public DelegateCommand SelectAllCommand
        {
            get
            {
                if(_selectAllCommand == null)
                    _selectAllCommand = new DelegateCommand(SelectAll, CanSelectAll);
                return _selectAllCommand;
            }
        }

        public DelegateCommand GraphAutoLayoutCommand
        {
            get {
                return _graphAutoLayoutCommand
                    ?? (_graphAutoLayoutCommand =
                        new DelegateCommand(DoGraphAutoLayout, CanDoGraphAutoLayout));
            }
        }

        private DelegateCommand _nodeToCodeCommand;
        public DelegateCommand NodeToCodeCommand
        {
            get
            {
                if (_nodeToCodeCommand == null)
                {
                    _nodeToCodeCommand = new DelegateCommand(NodeToCode, CanNodeToCode);
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

        public DelegateCommand ResetFitViewToggleCommand
        {
            get
            {
                if (_resetFitViewToggleCommand == null)
                    _resetFitViewToggleCommand = new DelegateCommand(ResetFitViewToggle, CanResetFitViewToggle);
                return _resetFitViewToggleCommand;
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

        public DelegateCommand SetArgumentLacingCommand
        {
            get
            {
                if (_setArgumentLacingCommand == null)
                {
                    _setArgumentLacingCommand = new DelegateCommand(
                        SetArgumentLacing, p => HasSelection);
                }

                return _setArgumentLacingCommand;
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

        public DelegateCommand ShowHideAllGeometryPreviewCommand
        {
            get
            {
                if (_showHideAllGeometryPreviewCommand == null)
                {
                    _showHideAllGeometryPreviewCommand = new DelegateCommand(
                        ShowHideAllGeometryPreview);
                }

                return _showHideAllGeometryPreviewCommand;
            }
        }

        public DelegateCommand ShowHideAllUpstreamPreviewCommand
        {
            get
            {
                if (_showHideAllUpstreamPreviewCommand == null)
                {
                    _showHideAllUpstreamPreviewCommand = new DelegateCommand(
                        ShowHideAllUpstreamPreview);
                }

                return _showHideAllUpstreamPreviewCommand;
            }
        }

        public DelegateCommand ShowInCanvasSearchCommand
        {
            get
            {
                if (_showInCanvasSearchCommand == null)
                    _showInCanvasSearchCommand = new DelegateCommand(OnRequestShowInCanvasSearch);

                return _showInCanvasSearchCommand;
            }
        }
         
        #endregion

        #region Properties for Command Data Binding

        public bool CanCopy
        {
            get { return DynamoViewModel.CanCopy(null); }
        }

        public bool CanPaste
        {
            get { return DynamoViewModel.CanPaste(null); }
        }

        public bool CanCopyOrPaste
        {
            get { return CanCopy || CanPaste; }
        }

        public bool AnyNodeVisible
        {
            get
            {
                return DynamoSelection.Instance.Selection.
                    OfType<NodeModel>().Any(n => n.IsVisible);
            }
        }

        public bool AnyNodeUpstreamVisible
        {
            get
            {
                return DynamoSelection.Instance.Selection.
                    OfType<NodeModel>().Any(n => n.IsUpstreamVisible);
            }
        }

        public bool HasSelection
        {
            get { return DynamoSelection.Instance.Selection.Count > 0; }
        }

        public bool IsGeometryOperationEnabled
        {
            get
            {
                if (DynamoSelection.Instance.Selection.Count <= 0)
                    return false; // No selection.

                // Menu options that are specific to geometry (show/hide all 
                // geometry previews, upstream previews, etc.) are only enabled
                // in the home workspace.
                // 
                return (this.Model is HomeWorkspaceModel);
            }
        }

        public LacingStrategy SelectionArgumentLacing
        {
            // TODO We may need a better way to do this
            // For now this returns the most common lacing strategy in the collection.
            get
            {
                // We were still hitting this getter when the Selection
                // was empty, and throwing an exception when attempting to
                // sort a null collection. If the Selection is empty, just
                // return First lacing.

                if(!DynamoSelection.Instance.Selection.Any())
                    return LacingStrategy.First;

                return DynamoSelection.Instance.Selection.OfType<NodeModel>()
                    .GroupBy(node => node.ArgumentLacing)
                    .OrderByDescending(group => group.Count())
                    .Select(group => group.Key).FirstOrDefault();
            }
        }

        #endregion
    }
}
