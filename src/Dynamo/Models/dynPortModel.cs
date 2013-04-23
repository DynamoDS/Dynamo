using System;
using System.Collections.ObjectModel;
using System.Linq;
using Dynamo.Controls;
using Dynamo.Nodes;
using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo.Connectors
{
    public class dynPortModel : NotificationObject
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
        
        bool isConnected;
        dynNode owner;
        int index;
        PortType portType;
        string name;
        ObservableCollection<dynConnector> connectors = new ObservableCollection<dynConnector>();

        #endregion

        #region public members

        public ObservableCollection<dynConnector> Connectors
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

        public dynNode Owner
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

        #endregion

        public dynPortModel()
        {
            Index = index;
            IsConnected = false;
            PortType = portType;
            Owner = owner;
            PortName = name;
        }

        public void Connect(dynConnector connector)
        {
            connectors.Add(connector);

            //throw the event for a connection
            OnPortConnected(EventArgs.Empty);

            IsConnected = true;
        }

        public void Disconnect(dynConnector connector)
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
                c.Kill();
        }
    }


    public class PortData
    {
        string nickName;
        string toolTip;
        Type portType;

        public string NickName
        {
            get { return nickName; }
            internal set { nickName = value; }
        }

        public string ToolTipString
        {
            get { return toolTip; }
            internal set { toolTip = value; }
        }

        public Type PortType
        {
            get { return portType; }
            set { portType = value; }
        }

        public PortData(string nickName, string tip, Type portType)
        {
            this.nickName = nickName;
            this.toolTip = tip;
            this.portType = portType;
        }
    }
}
