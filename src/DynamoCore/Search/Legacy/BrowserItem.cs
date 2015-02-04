using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Dynamo.Core;
using Dynamo.PackageManager;
using Dynamo.Search.SearchElements;


namespace Dynamo.Search
{
    public abstract class BrowserItem : NotificationObject
    {
        public abstract ObservableCollection<BrowserItem> Items { get; set; }

        /// <summary>
        ///     If this is a leaf and visible, add to items, otherwise, recurse on children
        /// </summary>
        /// <param name="items">The accumulator</param>
        public void GetVisibleLeaves(ref List<BrowserItem> items)
        {
            if (this.Visibility == true && this.Items.Count == 0)
            {
                items.Add(this);
            }
            else if (this.Visibility != true)
            {
                return;
            }
            else
            {
                foreach (var item in this.Items)
                {
                    item.GetVisibleLeaves(ref items);
                }
            }
        }

        /// <summary>
        /// A name (title) field for the BrowserItem
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// The height of the element in search
        /// </summary>
        private int _height = 30;
        public int Height
        {
            get { return _height; }
            set
            {
                _height = value;
                RaisePropertyChanged("Height");
            }
        }

        /// <summary>
        ///     Adds an element as a child of this one, while updating its parent and oldparent field
        /// </summary>
        /// <param name="elem">The element in question</param>
        internal void AddChild(BrowserInternalElement elem)
        {

            elem.OldParent = elem.Parent;
            if (elem.Parent != null)
                elem.Parent.Items.Remove(elem);

            elem.Parent = this;
            this.Items.Add(elem);
        }

        /// <summary>
        /// Collapse element and all its children
        /// </summary>
        public void CollapseToLeaves()
        {
            this.IsExpanded = false;
            foreach (var ele in Items)
            {
                ele.CollapseToLeaves();
            }
        }

        /// <summary>
        /// Hide element and all its children
        /// </summary>
        public void SetVisibilityToLeaves(bool visibility)
        {
            this.Visibility = visibility;
            foreach (var ele in Items)
            {
                ele.SetVisibilityToLeaves(visibility);
            }
        }

        /// <summary>
        /// Whether the item is visible or not
        /// </summary>
        private bool _visibility = true;
        public bool Visibility
        {
            get
            {
                return _visibility;
            }
            set
            {
                _visibility = value;
                RaisePropertyChanged("Visibility");
            }
        }

        /// <summary>
        /// Whether the item is selected or not
        /// </summary>
        private bool _isSelected = false;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                RaisePropertyChanged("IsSelected");
            }
        }

        /// <summary>
        /// Is the element expanded in the browser
        /// </summary>
        private bool _isExpanded = false;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                _isExpanded = value;
                RaisePropertyChanged("IsExpanded");
            }
        }

        public abstract void Execute();

        public delegate void BrowserItemHandler(BrowserItem ele);
        public event BrowserItemHandler Executed;
        protected void OnExecuted()
        {
            if (Executed != null)
            {
                Executed(this);
            }
        }

    }
}
