﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml;
using CoreNodeModels.Properties;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Newtonsoft.Json;

namespace CoreNodeModels
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

        [JsonIgnore]
        public ObservableCollection<DynamoDropDownItem> Items
        {
            get { return items; }
            set
            {
                items = value;
                RaisePropertyChanged("Items");
            }
        }
        public override NodeInputData InputData
        {
            //TODO There is not yet an appropriate input type
            //defined in the cogs graph schema to support dropdowns
            //which return arbitrary objects at some index - implement this
            //when that exists.
            get { return null; }
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
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData(outputName, string.Format(Resources.DropDownPortDataResultToolTip, outputName))));
            RegisterAllPorts();
            PopulateItems();
        }

        [JsonConstructor]
        protected DSDropDownBase(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
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
                Warning(Dynamo.Properties.Resources.NothingIsSelectedWarning);
            }
        }

        protected override bool UpdateValueCore(UpdateValueParams updateValueParams)
        {
            string name = updateValueParams.PropertyName;
            string value = updateValueParams.PropertyValue;

            if (name == "Value" && value != null)
            {
                selectedIndex = ParseSelectedIndex(value, Items);
                if (selectedIndex < 0)
                    Warning(Dynamo.Properties.Resources.NothingIsSelectedWarning);
                return true; // UpdateValueCore handled.
            }

            return base.UpdateValueCore(updateValueParams);
        }

        protected virtual int ParseSelectedIndex(string index, IList<DynamoDropDownItem> items)
        {
            return ParseSelectedIndexImpl(index, items);
        }

        public static int ParseSelectedIndexImpl(string index, IList<DynamoDropDownItem> items)
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
                selectedIndex = tempIndex > (items.Count - 1) ?
                    -1 :
                    tempIndex;
            }

            return selectedIndex;
        }

        protected virtual string SaveSelectedIndex(int index, IList<DynamoDropDownItem> items)
        {
            return SaveSelectedIndexImpl(index, items);
        }

        public static string SaveSelectedIndexImpl(int index, IList<DynamoDropDownItem> items)
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

        protected static string XmlEscape(string unescaped)
        {
            var doc = new XmlDocument();
            XmlNode node = doc.CreateElement("root");
            node.InnerText = unescaped;
            return node.InnerXml;
        }

        protected static string XmlUnescape(string escaped)
        {
            var doc = new XmlDocument();
            XmlNode node = doc.CreateElement("root");
            node.InnerXml = escaped;
            return node.InnerText;
        }

        public void PopulateItems()
        {
            var currentSelection = string.Empty;
            if (SelectedIndex >= 0 && (SelectedIndex < items.Count))
            {
                currentSelection = items.ElementAt(SelectedIndex).Name;
            }
            var selectionState = PopulateItemsCore(currentSelection);
            if (selectionState == SelectionState.Restore)
            {
                SelectedIndex = -1;
                for (int i = 0; i < items.Count; i++)
                {
                    if ((items.ElementAt(i)).Name.Equals(currentSelection))
                    {
                        SelectedIndex = i;
                    }
                }
            }
        }

        protected enum SelectionState
        {
            /// <summary>
            /// Derived class has determined the best selection. 
            /// Base class will not attempt to select another item.
            /// </summary>
            Done,

            /// <summary>
            /// Derived class could not determine the right selection
            /// and it is left to the base class to restore the previous 
            /// selection if there was one.
            /// </summary>
            Restore
        }

        /// <summary>
        /// Call this method to allow derived classes to populate the drop down 
        /// list items. An existing selection is provided as an argument so that
        /// it can be retained after drop down items are regenerated.
        /// </summary>
        /// <param name="currentSelection">Item text of an existing selected 
        /// drop down item, or string.Empty if there is no existing selection.
        /// </param>
        /// <returns>See SelectionState for more information.</returns>
        protected abstract SelectionState PopulateItemsCore(string currentSelection);

    }
}
