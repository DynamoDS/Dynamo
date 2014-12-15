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
        /// Sort this items children and then tell its children and recurse on children
        /// </summary>
        public void RecursivelySort()
        {
            this.Items = new ObservableCollection<BrowserItem>(this.Items.OrderBy(x => x.Name));

            // BrowserInternalElementForClasses object, if any, must 
            // always appear before any other nested namespaces. 
            var classes = this.Items.OfType<BrowserInternalElementForClasses>().FirstOrDefault();
            if (classes != null)
            {
                this.Items.Remove(classes);
                this.Items.Insert(0, classes);
            }

            this.Items.ToList().ForEach(x => x.RecursivelySort());
        }

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
                // TODO (Vladimir): check if this is needed
                //if (ele is ClassInformation) continue;
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
                // TODO (Vladimir): check if this is needed
                //if (ele is ClassInformation) continue;
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

        // TODO (Vladimir): ToggleIsExpandedCommand removed in new sources
        //private ToggleIsExpandedCommand _toggleIsExpanded;
        //public ToggleIsExpandedCommand ToggleIsExpanded
        //{
        //    get
        //    {
        //        if (_toggleIsExpanded == null)
        //            _toggleIsExpanded = new ToggleIsExpandedCommand(this);
        //        return _toggleIsExpanded;
        //    }
        //}

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

    // TODO (Vladimir): remove it
    //public class ToggleIsExpandedCommand : ICommand
    //{
    //    private BrowserItem item;

    //    public ToggleIsExpandedCommand(BrowserItem i)
    //    {
    //        this.item = i;
    //    }

    //    public void Execute(object parameters)
    //    {
    //        if (item is PackageManagerSearchElement)
    //        {
    //            item.IsExpanded = !item.IsExpanded;
    //            return;
    //        }

    //        if (item is SearchElementBase)
    //        {
    //            ((SearchElementBase)item).Execute();
    //            return;
    //        }
    //        var endState = !item.IsExpanded;
    //        if (item is BrowserInternalElement || item is BrowserRootElement)
    //        {
    //            dynamic element = item;

    //            // Collapse all expanded items on next level.
    //            if (endState)
    //            {
    //                foreach (var ele in element.Items)
    //                    ele.IsExpanded = false;
    //            }

    //            foreach (var ele in element.Siblings)
    //                ele.IsExpanded = false;

    //            //Walk down the tree expanding anything nested one layer deep
    //            //this can be removed when we have the hierachy implemented properly
    //            if (element.Items.Count == 1)
    //            {
    //                BrowserItem subElement = element.Items[0];
    //                while (subElement.Items.Count == 1)
    //                {
    //                    subElement.IsExpanded = true;
    //                    subElement = subElement.Items[0];
    //                }

    //                subElement.IsExpanded = true;
    //            }
    //        }

    //        item.IsExpanded = endState;
    //    }

    //    public event EventHandler CanExecuteChanged
    //    {
    //        add { CommandManager.RequerySuggested += value; }
    //        remove { CommandManager.RequerySuggested -= value; }
    //    }

    //    public bool CanExecute(object parameters)
    //    {
    //        return true;
    //    }
    //}
}
