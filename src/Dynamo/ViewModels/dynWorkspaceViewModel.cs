using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using Dynamo.Connectors;
using Dynamo.Controls;
using Dynamo.Nodes;
using Dynamo.Selection;
using Dynamo.Utilities;
using Microsoft.Practices.Prism.Commands;

namespace Dynamo
{
    public class dynWorkspaceViewModel: dynViewModelBase
    {
        public dynWorkspace _workspace;

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

        public string Name
        {
            get
            {
                if (_workspace == dynSettings.Controller.DynamoViewModel.Model.HomeSpace)
                    return "Home";
                return _workspace.Name;
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

        public dynWorkspace WorkspaceModel
        {
            get { return _workspace; }
        }

        public dynWorkspaceViewModel(dynWorkspace workspace)
        {
            _workspace = workspace;
            
            //respond to collection changes on the model by creating new view models
            //currently, view models are added for notes and nodes
            //connector view models are added during connection
            _workspace.Nodes.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Nodes_CollectionChanged);
            _workspace.Notes.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Notes_CollectionChanged);
            _workspace.Connectors.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Connectors_CollectionChanged);
            _workspace.PropertyChanged += Workspace_PropertyChanged;
        }

        void Connectors_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                //connector view models are added to the collection in during connector connection operations
                //we'll only respond to removal here
                foreach (var item in e.OldItems)
                {
                    _connectors.Remove(_connectors.First(x => x.ConnectorModel == item));
                }
            }
        }

        void Notes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if(e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    //add a corresponding note
                    var viewModel = new dynNoteViewModel(item as dynNoteModel);
                    _notes.Add(viewModel);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    _notes.Remove(_notes.First(x => x.Note == item));
                }
            }
        }

        void Nodes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    //add a corresponding note
                    var viewModel = new dynNodeViewModel(item as dynNode);
                    _nodes.Add(viewModel);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    _nodes.Remove(_nodes.First(x => x.NodeLogic == item));
                }
            }
        }

        void Workspace_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "Name")
                RaisePropertyChanged("Name");
        }
    }
}
