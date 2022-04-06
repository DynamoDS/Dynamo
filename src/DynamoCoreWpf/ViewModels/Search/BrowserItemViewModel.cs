﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using Dynamo.Configuration;
using Dynamo.Search;
using Dynamo.Search.SearchElements;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;
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

        private void ItemsOnCollectionChanged(object sender,
            NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (BrowserItem item in e.NewItems.OfType<BrowserItem>())
                    {
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
        ElementTypes ElementType { get; }
    }

    public class NodeCategoryViewModel : ViewModelBase, ISearchEntryViewModel 
    {
        public ICommand ClickedCommand { get; private set; }

        private string name;
        private string fullCategoryName;
        private string assembly;
        private bool visibility;
        private bool isExpanded;
        private bool isSelected;

        public event Action RequestReturnFocusToSearch;
        private void OnRequestReturnFocusToSearch()
        {
            if (RequestReturnFocusToSearch != null)
            {
                RequestReturnFocusToSearch();
            }
        }
        
        public event RequestBitmapSourceHandler RequestBitmapSource;
        public void OnRequestBitmapSource(IconRequestEventArgs e)
        {
            if (RequestBitmapSource != null)
            {
                RequestBitmapSource(e);
            }
        }
      
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

        public string FullCategoryName
        {
            get { return fullCategoryName; }
            set
            {
                if (value == fullCategoryName) return;
                fullCategoryName = value;
                RaisePropertyChanged("FullCategoryName");
            }
        }

        public string Assembly
        {
            get
            {
                if (string.IsNullOrEmpty(assembly))
                    return Configurations.DefaultAssembly;

                return assembly;
            }
            set
            {
                if (!string.IsNullOrEmpty(assembly)) return;
                assembly = value;
            }
        }


        private ElementTypes elementType;
        public ElementTypes ElementType
        {
            get
            {
                if (elementType == ElementTypes.None)
                    DetermineElementType();
                return elementType;
            }

            private set
            {
                if (value == elementType) return;
                elementType = value;
                RaisePropertyChanged("ElementType");
            }
        }

        private void DetermineElementType()
        {
            if (Items.Count == 0)
            {
                ElementType = ElementTypes.None;
                return;
            }

            // If at list one item is builtin, the whole category considers as builtin.
            if (Items.Any(item => item.ElementType.HasFlag(ElementTypes.BuiltIn)))
                ElementType = ElementTypes.BuiltIn;
            else
            {
                // If some items come from package, the whole category considers as package.
                if (Items.Any(item => item.ElementType.HasFlag(ElementTypes.Packaged)))
                    ElementType = ElementTypes.Packaged;
                else
                {
                    if (Items.Any(item => item.ElementType.HasFlag(ElementTypes.ZeroTouch)))
                        ElementType = ElementTypes.ZeroTouch;
                    else
                        if (Items.Any(item => item.ElementType.HasFlag(ElementTypes.CustomNode)))
                            ElementType = ElementTypes.CustomNode;
                }
            }
        }

        public ObservableCollection<ISearchEntryViewModel> Items { get; private set; }

        public ObservableCollection<NodeSearchElementViewModel> Entries { get; private set; }

        public ObservableCollection<NodeCategoryViewModel> SubCategories { get; private set; }

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

        public bool IsClassButton
        {
            get { return SubCategories.Count == 0; }
        }

        ///<summary>
        /// Small icon for class and method buttons.
        ///</summary>
        public ImageSource SmallIcon
        {
            get
            {
                ImageSource icon = GetIcon(Name + Configurations.SmallIconPostfix);

                // If there is no icon, use default.
                if (icon == null)
                    icon = LoadDefaultIcon();

                return icon;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Category name</param>
        public NodeCategoryViewModel(string name)
            : this(
                name,
                Enumerable.Empty<NodeSearchElementViewModel>(),
                Enumerable.Empty<NodeCategoryViewModel>()) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Category name</param>
        /// <param name="entries">Sub elements</param>
        /// <param name="subs">Sub Categories</param>
        public NodeCategoryViewModel(string name, IEnumerable<NodeSearchElementViewModel> entries, IEnumerable<NodeCategoryViewModel> subs)
        {
            ClickedCommand = new DelegateCommand(Expand);

            Name = name;
            Entries = entries != null? new ObservableCollection<NodeSearchElementViewModel>(entries.OrderBy(x => x.Name)) : new ObservableCollection<NodeSearchElementViewModel>();
            SubCategories = subs != null ? new ObservableCollection<NodeCategoryViewModel>(subs.OrderBy(x => x.Name)) : new ObservableCollection<NodeCategoryViewModel>();

            foreach (var category in SubCategories)
                category.PropertyChanged += CategoryOnPropertyChanged;

            Entries.CollectionChanged += OnCollectionChanged;
            SubCategories.CollectionChanged += OnCollectionChanged;
            SubCategories.CollectionChanged += SubCategoriesOnCollectionChanged;

            Items = new ObservableCollection<ISearchEntryViewModel>(
                SubCategories.Cast<ISearchEntryViewModel>().Concat(Entries));

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

        /// <summary>
        /// Dispose function
        /// </summary>
        public override void Dispose()
        {
            foreach (var category in SubCategories)
                category.PropertyChanged -= CategoryOnPropertyChanged;

            foreach (var item in Items)
                item.PropertyChanged -= ItemOnPropertyChanged;

            Entries.CollectionChanged -= OnCollectionChanged;
            SubCategories.CollectionChanged -= SubCategoriesOnCollectionChanged;
            Items.CollectionChanged -= ItemsOnCollectionChanged;
            base.Dispose();
        }

        /// <summary>
        /// Dispose the category and all the sub categories.
        /// note: does not seem to be called in Dynamo
        /// </summary>
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

            foreach (var ele in SubCategories.Where(cat => cat.IsExpanded))
            {
                ele.IsExpanded = false;
            }

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
            if (TreeViewItems != null)
            {                 
                //ClassDetails.IsExpanded = IsExpanded;
                TreeViewItems[0].IsExpanded = IsExpanded;
            }

            OnRequestReturnFocusToSearch();
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

            foreach (var item in Items)
                item.PropertyChanged -= ItemOnPropertyChanged;

            Items = new ObservableCollection<ISearchEntryViewModel>(
                Entries.Cast<ISearchEntryViewModel>().Concat(SubCategories)
                    .OrderBy(x => x.Name));

            Items.CollectionChanged += ItemsOnCollectionChanged;

            foreach (var item in Items)
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
                // Classes must be first in any case.
                if (entry is ClassesNodeCategoryViewModel)
                {
                    Items.Insert(0, entry);
                    continue;
                }

                var list = Items.Where(cat => !(cat is ClassesNodeCategoryViewModel));
                var nextLargerItemIndex = FindInsertionPointByName(list, entry.Name);

                // Node categories(i.e. namespaces) should be before members.
                if (entry is NodeSearchElementViewModel)
                {
                    if (nextLargerItemIndex >= 0)
                        Items.TryInsert(nextLargerItemIndex + SubCategories.Count, entry);
                    else
                        Items.Add(entry);
                }
                else if (entry is NodeCategoryViewModel)
                {
                    if (nextLargerItemIndex >= 0)
                    {
                        bool hasClasses = Items.FirstOrDefault() is ClassesNodeCategoryViewModel;

                        var offset = hasClasses ? 1 : 0;
                        Items.Insert(nextLargerItemIndex + offset, entry);
                    }
                    else
                        Items.Insert(Items.Count - Entries.Count, entry);
                }
            }
        }

        private ImageSource GetIcon(string fullNameOfIcon)
        {
            var iconRequest = new IconRequestEventArgs(Assembly, fullNameOfIcon);
            OnRequestBitmapSource(iconRequest);

            return iconRequest.Icon;
        }

        private ImageSource LoadDefaultIcon()
        {
            var iconRequest = new IconRequestEventArgs(Configurations.DefaultAssembly,
                Configurations.DefaultIcon);
            OnRequestBitmapSource(iconRequest);

            return iconRequest.Icon;
        }

        public void InsertSubCategory(NodeCategoryViewModel newSubCategory)
        {
            var list = SubCategories.Where(cat => !(cat is ClassesNodeCategoryViewModel));
            var nextLargerItemIndex = FindInsertionPointByName(list, newSubCategory.Name);

            if (nextLargerItemIndex >= 0)
            {
                bool hasClasses = SubCategories.FirstOrDefault() is ClassesNodeCategoryViewModel;
                var offset = hasClasses ? 1 : 0;
                SubCategories.Insert(nextLargerItemIndex + offset, newSubCategory);
            }
            else
                SubCategories.Add(newSubCategory);
        }

        internal static int FindInsertionPointByName(IEnumerable<ISearchEntryViewModel> list, string name)
        {
            var nextLargerItemIndex = -1;
            foreach (var item in list)
            {
                if (string.Compare(item.Name, name, StringComparison.Ordinal) >= 0)
                {
                    nextLargerItemIndex = list.ToList().IndexOf(item);
                    break;
                }
            }
            return nextLargerItemIndex;
        }

        private List<ClassInformationViewModel> treeViewIems;
        public List<ClassInformationViewModel> TreeViewItems
        {
            get
            {
                var ci = new ClassInformationViewModel();
                ci.IsRootCategoryDetails = false;
                ci.PopulateMemberCollections(this);
                treeViewIems = new List<ClassInformationViewModel>();
                treeViewIems.Add(ci);
                return treeViewIems;
            }            
        }

        protected ClassInformationViewModel classDetails;
        public ClassInformationViewModel ClassDetails
        {
            get
            {
                if (classDetails == null && IsClassButton)
                {
                    classDetails = new ClassInformationViewModel();
                    classDetails.IsRootCategoryDetails = false;
                    //classDetails.PopulateMemberCollections(this);
                    //if (listDetails == null && listDetails.Contains(classDetails))
                    //    listDetails = new List<ClassInformationViewModel>();
                    //listDetails.Add(classDetails);
                }

                return classDetails;
            }
        }       
    }

    public class RootNodeCategoryViewModel : NodeCategoryViewModel
    {      
        public RootNodeCategoryViewModel(string name) : base(name) { }

        public RootNodeCategoryViewModel(
            string name, IEnumerable<NodeSearchElementViewModel> entries,
            IEnumerable<NodeCategoryViewModel> subs)
            : base(name, entries, subs)
        { }
    }

    public class ClassesNodeCategoryViewModel : NodeCategoryViewModel
    {
        public NodeCategoryViewModel Parent { get; private set; }

        public ClassesNodeCategoryViewModel(NodeCategoryViewModel parent)
            : base(Configurations.ClassesDefaultName)
        {
            FullCategoryName = Configurations.ClassesDefaultName;
            Parent = parent;
        }
    }
}
