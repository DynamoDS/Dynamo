using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Selection;
using Dynamo.Utilities;
using Dynamo.Controls;
using System.Windows.Threading;
using System.Windows.Input;
using Dynamo.Core;

namespace Dynamo.ViewModels
{
    public delegate void PointEventHandler(object sender, EventArgs e);
    public delegate void NodeEventHandler(object sender, EventArgs e);
    public delegate void NoteEventHandler(object sender, EventArgs e);
    public delegate void ViewEventHandler(object sender, EventArgs e);
    public delegate void ZoomEventHandler(object sender, EventArgs e);
    public delegate void SelectionEventHandler(object sender, SelectionBoxUpdateArgs e);
    public delegate void ViewModelAdditionEventHandler(object sender, ViewModelEventArgs e);
    public delegate void WorkspacePropertyEditHandler(WorkspaceModel workspace);

    public partial class WorkspaceViewModel : ViewModelBase
    {
        #region Properties and Fields

        public WorkspaceModel _model;
        private bool _canFindNodesFromElements = false;
        public Dispatcher Dispatcher;

        public event PointEventHandler CurrentOffsetChanged;
        public event ZoomEventHandler ZoomChanged;
        public event ZoomEventHandler RequestZoomToViewportCenter;
        public event ZoomEventHandler RequestZoomToViewportPoint;
        public event ZoomEventHandler RequestZoomToFitView;
       
        public event NodeEventHandler RequestCenterViewOnElement;
        public event NodeEventHandler RequestNodeCentered;
        public event ViewEventHandler RequestAddViewToOuterCanvas;
        public event SelectionEventHandler RequestSelectionBoxUpdate;
        public event WorkspacePropertyEditHandler WorkspacePropertyEditRequested;

        /// <summary>
        /// Cursor Property Binding for WorkspaceView
        /// </summary>
        private Cursor currentCursor = null;
        public Cursor CurrentCursor
        {
            get { return currentCursor; }
            set { currentCursor = value; RaisePropertyChanged("CurrentCursor"); }
        }

        /// <summary>
        /// Force Cursor Property Binding for WorkspaceView
        /// </summary>
        private bool isCursorForced = false;
        public bool IsCursorForced
        {
            get { return isCursorForced; }
            set { isCursorForced = value; RaisePropertyChanged("IsCursorForced"); }
        }

        /// <summary>
        /// Convenience property
        /// </summary>
        public DynamoViewModel DynamoViewModel { get { return dynSettings.Controller.DynamoViewModel; } }

        /// <summary>
        /// Used during open and workspace changes to set the location of the workspace
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual void OnCurrentOffsetChanged(object sender, PointEventArgs e)
        {
            if (CurrentOffsetChanged != null)
            {
                Debug.WriteLine(string.Format("Setting current offset to {0}", e.Point));
                CurrentOffsetChanged(this, e);
            }
        }

        /// <summary>
        /// Used during open and workspace changes to set the zoom of the workspace
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual void OnZoomChanged(object sender, ZoomEventArgs e)
        {
            if (ZoomChanged != null)
            {
                //Debug.WriteLine(string.Format("Setting zoom to {0}", e.Zoom));
                ZoomChanged(this, e);
            }
            dynSettings.Controller.DynamoViewModel.HideInfoBubble(null);
        }

        /// <summary>
        /// For requesting registered workspace to zoom in center
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual void OnRequestZoomToViewportCenter(object sender, ZoomEventArgs e)
        {
            if (RequestZoomToViewportCenter != null)
            {
                RequestZoomToViewportCenter(this, e);
            }
        }

        /// <summary>
        /// For requesting registered workspace to zoom in out from a point
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual void OnRequestZoomToViewportPoint(object sender, ZoomEventArgs e)
        {
            if (RequestZoomToViewportPoint != null)
            {
                RequestZoomToViewportPoint(this, e);
            }
        }

        /// <summary>
        /// For requesting registered workspace to zoom in or out to fitview
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual void OnRequestZoomToFitView(object sender, ZoomEventArgs e)
        {
            if (RequestZoomToFitView != null)
            {
                RequestZoomToFitView(this, e);
            }
        }

