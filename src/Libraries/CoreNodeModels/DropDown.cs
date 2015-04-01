using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using System.Linq;
using System.Xml;

using Dynamo.Models;
using Dynamo.Properties;

namespace DSCoreNodesUI
{
    /// <summary>
    /// A class used to store a name and associated item for a drop down menu
    /// </summary>
    public class DynamoDropDownItem : IComparable
    {
        public string Name { get; set; }
        public object Item { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public DynamoDropDownItem(string name, object item)
        {
            Name = name;
            Item = item;
        }

        public int CompareTo(object obj)
        {
            var a = obj as DynamoDropDownItem;
            if (a == null)
                return 1;

            return Name.CompareTo(a);
        }

    }

    /// <summary>
    /// Base class for all nodes allowing selection using a drop-down
    /// </summary>
    public abstract class DSDropDownBase : NodeModel
    {
        protected DSDropDownBase()
        {
            ShouldDisplayPreviewCore = false;
        }

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
            OutPortData.Add(new PortData(outputName, string.Format(Properties.Resources.DropDownPortDataResultToolTip, outputName)));
            RegisterAllPorts();
            PopulateItems();
        }

        protected override void SerializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.SerializeCore(nodeElement, context);
            nodeElement.SetAttribute("index", SaveSelectedIndex(SelectedIndex, Items));            
        }

        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.DeserializeCore(nodeElement, context);
            // Drop downs previsouly saved their selected index as an int.
            // Between versions of host applications where the number or order of items
            // in a list would vary, this made loading of drop downs un-reliable.
            // We have upgraded drop downs to save their selected index as 
            // something like "9:Reference Point".

            var attrib = nodeElement.Attributes["index"];
            if (attrib == null)
                return;

            selectedIndex = ParseSelectedIndex(attrib.Value, Items);

            if (selectedIndex < 0)
            {
                Warning(Resources.NothingIsSelectedWarning);
            }
        }

        public static int ParseSelectedIndex(string index, IList<DynamoDropDownItem> items)
        {
            int selectedIndex = -1;

            var splits = index.Split(':');
            if (splits.Count() > 1)
            {
                var name = XmlUnescape(index.Substring(index.IndexOf(':') + 1));
                var item = items.FirstOrDefault(i => i.Name == name);
                selectedIndex = item != null ?
                    items.IndexOf(item) :
                    -1;
            }
            else
            {
                var tempIndex = Convert.ToInt32(index);
                selectedIndex = tempIndex > (items.Count - 1)? 
                    -1:
                    tempIndex ;
            }

            return selectedIndex;
        }

        public static string SaveSelectedIndex(int index, IList<DynamoDropDownItem> items )
        {
            // If nothing is selected or there are no
            // items in the collection, than return -1
            if (index == -1 || items.Count == 0)
            {
                return "-1";
            }
            
            var item = items[index];
            return string.Format("{0}:{1}", index, XmlEscape(item.Name));
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

        public abstract void PopulateItems();
    }
}
