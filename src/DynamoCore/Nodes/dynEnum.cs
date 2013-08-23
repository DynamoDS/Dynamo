using System;
using System.Xml;
using Dynamo.Models;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;

namespace Dynamo.Nodes
{
    [IsInteractive(true)]
    public abstract partial class dynEnum : dynNodeWithOneOutput
    {

        public int SelectedIndex { get; set; }
        public Array Items { get; set; }

        public dynEnum()
        {
            Items = new string[] {""};
            SelectedIndex = 0;
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
