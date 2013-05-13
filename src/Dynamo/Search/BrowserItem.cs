using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Dynamo.Commands;
using Dynamo.Search;
using Dynamo.Search.SearchElements;
using Dynamo.Utilities;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;
using System.Collections.Generic;

namespace Dynamo.Nodes.Search
{
    public abstract class BrowserItem : NotificationObject
    {

        public abstract ObservableCollection<BrowserItem> Items { get; set; }

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
                if (item is BrowserCategory)
                {
                    foreach (var ele in ((BrowserCategory) item).Siblings)
                    {
                        ele.IsExpanded = false;
                    }
                }

                if (item is RootBrowserCategory)
                {
                    foreach (var ele in ((RootBrowserCategory)item).Siblings)
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
