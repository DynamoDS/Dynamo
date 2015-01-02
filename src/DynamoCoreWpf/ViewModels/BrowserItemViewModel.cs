using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

using Dynamo.Search;

using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo.Wpf.ViewModels
{
    public abstract class BrowserItemViewModel : NotificationObject
    {
        public ICommand ToggleIsExpandedCommand { get; protected set; }
        public BrowserItem Model { get; private set; }
        public ObservableCollection<BrowserItemViewModel> Items { get; set; }

        protected BrowserItemViewModel(BrowserItem model)
        {
            Model = model;
            ToggleIsExpandedCommand = new DelegateCommand(Model.Execute);
            Items = new ObservableCollection<BrowserItemViewModel>();

            foreach (var item in Model.Items)
            {
                Items.Add(Wrap(item));
            }

            Model.Items.CollectionChanged += ItemsOnCollectionChanged;
        }

        /// <summary>
        /// Sort this items children and then tell its children and recurse on children
        /// </summary>
        public void RecursivelySort()
        {
            Items = new ObservableCollection<BrowserItemViewModel>(Items.OrderBy(x => x.Model.Name));
            Items.ToList().ForEach(x => x.RecursivelySort());
        }

        private void ItemsOnCollectionChanged(object sender, 
            NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (BrowserItem item in e.NewItems.OfType<BrowserItem>()) {
                        Items.Add(Wrap(item));
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Items.Clear();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (
                        var vm in
                            from object item in e.OldItems
                            select Items.First(x => x.Model == item))
                    {
                        Items.Remove(vm);
                    }
                    break;
            }
        }

        #region Wrap

        internal static BrowserItemViewModel Wrap(BrowserItem item)
        {
            dynamic itemDyn = item;
            return WrapExplicit(itemDyn);
        }

        internal static BrowserRootElementViewModel WrapExplicit(BrowserRootElement elem)
        {
            return new BrowserRootElementViewModel(elem);
        }

        internal static BrowserInternalElementViewModel WrapExplicit(BrowserInternalElement elem)
        {
            return new BrowserInternalElementViewModel(elem);
        }

        #endregion

    }

    public interface ISearchEntryViewModel : INotifyPropertyChanged, IDisposable
    {
        string Name { get; }
        bool Visibility { get; }
        bool IsSelected { get; }
        string Description { get; }
        ICommand ClickedCommand { get; }
    }

    public class NodeCategoryViewModel : NotificationObject, ISearchEntryViewModel
    {
        public ICommand ClickedCommand { get; private set; }

        private string name;
        private ObservableCollection<ISearchEntryViewModel> items;
        private ObservableCollection<NodeSearchElementViewModel> entries;
        private ObservableCollection<NodeCategoryViewModel> subCategories;
        private bool visibility;
        private bool isExpanded;
        private bool isSelected;

        public string Name
        {
            get { return name; }
            set
            {
                if (value == name) return;
                name = value;
                RaisePropertyChanged("Name");
            }
        }

        public ObservableCollection<ISearchEntryViewModel> Items
        {
            get { return items; }
            set
            {
                if (Equals(value, items)) return;
                items = value;
                RaisePropertyChanged("Items");
            }
        }

        public ObservableCollection<NodeSearchElementViewModel> Entries
        {
            get { return entries; }
            set
            {
                if (Equals(value, entries)) return;
                entries = value;
                RaisePropertyChanged("Entries");
            }
        }

        public ObservableCollection<NodeCategoryViewModel> SubCategories
        {
            get { return subCategories; }
            set
            {
                if (Equals(value, subCategories)) return;
                subCategories = value;
                RaisePropertyChanged("SubCategories");
            }
        }

        public bool Visibility
        {
            get { return visibility && Items.Any(item => item.Visibility); }
            set
            {
                if (value.Equals(visibility)) return;
                visibility = value;
                RaisePropertyChanged("Visibility");
            }
        }

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (value.Equals(isSelected)) return;
                isSelected = value;
                RaisePropertyChanged("IsSelected");
            }
        }

        public string Description
        {
            get
            {
                return "";
            }
        }

        public bool IsExpanded
        {
            get { return isExpanded; }
            set
            {
                if (value.Equals(isExpanded)) return;
                isExpanded = value;
                RaisePropertyChanged("IsExpanded");
            }
        }

        public NodeCategoryViewModel(string name)
            : this(
                name,
                Enumerable.Empty<NodeSearchElementViewModel>(),
                Enumerable.Empty<NodeCategoryViewModel>()) { }

        public NodeCategoryViewModel(string name, IEnumerable<NodeSearchElementViewModel> entries, IEnumerable<NodeCategoryViewModel> subs)
        {
            ClickedCommand = new DelegateCommand(Expand);

            Name = name;
            Entries = new ObservableCollection<NodeSearchElementViewModel>(entries);
            SubCategories = new ObservableCollection<NodeCategoryViewModel>(subs);

            foreach (var category in SubCategories)
                category.PropertyChanged += CategoryOnPropertyChanged;

            Entries.CollectionChanged += OnCollectionChanged;
            SubCategories.CollectionChanged += OnCollectionChanged;
            SubCategories.CollectionChanged += SubCategoriesOnCollectionChanged;

            Items = new ObservableCollection<ISearchEntryViewModel>(
                Entries.Cast<ISearchEntryViewModel>().Concat(SubCategories)
                    .OrderBy(x => x.Name));

            Items.CollectionChanged += ItemsOnCollectionChanged;
            
            foreach (var item in Items)
                item.PropertyChanged += ItemOnPropertyChanged;
            
            Visibility = true;
            IsExpanded = false;
        }

        private void ItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Move)
                return;


            if (args.NewItems != null)
            {
                foreach (var item in args.NewItems.Cast<ISearchEntryViewModel>())
                {
                    item.PropertyChanged += ItemOnPropertyChanged;
                }
            } 
            
            if (args.OldItems != null)
            {
                foreach (var item in args.OldItems.Cast<ISearchEntryViewModel>())
                {
                    item.PropertyChanged -= ItemOnPropertyChanged;
                }
            }
        }

        private void SubCategoriesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Move)
                return;

            if (args.NewItems != null)
            {
                foreach (var category in args.NewItems.Cast<NodeCategoryViewModel>())
                {
                    category.PropertyChanged += CategoryOnPropertyChanged;
                }
            }

            if (args.OldItems != null)
            {
                foreach (var category in args.OldItems.Cast<NodeCategoryViewModel>())
                {
                    category.PropertyChanged -= CategoryOnPropertyChanged;
                }
            }
        }

        public void Dispose()
        {
            foreach (var category in SubCategories)
                category.PropertyChanged -= CategoryOnPropertyChanged;

            foreach (var item in Items)
                item.PropertyChanged -= ItemOnPropertyChanged;
        }

        public void DisposeTree()
        {
            Dispose();

            foreach (var entry in Entries)
                entry.Dispose();

            foreach (var subCategory in SubCategories)
                subCategory.DisposeTree();
        }

        private void CategoryOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "IsExpanded")
            {
                var category = (NodeCategoryViewModel)sender;
                if (category.IsExpanded)
                {
                    foreach (var other in SubCategories.Where(other => other != category))
                    {
                        other.IsExpanded = false;
                    }
                }
            }
        }

        private void ItemOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "Visibility")
                RaisePropertyChanged("Visibility");
        }

        protected virtual void Expand()
        {
            var endState = !IsExpanded;

            //foreach (var ele in this.Siblings)
            //    ele.IsExpanded = false;

            //Walk down the tree expanding anything nested one layer deep
            //this can be removed when we have the hierachy implemented properly
            if (Items.Count == 1 && SubCategories.Any())
            {
                var subElement = SubCategories[0];
                while (subElement.Items.Count == 1)
                {
                    subElement.IsExpanded = true;
                    if (subElement.SubCategories.Any())
                        subElement = subElement.SubCategories[0];
                    else 
                        break;
                }

                subElement.IsExpanded = true;
            }

            IsExpanded = endState;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            switch (notifyCollectionChangedEventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddToItems(notifyCollectionChangedEventArgs.NewItems.Cast<ISearchEntryViewModel>());
                    break;
                case NotifyCollectionChangedAction.Remove:
                    RemoveFromItems(notifyCollectionChangedEventArgs.OldItems.Cast<ISearchEntryViewModel>());
                    break;
                case NotifyCollectionChangedAction.Replace:
                    RemoveFromItems(notifyCollectionChangedEventArgs.OldItems.Cast<ISearchEntryViewModel>());
                    AddToItems(notifyCollectionChangedEventArgs.NewItems.Cast<ISearchEntryViewModel>());
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    SyncItems();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SyncItems()
        {
            Items.CollectionChanged -= ItemsOnCollectionChanged;

            foreach (var item in items)
                item.PropertyChanged -= ItemOnPropertyChanged;

            Items = new ObservableCollection<ISearchEntryViewModel>(
                Entries.Cast<ISearchEntryViewModel>().Concat(SubCategories)
                    .OrderBy(x => x.Name));

            Items.CollectionChanged += ItemsOnCollectionChanged;

            foreach (var item in items)
                item.PropertyChanged += ItemOnPropertyChanged;
        }

        private void RemoveFromItems(IEnumerable<ISearchEntryViewModel> oldItems)
        {
            foreach (var entry in oldItems)
                Items.Remove(entry);   
        }

        private void AddToItems(IEnumerable<ISearchEntryViewModel> newItems)
        {
            foreach (var entry in newItems)
            {
                var first = Items.Select((x, i) => new { x.Name, Idx = i })
                    .FirstOrDefault(
                        x =>
                            string.Compare(
                                x.Name,
                                entry.Name,
                                StringComparison.Ordinal)
                                >= 0);
                if (first != null)
                    Items.Insert(first.Idx, entry);
                else
                    Items.Add(entry);
            }
        }
    }

    public class RootNodeCategoryViewModel : NodeCategoryViewModel
    {
        public RootNodeCategoryViewModel(string name) : base(name) { }

        public RootNodeCategoryViewModel(
            string name, IEnumerable<NodeSearchElementViewModel> entries,
            IEnumerable<NodeCategoryViewModel> subs) : base(name, entries, subs)
        { }
    }
}
