using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;
using ProtoCore.AST.AssociativeAST;

namespace DSCoreNodesUI
{
    /*
    [IsInteractive(true)]
    public abstract class Enum : DSDropDownBase
    {

        public int SelectedIndex { get; set; }
        public Array Items { get; set; }

        protected Enum()
        {
            Items = new string[] { "" };
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

        public void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

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
    }
    */

    public abstract class EnumAsInt : EnumBase
    {
        protected EnumAsInt(Type t):base(t){}

        public override System.Collections.Generic.IEnumerable<ProtoCore.AST.AssociativeAST.AssociativeNode> BuildOutputAst(System.Collections.Generic.List<ProtoCore.AST.AssociativeAST.AssociativeNode> inputAstNodes)
        {
            var rhs = AstFactory.BuildIntNode(SelectedIndex);
            var assignment = AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), rhs);

            return new[] { assignment };
        }

    }

    public abstract class EnumAsString : EnumBase
    {
        protected EnumAsString(Type t):base(t){}

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var rhs = AstFactory.BuildStringNode(Items[SelectedIndex].Item.ToString());
            var assignment = AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), rhs);

            return new[] { assignment };
        }
    }

    public abstract class EnumBase : DSDropDownBase
    {
        protected Type enum_internal;

        protected EnumBase(Type t): base(t.ToString())
        {
            enum_internal = t;
            PopulateItems();
        }

        protected override void PopulateItems()
        {
            if (enum_internal == null)
                return;

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
    public abstract class AllChildrenOfType : DSDropDownBase
    {
        private Type internal_type;

        protected AllChildrenOfType(Type t):base("Types")
        {
            internal_type = t;
            OutPortData.Add(new PortData("", string.Format("All types which inherit from {0}.", t.ToString()), typeof(object)));
            RegisterAllPorts();
            PopulateItems();
        }

        protected override void PopulateItems()
        {
            Items.Clear();

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
