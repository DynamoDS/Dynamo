using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.IO.Ports;
using Autodesk.DesignScript.Runtime;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Wpf;
using Dynamo.Nodes.Properties;


namespace Dynamo.Nodes
{
    public class ArduinoNodeViewCustomization : INodeViewCustomization<Arduino>
    {
        MenuItem comItem;
        MenuItem lastComItem;
        private Arduino arduinoModel;

        public void CustomizeView(Arduino model, NodeView nodeView)
        {
            string[] serialPortNames = SerialPort.GetPortNames();

            foreach (string portName in serialPortNames)
            {

                if (lastComItem != null)
                {
                    lastComItem.IsChecked = false; // uncheck last checked item
                }
                comItem = new MenuItem { Header = portName, IsCheckable = true, IsChecked = true };
                comItem.Checked += comItem_Checked;
                nodeView.MainContextMenu.Items.Add(comItem);

                arduinoModel.Port.PortName = portName;
                lastComItem = comItem;
            }
        }

        void comItem_Checked(object sender, RoutedEventArgs e)
        {
            var item = (MenuItem)e.Source;

            if (lastComItem != null)
            {
                lastComItem.IsChecked = false; // uncheck last checked item
            }

            if (arduinoModel.Port != null)
            {
                if (arduinoModel.Port.IsOpen)
                    arduinoModel.Port.Close();

                arduinoModel.Port.PortName = item.Header.ToString();
            }

            item.IsChecked = true;
            lastComItem = item;

        }

        public void Dispose() { }
    }

    [NodeName("Arduino")]
    [NodeCategory(BuiltinNodeCategories.IO_HARDWARE)]
    [NodeDescription("ArduinoDescription",typeof(Properties.Resources))]
    [IsVisibleInDynamoLibrary(false)]
    public class Arduino : NodeModel
    {
        public SerialPort Port { get; private set; }

        public Arduino()
        {
            InPortData.Add(new PortData("exec", Resources.ArduinoPortDataExecToolTip));
            OutPortData.Add(new PortData("arduino", Resources.ArduinoPortDataOutputToolTip));

            RegisterAllPorts();

            if (Port != null)
            {
                if (Port.IsOpen)
                    Port.Close();
            }
            Port = new SerialPort { BaudRate = 9600, NewLine = "\r\n", DtrEnable = true };
        }

        public override void Dispose()
        {
            if (Port != null)
            {
                if (Port.IsOpen)
                    Port.Close();
            }
            Port = null;
        }

        protected override void SerializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.SerializeCore(nodeElement, context);
            //Debug.WriteLine(pd.Object.GetType().ToString());
            XmlElement outEl = nodeElement.OwnerDocument.CreateElement(typeof(double).FullName);
            outEl.SetAttribute("value", Port.PortName);
            nodeElement.AppendChild(outEl);
        }

        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.DeserializeCore(nodeElement, context);
            Port.PortName =
                nodeElement.ChildNodes.Cast<XmlNode>()
                    .Where(subNode => subNode.Name == typeof(double).FullName && subNode.Attributes != null)
                    .Select(subNode => subNode.Attributes[0].Value)
                    .Last();
        }

        //public override Value Evaluate(FSharpList<Value> args)
        //{
        //    if (((Value.Number)args[0]).Item == 1)
        //    {
        //        if (port != null)
        //        {
        //            bool isOpen = true;

        //            if (isOpen == true)
        //            {
        //                if (!port.IsOpen)
        //                {
        //                    port.Open();
        //                }


        //            }
        //            else if (isOpen == false)
        //            {
        //                if (port.IsOpen)
        //                    port.Close();
        //            }
        //        }
        //    }

        //    return Value.NewContainer(port); // pass the port downstream
        //}

    }

    [NodeName("Read Arduino")]
    [NodeCategory(BuiltinNodeCategories.IO_HARDWARE)]
    [NodeDescription("ReadArduinoDescription", typeof(Properties.Resources))]
    public class ArduinoRead : NodeModel
    {
        SerialPort port;

        public ArduinoRead()
        {
            InPortData.Add(new PortData("arduino", Resources.PortDataArduinoToolTip));
            InPortData.Add(new PortData("delimiter", Resources.ArduionReadPortDataDelimiterToolTip));
            OutPortData.Add(new PortData("output", Resources.ArduinoReadPortDataOutputToolTip));

            RegisterAllPorts();
        }

        private string GetArduinoData(SerialPort port, string delim)
        {
            string data = port.ReadExisting();

            string[] allData = data.Split(delim.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            return allData.Any() ? allData[allData.Count()-2] : string.Empty;
        }


        //public override Value Evaluate(FSharpList<Value> args)
        //{
        //    port = (SerialPort)((Value.Container)args[0]).Item;
        //    var delim = ((Value.String) args[1]).Item;

        //    var lastValue = string.Empty;

        //    if (port != null)
        //    {
        //        bool isOpen = true;

        //        if (isOpen == true)
        //        {
        //            if (!port.IsOpen)
        //            {
        //                port.Open();
        //            }

        //            //get the values from the serial port as a list of strings
        //            lastValue = GetArduinoData(port, delim);
        //        }
        //    }

        //    return Value.NewString(lastValue);
        //}

    }

    [NodeName("Write Arduino")]
    [NodeCategory(BuiltinNodeCategories.IO_HARDWARE)]
    [NodeDescription("WriteArduinoDescription",typeof(Properties.Resources))]
    public class ArduinoWrite : NodeModel
    {
        SerialPort port;

        public ArduinoWrite()
        {
            InPortData.Add(new PortData("arduino", Resources.PortDataArduinoToolTip));
            InPortData.Add(new PortData("text", Resources.ArduionWritePortDataTextToolTip));
            OutPortData.Add(new PortData("success?", Resources.ArduinoWritePortDataOutputToolTip));

            RegisterAllPorts();
        }

        private void WriteDataToArduino(string dataLine)
        {

            dataLine = dataLine + "\r\n"; //termination
            port.WriteLine(dataLine);

        }

        //public override Value Evaluate(FSharpList<Value> args)
        //{

        //    port = (SerialPort)((Value.Container)args[0]).Item;
        //    string dataToWrite = ((Value.String)args[1]).Item;// ((Value.Container)args[1]).Item;

        //    if (port != null)
        //    {
        //        bool isOpen = true;// Convert.ToBoolean(InPortData[0].Object);

        //        if (isOpen == true)
        //        {
        //            if (!port.IsOpen)
        //            {
        //                port.Open();
        //            }

        //            //write data to the serial port
        //            WriteDataToArduino(dataToWrite);

        //        }
        //        else if (isOpen == false)
        //        {
        //            if (port.IsOpen)
        //                port.Close();
        //        }
        //    }
            

        //    return Value.NewNumber(1);// catch failures here 
        //}
    }
}
