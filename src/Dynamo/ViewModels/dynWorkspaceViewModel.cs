using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows;
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
        public dynWorkspace _workspace;
        private dynConnectorViewModel activeConnector;
        private bool isConnecting = false;

        public event EventHandler StopDragging;
        public event PointEventHandler CurrentOffsetChanged;
        public event NodeEventHandler RequestCenterViewOnElement;
        public event EventHandler UILocked;
        public event EventHandler UIUnlocked;
        public event NodeEventHandler RequestNodeCentered;
        public event ViewEventHandler RequestAddViewToOuterCanvas;

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
        ObservableCollection<dynNodeViewModel> _nodes = new ObservableCollection<dynNodeViewModel>();
        ObservableCollection<dynNoteViewModel> _notes = new ObservableCollection<dynNoteViewModel>(); 

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

        public DelegateCommand<object> CrossSelectCommand { get; set; }
        public DelegateCommand<object> ContainSelectCommand { get; set; }
        public DelegateCommand UpdateSelectedConnectorsCommand { get; set; }
        public DelegateCommand<object> SetCurrentOffsetCommand { get; set; }

        public string Name
        {
            get
            {
                if (_workspace == dynSettings.Controller.DynamoViewModel.Model.HomeSpace)
                    return "Home";
                return _workspace.Name;
            }
        }

        public dynConnectorViewModel ActiveConnector
        {
            get { return activeConnector; }
            set
            {
                activeConnector = value;
                RaisePropertyChanged("ActiveConnector");
            }
        }

        public Visibility EditNameVisibility
        {
            get
            {
                if (_workspace != dynSettings.Controller.DynamoViewModel.Model.HomeSpace)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public bool CanEditName
        {
            get { return _workspace != dynSettings.Controller.DynamoViewModel.Model.HomeSpace; }
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
                if (_workspace == dynSettings.Controller.DynamoViewModel.Model.HomeSpace)
                    return Color.FromArgb(0xFF, 0x4B, 0x4B, 0x4B);
                return Color.FromArgb(0xFF, 0x8A, 0x8A, 0x8A);
            }
        }

        public bool IsCurrentSpace
        {
            get { return dynSettings.Controller.DynamoModel.CurrentSpace == _workspace; }
        }

        /// <summary>
        /// Specifies the pan location of the view
        /// </summary>
        public Point CurrentOffset
        {
            get { return _workspace.CurrentOffset; }
            set
            {
                OnCurrentOffsetChanged(this, new PointEventArgs(value));
            }
        }

        public dynWorkspace WorkspaceModel
        {
            get { return _workspace; }
        }

        public dynWorkspaceViewModel(dynWorkspace workspace, DynamoViewModel vm)
        {
            _workspace = workspace;
            
            //respond to collection changes on the model by creating new view models
            //currently, view models are added for notes and nodes
            //connector view models are added during connection
            _workspace.Nodes.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Nodes_CollectionChanged);
            _workspace.Notes.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Notes_CollectionChanged);
            _workspace.Connectors.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Connectors_CollectionChanged);
            _workspace.PropertyChanged += Workspace_PropertyChanged;

            CrossSelectCommand = new DelegateCommand<object>(CrossingSelect, CanCrossSelect);
            ContainSelectCommand = new DelegateCommand<object>(ContainSelect, CanContainSelect);
            UpdateSelectedConnectorsCommand = new DelegateCommand(UpdateSelectedConnectors, CanUpdateSelectedConnectors);
            SetCurrentOffsetCommand = new DelegateCommand<object>(SetCurrentOffset, CanSetCurrentOffset);

            vm.UILocked += new EventHandler(DynamoViewModel_UILocked);
            vm.UIUnlocked += new EventHandler(DynamoViewModel_UIUnlocked);
        }

        void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "CurrentSpace":
                    RaisePropertyChanged("IsCurrentSpace");
                    break;
            }
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
                case NotifyCollectionChangedAction.Remove:
                    //connector view models are added to the collection in during connector connection operations
                    //we'll only respond to removal here
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

                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                    {
                        _notes.Remove(_notes.First(x => x.Note == item));
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
                        //add a corresponding note
                        var viewModel = new dynNodeViewModel(item as dynNode);
                        _nodes.Add(viewModel);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                    {
                        _nodes.Remove(_nodes.First(x => x.NodeLogic == item));
                    }
                    break;
            }
        }

        void Workspace_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Name":
                    RaisePropertyChanged("Name");
                    break;
                case "CurrentOffset":
                    RaisePropertyChanged("CurrentOffset");
                    break;
            }
        }

        private void ContainSelect(object parameters)
        {
            var rect = (Rect)parameters;

           
            foreach (dynNode n in WorkspaceModel.Nodes)
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

            foreach (dynNode n in WorkspaceModel.Nodes)
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
            IEnumerable<dynConnector> allConnectors = DynamoSelection.Instance.Selection
                                                               .Where(x => x is dynNode)
                                                               .Select(x => x as dynNode)
                                                               .SelectMany(
                                                                   el => el.OutPorts
                                                                           .SelectMany(x => x.Connectors)
                                                                           .Concat(
                                                                               el.InPorts.SelectMany(
                                                                                   x => x.Connectors))).Distinct();

            foreach (dynConnector connector in allConnectors)
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
            _workspace.CurrentOffset = new Point(p.X, p.Y);
        }

        private bool CanSetCurrentOffset(object parameter)
        {
            return true;
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