        public virtual void OnRequestCenterViewOnElement(object sender, ModelEventArgs e)
        {
            if (RequestCenterViewOnElement != null)
                RequestCenterViewOnElement(this, e);
        }

        public virtual void OnRequestNodeCentered(object sender, ModelEventArgs e)
        {
            if (RequestNodeCentered != null)
                RequestNodeCentered(this, e);
        }

        public virtual void OnRequestAddViewToOuterCanvas(object sender, ViewEventArgs e)
        {
            if (RequestAddViewToOuterCanvas != null)
                RequestAddViewToOuterCanvas(this, e);
        }

        public virtual void OnRequestSelectionBoxUpdate(object sender, SelectionBoxUpdateArgs e)
        {
            if (RequestSelectionBoxUpdate != null)
                RequestSelectionBoxUpdate(this, e);
        }

        public virtual void OnWorkspacePropertyEditRequested()
        {
            // extend this for all workspaces
            if (WorkspacePropertyEditRequested != null)
                WorkspacePropertyEditRequested(this.Model);
        }

        private CompositeCollection _workspaceElements = new CompositeCollection();
        public CompositeCollection WorkspaceElements
        {
            get { return _workspaceElements; }
            set
            {
                _workspaceElements = value;
                RaisePropertyChanged("Nodes");
                RaisePropertyChanged("WorkspaceElements");
            }
        }

        ObservableCollection<ConnectorViewModel> _connectors = new ObservableCollection<ConnectorViewModel>();
        private ObservableCollection<Watch3DFullscreenViewModel> _watches = new ObservableCollection<Watch3DFullscreenViewModel>();
        ObservableCollection<NodeViewModel> _nodes = new ObservableCollection<NodeViewModel>();
        ObservableCollection<NoteViewModel> _notes = new ObservableCollection<NoteViewModel>();
        ObservableCollection<InfoBubbleViewModel> _errors = new ObservableCollection<InfoBubbleViewModel>();
        ObservableCollection<InfoBubbleViewModel> preview = new ObservableCollection<InfoBubbleViewModel>();

        public ObservableCollection<ConnectorViewModel> Connectors
        {
            get { return _connectors; }
            set
            {
                _connectors = value;
                RaisePropertyChanged("Connectors");
            }
        }
        public ObservableCollection<NodeViewModel> Nodes
        {
            get { return _nodes; }
            set
            {
                _nodes = value;
                RaisePropertyChanged("Nodes");
            }
        }
        public ObservableCollection<NoteViewModel> Notes
        {
            get { return _notes; }
            set
            {
                _notes = value;
                RaisePropertyChanged("Notes");
            }
        }
        public ObservableCollection<InfoBubbleViewModel> Errors
        {
            get { return _errors; }
            set { _errors = value; RaisePropertyChanged("Errors"); }
        }
        public ObservableCollection<InfoBubbleViewModel> Previews
        {
            get { return preview; }
            set { preview = value; RaisePropertyChanged("Previews"); }
        }

        public string Name
        {
            get
            {
                if (_model == dynSettings.Controller.DynamoViewModel.Model.HomeSpace)
                    return "Home";
                return _model.Name;
            }
        }

        public string FileName
        {
            get { return _model.FileName; }
        }

        public bool CanEditName
        {
            get { return _model != dynSettings.Controller.DynamoViewModel.Model.HomeSpace; }
        }

        public bool IsCurrentSpace
        {
            get { return _model.IsCurrentSpace; }
        }

        public bool IsHomeSpace
        {
            get { return _model == dynSettings.Controller.DynamoModel.HomeSpace; }
        }

        public bool HasUnsavedChanges
        {
            get { return _model.HasUnsavedChanges; }
        }

        public WorkspaceModel Model
        {
            get { return _model; }
        }

        public ObservableCollection<Watch3DFullscreenViewModel> Watch3DViewModels
        {
            get { return _watches; }
            set
            {
                _watches = value;
                RaisePropertyChanged("Watch3DViewModels");
            }
        }

        public double Zoom
        {
            get { return _model.Zoom; }
        }

        public bool ZoomEnabled
        {
            get { return CanZoom(Configurations.ZoomIncrement); }
        }

