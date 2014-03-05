using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Selection;
using Dynamo.UI;
using Dynamo.Utilities;
using System.Windows.Threading;
using System.Windows.Input;
using Microsoft.Practices.Prism;

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

        private bool canFindNodesFromElements;
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
        private Cursor currentCursor;
        public Cursor CurrentCursor
        {
            get { return currentCursor; }
            set { currentCursor = value; RaisePropertyChanged("CurrentCursor"); }
        }

        /// <summary>
        /// Force Cursor Property Binding for WorkspaceView
        /// </summary>
        private bool isCursorForced;
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
                Debug.WriteLine("Setting current offset to {0}", e.Point);
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
                WorkspacePropertyEditRequested(Model);
        }

        private CompositeCollection workspaceElements = new CompositeCollection();
        public CompositeCollection WorkspaceElements
        {
            get { return workspaceElements; }
            set
            {
                workspaceElements = value;
                RaisePropertyChanged("Nodes");
                RaisePropertyChanged("WorkspaceElements");
            }
        }

        ObservableCollection<ConnectorViewModel> connectors = new ObservableCollection<ConnectorViewModel>();
        private ObservableCollection<Watch3DFullscreenViewModel> watches = new ObservableCollection<Watch3DFullscreenViewModel>();
        ObservableCollection<NodeViewModel> nodes = new ObservableCollection<NodeViewModel>();
        ObservableCollection<NoteViewModel> notes = new ObservableCollection<NoteViewModel>();
        ObservableCollection<InfoBubbleViewModel> errors = new ObservableCollection<InfoBubbleViewModel>();
        ObservableCollection<InfoBubbleViewModel> preview = new ObservableCollection<InfoBubbleViewModel>();

        public ObservableCollection<ConnectorViewModel> Connectors
        {
            get { return connectors; }
            set
            {
                connectors = value;
                RaisePropertyChanged("Connectors");
            }
        }
        public ObservableCollection<NodeViewModel> Nodes
        {
            get { return nodes; }
            set
            {
                nodes = value;
                RaisePropertyChanged("Nodes");
            }
        }
        public ObservableCollection<NoteViewModel> Notes
        {
            get { return notes; }
            set
            {
                notes = value;
                RaisePropertyChanged("Notes");
            }
        }
        public ObservableCollection<InfoBubbleViewModel> Errors
        {
            get { return errors; }
            set { errors = value; RaisePropertyChanged("Errors"); }
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
                if (Model == dynSettings.Controller.DynamoViewModel.Model.HomeSpace)
                    return "Home";
                return Model.Name;
            }
        }

        public string FileName
        {
            get { return Model.FileName; }
        }

        public bool CanEditName
        {
            get { return Model != dynSettings.Controller.DynamoViewModel.Model.HomeSpace; }
        }

        public bool IsCurrentSpace
        {
            get { return Model.IsCurrentSpace; }
        }

        public bool IsHomeSpace
        {
            get { return Model == dynSettings.Controller.DynamoModel.HomeSpace; }
        }

        public bool HasUnsavedChanges
        {
            get { return Model.HasUnsavedChanges; }
        }

        public WorkspaceModel Model { get; set; }

        public ObservableCollection<Watch3DFullscreenViewModel> Watch3DViewModels
        {
            get { return watches; }
            set
            {
                watches = value;
                RaisePropertyChanged("Watch3DViewModels");
            }
        }

        public double Zoom
        {
            get { return Model.Zoom; }
        }

        public bool ZoomEnabled
        {
            get { return CanZoom(Configurations.ZoomIncrement); }
        }

        public bool CanFindNodesFromElements
        {
            get { return canFindNodesFromElements; }
            set
            {
                canFindNodesFromElements = value;
                RaisePropertyChanged("CanFindNodesFromElements");
            }
        }

        public bool CanShowInfoBubble
        {
            get { return stateMachine.CurrentState == StateMachine.State.None; }
        }

        public Action FindNodesFromElements { get; set; }

        #endregion

        public WorkspaceViewModel(WorkspaceModel model)
        {
            Model = model;
            stateMachine = new StateMachine(this);

            //setup the composite collection
            var previewsColl = new CollectionContainer { Collection = Previews };
            workspaceElements.Add(previewsColl);

            var nodesColl = new CollectionContainer { Collection = Nodes };
            workspaceElements.Add(nodesColl);

            var connColl = new CollectionContainer { Collection = Connectors };
            workspaceElements.Add(connColl);

            var notesColl = new CollectionContainer { Collection = Notes };
            workspaceElements.Add(notesColl);

            var errorsColl = new CollectionContainer { Collection = Errors };
            workspaceElements.Add(errorsColl);

            // Add EndlessGrid
            var endlessGrid = new EndlessGridViewModel(this);
            workspaceElements.Add(endlessGrid);

            //respond to collection changes on the model by creating new view models
            //currently, view models are added for notes and nodes
            //connector view models are added during connection
            Model.Nodes.CollectionChanged += Nodes_CollectionChanged;
            Model.Notes.CollectionChanged += Notes_CollectionChanged;
            Model.Connectors.CollectionChanged += Connectors_CollectionChanged;


            DynamoSelection.Instance.Selection.CollectionChanged += AlignSelectionCanExecuteChanged;

            // sync collections
            Nodes_CollectionChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, Model.Nodes));
            Connectors_CollectionChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, Model.Connectors));
            Notes_CollectionChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, Model.Notes));
            Dispatcher = Dispatcher.CurrentDispatcher;
        }

        void DynamoViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ShouldBeHitTestVisible")
            {
                RaisePropertyChanged("ShouldBeHitTestVisible");
            }
        }

        void Connectors_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    connectors.AddRange(from object item in e.NewItems select new ConnectorViewModel(item as ConnectorModel));
                    break;
                case NotifyCollectionChangedAction.Reset:
                    connectors.Clear();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                    {
                        connectors.Remove(connectors.First(x => x.ConnectorModel == item));
                    }
                    break;
            }
        }

        void Notes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    notes.AddRange(
                        e.NewItems.Cast<NoteModel>().Select(item => new NoteViewModel(item)));
                    break;
                case NotifyCollectionChangedAction.Reset:
                    notes.Clear();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                    {
                        notes.Remove(notes.First(x => x.Model == item));
                    }
                    break;
            }
        }

        void Nodes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            IEnumerable<NodeViewModel> viewModels;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    viewModels = e.NewItems.OfType<NodeModel>()
                                  .Select(n => new NodeViewModel(n));

                    foreach (var vm in viewModels)
                    {
                        nodes.Add(vm);
                        Errors.Add(vm.ErrorBubble);
                        Previews.Add(vm.PreviewBubble);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    nodes.Clear();
                    Errors.Clear();
                    Previews.Clear();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    viewModels = e.OldItems.Cast<NodeModel>()
                                  .Select(node => nodes.First(x => x.NodeLogic == node));

                    foreach (NodeViewModel nodeViewModel in viewModels)
                    {
                        Previews.Remove(nodeViewModel.PreviewBubble);
                        Errors.Remove(nodeViewModel.ErrorBubble);
                        nodes.Remove(nodeViewModel);
                    }
                    break;
            }
        }

        void ModelPropertyChanged(object sender, PropertyChangedEventArgs e)
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
                    OnZoomChanged(this, new ZoomEventArgs(Model.Zoom));
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
            Nodes.ToList().ForEach(ele => DynamoSelection.Instance.Selection.Add(ele.NodeModel));
        }

        internal bool CanSelectAll(object parameter)
        {
            return true;
        }

        internal void SelectInRegion(Rect region, bool isCrossSelect)
        {
            bool fullyEnclosed = !isCrossSelect;

            foreach (var n in Model.Nodes.Cast<ModelBase>().Concat(Model.Notes))
            {
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
                var test = new Rect(x0, y0, locatable.Width, locatable.Height);
                return region.IntersectsWith(test);
            }

            double x1 = x0 + locatable.Width;
            double y1 = y0 + locatable.Height;
            return (region.Contains(x0, y0) && region.Contains(x1, y1));
        }

        public double GetSelectionAverageX()
        {
            return DynamoSelection.Instance.Selection.Where(x => x is ILocatable)
                           .Cast<ILocatable>()
                           .Select(x => x.CenterX)
                           .Average();
        }

        public double GetSelectionAverageY()
        {
            return DynamoSelection.Instance.Selection.Where(x => x is ILocatable)
                           .Cast<ILocatable>()
                           .Select(x => x.CenterY)
                           .Average();
        }

        public double GetSelectionMinX()
        {
            return DynamoSelection.Instance.Selection.Where(x => x is ILocatable)
                           .Cast<ILocatable>()
                           .Select(x => x.X)
                           .Min();
        }

        public double GetSelectionMinY()
        {
            return DynamoSelection.Instance.Selection.Where(x => x is ILocatable)
                           .Cast<ILocatable>()
                           .Select(x => x.Y)
                           .Min();
        }

        public double GetSelectionMaxX()
        {
            return DynamoSelection.Instance.Selection.Where(x => x is ILocatable)
                           .Cast<ILocatable>()
                           .Select(x => x.X + x.Width)
                           .Max();
        }

        public double GetSelectionMaxLeftX()
        {
            return DynamoSelection.Instance.Selection.Where(x => x is ILocatable)
                           .Cast<ILocatable>()
                           .Select(x => x.X)
                           .Max();
        }

        public double GetSelectionMaxY()
        {
            return DynamoSelection.Instance.Selection.Where(x => x is ILocatable)
                           .Cast<ILocatable>()
                           .Select(x => x.Y + x.Height)
                           .Max();
        }

        public double GetSelectionMaxTopY()
        {
            return DynamoSelection.Instance.Selection.Where(x => x is ILocatable)
                           .Cast<ILocatable>()
                           .Select(x => x.Y)
                           .Max();
        }

        public void AlignSelected(object parameter)
        {
            string alignType = parameter.ToString();

            if (DynamoSelection.Instance.Selection.Count <= 1) return;

            // All the models in the selection will be modified, 
            // record their current states before anything gets changed.
            SmartCollection<ISelectable> selection = DynamoSelection.Instance.Selection;
            IEnumerable<ModelBase> models = selection.OfType<ModelBase>();
            Model.RecordModelsForModification(models.ToList());

            var toAlign = DynamoSelection.Instance.Selection.Where(x => x is ILocatable)
                           .Cast<ILocatable>()
                           .ToList();

            if (alignType == "HorizontalCenter")  // make vertial line of elements
            {
                var xAll = GetSelectionAverageX();
                toAlign.ForEach(x => { x.CenterX = xAll; });
            }
            else if (alignType == "HorizontalLeft")
            {
                var xAll = GetSelectionMinX();
                toAlign.ForEach(x => { x.X = xAll; });
            }
            else if (alignType == "HorizontalRight")
            {
                var xAll = GetSelectionMaxX();
                toAlign.ForEach(x => { x.X = xAll - x.Width; });
            }
            else if (alignType == "VerticalCenter")
            {
                var yAll = GetSelectionAverageY();
                toAlign.ForEach(x => { x.CenterY = yAll; });

            }
            else if (alignType == "VerticalTop")
            {
                var yAll = GetSelectionMinY();
                toAlign.ForEach(x => { x.Y = yAll; });
            }
            else if (alignType == "VerticalBottom")
            {
                var yAll = GetSelectionMaxY();
                toAlign.ForEach(x => { x.Y = yAll - x.Height; });
            }
            else if (alignType == "VerticalDistribute")
            {
                if (DynamoSelection.Instance.Selection.Count <= 2) return;

                var yMin = GetSelectionMinY();
                var yMax = GetSelectionMaxTopY();
                var spacing = (yMax - yMin) / (DynamoSelection.Instance.Selection.Count - 1);
                int count = 0;

                toAlign.OrderBy(x => x.Y)
                           .ToList()
                           .ForEach(x => x.Y = yMin + spacing * count++);
            }
            else if (alignType == "HorizontalDistribute")
            {
                if (DynamoSelection.Instance.Selection.Count <= 2) return;

                var xMin = GetSelectionMinX();
                var xMax = GetSelectionMaxLeftX();
                var spacing = (xMax - xMin) / (DynamoSelection.Instance.Selection.Count - 1);
                int count = 0;

                toAlign.OrderBy(x => x.X)
                           .ToList()
                           .ForEach(x => x.X = xMin + spacing * count++);
            }

            toAlign.ForEach(x => x.ReportPosition());
        }

        private bool CanAlignSelected(string alignType)
        {
            return DynamoSelection.Instance.Selection.Count > 1;
        }

        private static bool CanAlignSelected(object parameter)
        {
            return DynamoSelection.Instance.Selection.Count > 1;
        }

        private void Hide(object parameters)
        {
            if (!Model.HasUnsavedChanges || dynSettings.Controller.DynamoViewModel.AskUserToSaveWorkspaceOrCancel(Model))
            {
                dynSettings.Controller.DynamoViewModel.Model.HideWorkspace(Model);
            }
        }

        private bool CanHide(object parameters)
        {
            // can hide anything but the home workspace
            return dynSettings.Controller.DynamoViewModel.Model.HomeSpace != Model;
        }

        private void SetCurrentOffset(object parameter)
        {
            var p = (Point)parameter;

            //set the current offset without triggering
            //any property change notices.
            if (Model.X != p.X && Model.Y != p.Y)
            {
                Model.X = p.X;
                Model.Y = p.Y;
            }
        }

        private static bool CanSetCurrentOffset(object parameter)
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

        private static bool CanCreateNodeFromSelection(object parameter)
        {
            return DynamoSelection.Instance.Selection.Count(x => x is NodeModel) > 1;
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
            return (!(zoom < 0) || !(Model.Zoom <= WorkspaceModel.ZOOM_MINIMUM))
                && (!(zoom > 0) || !(Model.Zoom >= WorkspaceModel.ZOOM_MAXIMUM));
        }

        private void SetZoom(object zoom)
        {
            Model.Zoom = Convert.ToDouble(zoom);
        }

        private static bool CanSetZoom(object zoom)
        {
            double setZoom = Convert.ToDouble(zoom);
            return setZoom >= WorkspaceModel.ZOOM_MINIMUM && setZoom <= WorkspaceModel.ZOOM_MAXIMUM;
        }

        private bool fitViewActualZoomToggle;
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
                if (!nodes.Any())
                    return;

                var nodeModels = nodes.Select(x => x.NodeModel).Where(x => x != null).ToList();

                var minMax =
                    nodeModels.Aggregate(
                        //Initialize to extremes
                        new
                        {
                            MinX = double.PositiveInfinity,
                            MaxX = double.NegativeInfinity,
                            MinY = double.PositiveInfinity,
                            MaxY = double.NegativeInfinity
                        },
                        //Compare each node dims with calculated mins and maxes
                        (a, x) =>
                            new
                            {
                                MinX = Math.Min(a.MinX, x.X),
                                MaxX = Math.Max(a.MaxX, x.X + x.Width),
                                MinY = Math.Min(a.MinY, x.Y),
                                MaxY = Math.Max(a.MaxY, x.Y + x.Height)
                            });

                //Update based on results
                minX = minMax.MinX;
                maxX = minMax.MaxX;
                minY = minMax.MinY;
                maxY = minMax.MaxY;
            }

            var offset = new Point(minX, minY);
            double focusWidth = maxX - minX;
            double focusHeight = maxY - minY;

            fitViewActualZoomToggle = !fitViewActualZoomToggle;
            ZoomEventArgs zoomArgs = fitViewActualZoomToggle
                ? new ZoomEventArgs(offset, focusWidth, focusHeight)
                : new ZoomEventArgs(offset, focusWidth, focusHeight, 1.0);

            OnRequestZoomToFitView(this, zoomArgs);
        }

        private static bool CanFitView(object o)
        {
            return true;
        }

        private void ResetFitViewToggle(object o)
        {
            fitViewActualZoomToggle = false;
        }

        private static bool CanResetFitViewToggle(object o)
        {
            return true;
        }

        private void TogglePan(object o)
        {
            RequestTogglePanMode();
        }

        private static bool CanTogglePan(object o)
        {
            return true;
        }

        private static void FindById(object id)
        {
            var idstring = id.ToString();

            var allNodes = dynSettings.Controller.DynamoModel.Nodes;

            var node =
                // get the first node whose GUID matches the given id
                allNodes.FirstOrDefault(x => x.GUID.ToString() == idstring)
                    // if none was found, look for custom node instances whose definition matches the given id
                    ?? allNodes.OfType<CustomNodeInstance>()
                               .FirstOrDefault(x => x.Definition.FunctionId.ToString() == idstring);

            if (node != null)
            {
                //select the element
                DynamoSelection.Instance.ClearSelection();
                DynamoSelection.Instance.Selection.Add(node);

                //focus on the element
                dynSettings.Controller.DynamoViewModel.ShowElement(node);
            }
            else
                DynamoLogger.Instance.Log("No node could be found with that Id.");
        }

        private static bool CanFindById(object id)
        {
            return !string.IsNullOrEmpty(id.ToString());
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
            dynSettings.Controller.VisualizationManager.UpdatingPaused = (bool)parameter;
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
