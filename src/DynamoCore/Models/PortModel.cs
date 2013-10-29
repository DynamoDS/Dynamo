using System;
using System.Collections.ObjectModel;
using System.Windows;
using Dynamo.FSchemeInterop;
using System.Windows.Media;
using System.Xml;
using Dynamo.Utilities;

namespace Dynamo.Models
{
    /// <summary>
    /// Interaction logic for dynPort.xaml
    /// </summary>
    public delegate void PortConnectedHandler(object sender, EventArgs e);
    public delegate void PortDisconnectedHandler(object sender, EventArgs e);
    public enum PortType { INPUT, OUTPUT };

    public class PortModel : ModelBase
    {
        #region events

        /// <summary>
        /// Event triggered when a port is connected.
        /// </summary>
        public event PortConnectedHandler PortConnected;

        /// <summary>
        /// Event triggered when a port is disconnected.
        /// </summary>
        public event PortConnectedHandler PortDisconnected;

        #endregion

        #region private fields
        bool isConnected;
        NodeModel owner;
        int index;
        PortType portType;
        string name;
        ObservableCollection<ConnectorModel> connectors = new ObservableCollection<ConnectorModel>();
        private bool _usingDefaultValue;
        private bool _defaultValueEnabled;
        private double _headerHeight = 20;
        private double _portHeight = 20;

        #endregion

        #region public members

        public ObservableCollection<ConnectorModel> Connectors
        {
            get { return connectors; }
            set { connectors = value; }
        }

        public string PortName
        {
            get { return name; }
            set
            {
                name = value;
                RaisePropertyChanged("PortName");
            }

        }

        public PortType PortType
        {
            get { return portType; }
            set { portType = value; }
        }

        public NodeModel Owner
        {
            get { return owner; }
            set
            {
                owner = value;
                RaisePropertyChanged("Owner");
            }
        }

        public int Index
        {
            get { return index; }
            set { index = value; }
        }

        public bool IsConnected
        {
            get
            { return isConnected; }
            set
            {
                isConnected = value;
                RaisePropertyChanged("IsConnected");
            }
        }

        public string ToolTipContent
        {
            get
            {
                if (Owner != null)
                {
                    if (PortType == PortType.INPUT)
                    {
                        return Owner.InPortData[index].ToolTipString;
                    }
                    else
                    {
                        return Owner.OutPortData[index].ToolTipString;
                    }
                }
                return "";
            }
        }

        public string DefaultValueTip
        {
            get
            {
                if (PortType == PortType.INPUT && Owner != null)
                {
                    var port = Owner.InPortData[index];
                    if (port.HasDefaultValue)
                        return FScheme.print(port.DefaultValue);
                }
                return "";
            }
        }

        /// <summary>
        /// Center is used by connected connectors to update their shape
        /// The "center" of a port is derived from the type of port and
        /// offsets from the node origin based on the port's index in the 
        /// ports collection.
        /// </summary>
        public Point Center
        {
            get
            {
                var pt = new Point();
                if (portType == PortType.INPUT)
                {
                    pt = new Point(owner.X, owner.Y + _headerHeight + 5 + _portHeight/2 + _portHeight*Index+1);
                }
                else if (portType == PortType.OUTPUT)
                {
                    pt = new Point(owner.X + owner.Width, owner.Y + _headerHeight + 5 + _portHeight / 2 + _portHeight * Index);
                }

                return pt;
            }
        }

        /// <summary>
        /// Controls whether this port is set to use it's default value (true) or yield a closure (false).
        /// </summary>
        public bool UsingDefaultValue
        {
            get { return _usingDefaultValue; }
            set
            {
                _usingDefaultValue = value; 
                RaisePropertyChanged("UsingDefaultValue");
            }
        }

        /// <summary>
        /// Controls whether the Use Default Value option is available.
        /// </summary>
        public bool DefaultValueEnabled
        {
            get { return _defaultValueEnabled; }
            set
            {
                _defaultValueEnabled = value;
                RaisePropertyChanged("DefaultValueEnabled");
            }
        }

        #endregion

        public PortModel(int index, PortType portType, NodeModel owner, string name)
        {
            Index = index;
            IsConnected = false;
            PortType = portType;
            Owner = owner;
            PortName = name;
            UsingDefaultValue = false;
            DefaultValueEnabled = false;
        }

        public void Connect(ConnectorModel connector)
        {
            connectors.Add(connector);

            //throw the event for a connection
            OnPortConnected(EventArgs.Empty);

            IsConnected = true;
        }

        public void Disconnect(ConnectorModel connector)
        {
            if (!connectors.Contains(connector))
                return;
            
            //throw the event for a connection
            OnPortDisconnected(EventArgs.Empty);

            //also trigger the model's connector deletion
            dynSettings.Controller.DynamoModel.OnConnectorDeleted(connector);

            connectors.Remove(connector);
            
            //don't set back to white if
            //there are still connectors on this port
            if (connectors.Count == 0)
            {
                IsConnected = false;
            }

            Owner.ValidateConnections();
        }

        /// <summary>
        /// Called when a port is connected.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnPortConnected(EventArgs e)
        {
            if (PortConnected != null)
                PortConnected(this, e);
        }

        /// <summary>
        /// Called when a port is disconnected.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnPortDisconnected(EventArgs e)
        {
            if (PortDisconnected != null)
                PortDisconnected(this, e);
        }

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            // We are not deserializing the ports.
            throw new NotImplementedException();
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            // We are not deserializing the ports.
            throw new NotImplementedException();
        }

        #endregion
    }

    public class PortData
    {
        public string NickName { get; internal set; }
        public string ToolTipString { get; internal set; }
        public Type PortType { get; set; }
        public FScheme.Value DefaultValue { get; set; }

        public PortData(string nickName, string tip, Type portType, FScheme.Value defaultValue=null)
        {
            NickName = nickName;
            ToolTipString = tip;
            PortType = portType;
            DefaultValue = defaultValue;
        }

        public bool HasDefaultValue 
        {
            get
            {
                return DefaultValue != null;
            }
        }
    }
}
