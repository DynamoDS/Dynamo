//Copyright 2013 Ian Keough

//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at

//http://www.apache.org/licenses/LICENSE-2.0

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Diagnostics;
using Dynamo.Nodes;

namespace Dynamo.Connectors
{
    public enum ConnectorType { BEZIER, POLYLINE };

    public delegate void ConnectorConnectedHandler(object sender, EventArgs e);

    public class dynConnectorModel: dynModelBase
    {

        #region properties

        public event ConnectorConnectedHandler Connected;

        protected virtual void OnConnected(EventArgs e)
        {
            if (Connected != null)
                Connected(this, e);
        }

        dynPortModel pStart;
        dynPortModel pEnd;

        public dynPortModel Start
        {
            get { return pStart; }
            set { pStart = value; }
        }

        public dynPortModel End
        {
            get { return pEnd; }
            set
            {
                pEnd = value;
            }
        }
        
        #endregion 

        #region constructors
        
        public dynConnectorModel(dynNodeModel start, dynNodeModel end, int startIndex, int endIndex, int portType, bool visible)
        {
            //don't try to create a connector with a bad start,
            //end, or if we're trying to connector the same
            //port to itself.
            if (start == null)
                throw new Exception("Can not create a connection with a null start port.");

            if (end == null)
                throw new Exception("Can not create a connection will a null end port.");

            if (start == end)
                throw new Exception("Can not connect a node to iteself.");

            pStart = start.OutPorts[startIndex];

            dynPortModel endPort = null;

            if (portType == 0)
                endPort = end.InPorts[endIndex];

            pStart.Connect(this);
            this.Connect(endPort);
        }

        public dynConnectorModel(dynNodeModel start, dynNodeModel end, int startIndex, int endIndex, int portType)
            : this(start, end, startIndex, endIndex, portType, true)
        { }
        #endregion
        
        public bool Connect(dynPortModel p)
        {
            //test if the port that you are connecting too is not the start port or the end port
            //of the current connector
            if (p.Equals(pStart) || p.Equals(pEnd))
            {
                return false;
            }

            //if the selected connector is also an output connector, return false
            //output ports can't be connected to eachother
            if (p.PortType == PortType.OUTPUT)
            {
                return false;
            }

            //test if the port that you are connecting to is an input and 
            //already has other connectors
            if (p.PortType == PortType.INPUT && p.Connectors.Count > 0)
            {
                return false;
            }

            //turn the line solid
            pEnd = p;

            if (pEnd != null)
            {
                //set the start and end values to equal so this 
                //starts evaulating immediately
                //pEnd.Owner.InPortData[p.Index].Object = pStart.Owner.OutPortData.Object;
                p.Connect(this);

                Debug.WriteLine("Ports no longer call update....is it still working?");
                //pEnd.Update();
            }

            return true;
        }

        public void Disconnect(dynPortModel p)
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

        public void NotifyConnectedPortsOfDeletion()
        {
            if (pStart != null && pStart.Connectors.Contains(this))
            {
                pStart.Disconnect(this);
            }
            if (pEnd != null && pEnd.Connectors.Contains(this))
            {
                pEnd.Disconnect(this);
            }
        }

    }

    public class InvalidPortException : ApplicationException
    {
        private string message;
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
