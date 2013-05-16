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
using System.Xml;
using System.IO.Ports;

using Microsoft.FSharp.Collections;

using Dynamo.Connectors;
using Dynamo.Utilities;
using Dynamo.FSchemeInterop;
using Value = Dynamo.FScheme.Value;


namespace Dynamo.Nodes
{
    [NodeName("Arduino")]
    [NodeCategory(BuiltinNodeCategories.IO_HARDWARE)]
    [NodeDescription("Manages connection to an Arduino microcontroller.")]
    public class dynArduino : dynNodeWithOneOutput
    {
        SerialPort port;
        System.Windows.Controls.MenuItem comItem;

        public dynArduino()
        {
            InPortData.Add(new PortData("exec", "Execution Interval", typeof(object)));
            OutPortData.Add(new PortData("arduino", "Serial port for later read/write", typeof(Value.Container)));

            RegisterAllPorts();

            if (port == null)
            {
                port = new SerialPort();
            }
            port.BaudRate = 9600;
            port.NewLine = "\r\n";
            port.DtrEnable = true;

        }

        public override void SetupCustomUIElements(Controls.dynNodeView NodeUI)
        {
            string[] serialPortNames = System.IO.Ports.SerialPort.GetPortNames();

            foreach (string portName in serialPortNames)
            {

                if (lastComItem != null)
                {
                    lastComItem.IsChecked = false; // uncheck last checked item
                }
                comItem = new System.Windows.Controls.MenuItem();
                comItem.Header = portName;
                comItem.IsCheckable = true;
                comItem.IsChecked = true;
                comItem.Checked += new System.Windows.RoutedEventHandler(comItem_Checked);
                NodeUI.MainContextMenu.Items.Add(comItem);

                port.PortName = portName;
                lastComItem = comItem;
            }
        }

        System.Windows.Controls.MenuItem lastComItem = null;
        
        void comItem_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem comItem = e.Source as System.Windows.Controls.MenuItem;

            if (lastComItem != null)
            {
                lastComItem.IsChecked = false; // uncheck last checked item
            }

            if (port != null)
            {
                if (port.IsOpen)
                    port.Close();
            }
            port.PortName = comItem.Header.ToString();
            comItem.IsChecked = true;
            lastComItem = comItem;
            
        }

        public override void Cleanup()
        {
            if (port != null)
            {
                if (port.IsOpen)
                    port.Close();
            }
            port = null;
        }

        public override void SaveElement(XmlDocument xmlDoc, XmlElement dynEl)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            XmlElement outEl = xmlDoc.CreateElement(typeof(double).FullName);
            outEl.SetAttribute("value", port.PortName);
            dynEl.AppendChild(outEl);
        }

        public override void LoadElement(XmlNode elNode)
        {
            foreach (XmlNode subNode in elNode.ChildNodes)
            {
                if (subNode.Name == typeof(double).FullName)
                {
                    port.PortName = subNode.Attributes[0].Value;
                }
            }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            if (((Value.Number)args[0]).Item == 1)
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

            return Value.NewContainer(port); // pass the port downstream
        }


    }

    [NodeName("Read Arduino")]
    [NodeCategory(BuiltinNodeCategories.IO_HARDWARE)]
    [NodeDescription("Reads values from an Arduino microcontroller.")]
    public class dynArduinoRead : dynNodeWithOneOutput
    {
        SerialPort port;
        int range;
        List<string> serialLine = new List<string>();


        public dynArduinoRead()
        {
            InPortData.Add(new PortData("arduino", "Arduino serial connection", typeof(object)));
            InPortData.Add(new PortData("range", "Number of lines to read", typeof(double)));
            OutPortData.Add(new PortData("output", "Serial output line", typeof(Value.List)));

            RegisterAllPorts();
        }

        private List<string> GetArduinoData()
        {
            string data = port.ReadExisting();
            List<string> serialRange = new List<string>();

            string[] allData = data.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (allData.Length > 2)
            {


                string lastData = allData[allData.Length - 2];
                string[] values = lastData.Split(new char[]{'\t'}, StringSplitOptions.RemoveEmptyEntries);

                //get the sensor values, tailing the values list and passing back a range from [range to count-2]
                int start = allData.Length - range - 2; //get the second to last element as the last is often truncated
                //int end = allData.Length - 2; 
                try
                {
                    serialRange = allData.ToList<string>().GetRange(0, range);
                    return serialRange;
                }
                catch
                {
                    return serialRange;
                }

            }

            return serialRange;

        }


        public override Value Evaluate(FSharpList<Value> args)
        {
            port = (SerialPort)((Value.Container)args[0]).Item;
            range = (int)((Value.Number)args[1]).Item;
            

            if (port != null)
            {
                bool isOpen = true;

                if (isOpen == true)
                {
                    if (!port.IsOpen)
                    {
                        port.Open();
                    }

                    //get the values from the serial port as a list of strings
                    serialLine = GetArduinoData();


                }
                else if (isOpen == false)
                {
                    if (port.IsOpen)
                        port.Close();
                }
            }


            return Value.NewList(Utils.SequenceToFSharpList(serialLine.Select(Value.NewString)));
        }


    }

    [NodeName("Write Arduino")]
    [NodeCategory(BuiltinNodeCategories.IO_HARDWARE)]
    [NodeDescription("Writes values to an Arduino microcontroller.")]
    public class dynArduinoWrite : dynNodeWithOneOutput
    {
        SerialPort port;

        public dynArduinoWrite()
        {
            InPortData.Add(new PortData("arduino", "Arduino serial connection", typeof(object)));
            InPortData.Add(new PortData("text", "Text to be written", typeof(string)));
            OutPortData.Add(new PortData("success?", "Whether or not the operation was successful.", typeof(Value.Number)));

            RegisterAllPorts();
        }

        private void WriteDataToArduino(string dataLine)
        {

            dataLine = dataLine + "\r\n"; //termination
            port.WriteLine(dataLine);

        }

        public override Value Evaluate(FSharpList<Value> args)
        {

            port = (SerialPort)((Value.Container)args[0]).Item;
            string dataToWrite = ((Value.String)args[1]).Item;// ((Value.Container)args[1]).Item;

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
            

            return Value.NewNumber(1);// catch failures here 
        }
    }
}
