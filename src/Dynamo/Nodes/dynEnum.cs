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
        }

        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            var comboBox = new ComboBox
                {
                    MinWidth = 150,
                    Padding = new Thickness(8),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center
                };

            nodeUI.inputGrid.Children.Add(comboBox);

            Grid.SetColumn(comboBox, 0);
            Grid.SetRow(comboBox, 0);

            comboBox.ItemsSource = this.Items;
            comboBox.SelectedIndex = this.SelectedIndex;

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

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement dynEl, SaveContext context)
        {
            dynEl.SetAttribute("index", this.SelectedIndex.ToString());
        }

        protected override void LoadNode(XmlNode elNode)
        {
            try
            {
                this.SelectedIndex = Convert.ToInt32(elNode.Attributes["index"].Value);
            }
            catch { }
        }
    }

    [IsInteractive(true)]
    public abstract class dynEnumAsInt : dynEnum
    {
        public dynEnumAsInt()
        {
            OutPortData.Add(new PortData("Int", "The index of the enum", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            if (this.SelectedIndex < this.Items.Length)
            {
                var value = Value.NewNumber(this.SelectedIndex);
                return value;
            }
            else
            {
                throw new Exception("There is nothing selected.");
            }
        }

    }

    [IsInteractive(true)]
    public abstract class dynEnumAsString : dynEnum
    {
        public dynEnumAsString()
        {
            OutPortData.Add(new PortData("String", "The enum as a string", typeof(Value.String)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            if (this.SelectedIndex < this.Items.Length)
            {
                var value = Value.NewString( Items.GetValue(this.SelectedIndex).ToString() );
                return value;
            }
            else
            {
                throw new Exception("There is nothing selected.");
            }
        }

    }

}
