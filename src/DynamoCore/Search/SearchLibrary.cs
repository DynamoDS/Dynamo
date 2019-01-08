using Dynamo.Utilities;
using Dynamo.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

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
        ///     Construct a SearchLibrary object
        /// </summary>
        /// <param name="logger"> (Optional) A logger to pass through to SearchDictionary for logging search data</param>
        internal SearchLibrary(ILogger logger = null) : base(logger)
        {
        }

        /// <summary>
        ///     Adds an entry to search.
        /// </summary>
        /// <param name="entry">search element</param>
        internal virtual void Add(TEntry entry)
        {
            Add(entry, entry.Name);
            Add(entry, entry.SearchTags, entry.SearchTagWeights);
            Add(entry, entry.Description, .1);
        }

        /// <summary>
        ///     Updates an entry in search.
        /// </summary>
        /// <param name="entry">search element</param>
        /// <param name="isCategoryChanged">true, if entry changed its category</param>
        internal void Update(TEntry entry, bool isCategoryChanged = false)
        {
            // If entry's category is changed, we need to delete this entry from search.
            // And add it to new category.
            if (isCategoryChanged)
                Remove(entry);

            Dictionary<string, double> keys;
            if (entryDictionary.TryGetValue(entry, out keys)) // Found the entry to update.
            {
                // Remove old tags.
                keys.Clear();
                keys.Add(entry.Name.ToLower(), 1);
                keys.Add(entry.Description.ToLower(), 0.1);

                foreach (var tag in entry.SearchTags.Select(x => x.ToLower()))
                {
                    keys.Add(tag, 0.5);
                }

                OnEntryUpdated(entry);
                return; // Entry updated.
            }

            // Entry could not be found, add it into the search collection.
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
    public static class SearchCategoryUtil
    {
        private sealed class SearchCategoryImpl<TEntry> : ISearchCategory<TEntry>
        {
            private SearchCategoryImpl(string name, IEnumerable<TEntry> entries, IEnumerable<SearchCategoryImpl<TEntry>> subCategories)
            {
                SubCategories = subCategories;
                Entries = entries;
                Name = name;
            }

            internal static SearchCategoryImpl<TEntry> Create(string categoryName, IEnumerable<SearchCategoryImpl<TEntry>> subCategories, IEnumerable<TEntry> entries)
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
        internal static ISearchCategory<TEntry> CategorizeSearchEntries<TEntry>(
            IEnumerable<TEntry> entries, Func<TEntry, ICollection<string>> categorySelector)
        {
            return entries.GroupByRecursive<TEntry, string, SearchCategoryImpl<TEntry>>(
                categorySelector,
                SearchCategoryImpl<TEntry>.Create,
                "Root");
        }


        /// <summary>
        /// Returns all nested categories from a sequence of search entries.
        /// </summary>
        /// <typeparam name="TEntry"></typeparam>
        /// <param name="category"></param>
        /// <returns></returns>
        internal static IEnumerable<string> GetAllCategoryNames<TEntry>(this ISearchCategory<TEntry> category)
        {
            yield return category.Name;
            foreach (var name in category.SubCategories.SelectMany(GetAllCategoryNames))
                yield return string.Format("{0}.{1}", category.Name, name);
        }
    }
}
