using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Dynamo.Nodes;

namespace Dynamo.Connectors
{
    public class dynPortModel : dynModelBase
    {
        #region events

        public event PortConnectedHandler PortConnected;
        public event PortConnectedHandler PortDisconnected;

        protected virtual void OnPortConnected(EventArgs e)
        {
            if (PortConnected != null)
                PortConnected(this, e);
        }
        protected virtual void OnPortDisconnected(EventArgs e)
        {
            if (PortDisconnected != null)
                PortDisconnected(this, e);
        }

        #endregion

        #region private fields
        Point center;
        bool isConnected;
        dynNodeModel owner;
        int index;
        PortType portType;
        string name;
        ObservableCollection<dynConnectorModel> connectors = new ObservableCollection<dynConnectorModel>();

        #endregion

        #region public members

        public ObservableCollection<dynConnectorModel> Connectors
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

        public dynNodeModel Owner
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
                    if (PortType == Dynamo.Connectors.PortType.INPUT)
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

        /// <summary>
        /// Center is used by connected connectors to update their shape
        /// It is updated by an event handler on the port view
        /// </summary>
        public Point Center
        {
            get
            {
                return center;
            }
            set
            {
                center = value;
                RaisePropertyChanged("Center");
            }
        }
        #endregion

        public dynPortModel(int index, PortType portType, dynNodeModel owner, string name)
        {
            Index = index;
            IsConnected = false;
            PortType = portType;
            Owner = owner;
            PortName = name;
        }

        public void Connect(dynConnectorModel connector)
        {
            connectors.Add(connector);

            //throw the event for a connection
            OnPortConnected(EventArgs.Empty);

            IsConnected = true;
        }

        public void Disconnect(dynConnectorModel connector)
        {
            //throw the event for a connection
            OnPortDisconnected(EventArgs.Empty);

            if (connectors.Contains(connector))
            {
                connectors.Remove(connector);
            }

            //don't set back to white if
            //there are still connectors on this port
            if (connectors.Count == 0)
            {
                IsConnected = false;
            }

            if (connectors.Count == 0)
                Owner.State = ElementState.DEAD;

        }

        internal void KillAllConnectors()
        {
            foreach (var c in connectors.ToList())
                c.NotifyConnectedPortsOfDeletion();
        }
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
