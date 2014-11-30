using System.Windows;

using Dynamo.Models;

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
        }

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
    }
}
