using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

using Dynamo.Controls;
using Dynamo.Connectors;
using Dynamo.Utilities;

using Dynamo.FSchemeInterop;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;

namespace Dynamo.Nodes
{
    [IsInteractive(true)]
    public abstract class dynEnum : dynNodeWithOneOutput
    {

        public int SelectedIndex { get; set; }
        public Array Items { get; set; }

        public dynEnum()
        {
            Items = new string[] {""};
            SelectedIndex = 0;
            OutPortData.Add(new PortData("", "Value", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override void SetupCustomUIElements(dynNodeView NodeUI)
        {
            var comboBox = new ComboBox
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center
                };

            NodeUI.inputGrid.Children.Add(comboBox);

            Grid.SetColumn(comboBox, 0);
            Grid.SetRow(comboBox, 0);

            comboBox.ItemsSource = this.Items;
            comboBox.SelectionChanged += delegate
            {
                if (comboBox.SelectedIndex == -1) return;
                this.RequiresRecalc = true;
                this.SelectedIndex = comboBox.SelectedIndex;
            };
        }

        public void WireToEnum(Array arr)
        {
            Items = arr;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            if (this.SelectedIndex < this.Items.Length)
            {
                var value = Value.NewContainer( this.SelectedIndex );
                return value;
            }
            else
            {
                throw new Exception("There is nothing selected.");
            }
        }

        public override void SaveElement(XmlDocument xmlDoc, XmlElement dynEl)
        {
            dynEl.SetAttribute("index", this.SelectedIndex.ToString());
        }

        public override void LoadElement(XmlNode elNode)
        {
            try
            {
                this.SelectedIndex = Convert.ToInt32(elNode.Attributes["index"].Value);
            }
            catch { }
        }
    }

    [IsInteractive(true)]
    public abstract class dynEnumAsString : dynEnum
    {

        public override Value Evaluate(FSharpList<Value> args)
        {
            if (this.SelectedIndex < this.Items.Length)
            {
                var value = Value.NewContainer( Items.GetValue(this.SelectedIndex) );
                return value;
            }
            else
            {
                throw new Exception("There is nothing selected.");
            }
        }

    }

}
