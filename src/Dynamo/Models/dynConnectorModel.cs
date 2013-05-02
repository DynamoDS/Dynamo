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

    public class dynConnectorModel:dynModelBase
    {
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
        
        #region constructors
        
        public dynConnectorModel(dynNodeModel start, dynNodeModel end, int startIndex, int endIndex, int portType, bool visible)
        {
            //don't try to create a connector with a bad start,
            //end, or if we're trying to connector the same
            //port to itself.
            if (start == null || end == null || start == end)
            {
                throw new Exception("Attempting to create connector with invalid start or end nodes.");
            }

            pStart = start.OutPorts[startIndex];

            dynPortModel endPort = null;

            if (portType == 0)
                endPort = end.InPorts[endIndex];

            //connect the two ports
            //get start point

            pStart.Connect(this);

            //isDrawing = true;

            this.Connect(endPort);

            //MVVM: the visibility of one connector type or another is now bound
            //to the DynamoViewModel's ConnectorType property
            //ConnectorType = dynSettings.Controller.DynamoViewModel.ConnectorType;
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
//MVVM : Do not make edits to the view here
            /*connector.StrokeDashArray.Clear();
            plineConnector.StrokeDashArray.Clear();*/
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

//MVVM : Do not make edits to the view here

            //turn the connector back to dashed
            /*connector.StrokeDashArray.Add(5);
            connector.StrokeDashArray.Add(2);

            plineConnector.StrokeDashArray.Add(5);
            plineConnector.StrokeDashArray.Add(2);*/

        }

        public void Kill()
        {
            if (pStart != null && pStart.Connectors.Contains(this))
            {
                pStart.Disconnect(this);
                //pStart.Connectors.Remove(this);
                //do not remove the owner's output element
            }
            if (pEnd != null && pEnd.Connectors.Contains(this))
            {
                pEnd.Disconnect(this);
                //remove the reference to the
                //dynElement attached to port A

                //if (pEnd.Index < pEnd.Owner.InPortData.Count)
                //{
                //   pEnd.Owner.InPortData[pEnd.Index].Object = null;
                //}
            }

            pStart = null;
            pEnd = null;

//MVVM : Do not manage view on canvas here
            /*dynSettings.Workbench.Children.Remove(connector);
            dynSettings.Workbench.Children.Remove(plineConnector);
            dynSettings.Workbench.Children.Remove(endDot);
            dynSettings.Bench.RemoveConnector(this);
             

            isDrawing = false;*/
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
