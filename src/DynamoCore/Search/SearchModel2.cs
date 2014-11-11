using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.DSEngine;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;

namespace Dynamo.Search
{
    public static class JaroWinkler
    {
        public static double StringDistance(string firstWord, string secondWord)
        {
            const double prefixAdustmentScale = 0.1;

            if ((firstWord != null) && (secondWord != null))
            {
                double dist = GetJaroDistance(firstWord, secondWord);
                int prefixLength = GetPrefixLength(firstWord, secondWord);
                return dist + prefixLength * prefixAdustmentScale * (1.0 - dist);
            }
            return 0.0;
        }

        private static double GetJaroDistance(string firstWord, string secondWord)
        {
            const double defaultMismatchScore = 0;

            if ((firstWord != null) && (secondWord != null))
            {
                //get half the length of the string rounded up 
                //(this is the distance used for acceptable transpositions)
                int halflen = Math.Min(firstWord.Length, secondWord.Length) / 2 + 1;

                //get common characters
                StringBuilder common1 = GetCommonCharacters(firstWord, secondWord, halflen);
                int commonMatches = common1.Length;

                //check for zero in common
                if (commonMatches == 0)
                {
                    return defaultMismatchScore;
                }

                StringBuilder common2 = GetCommonCharacters(secondWord, firstWord, halflen);

                //check for same length common strings returning 0.0f is not the same
                if (commonMatches != common2.Length)
                {
                    return defaultMismatchScore;
                }

                //get the number of transpositions
                int transpositions = 0;
                for (int i = 0; i < commonMatches; i++)
                {
                    if (common1[i] != common2[i])
                    {
                        transpositions++;
                    }
                }

                //calculate jaro metric
                transpositions /= 2;
                double tmp1 = commonMatches / (3.0 * firstWord.Length)
                    + commonMatches / (3.0 * secondWord.Length)
                    + (commonMatches - transpositions) / (3.0 * commonMatches);
                return tmp1;
            }
            return defaultMismatchScore;
        }

        private static StringBuilder GetCommonCharacters(string firstWord, string secondWord, int distanceSep)
        {
            if ((firstWord != null) && (secondWord != null))
            {
                var returnCommons = new StringBuilder();
                var copy = new StringBuilder(secondWord);
                for (int i = 0; i < firstWord.Length; i++)
                {
                    char ch = firstWord[i];
                    bool foundIt = false;
                    for (int j = Math.Max(0, i - distanceSep);
                         !foundIt && j < Math.Min(i + distanceSep, secondWord.Length);
                         j++)
                    {
                        if (copy[j] == ch)
                        {
                            foundIt = true;
                            returnCommons.Append(ch);
                            copy[j] = '#';
                        }
                    }
                }

                return returnCommons;
            }
            return null;
        }

        private static int GetPrefixLength(string firstWord, string secondWord)
        {
            const int minPrefixTestLength = 4;

            if ((firstWord != null) && (secondWord != null))
            {
                int n =
                    Math.Min(
                        minPrefixTestLength,
                        Math.Min(firstWord.Length, secondWord.Length));

                for (int i = 0; i < n; i++)
                {
                    if (firstWord[i] != secondWord[i])
                        return i;
                }

                return n;
            }
            return minPrefixTestLength;
        }
    }

    public interface ISource<out TItem>
    {
        event Action<TItem> ItemProduced;
    }

    public struct SearchResult<TItem>
    {
        public ISearchEntry<TItem> Result;
        public double SearchDistance;
    }

    public interface ISearchEntry<TItem>
    {
        IEnumerable<SearchResult<TItem>> Search(string query);
    }
    
    public abstract class SearchCategory<TEntry, TCategoryKey, TCategory, TItem> : ISearchEntry<TItem>
        where TEntry : ISearchEntry<TItem>
        where TCategory : SearchCategory<TEntry, TCategoryKey, TCategory, TItem>, new()
    {
        private readonly Dictionary<TCategoryKey, TCategory> categories =
            new Dictionary<TCategoryKey, TCategory>();
        private readonly List<TEntry> entries = new List<TEntry>();

        public IEnumerable<ISearchEntry<TItem>> Entries
        {
            get
            {
                return
                    Enumerable.Concat(
                        categories.Values,
                        entries.Cast<ISearchEntry<TItem>>());
            }
        }

        public void AddToSearch(
            TEntry entry, Stack<TCategoryKey> categoryKeys)
        {
            if (!categoryKeys.Any())
                entries.Add(entry);
            else
            {
                var key = categoryKeys.Pop();
                TCategory category;
                if (!categories.TryGetValue(key, out category))
                {
                    category = new TCategory();
                    categories[key] = category;
                }
                category.AddToSearch(entry, categoryKeys);
            }
        }

        public abstract IEnumerable<SearchResult<TItem>> Search(string query);
    }

    public abstract class SearchModel2<TEntry, TCategoryKey, TCategory, TItem> 
        : LogSourceBase, ISource<TItem>
        where TEntry : ISearchEntry<TItem>, ISource<TItem> 
        where TCategory : SearchCategory<TEntry, TCategoryKey, TCategory, TItem>, new()
    {
        private readonly TCategory rootCategory = new TCategory();

        protected abstract IEnumerable<TCategoryKey> GetCategoryKeysForEntry(TEntry entry);

        public virtual void AddToSearch(TEntry entry)
        {
            var keys = new Stack<TCategoryKey>(GetCategoryKeysForEntry(entry));
            rootCategory.AddToSearch(entry, keys);
        }

        public event Action<TItem> ItemProduced;
        protected virtual void OnItemProduced(TItem obj)
        {
            var handler = ItemProduced;
            if (handler != null) handler(obj);
        }

        public IEnumerable<ISearchEntry<TItem>> Search(string query)
        {
            return rootCategory.Search(query)
                .OrderByDescending(x => x.SearchDistance)
                .Select(x => x.Result);
        }
    }

    public class NodeSearchModel : SearchModel2<NodeSearchElement, string, NodeSearchCategory, NodeModel>
    {
        public override void AddToSearch(NodeSearchElement entry)
        {
            base.AddToSearch(entry);
            entry.ItemProduced += OnItemProduced;
        }

        protected override IEnumerable<string> GetCategoryKeysForEntry(NodeSearchElement entry)
        {
            return entry.Categories;
        }
    }

    public abstract class NodeSearchElement : ISearchEntry<NodeModel>, ISource<NodeModel>
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

        public IEnumerable<SearchResult<NodeModel>> Search(string query)
        {
            return
                SearchKeywords.Concat(Name.AsSingleton())
                    .SelectMany(baseName => new[] { baseName, PrependCategory(baseName) })
                    .Concat(Description.AsSingleton())
                    .Select(
                        word =>
                            new SearchResult<NodeModel>
                            {
                                Result = this,
                                SearchDistance = JaroWinkler.StringDistance(word, query)
                            });
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
    }

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

    public class NodeSearchCategory : SearchCategory<NodeSearchElement, string, NodeSearchCategory, NodeModel>
    {
        public override IEnumerable<SearchResult<NodeModel>> Search(string query)
        {
            return Entries.SelectMany(e => e.Search(query));
        }
    }
}