        public bool CanFindNodesFromElements
        {
            get { return _canFindNodesFromElements; }
            set
            {
                _canFindNodesFromElements = value;
                RaisePropertyChanged("CanFindNodesFromElements");
            }
        }

        public bool CanShowInfoBubble
        {
            get { return this.stateMachine.CurrentState == StateMachine.State.None; }
        }

        public Action FindNodesFromElements { get; set; }

        #endregion

        public WorkspaceViewModel(WorkspaceModel model, DynamoViewModel vm)
        {
            _model = model;
            stateMachine = new StateMachine(this);

            //setup the composite collection
            var previewsColl = new CollectionContainer { Collection = Previews };
            _workspaceElements.Add(previewsColl);

            var nodesColl = new CollectionContainer { Collection = Nodes };
            _workspaceElements.Add(nodesColl);

            var connColl = new CollectionContainer { Collection = Connectors };
            _workspaceElements.Add(connColl);

            var notesColl = new CollectionContainer { Collection = Notes };
            _workspaceElements.Add(notesColl);

            var errorsColl = new CollectionContainer { Collection = Errors };
            _workspaceElements.Add(errorsColl);

            // Add EndlessGrid
            var endlessGrid = new EndlessGridViewModel(this);
            _workspaceElements.Add(endlessGrid);

            //respond to collection changes on the model by creating new view models
            //currently, view models are added for notes and nodes
            //connector view models are added during connection
            _model.Nodes.CollectionChanged += Nodes_CollectionChanged;
            _model.Notes.CollectionChanged += Notes_CollectionChanged;
            _model.Connectors.CollectionChanged += Connectors_CollectionChanged;
            _model.PropertyChanged += ModelPropertyChanged;

            // sync collections
            Nodes_CollectionChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, _model.Nodes));
            Connectors_CollectionChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, _model.Connectors));
            Notes_CollectionChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, _model.Notes));
            Dispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;
        }

        void DynamoViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ShouldBeHitTestVisible")
            {
                RaisePropertyChanged("ShouldBeHitTestVisible");
            }
        }

        void Connectors_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems)
                    {
                        var viewModel = new ConnectorViewModel(item as ConnectorModel);
                        _connectors.Add(viewModel);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    _connectors.Clear();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                    {
                        _connectors.Remove(_connectors.First(x => x.ConnectorModel == item));
                    }
                    break;
            }
        }

        void Notes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems)
                    {
                        //add a corresponding note
                        var viewModel = new NoteViewModel(item as NoteModel);
                        _notes.Add(viewModel);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    _notes.Clear();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                    {
                        _notes.Remove(_notes.First(x => x.Model == item));
                    }
                    break;
            }
        }

        void Nodes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems)
                    {
                        if (item != null && item is NodeModel)
                        {
                            var node = item as NodeModel;

                            NodeViewModel nodeViewModel = new NodeViewModel(node);
                            _nodes.Add(nodeViewModel);
                            Errors.Add(nodeViewModel.ErrorBubble);
                            Previews.Add(nodeViewModel.PreviewBubble);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    _nodes.Clear();
                    Errors.Clear();
                    Previews.Clear();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                    {
                        var node = item as NodeModel;
                        NodeViewModel nodeViewModel = _nodes.First(x => x.NodeLogic == item);
                        Previews.Remove(nodeViewModel.PreviewBubble);
                        Errors.Remove(nodeViewModel.ErrorBubble);
                        _nodes.Remove(nodeViewModel);

                    }
                    break;
            }
        }

        void ModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Name":
                    RaisePropertyChanged("Name");
                    break;
                case "X":
                    break;
                case "Y":
                    break;
                case "Zoom":
                    OnZoomChanged(this, new ZoomEventArgs(_model.Zoom));
                    RaisePropertyChanged("Zoom");
                    ZoomInCommand.RaiseCanExecuteChanged();
                    ZoomOutCommand.RaiseCanExecuteChanged();
                    break;
                case "IsCurrentSpace":
                    RaisePropertyChanged("IsCurrentSpace");
                    RaisePropertyChanged("IsHomeSpace");
                    break;
                case "HasUnsavedChanges":
                    RaisePropertyChanged("HasUnsavedChanges");
                    break;
                case "FileName":
                    RaisePropertyChanged("FileName");
                    break;
            }
        }

        public void SelectAll(object parameter)
        {
            DynamoSelection.Instance.ClearSelection();
            this.Nodes.ToList().ForEach((ele) => DynamoSelection.Instance.Selection.Add(ele.NodeModel));
        }

        internal bool CanSelectAll(object parameter)
        {
            return true;
        }

        internal void SelectInRegion(Rect region, bool isCrossSelect)
        {
            bool fullyEnclosed = !isCrossSelect;

            foreach (NodeModel n in Model.Nodes)
            {
                double x0 = n.X;
                double y0 = n.Y;

                if (IsInRegion(region, n, fullyEnclosed))
                {
                    if (!DynamoSelection.Instance.Selection.Contains(n))
                        DynamoSelection.Instance.Selection.Add(n);
                }
                else
                {
                    if (n.IsSelected)
                        DynamoSelection.Instance.Selection.Remove(n);
                }
            }

            foreach (var n in Model.Notes)
            {
                double x0 = n.X;
                double y0 = n.Y;

                if (IsInRegion(region, n, fullyEnclosed))
                {
                    if (!DynamoSelection.Instance.Selection.Contains(n))
                        DynamoSelection.Instance.Selection.Add(n);
                }
                else
                {
                    if (n.IsSelected)
                        DynamoSelection.Instance.Selection.Remove(n);
                }
            }
        }

        private static bool IsInRegion(Rect region, ILocatable locatable, bool fullyEnclosed)
        {
            double x0 = locatable.X;
            double y0 = locatable.Y;

            if (false == fullyEnclosed) // Cross selection.
            {
                Rect test = new Rect(x0, y0, locatable.Width, locatable.Height);
                return region.IntersectsWith(test);
            }
            else // Contain selection.
            {
                double x1 = x0 + locatable.Width;
                double y1 = y0 + locatable.Height;
                return (region.Contains(x0, y0) && region.Contains(x1, y1));
            }
        }

        public double GetSelectionAverageX()
        {
            return DynamoSelection.Instance.Selection.Where((x) => x is ILocatable)
                           .Cast<ILocatable>()
                           .Select((x) => x.CenterX)
                           .Average();
        }

        public double GetSelectionAverageY()
        {
            return DynamoSelection.Instance.Selection.Where((x) => x is ILocatable)
                           .Cast<ILocatable>()
                           .Select((x) => x.CenterY)
                           .Average();
        }

        public double GetSelectionMinX()
        {
            return DynamoSelection.Instance.Selection.Where((x) => x is ILocatable)
                           .Cast<ILocatable>()
                           .Select((x) => x.X)
                           .Min();
        }

        public double GetSelectionMinY()
        {
            return DynamoSelection.Instance.Selection.Where((x) => x is ILocatable)
                           .Cast<ILocatable>()
                           .Select((x) => x.Y)
                           .Min();
        }

        public double GetSelectionMaxX()
        {
            return DynamoSelection.Instance.Selection.Where((x) => x is ILocatable)
                           .Cast<ILocatable>()
                           .Select((x) => x.X + x.Width)
                           .Max();
        }

        public double GetSelectionMaxLeftX()
        {
            return DynamoSelection.Instance.Selection.Where((x) => x is ILocatable)
                           .Cast<ILocatable>()
                           .Select((x) => x.X)
                           .Max();
        }

        public double GetSelectionMaxY()
        {
            return DynamoSelection.Instance.Selection.Where((x) => x is ILocatable)
                           .Cast<ILocatable>()
                           .Select((x) => x.Y + x.Height)
                           .Max();
        }

        public double GetSelectionMaxTopY()
        {
            return DynamoSelection.Instance.Selection.Where((x) => x is ILocatable)
                           .Cast<ILocatable>()
                           .Select((x) => x.Y)
                           .Max();
        }

        /// <summary>
        /// Get top left point of ILocatable element in this workspace
        /// </summary>
        public Point GetTopLeftPointInLocatables()
        {
            // List of elements that should be take into account
            List<ILocatable> iLocatableElements = new List<ILocatable>();
            iLocatableElements.AddRange(_model.Nodes.ToList<ILocatable>());
            iLocatableElements.AddRange(_model.Notes.ToList<ILocatable>());

            if (iLocatableElements.Count == 0) return new Point();

            Point topLeft = new Point(double.MaxValue, double.MaxValue);

            foreach (ILocatable n in iLocatableElements)
            {
                topLeft.X = Math.Min(n.X, topLeft.X);
                topLeft.Y = Math.Min(n.Y, topLeft.Y);
            }

            return topLeft;
        }

        /// <summary>
        /// Get bottom right point of ILocatable elements in this workspace
        /// </summary>
        public Point GetBottomRightPointInLocatables()
        {
            // List of elements that should be take into account
            List<ILocatable> iLocatableElements = new List<ILocatable>();
            iLocatableElements.AddRange(_model.Nodes.ToList<ILocatable>());
            iLocatableElements.AddRange(_model.Notes.ToList<ILocatable>());

            if (iLocatableElements.Count == 0) return new Point();

            Point bottomRight = new Point(double.MinValue, double.MinValue);

            foreach (ILocatable n in iLocatableElements)
            {
                bottomRight.X = Math.Max(n.X + n.Width, bottomRight.X);
                bottomRight.Y = Math.Max(n.Y + n.Height, bottomRight.Y);
            }

            return bottomRight;
        }

        public void AlignSelected(object parameter)
        {
            string alignType = parameter.ToString();

            if (DynamoSelection.Instance.Selection.Count <= 1) return;

            // All the models in the selection will be modified, 
            // record their current states before anything gets changed.
            SmartCollection<ISelectable> selection = DynamoSelection.Instance.Selection;
            IEnumerable<ModelBase> models = selection.OfType<ModelBase>();
            _model.RecordModelsForModification(models.ToList());

            var toAlign = DynamoSelection.Instance.Selection.Where((x) => x is ILocatable)
                           .Cast<ILocatable>()
                           .ToList();

            if (alignType == "HorizontalCenter")  // make vertial line of elements
            {
                var xAll = GetSelectionAverageX();
                toAlign.ForEach((x) => { x.CenterX = xAll; });
            }
            else if (alignType == "HorizontalLeft")
            {
                var xAll = GetSelectionMinX();
                toAlign.ForEach((x) => { x.X = xAll; });
            }
            else if (alignType == "HorizontalRight")
            {
                var xAll = GetSelectionMaxX();
                toAlign.ForEach((x) => { x.X = xAll - x.Width; });
            }
            else if (alignType == "VerticalCenter")
            {
                var yAll = GetSelectionAverageY();
                toAlign.ForEach((x) => { x.CenterY = yAll; });

            }
            else if (alignType == "VerticalTop")
            {
                var yAll = GetSelectionMinY();
                toAlign.ForEach((x) => { x.Y = yAll; });
            }
            else if (alignType == "VerticalBottom")
            {
                var yAll = GetSelectionMaxY();
                toAlign.ForEach((x) => { x.Y = yAll - x.Height; });
            }
            else if (alignType == "VerticalDistribute")
            {
                if (DynamoSelection.Instance.Selection.Count <= 2) return;

                var yMin = GetSelectionMinY();
                var yMax = GetSelectionMaxTopY();
                var spacing = (yMax - yMin) / (DynamoSelection.Instance.Selection.Count - 1);
                int count = 0;

                toAlign.OrderBy((x) => x.Y)
                           .ToList()
                           .ForEach((x) => x.Y = yMin + spacing * count++);
            }
            else if (alignType == "HorizontalDistribute")
            {
                if (DynamoSelection.Instance.Selection.Count <= 2) return;

                var xMin = GetSelectionMinX();
                var xMax = GetSelectionMaxLeftX();
                var spacing = (xMax - xMin) / (DynamoSelection.Instance.Selection.Count - 1);
                int count = 0;

                toAlign.OrderBy((x) => x.X)
                           .ToList()
                           .ForEach((x) => x.X = xMin + spacing * count++);
            }

            toAlign.ForEach(x=>x.ReportPosition());
        }

        private bool CanAlignSelected(string alignType)
        {
            return Selection.DynamoSelection.Instance.Selection.Count > 1;
        }

        private bool CanAlignSelected(object parameter)
        {
            return Selection.DynamoSelection.Instance.Selection.Count > 1;
        }

        private void Hide(object parameters)
        {
            if (!this.Model.HasUnsavedChanges || dynSettings.Controller.DynamoViewModel.AskUserToSaveWorkspaceOrCancel(this.Model))
            {
                dynSettings.Controller.DynamoViewModel.Model.HideWorkspace(this._model);
            }
        }

        private bool CanHide(object parameters)
        {
            // can hide anything but the home workspace
            return dynSettings.Controller.DynamoViewModel.Model.HomeSpace != this._model;
        }

        private void SetCurrentOffset(object parameter)
        {
            var p = (Point)parameter;

            //set the current offset without triggering
            //any property change notices.
            if (_model.X != p.X && _model.Y != p.Y)
            {
                _model.X = p.X;
                _model.Y = p.Y;
            }
        }

        private bool CanSetCurrentOffset(object parameter)
        {
            return true;
        }

        private void CreateNodeFromSelection(object parameter)
        {
            CollapseNodes(
                DynamoSelection.Instance.Selection.Where(x => x is NodeModel)
                    .Select(x => (x as NodeModel)));
        }

        //private void NodeFromSelectionCanExecuteChanged(object sender, NotifyCollectionChangedEventArgs e)
        //{
        //    NodeFromSelectionCommand.RaiseCanExecuteChanged();
        //}

        private void AlignSelectionCanExecuteChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            AlignSelectedCommand.RaiseCanExecuteChanged();
        }

        private bool CanCreateNodeFromSelection(object parameter)
        {
            if (DynamoSelection.Instance.Selection.Count(x => x is NodeModel) > 1)
            {
                return true;
            }
            return false;
        }

        private void ZoomIn(object o)
        {
            OnRequestZoomToViewportCenter(this, new ZoomEventArgs(Configurations.ZoomIncrement));
            ResetFitViewToggle(o);
        }

        private bool CanZoomIn(object o)
        {
            return CanZoom(Configurations.ZoomIncrement);
        }

        private void ZoomOut(object o)
        {
            OnRequestZoomToViewportCenter(this, new ZoomEventArgs(-Configurations.ZoomIncrement));
            ResetFitViewToggle(o);
        }

        private bool CanZoomOut(object o)
        {
            return CanZoom(-Configurations.ZoomIncrement);
        }

        private bool CanZoom(double zoom)
        {
            if ((zoom < 0 && _model.Zoom <= WorkspaceModel.ZOOM_MINIMUM)
                || (zoom > 0 && _model.Zoom >= WorkspaceModel.ZOOM_MAXIMUM))
                return false;
            return true;
        }

        private void SetZoom(object zoom)
        {
            _model.Zoom = Convert.ToDouble(zoom);
        }

        private bool CanSetZoom(object zoom)
        {
            double setZoom = Convert.ToDouble(zoom);
            if (setZoom >= WorkspaceModel.ZOOM_MINIMUM && setZoom <= WorkspaceModel.ZOOM_MAXIMUM)
                return true;
            else
                return false;
        }

        private bool _fitViewActualZoomToggle = false;
        private void FitView(object o)
        {
            // Get the offset and focus width & height (zoom if 100%)
            double minX, maxX, minY, maxY;

            // Get the width and height of area to fit
            if (DynamoSelection.Instance.Selection.Count > 0)
            {   // has selection
                minX = GetSelectionMinX();
                maxX = GetSelectionMaxX();
                minY = GetSelectionMinY();
                maxY = GetSelectionMaxY();
            }
            else
            {   // no selection, fitview all nodes
                if (_nodes.Count() <= 0) return;

                IEnumerable<ILocatable> nodes = _nodes.Select((x) => x.NodeModel).Where((x) => x is ILocatable).Cast<ILocatable>();
                minX = nodes.Select((x) => x.X).Min();
                maxX = nodes.Select((x) => x.X + x.Width).Max();
                minY = nodes.Select((y) => y.Y).Min();
                maxY = nodes.Select((y) => y.Y + y.Height).Max();
            }

            Point offset = new Point(minX, minY);
            double focusWidth = maxX - minX;
            double focusHeight = maxY - minY;
            ZoomEventArgs zoomArgs;

            _fitViewActualZoomToggle = !_fitViewActualZoomToggle;
            if (_fitViewActualZoomToggle)
                zoomArgs = new ZoomEventArgs(offset, focusWidth, focusHeight);
            else
                zoomArgs = new ZoomEventArgs(offset, focusWidth, focusHeight, 1.0);

            OnRequestZoomToFitView(this, zoomArgs);
        }

        private bool CanFitView(object o)
        {
            return true;
        }

        private void ResetFitViewToggle(object o)
        {
            _fitViewActualZoomToggle = false;
        }

        private bool CanResetFitViewToggle(object o)
        {
            return true;
        }

        private void TogglePan(object o)
        {
            RequestTogglePanMode();
        }

        private bool CanTogglePan(object o)
        {
            return true;
        }

        private void FindById(object id)
        {
            try
            {
                var node = dynSettings.Controller.DynamoModel.Nodes.First(x => x.GUID.ToString() == id.ToString());

                if (node != null)
                {
                    //select the element
                    DynamoSelection.Instance.ClearSelection();
                    DynamoSelection.Instance.Selection.Add(node);

                    //focus on the element
                    dynSettings.Controller.DynamoViewModel.ShowElement(node);

                    return;
                }
            }
            catch
            {
                DynamoLogger.Instance.Log("No node could be found with that Id.");
            }

            try
            {
                var function =
                    (Function)dynSettings.Controller.DynamoModel.Nodes.First(x => x is Function && ((Function)x).Definition.FunctionId.ToString() == id.ToString());

                if (function != null)
                {
                    //select the element
                    DynamoSelection.Instance.ClearSelection();
                    DynamoSelection.Instance.Selection.Add(function);

                    //focus on the element
                    dynSettings.Controller.DynamoViewModel.ShowElement(function);
                }
            }
            catch
            {
                DynamoLogger.Instance.Log("No node could be found with that Id.");
                return;
            }
        }

        private bool CanFindById(object id)
        {
            if (!string.IsNullOrEmpty(id.ToString()))
                return true;
            return false;
        }

        private void FindNodesFromSelection(object parameter)
        {
            FindNodesFromElements();
        }

        private bool CanFindNodesFromSelection(object parameter)
        {
            if (FindNodesFromElements != null)
                return true;
            return false;
        }

        /// <summary>
        ///     Collapse a set of nodes in the current workspace.  Has the side effects of prompting the user
        ///     first in order to obtain the name and category for the new node, 
        ///     writes the function to a dyf file, adds it to the FunctionDict, adds it to search, and compiles and 
        ///     places the newly created symbol (defining a lambda) in the Controller's FScheme Environment.  
        /// </summary>
        /// <param name="selectedNodes"> The function definition for the user-defined node </param>
        internal void CollapseNodes(IEnumerable<NodeModel> selectedNodes)
        {
            NodeCollapser.Collapse(selectedNodes, dynSettings.Controller.DynamoViewModel.CurrentSpace);
        }

        internal void Loaded()
        {
            RaisePropertyChanged("IsHomeSpace");

            // New workspace or swapped workspace to follow it offset and zoom
            OnCurrentOffsetChanged(this, new PointEventArgs(new Point(Model.X, Model.Y)));
            OnZoomChanged(this, new ZoomEventArgs(Model.Zoom));
        }

        private void PauseVisualizationManagerUpdates(object parameter)
        {
            dynSettings.Controller.VisualizationManager.UpdatingPaused = (bool) parameter;
        }

        private bool CanPauseVisualizationManagerUpdates(object parameter)
        {
            return true;
        }
    }

    public class ViewModelEventArgs : EventArgs
    {
        public NodeViewModel ViewModel { get; set; }
        public ViewModelEventArgs(NodeViewModel vm)
        {
            ViewModel = vm;
        }
    }
}
