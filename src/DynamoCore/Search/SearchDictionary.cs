using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Logging;
using Dynamo.Utilities;

namespace Dynamo.Search
{
    /// The search API in this class has been replaced with the Lucene search.

    /// <summary>
    ///     A dictionary of objects.
    /// </summary>
    public class SearchDictionary<V>
    {
        private ILogger logger;

        /// <summary>
        ///     Construct a SearchDictionary object
        /// </summary>
        /// <param name="logger"> (Optional) A logger to use to log search data</param>
        internal SearchDictionary(ILogger logger = null)
        {
            this.logger = logger;
        }

        /// <summary>
        ///     Dictionary of searchElement:(tag, weight)
        /// </summary>
        protected readonly Dictionary<V, Dictionary<string, double>> entryDictionary =
            new Dictionary<V, Dictionary<string, double>>();

        /// <summary>
        /// Dictionary of tag:(list(searchelement, weight)) which contains all nodes that share a tag 
        /// </summary>
        private List<IGrouping<string, Tuple<V, double>>> tagDictionary;        

        /// <summary>
        ///     All the current entries in search.
        /// </summary>
        public IEnumerable<V> SearchEntries
        {
            get { return entryDictionary.Keys; }
        }

        /// <summary>
        ///     The number of tags in the dictionary
        /// </summary>
        public int NumTags
        {
            get { return entryDictionary.Values.SelectMany(x => x.Keys).Count(); }
        }

        /// <summary>
        ///     The number of elements in the dictionary
        /// </summary>
        public int NumElements
        {
            get { return entryDictionary.Count; }
        }

        /// <summary>
        /// Event is fired when an entry is added.
        /// </summary>
        public event Action<V> EntryAdded;

        protected virtual void OnEntryAdded(V entry)
        {
            var handler = EntryAdded;
            if (handler != null) handler(entry);
            tagDictionary = null;
        }

        /// <summary>
        /// Event is fired when an entry is removed.
        /// </summary>
        public event Action<V> EntryRemoved;

        protected virtual void OnEntryRemoved(V entry)
        {
            var handler = EntryRemoved;
            if (handler != null) handler(entry);
            tagDictionary = null;
        }

        /// <summary>
        /// Event is fired when an entry is updated.
        /// </summary>
        public event Action<V> EntryUpdated;

        protected virtual void OnEntryUpdated(V entry)
        {
            var handler = EntryUpdated;
            if (handler != null) handler(entry);
            tagDictionary = null;
        }

        /// <summary>
        ///     Add a single element with a single tag
        /// </summary>
        /// <param name="value"> The object to add  </param>
        /// <param name="tag"> The string to identify it in search </param>
        /// <param name="weight"></param>
        internal void Add(V value, string tag, double weight = 1)
        {
            Add(value, tag.AsSingleton(), weight.AsSingleton());
        }

        /// <summary>
        ///     Add a list of elements with a single tag
        /// </summary>
        /// <param name="values"> List of objects to add  </param>
        /// <param name="tag"> The string to identify it in search </param>
        /// <param name="weight"></param>
        internal void Add(IEnumerable<V> values, string tag, double weight = 1)
        {
            foreach (var value in values)
                Add(value, tag, weight);
        }

        /// <summary>
        ///     Add a single element with a number of tags
        /// </summary>
        /// <param name="value"> The object to add  </param>
        /// <param name="tags"> The list of strings to identify it in search </param>
        /// <param name="weights">The list of corresponding weights coefficients</param>
        internal void Add(V value, IEnumerable<string> tags, IEnumerable<double> weights)
        {
            Dictionary<string, double> keys;
            if (!entryDictionary.TryGetValue(value, out keys))
            {
                keys = new Dictionary<string, double>();
                entryDictionary[value] = keys;
                OnEntryAdded(value);
            }

            int tagsCount = tags.Count();
            if (tagsCount != weights.Count())
                throw new ArgumentException("Number of weights should equal number of search tags.");

            for (int i = 0; i < tagsCount; i++)
            {
                var tag = tags.ElementAt(i).ToLower();
                keys[tag] = weights.ElementAt(i);
            }
        }

        /// <summary>
        ///     Add a coordinated list of objects and strings.
        /// </summary>
        /// <param name="values"> The objects to add. Must have the same cardinality as the second parameter</param>
        /// <param name="tags"> The list of strings to identify it in search. Must have the same cardinality as the first parameter </param>
        /// <param name="weight"></param>
        internal void Add(IEnumerable<V> values, IEnumerable<string> tags, double weight = 1)
        {
            var tagList = tags as IList<string> ?? tags.ToList();
            foreach (var value in values)
                Add(value, tagList, weight.AsSingleton());
        }

        /// <summary>
        ///     Remove an element from the search
        /// </summary>
        /// <param name="value"> The object to remove </param>
        /// <param name="tag">The tag to remove for the given value </param>
        internal bool Remove(V value, string tag)
        {
            Dictionary<string, double> keys;
            return entryDictionary.TryGetValue(value, out keys) && keys.Remove(tag)
                && (keys.Any() || Remove(value));
        }

        /// <summary>
        ///     Remove an element from the search
        /// </summary>
        /// <param name="value"> The object to remove </param>
        internal bool Remove(V value)
        {
            if (!entryDictionary.Remove(value))
                return false;
            OnEntryRemoved(value);
            return true;
        }

        /// <summary>
        ///     Remove elements from search based on a predicate
        /// </summary>
        /// <param name="removeCondition"> The predicate with which to test.  True results in removal. </param>
        internal int Remove(Func<V, bool> removeCondition)
        {
            var removals = entryDictionary.Keys.Where(removeCondition).ToList();
            foreach (var ele in removals)
                Remove(ele);
            return removals.Count;
        }

        /// <summary>
        ///     Removes elements from search, based on separate predicates for values and tags.
        /// </summary>
        /// <param name="valueCondition"></param>
        /// <param name="removeTagCondition"></param>
        /// <returns></returns>
        internal int Remove(Func<V, bool> valueCondition, Func<string, bool> removeTagCondition)
        {
            var count = 0;
            var removals = entryDictionary.Where(kv => valueCondition(kv.Key)).ToList();
            foreach (var removal in removals)
            {
                var tagRemovals = removal.Value.Keys.Where(removeTagCondition).ToList();
                foreach (var tagRemoval in tagRemovals)
                {
                    removal.Value.Remove(tagRemoval);
                    count++;
                }
                if (!removal.Value.Any())
                    Remove(removal.Key);
            }
            return count;
        }

        /// <summary>
        ///     Remove elements from search
        /// </summary>
        /// <param name="value"> The object to remove </param>
        /// <param name="tags"> The list of tags to remove. </param>
        internal int Remove(V value, IEnumerable<string> tags)
        {
            Dictionary<string, double> keys;
            if (!entryDictionary.TryGetValue(value, out keys))
                return 0;

            var count = tags.Count(tag => keys.Remove(tag));
            if (!keys.Any())
                Remove(value);
            return count;
        }

        /// <summary>
        ///     Returns the elements with a given tag
        /// </summary>
        /// <param name="tag"> The tag to match </param>
        /// <returns> The elements with the given tag </returns>
        internal IEnumerable<V> ByTag(string tag)
        {
            return entryDictionary.Where(kv => kv.Value.ContainsKey(tag)).Select(kv => kv.Key);
        }

        /// <summary>
        ///     Determines if this SearchDictionary contains a specific element.
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        internal bool Contains(V a)
        {
            return entryDictionary.Keys.Any(x => Equals(x, a));
        }

        #region Manual Searching
        #endregion
    }
}
