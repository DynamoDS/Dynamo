using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Xml;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.UI.Prompts;
using Microsoft.FSharp.Collections;

namespace Dynamo.Nodes
{
    [NodeName("Length")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("Enter a length in project units.")]
    [NodeSearchTags("Imperial", "Metric", "Length", "Project", "units")]
    public class LengthInput : NodeWithOneOutput
    {
        private double _value;
        public double Value
        {
            get { return _value; }
            set
            {
                _value = value;
                RaisePropertyChanged("Value");
            }
        }

        public LengthInput()
        {
            OutPortData.Add(new PortData("length", "The length. Stored internally as decimal feet.", typeof(FScheme.Value.Number)));
            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            return FScheme.Value.NewNumber(Value);
        }

        public override void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

            //add an edit window option to the 
            //main context window
            var editWindowItem = new System.Windows.Controls.MenuItem();
            editWindowItem.Header = "Edit...";
            editWindowItem.IsCheckable = false;

            nodeUI.MainContextMenu.Items.Add(editWindowItem);

            editWindowItem.Click += new RoutedEventHandler(editWindowItem_Click);
            //add a text box to the input grid of the control
            var tb = new DynamoTextBox();
            tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            nodeUI.inputGrid.Children.Add(tb);
            System.Windows.Controls.Grid.SetColumn(tb, 0);
            System.Windows.Controls.Grid.SetRow(tb, 0);
            tb.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF));

            tb.DataContext = this;
            tb.BindToProperty(new System.Windows.Data.Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Converter = new RevitProjectUnitsConverter(),
                //ConverterParameter = Measure,
                NotifyOnValidationError = false,
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            });

            tb.OnChangeCommitted += delegate { RequiresRecalc = true; };
        }

        protected override bool UpdateValueCore(string name, string value)
        {
            if (name == "Value")
            {
                var converter = new RevitProjectUnitsConverter();
                this.Value = ((double)converter.ConvertBack(value, typeof(double), null, null));
                return true; // UpdateValueCore handled.
            }

            return base.UpdateValueCore(name, value);
        }

        private void editWindowItem_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new EditWindow() { DataContext = this };
            editWindow.BindToProperty(null, new System.Windows.Data.Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Converter = new RevitProjectUnitsConverter(),
                //ConverterParameter = Measure,
                NotifyOnValidationError = false,
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            });

            editWindow.ShowDialog();
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            XmlElement outEl = xmlDoc.CreateElement(typeof(double).FullName);
            outEl.SetAttribute("value", Value.ToString(CultureInfo.InvariantCulture));
            nodeElement.AppendChild(outEl);
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                // this node now stores a double, having previously stored a measure type
                // by checking for the measure type as well we allow for loading of older files.
                if (subNode.Name.Equals(typeof(double).FullName) || subNode.Name.Equals("Dynamo.Measure.Foot"))
                {
                    Value = DeserializeValue(subNode.Attributes[0].Value);
                }
            }
        }

        public override string PrintExpression()
        {
            return Value.ToString();
        }

        protected double DeserializeValue(string val)
        {
            try
            {
                return Convert.ToDouble(val, CultureInfo.InvariantCulture);
            }
            catch
            {
                return 0;
            }
        }
    }
}
