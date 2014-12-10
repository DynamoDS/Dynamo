using System.Collections.ObjectModel;
using System.Linq;

namespace Dynamo.Search
{
    public class BrowserRootElement : BrowserItem
    {
        /// <summary>
        ///     The items inside of the browser item
        /// </summary>
        private ObservableCollection<BrowserItem> _items = new ObservableCollection<BrowserItem>();
        public override ObservableCollection<BrowserItem> Items { get { return _items; } set { _items = value; } }

        public ObservableCollection<BrowserRootElement> Siblings { get; set; }

        /// <summary>
        /// Name property </summary>
        /// <value>
        /// The name of the node </value>
        private string _name;
        public override string Name
        {
            get { return _name; }
        }

        public BrowserRootElement(string name, ObservableCollection<BrowserRootElement> siblings)
        {
            this.Height = 32;
            this.Siblings = siblings;
            this._name = name;
        }

        public void SortChildren()
        {
            this.Items = new ObservableCollection<BrowserItem>(this.Items.OrderBy(x => x.Name));
        }

        public BrowserRootElement(string name)
        {
            this.Height = 32;
            this.Siblings = null;
            this._name = name;
        }

        public override void Execute()
        {
            var endState = !this.IsExpanded;

            foreach (var ele in this.Siblings)
                ele.IsExpanded = false;

            //Walk down the tree expanding anything nested one layer deep
            //this can be removed when we have the hierachy implemented properly
            if (this.Items.Count == 1)
            {
                BrowserItem subElement = this.Items[0];

                while (subElement.Items.Count == 1)
                {
                    subElement.IsExpanded = true;
                    subElement = subElement.Items[0];
                }

                subElement.IsExpanded = true;

            }

            this.IsExpanded = endState;
        }

    }
}