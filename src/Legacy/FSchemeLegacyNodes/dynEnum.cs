using System;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using Dynamo.Models;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;

namespace Dynamo.Legacy
{
    [IsInteractive(true)]
    public abstract partial class Enum : NodeWithOneOutput
    {

        public int SelectedIndex { get; set; }
        public Array Items { get; set; }

        protected Enum()
        {
            Items = new[] {""};
            SelectedIndex = 0;
        }

        public void WireToEnum(Array arr)
        {
            Items = arr;
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            nodeElement.SetAttribute("index", SelectedIndex.ToString());
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            try
            {
                SelectedIndex = Convert.ToInt32(nodeElement.Attributes["index"].Value);
            }
            catch { }
        }
    }

    [IsInteractive(true)]
    public abstract class EnumAsInt : Enum
    {
        protected EnumAsInt()
        {
            OutPortData.Add(new PortData("Int", "The index of the enum", typeof(FScheme.Value.Number)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            if (SelectedIndex < Items.Length)
            {
                var value = FScheme.Value.NewNumber(SelectedIndex);
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
        protected EnumAsString()
        {
            OutPortData.Add(new PortData("String", "The enum as a string", typeof(FScheme.Value.String)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            if (SelectedIndex < Items.Length)
            {
                var value = FScheme.Value.NewString( Items.GetValue(SelectedIndex).ToString() );
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
            OutPortData.Add(new PortData("", "The members of the enumeration.", typeof(FScheme.Value.List)));
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
            OutPortData.Add(new PortData("", string.Format("All types which inherit from {0}.", t.ToString()), typeof(FScheme.Value.List)));
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

