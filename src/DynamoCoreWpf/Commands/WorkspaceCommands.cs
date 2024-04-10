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

        private DelegateCommand hideCommand;
        private DelegateCommand setCurrentOffsetCommand;
        private DelegateCommand nodeFromSelectionCommand;
        private DelegateCommand setZoomCommand;
        private DelegateCommand resetFitViewToggleCommand;
        private DelegateCommand findByIdCommand;
        private DelegateCommand focusNodeCommand;
        private DelegateCommand alignSelectedCommand;
        private DelegateCommand setArgumentLacingCommand;
        private DelegateCommand findNodesFromSelectionCommand;
        private DelegateCommand selectAllCommand;
        private DelegateCommand graphAutoLayoutCommand;
        private DelegateCommand showHideAllGeometryPreviewCommand;
        private DelegateCommand showInCanvasSearchCommand;
        private DelegateCommand pasteCommand;
        private DelegateCommand hideAllPopupCommand;
        private DelegateCommand showAllWiresCommand;
        private DelegateCommand hideAllWiresCommand;
        private DelegateCommand unpinAllPreviewBubblesCommand;

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
            get { return pasteCommand ?? (pasteCommand = new DelegateCommand(Paste, DynamoViewModel.CanPaste)); }
        }

        [JsonIgnore]
        public DelegateCommand SelectAllCommand
        {
            get
            {
                if(selectAllCommand == null)
                    selectAllCommand = new DelegateCommand(SelectAll, CanSelectAll);
                return selectAllCommand;
            }
        }

        [JsonIgnore]
        public DelegateCommand GraphAutoLayoutCommand
        {
            get {
                return graphAutoLayoutCommand
                    ?? (graphAutoLayoutCommand =
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
                if(hideCommand == null)
                    hideCommand = new DelegateCommand(Hide, CanHide);

                return hideCommand;
            }
        }

        [JsonIgnore]
        public DelegateCommand SetCurrentOffsetCommand
        {
            get
            {
                if(setCurrentOffsetCommand == null)
                    setCurrentOffsetCommand = new DelegateCommand(SetCurrentOffset, CanSetCurrentOffset);

                return setCurrentOffsetCommand;
            }
        }

        [JsonIgnore]
        public DelegateCommand NodeFromSelectionCommand
        {
            get
            {
                if(nodeFromSelectionCommand == null)
                    nodeFromSelectionCommand = new DelegateCommand(CreateNodeFromSelection, CanCreateNodeFromSelection);

                return nodeFromSelectionCommand;
            }
        }

        [JsonIgnore]
        public DelegateCommand SetZoomCommand
        {
            get
            {
                if(setZoomCommand == null)
                    setZoomCommand = new DelegateCommand(SetZoom, CanSetZoom);
                return setZoomCommand;
            }
        }

        [JsonIgnore]
        public DelegateCommand ResetFitViewToggleCommand
        {
            get
            {
                if (resetFitViewToggleCommand == null)
                    resetFitViewToggleCommand = new DelegateCommand(ResetFitViewToggle, CanResetFitViewToggle);
                return resetFitViewToggleCommand;
            }
        }

        [JsonIgnore]
        public DelegateCommand FindByIdCommand
        {
            get
            {
                if(findByIdCommand == null)
                    findByIdCommand = new DelegateCommand(FindById, CanFindById);

                return findByIdCommand;
            }
        }

        [JsonIgnore]
        public DelegateCommand FocusNodeCommand
        {
            get
            {
                if (focusNodeCommand == null)
                    focusNodeCommand = new DelegateCommand(FocusNode, CanFocusNode);

                return focusNodeCommand;
            }
        }

        [JsonIgnore]
        public DelegateCommand AlignSelectedCommand
        {
            get
            {
                if(alignSelectedCommand == null)
                    alignSelectedCommand = new DelegateCommand(AlignSelected, CanAlignSelected);

                return alignSelectedCommand;
            }
        }

        [JsonIgnore]
        public DelegateCommand SetArgumentLacingCommand
        {
            get
            {
                if (setArgumentLacingCommand == null)
                {
                    setArgumentLacingCommand = new DelegateCommand(
                        SetArgumentLacing, p => HasSelection);
                }

                return setArgumentLacingCommand;
            }
        }

        [JsonIgnore]
        public DelegateCommand FindNodesFromSelectionCommand
        {
            get
            {
                if(findNodesFromSelectionCommand == null)
                    findNodesFromSelectionCommand = new DelegateCommand(FindNodesFromSelection, CanFindNodesFromSelection);

                return findNodesFromSelectionCommand;
            }
        }

        [JsonIgnore]
        public DelegateCommand ShowHideAllGeometryPreviewCommand
        {
            get
            {
                if (showHideAllGeometryPreviewCommand == null)
                {
                    showHideAllGeometryPreviewCommand = new DelegateCommand(
                        ShowHideAllGeometryPreview);
                }

                return showHideAllGeometryPreviewCommand;
            }
        }

       
        [JsonIgnore]
        public DelegateCommand ShowInCanvasSearchCommand
        {
            get
            {
                if (showInCanvasSearchCommand == null)
                    showInCanvasSearchCommand = new DelegateCommand(OnRequestShowInCanvasSearch);

                return showInCanvasSearchCommand;
            }
        }

        /// <summary>
        /// View Command to hide all popup in special cases
        /// </summary>
        [JsonIgnore]
        public DelegateCommand HideAllPopupCommand
        {
            get
            {
                if (hideAllPopupCommand == null)
                    hideAllPopupCommand = new DelegateCommand(OnRequestHideAllPopup);

                return hideAllPopupCommand;
            }
        }

        /// <summary>
        /// View Command to show all connection wires (on current selection), if any are hidden
        /// </summary>
        [JsonIgnore]
        public DelegateCommand ShowAllWiresCommand
        {
            get
            {
                if(showAllWiresCommand == null) 
                    showAllWiresCommand = new DelegateCommand(ShowAllWires, CanShowAllWires);

                return showAllWiresCommand;
            }
        }

        /// <summary>
        /// View Command to hide all connection wires (on current selection), if any are shown
        /// </summary>
        [JsonIgnore]
        public DelegateCommand HideAllWiresCommand
        {
            get
            {
                if(hideAllWiresCommand == null)
                    hideAllWiresCommand = new DelegateCommand(HideAllWires, CanHideAllWires);

                return hideAllWiresCommand;
            }
        }

        /// <summary>
        /// View Command to hide all currently pinned preview bubbles within the workspace
        /// </summary>
        [JsonIgnore]
        public DelegateCommand UnpinAllPreviewBubblesCommand
        {
            get
            {
                if (unpinAllPreviewBubblesCommand == null)
                    unpinAllPreviewBubblesCommand = new DelegateCommand(UnpinAllPreviewBubbles, CanUnpinAllPreviewBubbles);
                return unpinAllPreviewBubblesCommand;
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
