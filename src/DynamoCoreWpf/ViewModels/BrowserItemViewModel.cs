using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;

using Dynamo.Search;
using Dynamo.Search.SearchElements;

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

    public interface ISearchEntryViewModel
    {
        string Name { get; }
        bool Visibility { get; }
    }

    public class NodeCategoryViewModel : NotificationObject, ISearchEntryViewModel
    {
        private string name;
        private ObservableCollection<ISearchEntryViewModel> items;
        private ObservableCollection<NodeSearchElementViewModel> entries;
        private ObservableCollection<NodeCategoryViewModel> subCategories;
        private bool visibility;
        private bool isExpanded;

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
            get { return visibility; }
            set
            {
                if (value.Equals(visibility)) return;
                visibility = value;
                RaisePropertyChanged("Visibility");
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
            Name = name;
            Entries = new ObservableCollection<NodeSearchElementViewModel>(entries);
            SubCategories = new ObservableCollection<NodeCategoryViewModel>(subs);

            Entries.CollectionChanged += SubCategoriesOnCollectionChanged;
            SubCategories.CollectionChanged += SubCategoriesOnCollectionChanged;

            Items = new ObservableCollection<ISearchEntryViewModel>(
                Entries.Cast<ISearchEntryViewModel>().Concat(SubCategories)
                    .OrderBy(x => x.Name));

            Visibility = true;
            IsExpanded = false;
        }

        private void SubCategoriesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
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
            Items = new ObservableCollection<ISearchEntryViewModel>(
                Entries.Cast<ISearchEntryViewModel>().Concat(SubCategories)
                    .OrderBy(x => x.Name));
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
