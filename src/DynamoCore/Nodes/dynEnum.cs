using System;
using System.Xml;
using Dynamo.Models;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;

namespace Dynamo.Nodes
{
    [IsInteractive(true)]
    public abstract partial class Enum : NodeWithOneOutput
    {

        public int SelectedIndex { get; set; }
        public Array Items { get; set; }

        public Enum()
        {
            Items = new string[] {""};
            SelectedIndex = 0;
        }

        public void WireToEnum(Array arr)
        {
            Items = arr;
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            nodeElement.SetAttribute("index", this.SelectedIndex.ToString());
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            try
            {
                this.SelectedIndex = Convert.ToInt32(nodeElement.Attributes["index"].Value);
            }
            catch { }
        }

        #region Serialization/Deserialization methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); //Base implementation must be called
            if (context == SaveContext.Undo)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                helper.SetAttribute("enumIndex", SelectedIndex);
            }
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); //Base implementation must be called
            if (context == SaveContext.Undo)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                SelectedIndex = helper.ReadInteger("enumIndex");
            }
        }

        #endregion
    }

    [IsInteractive(true)]
    public abstract class EnumAsInt : Enum
    {
        public EnumAsInt()
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
    public abstract class EnumAsString : Enum
    {
        public EnumAsString()
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
