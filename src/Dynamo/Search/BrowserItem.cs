using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Dynamo.Search.SearchElements;
using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo.Nodes.Search
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
           if (this.Visibility == Visibility.Visible && this.Items.Count == 0)
           {
               items.Add(this);
           }
           else if (this.Visibility != Visibility.Visible)
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
        public void AddChild(BrowserInternalElement elem)
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
        public void SetVisibilityToLeaves(Visibility visibility)
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
        private Visibility _visibility = Visibility.Visible;
        public Visibility Visibility
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

        public ToggleIsExpandedCommand _toggleIsExpanded;
        public ToggleIsExpandedCommand ToggleIsExpanded
        {
            get
            {
                if (_toggleIsExpanded == null)
                    _toggleIsExpanded = new ToggleIsExpandedCommand(this);
                return _toggleIsExpanded;
            }
        }

        public class ToggleIsExpandedCommand : ICommand
        {
            private BrowserItem item;

            public ToggleIsExpandedCommand(BrowserItem i)
            {
                this.item = i;
            }

            public void Execute(object parameters)
            {
                if (item is SearchElementBase)
                {
                    ((SearchElementBase) item).Execute();
                    return;
                }
                var endState = !item.IsExpanded;
                if (item is BrowserInternalElement)
                {
                    foreach (var ele in ((BrowserInternalElement) item).Siblings)
                    {
                        ele.IsExpanded = false;
                    }
                }

                if (item is BrowserRootElement)
                {
                    foreach (var ele in ((BrowserRootElement)item).Siblings)
                    {
                        ele.IsExpanded = false;
                    }
                }
                item.IsExpanded = endState;
            }

            public event EventHandler CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }

            public bool CanExecute(object parameters)
            {
                return true;
            }
        }
    }
}
