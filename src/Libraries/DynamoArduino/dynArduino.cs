using System;
using System.IO.Ports;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using Autodesk.DesignScript.Runtime;
using Dynamo.Controls;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Nodes.Properties;
using Dynamo.Wpf;


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
    [NodeDescription("ArduinoDescription",typeof(Dynamo.Nodes.Properties.Resources))]
    [IsVisibleInDynamoLibrary(false)]
    public class Arduino : NodeModel
    {
        public SerialPort Port { get; private set; }

        public Arduino()
        {
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("exec", Resources.ArduinoPortDataExecToolTip)));
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("arduino", Resources.ArduinoPortDataOutputToolTip)));

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
    }

    [NodeName("Read Arduino")]
    [NodeCategory(BuiltinNodeCategories.IO_HARDWARE)]
    [NodeDescription("ReadArduinoDescription", typeof(Dynamo.Nodes.Properties.Resources))]
    public class ArduinoRead : NodeModel
    {
        SerialPort port;

        public ArduinoRead()
        {
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("arduino", Resources.PortDataArduinoToolTip)));
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("delimiter", Resources.ArduionReadPortDataDelimiterToolTip)));
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("output", Resources.ArduinoReadPortDataOutputToolTip)));

            RegisterAllPorts();
        }

        private string GetArduinoData(SerialPort port, string delim)
        {
            string data = port.ReadExisting();

            string[] allData = data.Split(delim.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            return allData.Any() ? allData[allData.Count()-2] : string.Empty;
        }
    }

    [NodeName("Write Arduino")]
    [NodeCategory(BuiltinNodeCategories.IO_HARDWARE)]
    [NodeDescription("WriteArduinoDescription",typeof(Dynamo.Nodes.Properties.Resources))]
    public class ArduinoWrite : NodeModel
    {
        SerialPort port;

        public ArduinoWrite()
        {
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("arduino", Resources.PortDataArduinoToolTip)));
            InPorts.Add(new PortModel(PortType.Input, this,  new PortData("text", Resources.ArduionWritePortDataTextToolTip)));
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("success?", Resources.ArduinoWritePortDataOutputToolTip)));

            RegisterAllPorts();
        }

        private void WriteDataToArduino(string dataLine)
        {
            dataLine = dataLine + "\r\n"; //termination
            port.WriteLine(dataLine);
        }
    }
}
