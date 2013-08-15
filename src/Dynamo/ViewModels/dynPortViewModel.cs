﻿using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Linq;
using Dynamo.Connectors;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Microsoft.Practices.Prism.Commands;

namespace Dynamo.Connectors
{
    public class dynPortViewModel : dynViewModelBase
    {
        private readonly dynPortModel _port;
        private readonly dynNodeModel _node;

        public DelegateCommand SetCenterCommand { get; set; }
        
        public dynPortModel PortModel
        {
            get { return _port; }
        }

        public string ToolTipContent
        {
            get { return _port.ToolTipContent; }
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

        public DelegateCommand ConnectCommand { get; set; }
        public DelegateCommand HighlightCommand { get; set; }
        public DelegateCommand UnHighlightCommand { get; set; }

        public dynPortViewModel(dynPortModel port, dynNodeModel node)
        {
            _node = node;
            _port = port;
            _port.PropertyChanged += _port_PropertyChanged;
            _node.PropertyChanged += _node_PropertyChanged;
            ConnectCommand = new DelegateCommand(Connect, CanConnect);
            HighlightCommand = new DelegateCommand(Highlight, CanHighlight);
            UnHighlightCommand = new DelegateCommand(UnHighlight, CanUnHighlight);
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

        private void Connect()
        {
            // if this is a 
            if (!dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.IsConnecting)
            {
                //test if port already has a connection if so grab it
                //and begin connecting to somewhere else
                //don't allow the grabbing of the start connector
                if (_port.Connectors.Count > 0 && _port.Connectors[0].Start != _port)
                {
                    //define the new active connector
                    var c = new dynConnectorViewModel(_port.Connectors[0].Start);
                    dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.ActiveConnector = c;
                    dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.IsConnecting = true;

                    //disconnect the connector model from its start and end ports
                    //and remove it from the connectors collection. this will also
                    //remove the view model
                    var successfulRemoval = dynSettings.Controller.DynamoViewModel.CurrentSpace.Connectors.Remove(_port.Connectors[0]);
                    _port.Connectors[0].NotifyConnectedPortsOfDeletion();

                }
                else
                {
                    try
                    {
                        //Create a connector view model to begin drawing
                        if (_port.PortType != PortType.INPUT)
                        {
                            var c = new dynConnectorViewModel(_port);
                            dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.ActiveConnector = c;
                            dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.IsConnecting = true;
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
                    dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.Model.Connectors.Remove(
                       connToRemove);
                    _port.Disconnect(connToRemove);
                    var startPort = connToRemove.Start;
                    startPort.Disconnect(connToRemove);
                }

                // create the new connector model
                var start = dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.ActiveConnector.ActiveStartPort;
                var end = _port;

                var newConnectorModel = dynConnectorModel.Make(start.Owner, end.Owner, start.Index, end.Index, 0);

                // the connector is invalid
                if (newConnectorModel == null)
                {
                    return;
                }

                // Add to the current workspace
                dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.Model.Connectors.Add(newConnectorModel);

                // Cleanup
                dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.IsConnecting = false;
                dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.ActiveConnector = null;
            }
        }

        private bool CanConnect()
        {
            return true;
        }

        private void Highlight()
        {
           var connectorViewModels = dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.Connectors.Where(
                x => _port.Connectors.Contains(x.ConnectorModel));
        }

        private bool CanHighlight()
        {
            return true;
        }

        private void UnHighlight()
        {
            
        }

        private bool CanUnHighlight()
        {
            return true;
        }
    }
}
