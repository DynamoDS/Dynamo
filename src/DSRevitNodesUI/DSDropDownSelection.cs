using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Xml;
using Autodesk.Revit.DB;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Utilities;
using ProtoCore.AST.AssociativeAST;
using RevitServices.Persistence;
using Binding = System.Windows.Data.Binding;

namespace Dynamo.Nodes
{
    /// <summary>
    /// Base class for all nodes allowing selection using a drop-down
    /// </summary>
    public abstract class DSDropDownBase : NodeModel
    {
        private ObservableCollection<DynamoDropDownItem> items = new ObservableCollection<DynamoDropDownItem>();
        public ObservableCollection<DynamoDropDownItem> Items
        {
            get { return items; }
            set
            {
                items = value;
                RaisePropertyChanged("Items");
            }
        }

        private int selectedIndex = 0;
        public int SelectedIndex
        {
            get { return selectedIndex; }
            set
            {
                //do not allow selected index to
                //go out of range of the items collection
                if (value > Items.Count - 1)
                {
                    selectedIndex = -1;
                }
                else
                    selectedIndex = value;
                RaisePropertyChanged("SelectedIndex");
            }
        }

        protected DSDropDownBase(string outputName)
        {
            Items.CollectionChanged += Items_CollectionChanged;
            OutPortData.Add(new PortData(outputName, string.Format("The selected {0}", outputName), typeof(object)));
            RegisterAllPorts();
            PopulateItems();
        }

        void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //sort the collection when changed
            //rehook the collection changed event
            var sortedItems = from item in Items
                              orderby item.Name
                              select item;
            Items = sortedItems.ToObservableCollection();
            Items.CollectionChanged += Items_CollectionChanged;
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

        protected abstract void PopulateItems();

        /// <summary>
        /// When the dropdown is opened, the node's implementation of PopulateItems is called
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void combo_DropDownOpened(object sender, EventArgs e)
        {
            PopulateItems();
        }

        public void SetupCustomUIElements(dynNodeView nodeUI)
        {
            //add a drop down list to the window
            var combo = new ComboBox
            {
                Width = 300,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center
            };
            nodeUI.inputGrid.Children.Add(combo);
            System.Windows.Controls.Grid.SetColumn(combo, 0);
            System.Windows.Controls.Grid.SetRow(combo, 0);

            combo.DropDownOpened += combo_DropDownOpened;
            combo.SelectionChanged += delegate
            {
                if (combo.SelectedIndex != -1)
                    RequiresRecalc = true;
            };

            combo.DataContext = this;
            //bind this combo box to the selected item hash

            var bindingVal = new System.Windows.Data.Binding("Items") { Mode = BindingMode.TwoWay, Source = this };
            combo.SetBinding(ItemsControl.ItemsSourceProperty, bindingVal);

            //bind the selected index to the 
            var indexBinding = new Binding("SelectedIndex")
            {
                Mode = BindingMode.TwoWay,
                Source = this
            };
            combo.SetBinding(Selector.SelectedIndexProperty, indexBinding);
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            return base.BuildOutputAst(inputAstNodes);
        }
    }

    [NodeName("Select Family Type")]
    [NodeCategory(BuiltinNodeCategories.REVIT_FAMILIES)]
    [NodeDescription("Select a Family Type.")]
    [NodeSearchTags("family", "type")]
    [IsInteractive(true)]
    [IsDesignScriptCompatible]
    public class DSFamilyTypeSelection : DSDropDownBase
    {
        private Type internalType;

        public DSFamilyTypeSelection() : base("Family Type"){}

        protected override void PopulateItems()
        {
            Items.Clear();

            var fec = new FilteredElementCollector(DocumentManager.GetInstance().CurrentDBDocument);
            fec.OfClass(typeof(Family));

            foreach (Family family in fec.ToElements())
            {
                foreach (FamilySymbol fs in family.Symbols)
                {
                    Items.Add(new DynamoDropDownItem(fs.Name, fs));
                }
            }
        }
    }

    [NodeName("Select Wall Type")]
    [NodeCategory(BuiltinNodeCategories.REVIT_FAMILIES)]
    [NodeDescription("Select a Wall Type.")]
    [NodeSearchTags("wall", "type")]
    [IsInteractive(true)]
    [IsDesignScriptCompatible]
    public class DSWallTypeSelection : DSDropDownBase
    {
        public DSWallTypeSelection(): base("Wall Type"){}

        protected override void PopulateItems()
        {
            Items.Clear();

            var fec = new FilteredElementCollector(DocumentManager.GetInstance().CurrentDBDocument);
            fec.OfClass(typeof(WallType));
            foreach (WallType wt in fec.ToElements())
            {
                Items.Add(new DynamoDropDownItem(wt.Name, wt));
            }
        }
    }

    [NodeName("Select Floor Type")]
    [NodeCategory(BuiltinNodeCategories.REVIT_FAMILIES)]
    [NodeDescription("Select a Floor Type.")]
    [NodeSearchTags("wall", "type")]
    [IsInteractive(true)]
    [IsDesignScriptCompatible]
    public class DSFloorTypeSelection : DSDropDownBase
    {
        public DSFloorTypeSelection() : base("Floor Type") { }

        protected override void PopulateItems()
        {
            Items.Clear();

            var fec = new FilteredElementCollector(DocumentManager.GetInstance().CurrentDBDocument);
            fec.OfClass(typeof(FloorType));
            foreach (FloorType ft in fec.ToElements())
            {
                Items.Add(new DynamoDropDownItem(ft.Name, ft));
            }
        }
    }
}
