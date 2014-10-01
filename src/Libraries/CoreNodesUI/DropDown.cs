using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Xml;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.UI;

namespace DSCoreNodesUI
{
    /// <summary>
    /// Base class for all nodes allowing selection using a drop-down
    /// </summary>
    public abstract class DSDropDownBase : NodeModel, IWpfNode
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

        protected DSDropDownBase(WorkspaceModel workspaceModel, string outputName) : base(workspaceModel)
        {
            OutPortData.Add(new PortData(outputName, string.Format("The selected {0}", outputName)));
            RegisterAllPorts();
            PopulateItems();
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            var item = Items[SelectedIndex];
            nodeElement.SetAttribute("index", string.Format("{0}:{1}", SelectedIndex, XmlEscape(item.Name)));
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            // Drop downs previsouly saved their selected index as an int.
            // Between versions of host applications where the number or order of items
            // in a list would vary, this made loading of drop downs un-reliable.
            // We have upgraded drop downs to save their selected index as 
            // something like "9:Reference Point".

            var attrib = nodeElement.Attributes["index"];
            if (attrib == null)
                return;

            var index = attrib.Value;
            var splits = index.Split(':');
            if (splits.Count() > 1)
            {
                var name = XmlUnescape(index.Substring(index.IndexOf(':') + 1));
                var item = Items.FirstOrDefault(i => i.Name == name);
                SelectedIndex = item != null ? 
                    Items.IndexOf(item) : 
                    Convert.ToInt32(nodeElement.Attributes["index"].Value);
            }
            else
            {
                SelectedIndex = Convert.ToInt32(nodeElement.Attributes["index"].Value);  
            }
        }

        private static string XmlEscape(string unescaped)
        {
            var doc = new XmlDocument();
            XmlNode node = doc.CreateElement("root");
            node.InnerText = unescaped;
            return node.InnerXml;
        }

        private static string XmlUnescape(string escaped)
        {
            var doc = new XmlDocument();
            XmlNode node = doc.CreateElement("root");
            node.InnerXml = escaped;
            return node.InnerText;
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
                Width = System.Double.NaN,
                MinWidth = 100,
                Height = Configurations.PortHeightInPixels,
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

            combo.DropDownClosed += delegate
            {
                //disallow selection of nothing
                if (combo.SelectedIndex == -1)
                {
                    SelectedIndex = 0;
                }
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

        protected override bool ShouldDisplayPreviewCore()
        {
            return false; // Previews are not shown for this node type.
        }
    }
}
