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
    [ElementDescription("Manages connection to an Arduino microcontroller.")]
    [RequiresTransaction(false)]
    public class dynArduino : dynNode
    {
        SerialPort port;
        System.Windows.Controls.MenuItem com4Item;
        System.Windows.Controls.MenuItem com3Item;

        public dynArduino()
        {
            InPortData.Add(new PortData("exec", "Execution Interval", typeof(object)));
            OutPortData = new PortData("arduino serial", "Serial port for later read/write", typeof(object));

            base.RegisterInputsAndOutputs();

            port = new SerialPort("COM4", 9600);
            port.NewLine = "\r\n";
            port.DtrEnable = true;

            com3Item = new System.Windows.Controls.MenuItem();
            com3Item.Header = "COM3";
            com3Item.IsCheckable = true;
            com3Item.IsChecked = false;
            com3Item.Checked += new System.Windows.RoutedEventHandler(com3Item_Checked);

            com4Item = new System.Windows.Controls.MenuItem();
            com4Item.Header = "COM4";
            com4Item.IsCheckable = true;
            com4Item.IsChecked = true;
            com4Item.Checked += new System.Windows.RoutedEventHandler(com4Item_Checked);

            this.MainContextMenu.Items.Add(com3Item);
            this.MainContextMenu.Items.Add(com4Item);
            port.PortName = "COM4";

            this.dynElementDestroyed += new dynElementDestroyedHandler(OnDynArduinoDestroyed);
            this.dynElementReadyToDestroy += new dynElementReadyToDestroyHandler(OnDynArduinoReadyToDestroy);

        }

        public event dynElementDestroyedHandler dynElementDestroyed;
        public event dynElementReadyToDestroyHandler dynElementReadyToDestroy;

        public delegate void dynElementDestroyedHandler(object sender, EventArgs e);
        public delegate void dynElementReadyToDestroyHandler(object sender, EventArgs e);

        void com4Item_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (port != null)
            {
                if (port.IsOpen)
                    port.Close();
            }
            port.PortName = "COM4";
            com4Item.IsChecked = true;
            com3Item.IsChecked = false;
        }

        void com3Item_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (port != null)
            {
                if (port.IsOpen)
                    port.Close();
            }
            port.PortName = "COM3";
            com4Item.IsChecked = false;
            com3Item.IsChecked = true;
        }



        void OnDynArduinoDestroyed(object sender, EventArgs e)
        {
            if (dynElementDestroyed != null)
            {

                if (port != null)
                {
                    if (port.IsOpen)
                        port.Close();
                }
                port = null;

                dynElementDestroyed(this, e);
            }
        }

        void OnDynArduinoReadyToDestroy(object sender, EventArgs e)
        {
            if (port != null)
            {
                if (port.IsOpen)
                    port.Close();
            }
            port = null;

            dynElementDestroyed(this, e);
        }


        public override Expression Evaluate(FSharpList<Expression> args)
        {
            if (((Expression.Number)args[0]).Item == 1)
            {
                if (port != null)
                {
                    bool isOpen = true;

                    if (isOpen == true)
                    {
                        if (!port.IsOpen)
                        {
                            port.Open();
                        }


                    }
                    else if (isOpen == false)
                    {
                        if (port.IsOpen)
                            port.Close();
                    }
                }
            }

            return Expression.NewContainer(port); // pass the port downstream
        }


    }

    [ElementName("Read Arduino")]
    [ElementCategory(BuiltinElementCategories.MISC)]
    [ElementDescription("Reads values from an Arduino microcontroller.")]
    [RequiresTransaction(false)]
    public class dynArduinoRead : dynNode
    {
        SerialPort port;


        public dynArduinoRead()
        {
            InPortData.Add(new PortData("arduino", "Arduino serial connection", typeof(object)));
            OutPortData = new PortData("output", "Serial output line", typeof(string));

            base.RegisterInputsAndOutputs();

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
                this.serialLine = sensorString;

            }

        }

        int data;
        string serialLine;

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            port = (SerialPort)((Expression.Container)args[0]).Item;

            if (port != null)
            {
                bool isOpen = true;

                if (isOpen == true)
                {
                    if (!port.IsOpen)
                    {
                        port.Open();
                    }

                    //get the value from the serial port as a string
                    GetArduinoData();

                }
                else if (isOpen == false)
                {
                    if (port.IsOpen)
                        port.Close();
                }
            }
            

            return Expression.NewString(this.serialLine);
        }


    }

    [ElementName("Write Arduino")]
    [ElementCategory(BuiltinElementCategories.MISC)]
    [ElementDescription("Writes values to an Arduino microcontroller.")]
    [RequiresTransaction(false)]
    public class dynArduinoWrite : dynNode
    {
        SerialPort port;

        public dynArduinoWrite()
        {
            InPortData.Add(new PortData("arduino", "Arduino serial connection", typeof(object)));
            InPortData.Add(new PortData("text", "Text to be written", typeof(string)));
            OutPortData = new PortData("success?", "Whether or not the operation was successful.", typeof(bool));

            base.RegisterInputsAndOutputs();
        }


        private void WriteDataToArduino(string dataLine)
        {

            dataLine = dataLine + "\r\n"; //termination
            port.WriteLine(dataLine);

        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {

            port = (SerialPort)((Expression.Container)args[0]).Item;
            string dataToWrite = ((Expression.String)args[1]).Item;// ((Expression.Container)args[1]).Item;

            if (port != null)
            {
                bool isOpen = true;// Convert.ToBoolean(InPortData[0].Object);

                if (isOpen == true)
                {
                    if (!port.IsOpen)
                    {
                        port.Open();
                    }

                    //write data to the serial port
                    WriteDataToArduino(dataToWrite);

                }
                else if (isOpen == false)
                {
                    if (port.IsOpen)
                        port.Close();
                }
            }
            

            return Expression.NewNumber(1);// catch failures here 
        }


    }
}
