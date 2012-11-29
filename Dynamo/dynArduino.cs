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

    [ElementName("Arduino")]
    [ElementCategory(BuiltinElementCategories.MISC)]
    [ElementDescription("Reads values from an Arduino microcontroller.")]
    [RequiresTransaction(false)]
    public class dynArduino : dynNode
    {
        SerialPort port;
        System.Windows.Controls.MenuItem com4Item;
        System.Windows.Controls.MenuItem com3Item;

        public dynArduino()
        {
            InPortData.Add(new PortData("exec", "Execution Interval", typeof(object)));
            OutPortData = new PortData("output", "Serial output", typeof(double));

            base.RegisterInputsAndOutputs();

            port = new SerialPort("COM3", 9600);
            port.NewLine = "\r\n";
            port.DtrEnable = true;

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
            port.PortName = "COM3";
        }

        void com4Item_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            port.PortName = "COM4";
            com4Item.IsChecked = true;
            com3Item.IsChecked = false;
        }

        void com3Item_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            port.PortName = "COM3";
            com4Item.IsChecked = false;
            com3Item.IsChecked = true;
        }

        private void GetArduinoData()
        {
            //data comes off this port looking like 
            //sensor = xxx\toutput = xxx
            //sensor = xxx\toutput = xxx

            string data = port.ReadExisting();

            string[] allData = data.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (allData.Length > 2)
            {
                //get the second to last element
                //the last is often truncated
                string lastData = allData[allData.Length - 2];
                string[] values = lastData.Split(new char[]{'\t'}, StringSplitOptions.RemoveEmptyEntries);

                //get the sensor value
                string sensorString = values[0];
                string[] sensorValues = values[0].Split(new char[]{'='}, StringSplitOptions.RemoveEmptyEntries);

                if (sensorValues.Length > 0)
                {
                    this.data = Convert.ToInt16(sensorValues[1]);
                    this.IsDirty = true;
                }
            }

        }

        int data;

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            if (((Expression.Number)args[0]).Item == 1)
            {
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

                    }
                    else if (isOpen == false)
                    {
                        if (port.IsOpen)
                            port.Close();
                    }
                }
            }

            return Expression.NewNumber(this.data);
        }

    }
}
