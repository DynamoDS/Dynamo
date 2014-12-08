using System.ComponentModel;
using System.Xml;

using Dynamo.Annotations;
using Dynamo.DSEngine;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

using DynamoUtilities;

namespace Dynamo.Search
{
    /// <summary>
    ///     Has a collection of strings that can be used to identifiy the instance
    ///     in a search.
    /// </summary>
    public interface ISearchEntry
    {
        ICollection<string> SearchTags { get; }
    }

    /// <summary>
    ///     Searchable library of item sources.
    /// </summary>
    /// <typeparam name="TEntry">Type of searchable elements.</typeparam>
    /// <typeparam name="TItem">Type of items produced by searchable elements.</typeparam>
    public class SearchLibrary<TEntry, TItem> : SearchDictionary<TEntry>, ISource<TItem> 
        where TEntry : ISearchEntry, ISource<TItem>
    {
        protected override void OnEntryRemoved(TEntry entry)
        {
            base.OnEntryRemoved(entry);
            entry.ItemProduced -= OnItemProduced;
        }

        protected override void OnEntryAdded(TEntry entry)
        {
            base.OnEntryAdded(entry);
            entry.ItemProduced += OnItemProduced;
        }
        
        /// <summary>
        ///     Produces an item whenever a search element produces an item.
        /// </summary>
        public event Action<TItem> ItemProduced;
        protected virtual void OnItemProduced(TItem item)
        {
            var handler = ItemProduced;
            if (handler != null) handler(item);
        }
    }

    /// <summary>
    ///     Searchable library of NodeSearchElements that can produce NodeModels.
    /// </summary>
    public class NodeSearchModel : CategorizedSearchModel<NodeSearchElement, NodeModel>
    {
        public void Add(NodeSearchElement entry)
        {
            Library.Add(entry, entry.SearchKeywords);
        }

        public void Remove(NodeSearchElement entry)
        {
            Library.Remove(entry);
        }

        //TODO(Steve): Search() method

        protected override ICollection<string> GetCategories(NodeSearchElement entry)
        {
            return entry.Categories;
        }

        #region Xml Dump
        public void DumpLibraryToXml(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return;

            var document = ComposeXmlForLibrary();
            document.Save(fileName);
        }

        public XmlDocument ComposeXmlForLibrary()
        {
            var document = XmlHelper.CreateDocument("LibraryTree");

            foreach (var category in Root.SubCategories)
                AddCategoryToXml(document.DocumentElement, category);

            foreach (var entry in Root.Entries)
                AddEntryToXml(document.DocumentElement, entry);

            return document;
        }

        private static void AddEntryToXml(XmlNode parent, NodeSearchElement entry)
        {
            var element = XmlHelper.AddNode(parent, entry.GetType().ToString());
            XmlHelper.AddNode(element, "FullCategoryName", entry.FullCategoryName);
            XmlHelper.AddNode(element, "Name", entry.Name);
            XmlHelper.AddNode(element, "Description", entry.Description);
        }

        private static void AddCategoryToXml(
            XmlNode parent, ISearchCategory<NodeSearchElement> category)
        {
            var element = XmlHelper.AddNode(parent, category.GetType().ToString());
            XmlHelper.AddAttribute(element, "Name", category.Name);

            foreach (var subCategory in category.SubCategories)
                AddCategoryToXml(element, subCategory);

            foreach (var entry in category.Entries)
                AddEntryToXml(element, entry);
        }
        #endregion
    }

    /// <summary>
    /// TODO
    /// </summary>
    /// <typeparam name="TEntry"></typeparam>
    public interface ISearchCategory<out TEntry>
    {
        string Name { get; }
        IEnumerable<TEntry> Entries { get; }
        IEnumerable<ISearchCategory<TEntry>> SubCategories { get; }
    }
    
    /// <summary>
    /// TODO
    /// </summary>
    /// <typeparam name="TEntry"></typeparam>
    /// <typeparam name="TItem"></typeparam>
    public abstract class CategorizedSearchModel<TEntry, TItem>  : ISource<TItem>
        where TEntry : ISearchEntry, ISource<TItem>
    {
        private sealed class SearchCategory : ISearchCategory<TEntry>
        {
            private SearchCategory(string name, IEnumerable<TEntry> entries, IEnumerable<SearchCategory> subCategories)
            {
                SubCategories = subCategories;
                Entries = entries;
                Name = name;
            }

            public static SearchCategory Create(string categoryName, IEnumerable<SearchCategory> subCategories, IEnumerable<TEntry> entries)
            {
                return new SearchCategory(categoryName, entries, subCategories);
            }

            public string Name { get; private set; }
            public IEnumerable<TEntry> Entries { get; private set; }
            public IEnumerable<ISearchCategory<TEntry>> SubCategories { get; private set; }
        }

        protected readonly SearchLibrary<TEntry, TItem> Library;
        
        public ISearchCategory<TEntry> Root
        {
            get
            {
                return Library.SearchEntries.GroupByRecursive<TEntry, string, SearchCategory>(
                    GetCategories,
                    SearchCategory.Create,
                    "Root");
            }
        }

        protected abstract ICollection<string> GetCategories(TEntry entry);

        protected CategorizedSearchModel()
        {
            Library = new SearchLibrary<TEntry, TItem>();
            Library.ItemProduced += OnItemProduced;
        }

        public event Action<TItem> ItemProduced;
        protected virtual void OnItemProduced(TItem obj)
        {
            var handler = ItemProduced;
            if (handler != null) handler(obj);
        }
    }

