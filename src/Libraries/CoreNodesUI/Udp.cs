using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;
using ProtoCore.AST.AssociativeAST;

namespace DSCoreNodesUI
{
    [NodeName("UDP Listener")]
    [NodeCategory(BuiltinNodeCategories.IO_HARDWARE)]
    [NodeDescription("Listens for data from the web using a UDP port")]
    [IsDesignScriptCompatible]
    public class UdpListener : NodeModel
    {
        private UdpClient listener;
        private IPEndPoint groupEP;
        private string UDPResponse = "";
        private const int listenPort = 7777;

        public UdpListener()
        {
            InPortData.Add(new PortData("port", "A UDP port to listen to (int)."));
            OutPortData.Add(new PortData("message", "The message (string)."));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;

            ListenOnUDP();

            dynSettings.Controller.DynamoModel.WorkspaceCleared += DynamoModel_WorkspaceCleared;
        }

        void DynamoModel_WorkspaceCleared(object sender, EventArgs e)
        {
            if (listener != null)
            {
                listener.Close();
                listener = null;
            }
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var stringNode = AstFactory.BuildStringNode(UDPResponse);
            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), stringNode) };
        }

        #region legacy async behavior - keep until we decide not to use

        private void ListenOnUDP()
        {
            // UDP sample from http://stackoverflow.com/questions/8274247/udp-listener-respond-to-client

            if (listener == null)
            {
                listener = new UdpClient(listenPort);
                groupEP = new IPEndPoint(IPAddress.Any, listenPort);
            }

            try
            {
                var s = new UdpState
                {
                    endPoint = groupEP,
                    udpClient = listener
                };

                dynSettings.DynamoLogger.Log("Waiting for broadcast");
                listener.BeginReceive(ReceiveCallback, s);
            }
            catch (Exception e)
            {
                UDPResponse = "";
                dynSettings.DynamoLogger.Log(e.ToString());
            }
        }

        internal class UdpState
        {
            public IPEndPoint endPoint;
            public UdpClient udpClient;
        }

        public void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                var u = ((UdpState)(ar.AsyncState)).udpClient;
                var e = ((UdpState)(ar.AsyncState)).endPoint;

                var receiveBytes = u.EndReceive(ar, ref e);
                var receiveString = Encoding.ASCII.GetString(receiveBytes);

                UDPResponse = Encoding.ASCII.GetString(receiveBytes, 0, receiveBytes.Length);
                var verboseLog = "Received broadcast from " + e.ToString() + ":\n" + UDPResponse + "\n";
                dynSettings.DynamoLogger.Log(verboseLog);

                dynSettings.DynamoLogger.Log(string.Format("Received: {0}", receiveString));

                //kick off listening again
                u.BeginReceive(ReceiveCallback, ar.AsyncState);

                RequiresRecalc = true;
            }
            catch (Exception e)
            {
                UDPResponse = "";
                dynSettings.DynamoLogger.Log(e.ToString());
            }
        }

        #endregion
    }
}
