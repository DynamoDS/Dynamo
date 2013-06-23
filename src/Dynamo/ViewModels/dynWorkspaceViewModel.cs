using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Dynamo.Connectors;
using Dynamo.Controls;
using Dynamo.Nodes;
using Dynamo.Selection;
using Dynamo.Utilities;
using Microsoft.Practices.Prism.Commands;

namespace Dynamo
{
    public delegate void PointEventHandler(object sender, EventArgs e);
    public delegate void NodeEventHandler(object sender, EventArgs e);
    public delegate void NoteEventHandler(object sender, EventArgs e);
    public delegate void ViewEventHandler(object sender, EventArgs e);

    public class dynWorkspaceViewModel: dynViewModelBase
    {
        #region Properties and Fields

        public dynWorkspaceModel _model;

        private bool isConnecting = false;
        private double zoom = 1.0;

        public event EventHandler StopDragging;
        public event PointEventHandler CurrentOffsetChanged;
        public event NodeEventHandler RequestCenterViewOnElement;
        //public event EventHandler UILocked;
        //public event EventHandler UIUnlocked;
        public event NodeEventHandler RequestNodeCentered;
        //public event NoteEventHandler RequestNoteCentered;
        public event ViewEventHandler RequestAddViewToOuterCanvas;

        private bool _watchEscapeIsDown = false;