    /// <summary>
    /// TODO
    /// </summary>
    public abstract class NodeSearchElement : INotifyPropertyChanged, ISearchEntry, ISource<NodeModel>
    {
        private readonly HashSet<string> keywords = new HashSet<string>();
        private string fullCategoryName;
        private string description;
        private string name;

        public ICollection<string> Categories
        {
            get { return FullCategoryName.Split('.'); }
        }

        public string FullCategoryName
        {
            get { return fullCategoryName; }
            set
            {
                if (value == fullCategoryName) return;
                fullCategoryName = value;
                OnPropertyChanged("FullCategoryName");
                OnPropertyChanged("Categories");
            }
        }

        public string Name
        {
            get { return name; }
            set
            {
                if (value == name) return;
                name = value;
                OnPropertyChanged("Name");
            }
        }

        public double Weight = 1;

        public ICollection<string> SearchKeywords
        {
            get { return keywords; }
        }

        public string Description
        {
            get { return description; }
            set
            {
                if (value == description) return;
                description = value;
                OnPropertyChanged("Description");
            }
        }

        public event Action<NodeModel> ItemProduced;
        protected virtual void OnItemProduced(NodeModel obj)
        {
            var handler = ItemProduced;
            if (handler != null) handler(obj);
        }
        
        protected abstract NodeModel ConstructNewNodeModel();

        public void ProduceNode()
        {
            OnItemProduced(ConstructNewNodeModel());
        }

        ICollection<string> ISearchEntry.SearchTags
        {
            get
            {
                return
                    SearchKeywords.Concat(Name.AsSingleton())
                        .SelectMany(
                            baseName => new[] { baseName, FullCategoryName + "." + baseName })
                        .Concat(Description.AsSingleton())
                        .ToList();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// TODO
    /// </summary>
    public class NodeModelSearchElement : NodeSearchElement
    {
        private readonly Func<NodeModel> constructor; 

        public NodeModelSearchElement(TypeLoadData typeLoadData)
        {
            Name = typeLoadData.Name;
            foreach (var aka in typeLoadData.AlsoKnownAs.Concat(typeLoadData.SearchKeys))
                SearchKeywords.Add(aka);
            FullCategoryName = typeLoadData.Category;
            Description = typeLoadData.Description;

            constructor = typeLoadData.Type.GetDefaultConstructor<NodeModel>();
        }

        protected override NodeModel ConstructNewNodeModel()
        {
            return constructor();
        }
    }

    /// <summary>
    /// TODO
    /// </summary>
    public class ZeroTouchSearchElement : NodeSearchElement
    {
        private readonly FunctionDescriptor functionDescriptor;

        public ZeroTouchSearchElement(FunctionDescriptor functionDescriptor)
        {
            this.functionDescriptor = functionDescriptor;

            var displayName = functionDescriptor.UserFriendlyName;
            if (functionDescriptor.IsOverloaded)
                displayName += "(" + string.Join(", ", functionDescriptor.Parameters) + ")";

            Name = displayName;
            FullCategoryName = functionDescriptor.Category;
            Description = functionDescriptor.Description;
            foreach (var tag in functionDescriptor.GetSearchTags())
                SearchKeywords.Add(tag);
        }

        protected override NodeModel ConstructNewNodeModel()
        {
            if (functionDescriptor.IsVarArg)
                return new DSVarArgFunction(functionDescriptor);
            return new DSFunction(functionDescriptor);
        }
    }

    /// <summary>
    /// TODO
    /// </summary>
    public class CustomNodeSearchElement : NodeSearchElement
    {
        private readonly CustomNodeManager customNodeManager;
        private Guid id;

        public CustomNodeSearchElement(CustomNodeManager customNodeManager, CustomNodeInfo info)
        {
            this.customNodeManager = customNodeManager;
            SyncWithCustomNodeInfo(info);
        }

        public void SyncWithCustomNodeInfo(CustomNodeInfo info)
        {
            id = info.FunctionId;
            Name = info.Name;
            FullCategoryName = info.Category;
            Description = info.Description;
        }

        protected override NodeModel ConstructNewNodeModel()
        {
            return customNodeManager.CreateCustomNodeInstance(id);
        }
    }
}
