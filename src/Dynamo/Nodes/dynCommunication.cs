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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Windows.Threading;
using System.Security.Cryptography;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Dynamo.Connectors;
using Value = Dynamo.FScheme.Value;

namespace Dynamo.Nodes
{
    [NodeName("Web Request")]
    [NodeCategory(BuiltinNodeCategories.IO_HARDWARE)]
    [NodeDescription("Fetches data from the web using a URL.")]
    public class dynWebRequest : dynNodeWithOneOutput
    {
        public dynWebRequest()
        {
            InPortData.Add(new PortData("url", "A URL to query.", typeof(Value.String)));
            OutPortData.Add(new PortData("str", "The string returned from the web request.", typeof(Value.String)));
            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            string url = ((Value.String)args[0]).Item;

            //send a webrequest to the URL
            // Initialize the WebRequest.
            WebRequest myRequest = WebRequest.Create(url);

            // Return the response. 
            WebResponse myResponse = myRequest.GetResponse();

            Stream dataStream = myResponse.GetResponseStream();

            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);

            // Read the content.
            string responseFromServer = reader.ReadToEnd();

            reader.Close();

            // Close the response to free resources.
            myResponse.Close();

            return Value.NewString(responseFromServer);
        }

    }

    [NodeName("UDP Listener")]
    [NodeCategory(BuiltinNodeCategories.IO_HARDWARE)]
    [NodeDescription("Listens for data from the web using a UDP port")]
    public class dynUDPListener : dynNodeWithOneOutput
    {
        public dynUDPListener()
        {
            InPortData.Add(new Connectors.PortData("exec", "Execution Interval", typeof(Value.Number)));
            InPortData.Add(new Connectors.PortData("udp port", "A UDP port to listen to.", typeof(object)));
            OutPortData.Add(new Connectors.PortData("str", "The string returned from the web request.", typeof(Value.String)));

            RegisterAllPorts();
        }

        private delegate void LogDelegate(string msg);
        private delegate void UDPListening();

        public string UDPResponse = "";
        int listenPort;
        //bool UDPInitialized = false;

        public class UdpState
        {
            public IPEndPoint e;
            public UdpClient u;

        }

        public static bool messageReceived = false;

        public void ReceiveCallback(IAsyncResult ar)
        {
            LogDelegate log = new LogDelegate(dynSettings.Controller.DynamoViewModel.Log);

            try
            {
                UdpClient u = (UdpClient)((UdpState)(ar.AsyncState)).u;
                IPEndPoint e = (IPEndPoint)((UdpState)(ar.AsyncState)).e;

                Byte[] receiveBytes = u.EndReceive(ar, ref e);
                string receiveString = Encoding.ASCII.GetString(receiveBytes);

                UDPResponse = Encoding.ASCII.GetString(receiveBytes, 0, receiveBytes.Length);
                string verboseLog = "Received broadcast from " + e.ToString() + ":\n" + UDPResponse + "\n";
                log(verboseLog);

                Console.WriteLine("Received: {0}", receiveString);
                messageReceived = true;
            }
            catch (Exception e)
            {
                UDPResponse = "";
                log(e.ToString());
            }
        }

        private void ListenOnUDP()
        {

            LogDelegate log = new LogDelegate(dynSettings.Controller.DynamoViewModel.Log);

            // UDP sample from http://stackoverflow.com/questions/8274247/udp-listener-respond-to-client
            UdpClient listener;
            IPEndPoint groupEP;
            listener = new UdpClient(listenPort);
            groupEP = new IPEndPoint(IPAddress.Any, listenPort);

            try
            {

                if (messageReceived == false)
                {

                    UdpState s = new UdpState();
                    s.e = groupEP;
                    s.u = listener;



                    log("Waiting for broadcast");
                    listener.BeginReceive(new AsyncCallback(ReceiveCallback), s);
                    //byte[] bytes = listener.Receive(ref groupEP);
                }
            }
            catch (Exception e)
            {
                UDPResponse = "";
                log(e.ToString());
            }
            finally
            {
                if (messageReceived == true)
                {
                    listener.Close();
                    messageReceived = false;
                }
            }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            listenPort = (int)((Value.Number)args[1]).Item; // udp port to listen to


            if (((Value.Number)args[0]).Item == 1) // if exec node has pumped
            {
                //MVVM: now using node's dispatch on UI thread method
                //NodeUI.Dispatcher.BeginInvoke(new UDPListening(ListenOnUDP));
                DispatchOnUIThread(ListenOnUDP);
            }



            return Value.NewString(UDPResponse);
        }

    }
}
