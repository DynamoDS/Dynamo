using System;
using System.Xml;
using Dynamo.Utilities;

namespace Dynamo.Models
{
    public enum ConnectorType { BEZIER, POLYLINE };

    public delegate void ConnectorConnectedHandler(object sender, EventArgs e);
    
    public class ConnectorModel : ModelBase
    {
        #region properties
        
        public event ConnectorConnectedHandler Connected;

        protected virtual void OnConnected(EventArgs e)
        {
            if (Connected != null)
                Connected(this, e);
        }

        private PortModel pStart;
        private PortModel pEnd;

        public PortModel Start
        {
            get { return pStart; }
            set { pStart = value; }
        }

        public PortModel End
        {
            get { return pEnd; }
            set
            {
                pEnd = value;
            }
        }
        
        #endregion 

        #region constructors

        /// <summary>
        /// Factory method to create a connector.  Checks to make sure that the start and end ports are valid, 
        /// otherwise returns null.
        /// </summary>
        /// <param name="start">The port where the connector starts</param>
        /// <param name="end">The port where the connector ends</param>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        /// <param name="guid"></param>
        /// <returns>The valid connector model or null if the connector is invalid</returns>
        internal static ConnectorModel Make(
            NodeModel start, NodeModel end, int startIndex, int endIndex, Guid? guid = null)
        {
            if (start != null && end != null && start != end && startIndex >= 0
                && endIndex >= 0 && start.OutPorts.Count > startIndex
                && end.InPorts.Count > endIndex)
            {
                return new ConnectorModel(start, end, startIndex, endIndex, guid ?? Guid.NewGuid());
            }

            return null;
        }

        private ConnectorModel(
            NodeModel start, NodeModel end, int startIndex, int endIndex, Guid guid)
        {
            GUID = guid;
            pStart = start.OutPorts[startIndex];

            PortModel endPort = end.InPorts[endIndex];

            pStart.Connect(this);
            Connect(endPort);
        }

        #endregion
        
        public bool Connect(PortModel p)
        {
            //test if the port that you are connecting too is not the start port or the end port
            //of the current connector
            if (p.Equals(pStart) || p.Equals(pEnd))
            {
                return false;
            }

            //if the selected connector is also an output connector, return false
            //output ports can't be connected to eachother
            if (p.PortType == PortType.Output)
            {
                return false;
            }

            //test if the port that you are connecting to is an input and 
            //already has other connectors
            if (p.PortType == PortType.Input && p.Connectors.Count > 0)
            {
                p.Disconnect(p.Connectors[0]);
            }

            //turn the line solid
            pEnd = p;

            if (pEnd != null)
            {
                p.Connect(this);
            }

            return true;
        }

        public void Disconnect(PortModel p)
        {
            if (p.Equals(pStart))
            {
                pStart = null;
            }

            if (p.Equals(pEnd))
            {
                pEnd = null;
            }

            p.Disconnect(this);

        }

        public void Delete()
        {
            if (pStart != null && pStart.Connectors.Contains(this))
            {
                pStart.Disconnect(this);
            }
            if (pEnd != null && pEnd.Connectors.Contains(this))
            {
                pEnd.Disconnect(this);
            }
            OnDeleted();
        }

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            var helper = new XmlElementHelper(element);
            helper.SetAttribute("guid", GUID);
            helper.SetAttribute("start", Start.Owner.GUID);
            helper.SetAttribute("start_index", Start.Index);
            helper.SetAttribute("end", End.Owner.GUID);
            helper.SetAttribute("end_index", End.Index);
            //helper.SetAttribute("portType", ((int) End.PortType));
        }

        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            //This is now handled via NodeGraph.LoadConnectorFromXml

            /*
            var helper = new XmlElementHelper(element);

            // Restore some information from the node attributes.
            GUID = helper.ReadGuid("guid", GUID);
            Guid startNodeId = helper.ReadGuid("start");
            int startIndex = helper.ReadInteger("start_index");
            Guid endNodeId = helper.ReadGuid("end");
            int endIndex = helper.ReadInteger("end_index");
            var portType = ((PortType)helper.ReadInteger("portType"));

            // Get to the start and end nodes that this connector connects to.
            var startNode = workspaceModel.GetModelInternal(startNodeId) as NodeModel;
            var endNode = workspaceModel.GetModelInternal(endNodeId) as NodeModel;

            pStart = startNode.OutPorts[startIndex];
            PortModel endPort = null;
            if (portType == PortType.Input)
                endPort = endNode.InPorts[endIndex];

            pStart.Connect(this);
            Connect(endPort);*/
        }

        #endregion

        public event Action Deleted;
        protected virtual void OnDeleted()
        {
            var handler = Deleted;
            if (handler != null) handler();
        }
    }

    public class InvalidPortException : ApplicationException
    {
        private readonly string message;
        public override string Message
        {
            get { return message; }
        }

        public InvalidPortException()
        {
            message = "Connection port is not valid.";
        }
    }
}
