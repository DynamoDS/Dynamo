using Dynamo.DSEngine;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

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
    ///     
    /// </summary>
    /// <typeparam name="TEntry"></typeparam>
    /// <typeparam name="TItem"></typeparam>
    public class SearchLibrary<TEntry, TItem> : LogSourceBase, ISource<TItem> 
        where TEntry : ISearchEntry, ISource<TItem>
    {
        private readonly SearchDictionary<TEntry> searchDictionary = new SearchDictionary<TEntry>();

        public SearchLibrary()
        {
            searchDictionary.EntryAdded += SearchDictionaryOnEntryAdded;
            searchDictionary.EntryRemoved += SearchDictionaryOnEntryRemoved;
        }

        private void SearchDictionaryOnEntryRemoved(TEntry tEntry)
        {
            tEntry.ItemProduced -= OnItemProduced;
        }

        private void SearchDictionaryOnEntryAdded(TEntry tEntry)
        {
            tEntry.ItemProduced += OnItemProduced;
        }
        
        public event Action<TItem> ItemProduced;
        protected virtual void OnItemProduced(TItem item)
        {
            var handler = ItemProduced;
            if (handler != null) handler(item);
        }

        public void AddToLibrary(TEntry entry)
        {
            searchDictionary.Add(entry, entry.SearchTags);
        }

        public bool RemoveFromLibrary(TEntry entry)
        {
            return searchDictionary.Remove(entry);
        }
    }

    /// <summary>
    /// TODO
    /// </summary>
    public class NodeSearchModel : SearchLibrary<NodeSearchElement, NodeModel> { }

    /// <summary>
    /// TODO
    /// </summary>
    public abstract class NodeSearchElement : ISearchEntry, ISource<NodeModel>
    {
        public IEnumerable<string> Categories { get; set; }

        public string FullCategoryName
        {
            get { return string.Join(".", Categories); }
        }

        public string Name { get; set; }
        public double Weight = 1;

        public ICollection<string> SearchKeywords
        {
            get { return keywords; }
        }

        private readonly HashSet<string> keywords = new HashSet<string>();

        public string Description { get; set; }

        public event Action<NodeModel> ItemProduced;

        protected virtual void OnItemProduced(NodeModel obj)
        {
            var handler = ItemProduced;
            if (handler != null) handler(obj);
        }

        private string PrependCategory(string name)
        {
            return string.Join(".", Categories.Concat(name.AsSingleton()));
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
                        .SelectMany(baseName => new[] { baseName, PrependCategory(baseName) })
                        .Concat(Description.AsSingleton())
                        .ToList();
            }
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
            Categories = typeLoadData.Category.Split('.');
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
            Categories = functionDescriptor.Category.Split('.');
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
        private readonly Guid id;

        public CustomNodeSearchElement(CustomNodeManager customNodeManager, CustomNodeInfo info)
        {
            this.customNodeManager = customNodeManager;
            id = info.Guid;
            Name = info.Name;
            Categories = info.Category.AsSingleton();
            Description = info.Description;
        }

        protected override NodeModel ConstructNewNodeModel()
        {
            return customNodeManager.CreateCustomNodeInstance(id);
        }
    }
}

