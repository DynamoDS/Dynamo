using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Data;
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
    public delegate void ViewEventHandler(object sender, EventArgs e);

    public class dynWorkspaceViewModel: dynViewModelBase
    {
        #region Properties and Fields

        public dynWorkspaceModel _model;
        
        private bool isConnecting = false;

        public event EventHandler StopDragging;
        public event PointEventHandler CurrentOffsetChanged;
        public event NodeEventHandler RequestCenterViewOnElement;
        public event EventHandler UILocked;
        public event EventHandler UIUnlocked;
        public event NodeEventHandler RequestNodeCentered;
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
        public virtual void OnRequestCenterViewOnElement(object sender, NodeEventArgs e)
        {
            if (RequestCenterViewOnElement != null)
                RequestCenterViewOnElement(this, e);
        }
        public virtual void OnUILocked(object sender, EventArgs e)
        {
            if (UILocked != null)
                UILocked(this, e);
        }
        public virtual void OnUIUnlocked(object sender, EventArgs e)
        {
            if (UIUnlocked != null)
                UIUnlocked(this, e);
        }
        public virtual void OnRequestNodeCentered(object sender, NodeEventArgs e)
        {
            if (RequestNodeCentered != null)
                RequestNodeCentered(this, e);
        }
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

        public Color BackgroundColor
        {
            get
            {
                if (_model == dynSettings.Controller.DynamoViewModel.Model.HomeSpace)
                    return Color.FromArgb(0xFF, 0x4B, 0x4B, 0x4B);
                return Color.FromArgb(0xFF, 0x8A, 0x8A, 0x8A);
            }
        }

        public bool IsCurrentSpace
        {
            get { return _model.IsCurrentSpace; }
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

        #endregion

        public dynWorkspaceViewModel(dynWorkspaceModel model, DynamoViewModel vm)
        {
            _model = model;

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
            
            Watch3DViewModels.Add(new Watch3DFullscreenViewModel(this));

            //respond to collection changes on the model by creating new view models
            //currently, view models are added for notes and nodes
            //connector view models are added during connection
            _model.Nodes.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Nodes_CollectionChanged);
            _model.Notes.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Notes_CollectionChanged);
            _model.Connectors.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Connectors_CollectionChanged);
            _model.PropertyChanged += ModelPropertyChanged;

            HideCommand = new DelegateCommand<object>(Hide, CanHide);
            CrossSelectCommand = new DelegateCommand<object>(CrossingSelect, CanCrossSelect);
            ContainSelectCommand = new DelegateCommand<object>(ContainSelect, CanContainSelect);
            UpdateSelectedConnectorsCommand = new DelegateCommand(UpdateSelectedConnectors, CanUpdateSelectedConnectors);
            SetCurrentOffsetCommand = new DelegateCommand<object>(SetCurrentOffset, CanSetCurrentOffset);
            NodeFromSelectionCommand = new DelegateCommand(CreateNodeFromSelection, CanCreateNodeFromSelection);
            DynamoSelection.Instance.Selection.CollectionChanged += NodeFromSelectionCanExecuteChanged;

            vm.UILocked += new EventHandler(DynamoViewModel_UILocked);
            vm.UIUnlocked += new EventHandler(DynamoViewModel_UIUnlocked);

            // sync collections
            Nodes_CollectionChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, _model.Nodes));
            Connectors_CollectionChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, _model.Connectors));
            Notes_CollectionChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, _model.Notes));
        }

        void DynamoViewModel_UIUnlocked(object sender, EventArgs e)
        {
            OnUIUnlocked(this, EventArgs.Empty);
        }

        void DynamoViewModel_UILocked(object sender, EventArgs e)
        {
            OnUILocked(this, EventArgs.Empty);
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

        void Notes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
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
                    break;
                case "HasUnsavedChanges":
                    RaisePropertyChanged("HasUnsavedChanges");
                    break;
                case "FilePath":
                    RaisePropertyChanged("FilePath");
                    break;
            }
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
                //check if the node is within the boundary
                //double x0 = Canvas.GetLeft(n);
                //double y0 = Canvas.GetTop(n);
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
                //check if the node is within the boundary
                //double x0 = Canvas.GetLeft(n);
                //double y0 = Canvas.GetTop(n);

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

        private void UpdateSelectedConnectors()
        {

            var allConnectors = DynamoSelection.Instance.Selection.OfType<dynNodeModel>()
                                                               .SelectMany(
                                                                   el => el.OutPorts
                                                                           .SelectMany(x => x.Connectors)
                                                                           .Concat(
                                                                               el.InPorts.SelectMany(
                                                                                   x => x.Connectors))).Distinct();

            foreach (dynConnectorModel connector in allConnectors)
            {
                Debug.WriteLine("Connectors no longer call redraw....is it still working?");
                //connector.Redraw();
            }
        }

        private bool CanUpdateSelectedConnectors()
        {
            return true;
        }

        private void SetCurrentOffset(object parameter)
        {
            var p = (Point)parameter;

            //set the current offset without triggering
            //any property change notices.
            //_model.CurrentOffset = new Point(p.X, p.Y);
            _model.X = p.X;
            _model.Y = p.Y;
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

        /// <summary>
        ///     Collapse a set of nodes in the current workspace.  Has the side effects of prompting the user
        ///     first in order to obtain the name and category for the new node, 
        ///     writes the function to a dyf file, adds it to the FunctionDict, adds it to search, and compiles and 
        ///     places the newly created symbol (defining a lambda) in the Controller's FScheme Environment.  
        /// </summary>
        /// <param name="selectedNodes"> The function definition for the user-defined node </param>
        internal void CollapseNodes(IEnumerable<dynNodeModel> selectedNodes)
        {
            Dynamo.Utilities.NodeCollapser.Collapse(selectedNodes, dynSettings.Controller.DynamoViewModel.CurrentSpace);
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
