using Dynamo.Interfaces;
using Dynamo.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Search.Interfaces;

namespace Dynamo.Search
{
    /// <summary>
    ///     Searchable library of item sources.
    /// </summary>
    /// <typeparam name="TEntry">Type of searchable elements.</typeparam>
    /// <typeparam name="TItem">Type of items produced by searchable elements.</typeparam>
    public class SearchLibrary<TEntry, TItem> : SearchDictionary<TEntry>, ISource<TItem> 
        where TEntry : ISearchEntry, ISource<TItem>
    {
        /// <summary>
        ///     Adds an entry to search.
        /// </summary>
        /// <param name="entry"></param>
        public void Add(TEntry entry)
        {
            Add(entry, entry.Name);
            Add(entry, entry.SearchTags, .5);
            Add(entry, entry.Description, .1);
        }

        /// <summary>
        ///     Updates an entry in search.
        /// </summary>
        /// <param name="entry"></param>
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
    ///     Utility methods for categorizing search elements.
    /// </summary>
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

        /// <summary>
        ///     Categorizes a sequence search entries.
        /// </summary>
        /// <typeparam name="TEntry"></typeparam>
        /// <param name="entries"></param>
        /// <param name="categorySelector"></param>
        /// <returns></returns>
        public static ISearchCategory<TEntry> CategorizeSearchEntries<TEntry>(
            IEnumerable<TEntry> entries, Func<TEntry, ICollection<string>> categorySelector)
        {
            return entries.GroupByRecursive<TEntry, string, SearchCategoryImpl<TEntry>>(
                categorySelector,
                SearchCategoryImpl<TEntry>.Create,
                "Root");
        }


        /// <summary>
        ///     Gets all nested categories from a sequence of search entries.
        /// </summary>
        /// <typeparam name="TEntry"></typeparam>
        /// <param name="category"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetAllCategoryNames<TEntry>(this ISearchCategory<TEntry> category)
        {
            yield return category.Name;
            foreach (var name in category.SubCategories.SelectMany(GetAllCategoryNames))
                yield return string.Format("{0}.{1}", category.Name, name);
        }
    }
}
