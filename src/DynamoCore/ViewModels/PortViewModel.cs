using System;
using System.Data;
using System.Windows;
using System.Windows.Input;
using Dynamo.Models;
using ProtoCore.AST.AssociativeAST;

namespace Dynamo.ViewModels
{
    public partial class PortViewModel : ViewModelBase
    {

        #region Properties/Fields

        private readonly PortModel _port;
        private readonly NodeViewModel _node;

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

        public double Height
        {
            get { return _port.Height; }
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

        public Thickness MarginThickness
        {
            get { return _port.MarginThickness; }
            set { _port.MarginThickness = value; }
        }

        public PortEventType EventType { get; set; }
      
        public PortPosition Position
        {
            get
            {
                if (PortType == PortType.INPUT)
                {
                    if (_node.InPorts.Count > 1)
                    {
                        int pos = _node.InPorts.IndexOf(this);
                        if (pos == 0) //first port 
                            return PortPosition.Top;
                        if (pos == _node.InPorts.Count - 1)
                            return PortPosition.Last;
                        return PortPosition.Middle;
                    }
                }

                if (PortType == PortType.OUTPUT)
                {
                    if (_node.OutPorts.Count > 1)
                    {
                        int pos = _node.OutPorts.IndexOf(this);                      
                        if (pos == 0) //first port 
                            return PortPosition.Top;
                        if (pos == _node.OutPorts.Count - 1)
                            return PortPosition.Last;
                        return PortPosition.Middle;
                    }
                }

                return PortPosition.First;
            }
        }
        #endregion

        #region events
        public event EventHandler MouseEnter;
        public event EventHandler MouseLeave;
        public event EventHandler MouseLeftButtonDown;       
        #endregion



        public PortViewModel(NodeViewModel node, PortModel port)
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
                case "MarginThickness":
                    RaisePropertyChanged("MarginThickness");
                    break;
            }
            
        }

        private void Connect(object parameter)
        {
            DynamoViewModel dynamoViewModel = this._node.DynamoViewModel;
            WorkspaceViewModel workspaceViewModel = dynamoViewModel.CurrentSpaceViewModel;
            workspaceViewModel.HandlePortClicked(this);
        }

        private bool CanConnect(object parameter)
        {
            return true;
        }

        /// <summary>
        /// Handles the Mouse enter event on the port
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        private void OnRectangleMouseEnter(object parameter)
        {
            if (MouseEnter != null)
                MouseEnter(parameter, null);
        }

        /// <summary>
        /// Handles the Mouse leave on the port
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        private void OnRectangleMouseLeave(object parameter)
        {
            if (MouseLeave != null)
                MouseLeave(parameter, null);
        }

        /// <summary>
        /// Handles the Mouse left button down on the port
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        private void OnRectangleMouseLeftButtonDown(object parameter)
        {
            if (MouseLeftButtonDown != null)
                MouseLeftButtonDown(parameter, null);
        }
    }
}
