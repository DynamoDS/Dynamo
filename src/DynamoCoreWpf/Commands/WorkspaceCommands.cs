using System.Linq;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Selection;
using Dynamo.UI.Commands;
using Dynamo.Utilities;
using Newtonsoft.Json;

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
        private DelegateCommand _showInCanvasSearchCommand;
        private DelegateCommand _pasteCommand;
        private DelegateCommand _computeRunStateCommand;

        #endregion

        #region Public Delegate Commands

        [JsonIgnore]
        public DelegateCommand CopyCommand
        {
            get { return DynamoViewModel.CopyCommand; }
        }

        [JsonIgnore]
        public DelegateCommand PasteCommand
        {
            get { return _pasteCommand ?? (_pasteCommand = new DelegateCommand(Paste, DynamoViewModel.CanPaste)); }
        }

        [JsonIgnore]
        public DelegateCommand SelectAllCommand
        {
            get
            {
                if(_selectAllCommand == null)
                    _selectAllCommand = new DelegateCommand(SelectAll, CanSelectAll);
                return _selectAllCommand;
            }
        }

        [JsonIgnore]
        public DelegateCommand GraphAutoLayoutCommand
        {
            get {
                return _graphAutoLayoutCommand
                    ?? (_graphAutoLayoutCommand =
                        new DelegateCommand(DoGraphAutoLayout, CanDoGraphAutoLayout));
            }
        }

        private DelegateCommand _nodeToCodeCommand;
        [JsonIgnore]
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

        [JsonIgnore]
        public DelegateCommand HideCommand
        {
            get
            {
                if(_hideCommand == null)
                    _hideCommand = new DelegateCommand(Hide, CanHide);

                return _hideCommand;
            }
        }

        [JsonIgnore]
        public DelegateCommand SetCurrentOffsetCommand
        {
            get
            {
                if(_setCurrentOffsetCommand == null)
                    _setCurrentOffsetCommand = new DelegateCommand(SetCurrentOffset, CanSetCurrentOffset);

                return _setCurrentOffsetCommand;
            }
        }

        [JsonIgnore]
        public DelegateCommand NodeFromSelectionCommand
        {
            get
            {
                if(_nodeFromSelectionCommand == null)
                    _nodeFromSelectionCommand = new DelegateCommand(CreateNodeFromSelection, CanCreateNodeFromSelection);

                return _nodeFromSelectionCommand;
            }
        }

        [JsonIgnore]
        public DelegateCommand SetZoomCommand
        {
            get
            {
                if(_setZoomCommand == null)
                    _setZoomCommand = new DelegateCommand(SetZoom, CanSetZoom);
                return _setZoomCommand;
            }
        }

        [JsonIgnore]
        public DelegateCommand ResetFitViewToggleCommand
        {
            get
            {
                if (_resetFitViewToggleCommand == null)
                    _resetFitViewToggleCommand = new DelegateCommand(ResetFitViewToggle, CanResetFitViewToggle);
                return _resetFitViewToggleCommand;
            }
        }

        [JsonIgnore]
        public DelegateCommand FindByIdCommand
        {
            get
            {
                if(_findByIdCommand == null)
                    _findByIdCommand = new DelegateCommand(FindById, CanFindById);

                return _findByIdCommand;
            }
        }

        [JsonIgnore]
        public DelegateCommand AlignSelectedCommand
        {
            get
            {
                if(_alignSelectedCommand == null)
                    _alignSelectedCommand = new DelegateCommand(AlignSelected, CanAlignSelected);

                return _alignSelectedCommand;
            }
        }

        [JsonIgnore]
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

        [JsonIgnore]
        public DelegateCommand FindNodesFromSelectionCommand
        {
            get
            {
                if(_findNodesFromSelectionCommand == null)
                    _findNodesFromSelectionCommand = new DelegateCommand(FindNodesFromSelection, CanFindNodesFromSelection);

                return _findNodesFromSelectionCommand;
            }
        }

        [JsonIgnore]
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

       
        [JsonIgnore]
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

        [JsonIgnore]
        public bool CanCopy
        {
            get { return DynamoViewModel.CanCopy(null); }
        }

        [JsonIgnore]
        public bool CanPaste
        {
            get { return DynamoViewModel.CanPaste(null); }
        }

        [JsonIgnore]
        public bool CanCopyOrPaste
        {
            get { return CanCopy || CanPaste; }
        }

        [JsonIgnore]
        public bool AnyNodeVisible
        {
            get
            {
                return DynamoSelection.Instance.Selection.
                    OfType<NodeModel>().Any(n => n.IsVisible);
            }
        }

        [JsonIgnore]
        public bool HasSelection
        {
            get { return DynamoSelection.Instance.Selection.Count > 0; }
        }

        [JsonIgnore]
        public bool IsGeometryOperationEnabled
        {
            get
            {
                if (DynamoSelection.Instance.Selection.Count <= 0)
                    return false; // No selection.

                // Menu options that are specific to geometry (show/hide all 
                // geometry previews, etc.) are only enabled
                // in the home workspace.
                // 
                return (this.Model is HomeWorkspaceModel);
            }
        }

        [JsonIgnore]
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