        public virtual void OnCurrentOffsetChanged(object sender, PointEventArgs e)
        {
            if (CurrentOffsetChanged != null)
                CurrentOffsetChanged(this, e);
        }
        public virtual void OnStopDragging(object sender, EventArgs e)
        {
            if (StopDragging != null)
                StopDragging(this, e);
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

        //public virtual void OnRequestNoteCentered(object sender, NoteEventArgs e)
        //{
        //    if (RequestNoteCentered != null)
        //        RequestNoteCentered(this, e);
        //}

        public virtual void OnRequestAddViewToOuterCanvas(object sender, ViewEventArgs e)
        {
            if (RequestAddViewToOuterCanvas != null)
                RequestAddViewToOuterCanvas(this, e);
        }

        ObservableCollection<dynConnectorViewModel> _connectors = new ObservableCollection<dynConnectorViewModel>();
        private ObservableCollection<Watch3DFullscreenViewModel> _watches = new ObservableCollection<Watch3DFullscreenViewModel>();
        ObservableCollection<dynNodeViewModel> _nodes = new ObservableCollection<dynNodeViewModel>();
        ObservableCollection<dynNoteViewModel> _notes = new ObservableCollection<dynNoteViewModel>();
        
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

        public ObservableCollection<dynConnectorViewModel> Connectors
        {
            get { return _connectors; }
            set { 
                _connectors = value;
                RaisePropertyChanged("Connectors");
            }
        }
        public ObservableCollection<dynNodeViewModel> Nodes
        {
            get { return _nodes; }
            set
            {
                _nodes = value;
                RaisePropertyChanged("Nodes");
            }
        }
        public ObservableCollection<dynNoteViewModel> Notes
        {
            get { return _notes; }
            set
            {
                _notes = value;
                RaisePropertyChanged("Notes");
            }
        }

        public DelegateCommand<object> HideCommand { get; set; }
        public DelegateCommand<object> CrossSelectCommand { get; set; }
        public DelegateCommand<object> ContainSelectCommand { get; set; }
        public DelegateCommand UpdateSelectedConnectorsCommand { get; set; }
        public DelegateCommand<object> SetCurrentOffsetCommand { get; set; }
        public DelegateCommand NodeFromSelectionCommand { get; set; }
        public DelegateCommand<object> SetZoomCommand { get; set; }
        public DelegateCommand<object> FindByIdCommand { get; set; }

        public DelegateCommand<string> AlignSelectedCommand { get; set; }

        public string Name
        {
            get
            {
                if (_model == dynSettings.Controller.DynamoViewModel.Model.HomeSpace)
                    return "Home";
                return _model.Name;
            }
        }

        public string FilePath
        {
            get { return _model.FilePath; }
        }

        public void FullscreenChanged()
        {
            RaisePropertyChanged("FullscreenWatchVisible");

            if (dynSettings.Controller.IsProcessingCommandQueue)
                return;

            dynSettings.Controller.RunCommand( dynSettings.Controller.DynamoViewModel.RunExpressionCommand, null );
        }

        public bool FullscreenWatchVisible
        {
            get { return dynSettings.Controller.DynamoViewModel.FullscreenWatchShowing; }
        }

        private dynConnectorViewModel activeConnector;
        public dynConnectorViewModel ActiveConnector
        {
            get { return activeConnector; }
            set
            {
                if (value != null)
                {
                    WorkspaceElements.Add(value);
                    activeConnector = value;
                }    
                else
                {
                    WorkspaceElements.Remove(activeConnector);
                }
                
                RaisePropertyChanged("ActiveConnector");
            }
        }

        public Visibility EditNameVisibility
        {
            get
            {
                if (_model != dynSettings.Controller.DynamoViewModel.Model.HomeSpace)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public bool CanEditName
        {
            get { return _model != dynSettings.Controller.DynamoViewModel.Model.HomeSpace; }
        }

        public bool IsConnecting
        {
            get { return isConnecting; }
            set { isConnecting = value; }
        }

        //public Color BackgroundColor
        //{
        //    get
        //    {
        //        if (_model == dynSettings.Controller.DynamoViewModel.Model.HomeSpace)
        //            return Color.FromRgb(220, 220, 220);
        //        return Color.FromRgb(255, 255, 220);
        //    }
        //}

        public bool IsCurrentSpace
        {
            get { return _model.IsCurrentSpace; }
        }

        public bool IsHomeSpace
        {
            get { return _model == dynSettings.Controller.DynamoModel.HomeSpace; }
        }

        public bool WatchEscapeIsDown
        {
            get { return _watchEscapeIsDown; }
            set 
            { 
                _watchEscapeIsDown = value;
                RaisePropertyChanged("WatchEscapeIsDown");
                RaisePropertyChanged("ShouldBeHitTestVisible");
            }
        }

        public bool ShouldBeHitTestVisible
        {
            get { return !WatchEscapeIsDown; }
        }

        public bool HasUnsavedChanges
        {
            get { return _model.HasUnsavedChanges; }
        }

        /// <summary>
        /// Specifies the pan location of the view
        /// </summary>
        public Point CurrentOffset
        {
            get
            {
                return new Point(_model.X, _model.Y);
            }
            set
            {
                OnCurrentOffsetChanged(this, new PointEventArgs(value));
            }
        }

        public dynWorkspaceModel Model
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
            get { return zoom; }
            set 
            { 
                zoom = value;
                RaisePropertyChanged("Zoom");
            }
        }

        #endregion

        public dynWorkspaceViewModel(dynWorkspaceModel model, DynamoViewModel vm)
        {
            _model = model;

            this.CurrentOffset = new Point(10,10);

            var nodesColl = new CollectionContainer();
            nodesColl.Collection = Nodes;
            WorkspaceElements.Add(nodesColl);

            var connColl = new CollectionContainer();
            connColl.Collection = Connectors;
            WorkspaceElements.Add(connColl);

            var notesColl = new CollectionContainer();
            notesColl.Collection = Notes;
            WorkspaceElements.Add(notesColl);

            //var watch3DColl = new CollectionContainer();
            //watch3DColl.Collection = Watch3DViewModels;
            //WorkspaceElements.Add(watch3DColl);
            
            //Watch3DViewModels.Add(new Watch3DFullscreenViewModel(this));

            //respond to collection changes on the model by creating new view models
            //currently, view models are added for notes and nodes
            //connector view models are added during connection
            _model.Nodes.CollectionChanged += Nodes_CollectionChanged;
            _model.Notes.CollectionChanged += Notes_CollectionChanged;
            _model.Connectors.CollectionChanged += Connectors_CollectionChanged;
            _model.PropertyChanged += ModelPropertyChanged;

            HideCommand = new DelegateCommand<object>(Hide, CanHide);
            CrossSelectCommand = new DelegateCommand<object>(CrossingSelect, CanCrossSelect);
            ContainSelectCommand = new DelegateCommand<object>(ContainSelect, CanContainSelect);
            SetCurrentOffsetCommand = new DelegateCommand<object>(SetCurrentOffset, CanSetCurrentOffset);
            NodeFromSelectionCommand = new DelegateCommand(CreateNodeFromSelection, CanCreateNodeFromSelection);
            SetZoomCommand = new DelegateCommand<object>(SetZoom, CanSetZoom);
            FindByIdCommand = new DelegateCommand<object>(FindById, CanFindById);
            AlignSelectedCommand = new DelegateCommand<string>(AlignSelected, CanAlignSelected);

            DynamoSelection.Instance.Selection.CollectionChanged += NodeFromSelectionCanExecuteChanged;

            // sync collections
            Nodes_CollectionChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, _model.Nodes));
            Connectors_CollectionChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, _model.Connectors));
            Notes_CollectionChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, _model.Notes));
        }

        void Connectors_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems)
                    {
                        var viewModel = new dynConnectorViewModel(item as dynConnectorModel);
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
                        var viewModel = new dynNoteViewModel(item as dynNoteModel);
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
                        if (item != null && item is dynNodeModel)
                        {
                            _nodes.Add(new dynNodeViewModel(item as dynNodeModel));
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    _nodes.Clear();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                    {
                        _nodes.Remove(_nodes.First(x => x.NodeLogic == item));
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
                    RaisePropertyChanged("CurrentOffset");
                    break;
                case "Y":
                    RaisePropertyChanged("CurrentOffset");
                    break;
                case "IsCurrentSpace":
                    RaisePropertyChanged("IsCurrentSpace");
                    RaisePropertyChanged("IsHomeSpace");
                    break;
                case "HasUnsavedChanges":
                    RaisePropertyChanged("HasUnsavedChanges");
                    break;
                case "FilePath":
                    RaisePropertyChanged("FilePath");
                    break;
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

        private void AlignSelected(string alignType)
        {
            if (alignType == "HorizontalCenter")  // make vertial line of elements
            {
                var xAll = GetSelectionAverageX();
                DynamoSelection.Instance.Selection.Where((x) => x is ILocatable)
                           .Cast<ILocatable>()
                           .ToList().ForEach((x) => { x.CenterX = xAll; });
            }
            else if (alignType == "HorizontalLeft")
            {
                var xAll = GetSelectionMinX();
                DynamoSelection.Instance.Selection.Where((x) => x is ILocatable)
                           .Cast<ILocatable>()
                           .ToList().ForEach((x) => { x.X = xAll; });
            }
            else if (alignType == "HorizontalRight")
            {
                var xAll = GetSelectionMaxX();
                DynamoSelection.Instance.Selection.Where((x) => x is ILocatable)
                           .Cast<ILocatable>()
                           .ToList().ForEach((x) => { x.X = xAll - x.Width; });
            }
            else if (alignType == "VerticalCenter")
            {
                var yAll = GetSelectionAverageY();
                DynamoSelection.Instance.Selection.Where((x) => x is ILocatable)
                           .Cast<ILocatable>()
                           .ToList().ForEach((x) => { x.CenterY = yAll; });
            }
            else if (alignType == "VerticalTop")
            {
                var yAll = GetSelectionMinY();
                DynamoSelection.Instance.Selection.Where((x) => x is ILocatable)
                           .Cast<ILocatable>()
                           .ToList().ForEach((x) => { x.Y = yAll; });
            }
            else if (alignType == "VerticalBottom")
            {
                var yAll = GetSelectionMaxY();
                DynamoSelection.Instance.Selection.Where((x) => x is ILocatable)
                           .Cast<ILocatable>()
                           .ToList().ForEach((x) => { x.Y = yAll - x.Height; });
            }
            else if (alignType == "VerticalDistribute")
            {
                if (DynamoSelection.Instance.Selection.Count <= 2) return;

                var yMin = GetSelectionMinY();
                var yMax = GetSelectionMaxTopY();
                var spacing = (yMax - yMin)/(DynamoSelection.Instance.Selection.Count - 1);
                int count = 0;

                DynamoSelection.Instance.Selection.Where((x) => x is ILocatable)
                           .Cast<ILocatable>()
                           .OrderBy((x) => x.Y)
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

                DynamoSelection.Instance.Selection.Where((x) => x is ILocatable)
                           .Cast<ILocatable>()
                           .OrderBy((x) => x.X)
                           .ToList()
                           .ForEach((x) => x.X = xMin + spacing * count++);
            }

            UpdateSelectionOffsets();

        }

        private void UpdateSelectionOffsets()
        {
            var sel = new List<ISelectable>();
            foreach (var ele in DynamoSelection.Instance.Selection)
            {
                sel.Add(ele);
            }
            DynamoSelection.Instance.ClearSelection();
            foreach (var ele in sel)
            {
                DynamoSelection.Instance.Selection.Add(ele);
            }
        }

        private bool CanAlignSelected(string alignType)
        {
            return true;
        }

        private void Hide(object parameters)
        {
            if ( !this.Model.HasUnsavedChanges|| dynSettings.Controller.DynamoViewModel.AskUserToSaveWorkspaceOrCancel(this.Model))
            {
                dynSettings.Controller.DynamoViewModel.Model.HideWorkspace(this._model);
            }
        }

        private bool CanHide(object parameters)
        {
            // can hide anything but the home workspace
            return dynSettings.Controller.DynamoViewModel.Model.HomeSpace != this._model;
        }

        private void ContainSelect(object parameters)
        {
            var rect = (Rect)parameters;

            foreach (dynNodeModel n in Model.Nodes)
            {
                double x0 = n.X;
                double y0 = n.Y;
                double x1 = x0 + n.Width;
                double y1 = y0 + n.Height;

                bool contains = rect.Contains(x0, y0) && rect.Contains(x1, y1);
                if (contains)
                {
                    if (!DynamoSelection.Instance.Selection.Contains(n))
                        DynamoSelection.Instance.Selection.Add(n);
                }
            }

            foreach (var n in Model.Notes)
            {
                double x0 = n.X;
                double y0 = n.Y;
                double x1 = x0 + n.Width;
                double y1 = y0 + n.Height;

                bool contains = rect.Contains(x0, y0) && rect.Contains(x1, y1);
                if (contains)
                {
                    if (!DynamoSelection.Instance.Selection.Contains(n))
                        DynamoSelection.Instance.Selection.Add(n);
                }
            }
        }

        private bool CanContainSelect(object parameters)
        {
            return true;
        }

        private void CrossingSelect(object parameters)
        {
            var rect = (Rect)parameters;

            foreach (dynNodeModel n in Model.Nodes)
            {
                double x0 = n.X;
                double y0 = n.Y;

                bool intersects = rect.IntersectsWith(new Rect(x0, y0, n.Width, n.Height));
                if (intersects)
                {
                    if (!DynamoSelection.Instance.Selection.Contains(n))
                        DynamoSelection.Instance.Selection.Add(n);
                }
            }

            foreach (var n in Model.Notes)
            {
                double x0 = n.X;
                double y0 = n.Y;

                bool intersects = rect.IntersectsWith(new Rect(x0, y0, n.Width, n.Height));
                if (intersects)
                {
                    if (!DynamoSelection.Instance.Selection.Contains(n))
                        DynamoSelection.Instance.Selection.Add(n);
                }
            }
        }

        private bool CanCrossSelect(object parameters)
        {
            return true;
        } 

        private void SetCurrentOffset(object parameter)
        {
            var p = (Point)parameter;

            //set the current offset without triggering
            //any property change notices.
            //_model.CurrentOffset = new Point(p.X, p.Y);
            if (_model.X != p.X && _model.Y != p.Y)
            {
                //Debug.WriteLine("Setting workspace offset.");
                _model.X = p.X;
                _model.Y = p.Y;
            }
        }

        private bool CanSetCurrentOffset(object parameter)
        {
            return true;
        }

        private void CreateNodeFromSelection()
        {
            CollapseNodes(
                DynamoSelection.Instance.Selection.Where(x => x is dynNodeModel)
                    .Select(x => (x as dynNodeModel)));
        }

        private void NodeFromSelectionCanExecuteChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            NodeFromSelectionCommand.RaiseCanExecuteChanged();
        }

        private bool CanCreateNodeFromSelection()
        {
            if (DynamoSelection.Instance.Selection.Count(x => x is dynNodeModel) > 1)
            {
                return true;
            }
            return false;
        }

        private void SetZoom(object zoom)
        {
            Zoom = Convert.ToDouble(zoom);
        }

        private bool CanSetZoom(object zoom)
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
                dynSettings.Controller.DynamoViewModel.Log("No node could be found with that Id.");
            }

            try
            {
                var function =
                    (dynFunction)dynSettings.Controller.DynamoModel.Nodes.First(x => x is dynFunction && ((dynFunction)x).Definition.FunctionId.ToString() == id.ToString());

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
                dynSettings.Controller.DynamoViewModel.Log("No node could be found with that Id.");
                return;
            }
        }

        private bool CanFindById(object id)
        {
            if (!string.IsNullOrEmpty(id.ToString()))
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
        internal void CollapseNodes(IEnumerable<dynNodeModel> selectedNodes)
        {
            NodeCollapser.Collapse(selectedNodes, dynSettings.Controller.DynamoViewModel.CurrentSpace);
        }
    }

    public class NodeViewEventArgs:EventArgs
    {
        dynNodeViewModel ViewModel { get; set; }
        public NodeViewEventArgs(dynNodeViewModel vm)
        {
            ViewModel = vm;
        }
    }
}
