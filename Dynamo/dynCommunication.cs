//Copyright 2012 Ian Keough

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
using Microsoft.FSharp.Collections;
using Expression = Dynamo.FScheme.Expression;

namespace Dynamo.Elements
{
   [ElementName("Web Request")]
   [ElementCategory(BuiltinElementCategories.MISC)]
   [ElementDescription("Fetches data from the web using a URL.")]
   [RequiresTransaction(false)]
   public class dynWebRequest : dynNode
   {
      public dynWebRequest()
      {
         InPortData.Add(new Connectors.PortData("url", "A URL to query.", typeof(dynString)));
         OutPortData = new Connectors.PortData("str", "The string returned from the web request.", typeof(dynString));
         //OutPortData[0].Object = this.Tree;

         base.RegisterInputsAndOutputs();
      }

      public override Expression Evaluate(FSharpList<Expression> args)
      {
         string url = ((Expression.String)args[0]).Item;

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

         return Expression.NewString(responseFromServer);
      }

   }

   [ElementName("UDP Listener")]
   [ElementCategory(BuiltinElementCategories.MISC)]
   [ElementDescription("Listens for data from the web using a UDP port")]
   [RequiresTransaction(false)]
   public class dynUDPListener : dynNode
   {
       public dynUDPListener()
       {
           InPortData.Add(new Connectors.PortData("exec", "Execution Interval", typeof(object)));
           InPortData.Add(new Connectors.PortData("udp port", "A UDP port to listen to.", typeof(object)));
           OutPortData = new Connectors.PortData("str", "The string returned from the web request.", typeof(object));


           base.RegisterInputsAndOutputs();
       }

       private delegate void LogDelegate(string msg);
       private delegate void UDPListening();

       private string UDPResponse = "";
       int listenPort;

       private void ListenOnUDP()
       {
           
           LogDelegate log = new LogDelegate(this.Bench.Log);

           // UDP sample from http://stackoverflow.com/questions/8274247/udp-listener-respond-to-client

           UdpClient listener = new UdpClient(listenPort);
           IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, listenPort);

           try
           {

                log("Waiting for broadcast");
                byte[] bytes = listener.Receive(ref groupEP);
                UDPResponse = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
                string verboseLog = "Received broadcast from " + groupEP.ToString() + ":\n" + UDPResponse + "\n";
                log(verboseLog);
               
           }
           catch (Exception e)
           {
               UDPResponse = "";
               log(e.ToString());
           }
           finally
           {
               listener.Close();
           }
       }

       public override Expression Evaluate(FSharpList<Expression> args)
       {
           listenPort = (int)((Expression.Number)args[1]).Item; // udp port to listen to


           if (((Expression.Number)args[0]).Item == 1) // if exec node has pumped
           {
               this.Dispatcher.BeginInvoke(new UDPListening(ListenOnUDP));
           }
                         


           return Expression.NewString(UDPResponse);
       }
      
   }
}
