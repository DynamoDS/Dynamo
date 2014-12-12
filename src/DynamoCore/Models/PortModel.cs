using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Xml;

using Dynamo.UI;
using Dynamo.Utilities;

namespace Dynamo.Models
{
    /// <summary>
    /// Interaction logic for dynPort.xaml
    /// </summary>
    public delegate void PortConnectedHandler(object sender, EventArgs e);
    public delegate void PortDisconnectedHandler(object sender, EventArgs e);
    public enum PortType { Input, Output };

    public class PortModel : ModelBase
    {
        #region events

        /// <summary>
        /// Event triggered when a port is connected.
        /// </summary>
        public event Action<ConnectorModel> PortConnected;

        /// <summary>
        /// Event triggered when a port is disconnected.
        /// </summary>
        public event PortConnectedHandler PortDisconnected;

        #endregion

        #region private fields
        bool isConnected;
        NodeModel owner;
        PortType portType;
        string name;
        ObservableCollection<ConnectorModel> connectors = new ObservableCollection<ConnectorModel>();
        private bool usingDefaultValue;
        private bool defaultValueEnabled;
        private Thickness marginThickness;
        
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
            get { return owner.GetPortIndexAndType(this, out portType); }
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
                return Owner != null
                    ? (PortType == PortType.Input
                        ? Owner.InPortData[Index].ToolTipString
                        : Owner.OutPortData[Index].ToolTipString)
                    : "";
            }
        }

        public string DefaultValueTip
        {
            get
            {
                if (PortType == PortType.Input && Owner != null)
                {
                    var port = Owner.InPortData[Index];
                    if (port.HasDefaultValue)
                        return port.DefaultValue.ToString();
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
        public Point2D Center
        {
            get
            {
                double halfHeight = Height * 0.5;
                const double headerHeight = 25;

                double offset = owner.GetPortVerticalOffset(this);
                double y = owner.Y + headerHeight + 5 + halfHeight + offset;

                switch (portType)
                {
                    case PortType.Input:
                        return new Point2D(owner.X, y);
                    case PortType.Output:
                        return new Point2D(owner.X + owner.Width, y);
                }

                return new Point2D();
            }
        }

        /// <summary>
        /// Controls whether this port is set to use it's default value (true) or yield a closure (false).
        /// </summary>
        public bool UsingDefaultValue
        {
            get { return usingDefaultValue; }
            set
            {
                usingDefaultValue = value; 
                RaisePropertyChanged("UsingDefaultValue");
            }
        }

        /// <summary>
        /// Controls whether the Use Default Value option is available.
        /// </summary>
        public bool DefaultValueEnabled
        {
            get { return defaultValueEnabled; }
            set
            {
                defaultValueEnabled = value;
                RaisePropertyChanged("DefaultValueEnabled");
            }
        }

        /// <summary>
        /// Controls the space between successive output ports
        /// </summary>
        public Thickness MarginThickness
        {
            get { return marginThickness; }
            set
            {
                marginThickness = value;
                RaisePropertyChanged("MarginThickness");
            }
        }

        public SnapExtensionEdges extensionEdges { get; set; }        
    
        #endregion

        public PortModel(PortType portType, NodeModel owner, PortData data)
        {
            IsConnected = false;
            PortType = portType;
            Owner = owner;
            PortName = data.NickName;
            UsingDefaultValue = false;
            DefaultValueEnabled = false;
            MarginThickness = new Thickness(0);

            Height = Math.Abs(data.Height) < 0.001 ? Configurations.PortHeightInPixels : data.Height;
        }

        /// <summary>
        /// Deletes all connectors attached to this PortModel.
        /// </summary>
        public void DestroyConnectors()
        {
            if (Owner == null)
                return;

            while (Connectors.Any())
            {
                ConnectorModel connector = Connectors[0];
                connector.Delete();
            }
        }

        public void Connect(ConnectorModel connector)
        {
            connectors.Add(connector);

            //throw the event for a connection
            OnPortConnected(connector);

            IsConnected = true;
        }

        public void Disconnect(ConnectorModel connector)
        {
            if (!connectors.Contains(connector))
                return;
            
            //throw the event for a connection
            OnPortDisconnected(EventArgs.Empty);

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
        /// <param name="connector"></param>
        protected virtual void OnPortConnected(ConnectorModel connector)
        {
            if (PortConnected != null)
                PortConnected(connector);
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

        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            // We are not deserializing the ports.
            throw new NotImplementedException();
        }

        #endregion
    }

    public class PortData
    {
        public string NickName { get; set; }
        public string ToolTipString { get; set; }
        public object DefaultValue { get; set; }
        public double VerticalMargin { get; set; }

        public double Height { get; set; }

        public PortData(string nickName, string tip) : this(nickName, tip, null) { }

        public PortData(string nickName, string toolTipString, object defaultValue)
        {
            NickName = nickName;
            ToolTipString = toolTipString;
            DefaultValue = defaultValue;
            VerticalMargin = 0;
            Height = 0;
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
