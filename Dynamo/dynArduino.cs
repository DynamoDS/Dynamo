//Copyright © Autodesk, Inc. 2012. All rights reserved.
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.FSharp.Collections;
using System.IO.Ports;
using Dynamo.Connectors;
using Expression = Dynamo.FScheme.Expression;

namespace Dynamo.Elements
{
   public enum COMPort { COM3, COM4 };

   [ElementName("Arduino")]
   [ElementCategory(BuiltinElementCategories.MISC)]
   [ElementDescription("An element which allows you to read from an Arduino microcontroller.")]
   [RequiresTransaction(false)]
   public class dynArduino : dynElement
   {
      SerialPort port;
      //string lastData = "";
      //COMPort portState;
      System.Windows.Controls.MenuItem com4Item;
      System.Windows.Controls.MenuItem com3Item;

      public dynArduino()
      {
         //InPortData.Add(new PortData(null, "loop", "The loop to execute.", typeof(dynLoop)));
         //InPortData.Add(new PortData(null, "i/o", "Switch Arduino on?", typeof(bool)));
         //InPortData.Add(new PortData(null, "tim", "How often to receive updates.", typeof(double)));

         OutPortData = new PortData("output", "Serial output", typeof(double));
         //OutPortData[0].Object = this.Tree;

         base.RegisterInputsAndOutputs();

         port = new SerialPort("COM3", 9600);
         port.NewLine = "\r\n";
         port.DtrEnable = true;
         //port.DataReceived += new SerialDataReceivedEventHandler(serialPort1_DataReceived);

         com3Item = new System.Windows.Controls.MenuItem();
         com3Item.Header = "COM3";
         com3Item.IsCheckable = true;
         com3Item.IsChecked = true;
         com3Item.Checked += new System.Windows.RoutedEventHandler(com3Item_Checked);

         com4Item = new System.Windows.Controls.MenuItem();
         com4Item.Header = "COM4";
         com4Item.IsCheckable = true;
         com4Item.IsChecked = false;
         com4Item.Checked += new System.Windows.RoutedEventHandler(com4Item_Checked);

         this.MainContextMenu.Items.Add(com3Item);
         this.MainContextMenu.Items.Add(com4Item);
         //portState = COMPort.COM3;
         port.PortName = "COM3";
      }

      void com4Item_Checked(object sender, System.Windows.RoutedEventArgs e)
      {
         //portState = COMPort.COM4;
         com4Item.IsChecked = true;
         com3Item.IsChecked = false;
      }

      void com3Item_Checked(object sender, System.Windows.RoutedEventArgs e)
      {
         //portState = COMPort.COM3;
         com4Item.IsChecked = false;
         com3Item.IsChecked = true;
      }

      //public override void Draw()
      //{
      //   if (CheckInputs())
      //   {
      //      //add one branch
      //      //this.Tree.Trunk.Branches.Add(new DataTreeBranch());
      //      //this.Tree.Trunk.Branches[0].Leaves.Add(null);

      //      if (port != null)
      //      {
      //         bool isOpen = Convert.ToBoolean(InPortData[0].Object);

      //         if (isOpen == true)
      //         {
      //            if (!port.IsOpen)
      //            {
      //               if (portState == COMPort.COM3)
      //                  port.PortName = "COM3";
      //               else
      //                  port.PortName = "COM4";

      //               port.Open();
      //            }

      //            //get the analog value from the serial port
      //            GetArduinoData();

      //            //i don't know why this works 
      //            //but OnDynElementReadyToBuild doesn't
      //            //this.UpdateOutputs();
      //            //OnDynElementReadyToBuild(EventArgs.Empty);
      //         }
      //         else if (isOpen == false)
      //         {
      //            if (port.IsOpen)
      //               port.Close();
      //         }

      //      }

      //   }
      //}

      private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
      {
         if (CheckInputs())
         {
            //add one branch
            //this.Tree.Trunk.Branches.Add(new DataTreeBranch());
            //this.Tree.Trunk.Branches[0].Leaves.Add(null);

            if (port != null)
            {
               bool isOpen = true;// Convert.ToBoolean(InPortData[0].Object);

               if (isOpen == true)
               {
                  if (!port.IsOpen)
                  {
                     port.Open();
                  }

                  //get the analog value from the serial port
                  GetArduinoData();

                  //i don't know why this works 
                  //but OnDynElementReadyToBuild doesn't
                  this.UpdateOutputs();
                  //OnDynElementReadyToBuild(EventArgs.Empty);
               }
               else if (isOpen == false)
               {
                  if (port.IsOpen)
                     port.Close();
               }

            }

         }
      }

      private void GetArduinoData()
      {
         //string data = port.ReadExisting();
         //lastData += data;
         //string[] allData = lastData.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
         //if (allData.Length > 0)
         //{
         //lastData = allData[allData.Length - 1];
         //this.Tree.Trunk.Branches[0].Leaves[0] = lastData;
         //this.OutPortData[0].Object = Convert.ToDouble(lastData);
         //}

         //int data = 255;
         while (port.BytesToRead > 0)
         {
            this.data = port.ReadByte();
            this.IsDirty = true;
         }

         //this.OutPortData[0].Object = Convert.ToDouble(data);
      }

      //public override void Update()
      //{
      //   OnDynElementReadyToBuild(EventArgs.Empty);
      //}

      int data;

      public override Expression Evaluate(FSharpList<Expression> args)
      {
         return Expression.NewNumber(this.data);
      }

   }
}
