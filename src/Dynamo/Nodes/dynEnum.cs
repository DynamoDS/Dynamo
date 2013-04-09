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
        ComboBox combo;

        public dynEnum()
        {
            //widen the control
            NodeUI.topControl.Width = 300;

            //add a drop down list to the window
            combo = new ComboBox();
            combo.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            combo.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            NodeUI.inputGrid.Children.Add(combo);

            System.Windows.Controls.Grid.SetColumn(combo, 0);
            System.Windows.Controls.Grid.SetRow(combo, 0);

            combo.SelectionChanged += delegate
            {
                if (combo.SelectedIndex != -1)
                    this.RequiresRecalc = true;
            };
            
            OutPortData.Add(new PortData("", "Enum", typeof(object)));

            NodeUI.RegisterAllPorts();
        }

        public void WireToEnum(Array arr)
        {
            combo.ItemsSource = arr;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            if (combo.SelectedItem != null)
            {
                return Value.NewContainer(combo.SelectedItem);
            }
            else
            {
                throw new Exception("There is nothing selected.");
            }
        }

        public override void SaveElement(XmlDocument xmlDoc, XmlElement dynEl)
        {
            dynEl.SetAttribute("index", this.combo.SelectedIndex.ToString());
        }

        public override void LoadElement(XmlNode elNode)
        {
            try
            {
                combo.SelectedIndex = Convert.ToInt32(elNode.Attributes["index"].Value);
            }
            catch { }
        }
    }

    [IsInteractive(true)]
    [NodeName("Enum")]
    [NodeCategory(BuiltinNodeCategories.MISC)]
    [NodeDescription("Represent an enum in a drop-down list.")]
    public class dynEnum1 : dynEnum
    {
        enum test { TOM, DICK, LARRY };

        public dynEnum1()
        {
            WireToEnum(Enum.GetValues(typeof(test)));
        }
    }
}
