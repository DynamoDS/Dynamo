using System;
using System.Diagnostics;
using System.Linq;
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

    /// <summary>
    /// Creates a drop down node which lists the constants of a provided enum.
    /// </summary>
    [IsInteractive(true)]
    public abstract class EnumAsConstants : DropDrownBase
    {
        protected Type enum_internal;

        protected EnumAsConstants(Type t)
        {
            enum_internal = t;
            OutPortData.Add(new PortData("", "The members of the enumeration.", typeof(Value.List)));
            RegisterAllPorts();
            PopulateItems();
        }

        public override void PopulateItems()
        {
            Items.Clear();
            foreach (var constant in System.Enum.GetValues(enum_internal))
            {
                Items.Add(new DynamoDropDownItem(constant.ToString(), constant));
            }

            Items = Items.OrderBy(x => x.Name).ToObservableCollection<DynamoDropDownItem>();
        }
    }

    /// <summary>
    /// The drop down node base class which lists all loaded types which are children
    /// of the provided type.
    /// </summary>
    [IsInteractive(true)]
    public abstract class AllChildrenOfType : DropDrownBase
    {
        private Type internal_type;

        protected AllChildrenOfType(Type t)
        {
            internal_type = t;
            OutPortData.Add(new PortData("", string.Format("All types which inherit from {0}.", t.ToString()), typeof(Value.List)));
            RegisterAllPorts();
            PopulateItems();
        }

        public override void PopulateItems()
        {
            Items.Clear();
            
            //var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var childTypes = internal_type.Assembly.GetTypes().Where(type => type.IsSubclassOf(internal_type));
            
            foreach (var childType in childTypes)
            {
                Debug.WriteLine(childType);
                var simpleName = childType.ToString().Split('.').LastOrDefault();
                Items.Add(new DynamoDropDownItem(simpleName, childType));
            }

            Items = Items.OrderBy(x => x.Name).ToObservableCollection<DynamoDropDownItem>();
        }

    }
}

