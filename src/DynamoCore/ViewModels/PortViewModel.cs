using System;
using System.Diagnostics;
using System.Windows;
using System.Linq;
using Dynamo.Models;
using Dynamo.Utilities;
using System.Collections.Generic;

namespace Dynamo.ViewModels
{
    public partial class PortViewModel : ViewModelBase
    {
        private readonly PortModel _port;
        private readonly NodeModel _node;

        public PortModel PortModel
        {
            get { return _port; }
        }

        public string ToolTipContent
        {
            get { return _port.ToolTipContent; }
        }

        public string DefaultValueTip
        {
            get { return _port.DefaultValueTip; }
        }

        public string PortName
        {
            get
            {
                return _port.PortName;
            }
            set 
            { 
                _port.PortName = value;
            }
        }

        public PortType PortType
        {
            get { return _port.PortType; }
        }
        
        public bool IsSelected
        {
            get { return _node.IsSelected; }
        }

        public bool IsConnected
        {
            get { return _port.IsConnected; }
        }

        public Point Center
        {
            get { return _port.Center; }
        }

        public ElementState State
        {
            get { return _node.State; }    
        }

        public bool DefaultValueEnabled
        {
            get { return _port.DefaultValueEnabled; }
            set { _port.DefaultValueEnabled = value; }
        }

        public bool UsingDefaultValue
        {
            get { return _port.UsingDefaultValue; }
            set { _port.UsingDefaultValue = value; }
        }

        public PortViewModel(PortModel port, NodeModel node)
        {
            _node = node;
            _port = port;

            _port.PropertyChanged += _port_PropertyChanged;
            _node.PropertyChanged += _node_PropertyChanged;
        }

        void _node_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsSelected":
                    RaisePropertyChanged("IsSelected");
                    break;
                case "State":
                    RaisePropertyChanged("State");
                    break;
                case "ToolTipContent":
                    RaisePropertyChanged("ToolTipContent");
                    break;
            }
        }

        void _port_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ToolTipContent":
                    RaisePropertyChanged("ToolTipContent");
                    break;
                case "PortType":
                    RaisePropertyChanged("PortType");
                    break;
                case "PortName":
                    RaisePropertyChanged("PortName");
                    break;
                case "IsConnected":
                    RaisePropertyChanged("IsConnected");
                    break;
                case "Center":
                    RaisePropertyChanged("Center");
                    break;
                case "DefaultValueEnabled":
                    RaisePropertyChanged("DefaultValueEnabled");
                    break;
                case "UsingDefaultValue":
                    RaisePropertyChanged("UsingDefaultValue");
                    break;
            }
            
        }

        /// <summary>
        /// Update the port model's center, triggering an update to all connectors
        /// </summary>
        /// <param name="center"></param>
        public void UpdateCenter(Point center)
        {
            _port.Center = center;
        }

        private void Connect(object parameter)
        {
            DynamoViewModel dynamoViewModel = dynSettings.Controller.DynamoViewModel;
            WorkspaceModel workspaceModel = dynamoViewModel.CurrentSpace;
            WorkspaceViewModel workspaceViewModel = dynamoViewModel.CurrentSpaceViewModel;

            // if this is a 
            if (!workspaceViewModel.IsConnecting)
            {
                //test if port already has a connection if so grab it
                //and begin connecting to somewhere else
                //don't allow the grabbing of the start connector
                if (_port.Connectors.Count > 0 && _port.Connectors[0].Start != _port)
                {
                    //define the new active connector
                    var c = new ConnectorViewModel(_port.Connectors[0].Start);
                    workspaceViewModel.ActiveConnector = c;
                    workspaceViewModel.IsConnecting = true;

                    //disconnect the connector model from its start and end ports
                    //and remove it from the connectors collection. this will also
                    //remove the view model
                    ConnectorModel connector = _port.Connectors[0];
                    if (workspaceModel.Connectors.Contains(connector))
                    {
                        List<ModelBase> models = new List<ModelBase>();
                        models.Add(connector);
                        workspaceModel.RecordAndDeleteModels(models);
                        connector.NotifyConnectedPortsOfDeletion();
                    }
                }
                else
                {
                    try
                    {
                        //Create a connector view model to begin drawing
                        if (_port.PortType != PortType.INPUT)
                        {
                            var c = new ConnectorViewModel(_port);
                            workspaceViewModel.ActiveConnector = c;
                            workspaceViewModel.IsConnecting = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            }
            else  // attempt to complete the connection
            {
                if (_port.PortType != PortType.INPUT)
                {
                    return;
                }

                //remove connector if one already exists
                if (_port.Connectors.Count > 0)
                {
                    var connToRemove = _port.Connectors[0];
                    workspaceModel.Connectors.Remove(connToRemove);
                    _port.Disconnect(connToRemove);
                    var startPort = connToRemove.Start;
                    startPort.Disconnect(connToRemove);
                }

                // create the new connector model
                var start = workspaceViewModel.ActiveConnector.ActiveStartPort;
                var end = _port;

                var newConnectorModel = ConnectorModel.Make(start.Owner,
                    end.Owner, start.Index, end.Index, 0);

                // the connector is invalid
                if (newConnectorModel == null)
                {
                    return;
                }

                // Add to the current workspace
                workspaceModel.Connectors.Add(newConnectorModel);

                // Cleanup
                workspaceViewModel.IsConnecting = false;
                workspaceViewModel.ActiveConnector = null;

                // Record the creation of connector in the undo recorder.
                workspaceModel.RecordCreatedModel(newConnectorModel);
            }
        }

        private bool CanConnect(object parameter)
        {
            return true;
        }

        private void Highlight(object parameter)
        {
           var connectorViewModels = dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.Connectors.Where(
                x => _port.Connectors.Contains(x.ConnectorModel));
        }

        private bool CanHighlight(object parameter)
        {
            return true;
        }

        private void UnHighlight(object parameter)
        {
            
        }

        private bool CanUnHighlight(object parameter)
        {
            return true;
        }
    }
}
