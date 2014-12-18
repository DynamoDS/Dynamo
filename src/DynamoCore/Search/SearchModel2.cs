﻿using System.ComponentModel;
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

using ProtoCore.Compiler.Associative;

namespace Dynamo.Search
{
    /// <summary>
    ///     Has a collection of strings that can be used to identifiy the instance
    ///     in a search.
    /// </summary>
    public interface ISearchEntry
    {
        string Name { get; }
        ICollection<string> SearchTags { get; }
        string Description { get; }
    }

    /// <summary>
    ///     Searchable library of item sources.
    /// </summary>
    /// <typeparam name="TEntry">Type of searchable elements.</typeparam>
    /// <typeparam name="TItem">Type of items produced by searchable elements.</typeparam>
    public class SearchLibrary<TEntry, TItem> : SearchDictionary<TEntry>, ISource<TItem> 
        where TEntry : ISearchEntry, ISource<TItem>
    {
        public void Add(TEntry entry)
        {
            Add(entry, entry.Name);
            Add(entry, entry.SearchTags, .5);
            Add(entry, entry.Description, .1);
        }

        public void Update(TEntry entry)
        {
            Remove(entry);
            Add(entry);
        }

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
    public class NodeSearchModel : SearchLibrary<NodeSearchElement, NodeModel>
    {
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

            var root = SearchCategory.CategorizeSearchEntries(
                SearchEntries,
                entry => entry.Categories);

            foreach (var category in root.SubCategories)
                AddCategoryToXml(document.DocumentElement, category);

            foreach (var entry in root.Entries)
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
            var element = XmlHelper.AddNode(parent, "Category");
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

    public static class SearchCategory
    {
        private sealed class SearchCategoryImpl<TEntry> : ISearchCategory<TEntry>
        {
            private SearchCategoryImpl(string name, IEnumerable<TEntry> entries, IEnumerable<SearchCategoryImpl<TEntry>> subCategories)
            {
                SubCategories = subCategories;
                Entries = entries;
                Name = name;
            }

            public static SearchCategoryImpl<TEntry> Create(string categoryName, IEnumerable<SearchCategoryImpl<TEntry>> subCategories, IEnumerable<TEntry> entries)
            {
                return new SearchCategoryImpl<TEntry>(categoryName, entries, subCategories);
            }

            public string Name { get; private set; }
            public IEnumerable<TEntry> Entries { get; private set; }
            public IEnumerable<ISearchCategory<TEntry>> SubCategories { get; private set; }
        }

        public static ISearchCategory<TEntry> CategorizeSearchEntries<TEntry>(
            IEnumerable<TEntry> entries, Func<TEntry, ICollection<string>> categorySelector)
        {
            return entries.GroupByRecursive<TEntry, string, SearchCategoryImpl<TEntry>>(
                categorySelector,
                SearchCategoryImpl<TEntry>.Create,
                "Root");
        }

        public static IEnumerable<string> GetAllCategoryNames<TEntry>(this ISearchCategory<TEntry> category)
        {
            yield return category.Name;
            foreach (var name in category.SubCategories.SelectMany(GetAllCategoryNames))
                yield return string.Format("{0}.{1}", category.Name, name);
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
        private bool isVisibleInSearch = true;

        //TODO(Steve): This probably should exist only in the ViewModel
        public bool IsVisibleInSearch
        {
            get { return isVisibleInSearch; }
            set
            {
                if (value.Equals(isVisibleInSearch)) return;
                isVisibleInSearch = value;
                OnPropertyChanged("IsVisibleInSearch");
            }
        }

        public ICollection<string> Categories
        {
            get { return SplitCategoryName(FullCategoryName).ToList(); }
        }

        public const char CATEGORY_DELIMITER = '.';

        /// <summary>
        /// Split a category name into individual category names splitting be DEFAULT_DELIMITER
        /// </summary>
        /// <param name="categoryName">The name</param>
        /// <returns>A list of output</returns>
        public static IEnumerable<string> SplitCategoryName(string categoryName)
        {
            if (String.IsNullOrEmpty(categoryName))
                return Enumerable.Empty<string>();

            return
                categoryName.Split(CATEGORY_DELIMITER)
                    .Where(x => x != CATEGORY_DELIMITER.ToString() && !String.IsNullOrEmpty(x));
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

        string ISearchEntry.Name 
        {
            get { return FullCategoryName + "." + Name; }
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
                return SearchKeywords.ToList();
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
    public class NodeModelSearchElement : NodeModelSearchElementBase
    {
        private readonly Func<NodeModel> constructor; 

        public NodeModelSearchElement(TypeLoadData typeLoadData) : base(typeLoadData)
        {
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
    public abstract class NodeModelSearchElementBase : NodeSearchElement
    {
        protected NodeModelSearchElementBase(TypeLoadData typeLoadData)
        {
            Name = typeLoadData.Name;
            foreach (var aka in typeLoadData.AlsoKnownAs.Concat(typeLoadData.SearchKeys))
                SearchKeywords.Add(aka);
            FullCategoryName = typeLoadData.Category;
            Description = typeLoadData.Description;
        }
    }

    /// <summary>
    /// TODO
    /// </summary>
    public class CodeBlockNodeSearchElement : NodeModelSearchElementBase
    {
        private readonly LibraryServices libraryServices;

        public CodeBlockNodeSearchElement(TypeLoadData data, LibraryServices manager)
            : base(data)
        {
            libraryServices = manager;
        }

        protected override NodeModel ConstructNewNodeModel()
        {
            return new CodeBlockNodeModel(libraryServices);
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
        private readonly ICustomNodeSource customNodeManager;
        public Guid ID { get; private set; }
        private string path;

        public string Path
        {
            get { return path; }
            private set
            {
                if (value == path) return;
                path = value;
                OnPropertyChanged("Path");
            }
        }

        public CustomNodeSearchElement(ICustomNodeSource customNodeManager, CustomNodeInfo info)
        {
            this.customNodeManager = customNodeManager;
            SyncWithCustomNodeInfo(info);
        }

        public void SyncWithCustomNodeInfo(CustomNodeInfo info)
        {
            ID = info.FunctionId;
            Name = info.Name;
            FullCategoryName = info.Category;
            Description = info.Description;
            Path = info.Path;
        }

        protected override NodeModel ConstructNewNodeModel()
        {
            return customNodeManager.CreateCustomNodeInstance(ID);
        }
    }
}
