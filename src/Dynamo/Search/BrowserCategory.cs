using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Dynamo.Nodes.Search
{

    public class RootBrowserCategory : BrowserItem
    {

        /// <summary>
        ///     The items inside of the browser item
        /// </summary>
        private ObservableCollection<BrowserItem> _items = new ObservableCollection<BrowserItem>();
        public override ObservableCollection<BrowserItem> Items { get { return _items; } set { _items = value; } }

        public ObservableCollection<RootBrowserCategory> Siblings { get; set; }

        /// <summary>
        /// Name property </summary>
        /// <value>
        /// The name of the node </value>
        private string _name;
        public override string Name
        {
            get { return _name; }
        }

        public RootBrowserCategory(string name, ObservableCollection<RootBrowserCategory> siblings)
        {
            this.Height = 32;
            this.Siblings = siblings;
            this._name = name;
        }

    }

    public class BrowserCategory : BrowserItem
    {

        /// <summary>
        ///     The items inside of the browser item
        /// </summary>
        private ObservableCollection<BrowserItem> _items = new ObservableCollection<BrowserItem>();
        public override ObservableCollection<BrowserItem> Items { get { return _items; } set { _items = value; } }

        public ObservableCollection<BrowserItem> Siblings { get; set; }

        /// <summary>
        /// Name property </summary>
        /// <value>
        /// The name of the node </value>
        private string _name;
        public override string Name
        {
            get { return _name; }
        }

        public BrowserCategory()
        {
            this._name = "Default";
            this.Siblings = new ObservableCollection<BrowserItem>();
        }

        public BrowserCategory(string name, ObservableCollection<BrowserItem> siblings)
        {
            this.Siblings = siblings;
            this._name = name;
        }

    }
}

