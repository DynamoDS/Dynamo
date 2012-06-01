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
using System.IO;
using Microsoft.FSharp.Collections;
using Expression = Dynamo.FScheme.Expression;

namespace Dynamo.Elements
{
   [ElementName("Web Request")]
   [ElementCategory(BuiltinElementCategories.MISC)]
   [ElementDescription("An element which gathers data from the web using a URL.")]
   [RequiresTransaction(false)]
   public class dynWebRequest : dynElement
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

      //public override void Draw()
      //{
      //   if (CheckInputs())
      //   {
      //      //send a webrequest to the URL
      //      // Initialize the WebRequest.
      //      WebRequest myRequest = WebRequest.Create(InPortData[0].Object.ToString());

      //      // Return the response. 
      //      WebResponse myResponse = myRequest.GetResponse();

      //      Stream dataStream = myResponse.GetResponseStream();

      //      // Open the stream using a StreamReader for easy access.
      //      StreamReader reader = new StreamReader(dataStream);

      //      // Read the content.
      //      string responseFromServer = reader.ReadToEnd();

      //      // Code to use the WebResponse goes here.
      //      this.Tree.Trunk.Leaves.Add(responseFromServer);

      //      reader.Close();

      //      // Close the response to free resources.
      //      myResponse.Close();
      //   }
      //}

      //public override void Update()
      //{
      //   OnDynElementReadyToBuild(EventArgs.Empty);
      //}
   }
}
