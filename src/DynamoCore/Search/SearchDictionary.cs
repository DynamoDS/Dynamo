using Dynamo.Utilities;
using Dynamo.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Dynamo.Search
{
    /// <summary>
    ///     A dictionary of objects for search
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

        protected readonly Dictionary<V, Dictionary<string, double>> entryDictionary =
            new Dictionary<V, Dictionary<string, double>>();

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

        /// <summary>
        /// Check if key matches with query string. The query string could
        /// contains multiple sub query strings which are separated with 
        /// space character. The function returns true if the key sequentially
        /// matches with each sub query strings. E.g., 
        /// "Autodesk.Geometry.Point.ByCoordinate" matches with query string
        /// "geometry point by".
        /// </summary>
        /// <param name="key"></param>
        /// <param name="subPatterns"></param>
        /// <returns></returns>
        private static bool MatchWithQueryString(string key, string[] subPatterns)
        {
            int numberOfMatchSymbols = 0;
            int numberOfAllSymbols = 0;
            //for each word
            foreach (var subPattern in subPatterns)
            { //for each continuous substring in the word starting with the full word
                for (int i = subPattern.Length; i >= 1; i--)
                {
                    var part = subPattern.Substring(0, i);
                    if (key.IndexOf(part) != -1)
                    {   //if we find a match record the amount of the match and goto the next word
                        numberOfMatchSymbols += part.Length;
                        break;
                    }
                }
                numberOfAllSymbols += subPattern.Length;
            }
            //ratio of all symbols to matched partial words >.8 for match
            return (double)numberOfMatchSymbols / numberOfAllSymbols > 0.8;
        }

        private static string[] SplitOnWhiteSpace(string s)
        {
            return s.Split(null);
        }

        private static bool ContainsSpecialCharacters(string element)
        {
            return element.Contains("*") || element.Contains(".") || element.Contains(" ")
                || element.Contains("\\");
        }
        #endregion

        /// <summary>
        /// Converts entryDictionary from a dictionary of searchElement:(tag,weight)
        /// to a dictionary of tag:(list(searchelement,weight))
        /// which contains all nodes which share a tag 
        /// </summary>
        internal void RebuildTagDictionary()
        {
            tagDictionary = entryDictionary
                .SelectMany(
                    entryAndTags =>
                        entryAndTags.Value.Select(
                            tagAndWeight =>
                                new
                                {
                                    Tag = tagAndWeight.Key,
                                    Weight = tagAndWeight.Value,
                                    Entry = entryAndTags.Key
                                }))
                .GroupBy(
                    tagWeightAndEntry => tagWeightAndEntry.Tag,
                    tagWeightAndEntry =>
                        Tuple.Create(tagWeightAndEntry.Entry, tagWeightAndEntry.Weight)).ToList();
        }

        /// <summary>
        /// Search for elements in the dictionary based on the query
        /// </summary>
        /// <param name="query"> The query </param>
        /// <param name="minResultsForTolerantSearch">Minimum number of results in the original search strategy to justify doing more tolerant search</param>
        internal IEnumerable<V> Search(string query, int minResultsForTolerantSearch = 0)
        {
#if DEBUG
            Stopwatch stopwatch = null;
            if (this.logger != null)
            {
                stopwatch = new Stopwatch();
                stopwatch.Start();
            }
#endif

            var searchDict = new Dictionary<V, double>();

            if (tagDictionary == null)
            {
                RebuildTagDictionary();
            }

            query = query.ToLower();

            var subPatterns = SplitOnWhiteSpace(query);

            // Add full (unsplit by whitespace) query to subpatterns
            var subPatternsList = subPatterns.ToList();
            subPatternsList.Insert(0, query);
            subPatterns = (subPatternsList).ToArray();

            foreach (var pair in tagDictionary.Where(x => MatchWithQueryString(x.Key, subPatterns)))
            {
                ComputeWeightAndAddToDictionary(query, pair, searchDict);
            }

            var orderedSearchDict = searchDict.OrderByDescending(x => x.Value);

            var searchResults = orderedSearchDict.Select(x => x.Key);

            // return only the top 20 search results
            searchResults = searchResults.Take(20);
            
#if DEBUG
            if (this.logger != null)
            {
                stopwatch.Stop();

                var message = 
                    string.Format(
                        "Searching for: \"{0}\", [Entries:{1}, Tags:{2}] : {3}ms -> {4}", 
                            query,
                            entryDictionary.Count,
                            tagDictionary.Count,
                            stopwatch.ElapsedMilliseconds,
                            searchResults.Count());
                this.logger.Log(message);
            }
#endif

            return searchResults;
        }

        private static void ComputeWeightAndAddToDictionary(
            string query,
            IGrouping<string, Tuple<V, double>> pair, 
            Dictionary<V, double> searchDict)
        {
            // it has a match, how close is it to matching the entire string?
            double matchCloseness = ((double)query.Length) / pair.Key.Length;

            foreach (var eleAndWeight in pair)
            {
                var ele = eleAndWeight.Item1;
                double weight = matchCloseness * eleAndWeight.Item2;

                // we may have seen V before
                if (searchDict.ContainsKey(ele))
                {
                    // if we have, update its weight if better than the current one
                    if (searchDict[ele] < weight) searchDict[ele] = weight;
                }
                else
                {
                    // if we haven't seen it, add it to the dictionary for this search
                    searchDict.Add(ele, weight);
                }
            }
        }
		
        /// <summary>
        /// Returns all tags for search specified element
        /// </summary>
        /// <param name="element">The element to match</param>
        /// <returns>All tags of the given element</returns>
        internal IEnumerable<string> GetTags(V element)
        {
            if (entryDictionary.ContainsKey(element))
                return entryDictionary[element].Keys;
            return null;
        }

        /// <summary>
        /// Returns all weights for search specified element
        /// </summary>
        /// <param name="element">The element to match</param>
        /// <returns>All tags of the given element</returns>
        internal IEnumerable<double> GetWeights(V element)
        {
            if (entryDictionary.ContainsKey(element))
                return entryDictionary[element].Values;
            return null;
        }
    }
}