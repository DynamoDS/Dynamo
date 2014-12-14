using Dynamo.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Dynamo.Search
{
    /// <summary>
    ///     A dictionary of objects for search
    /// </summary>
    public class SearchDictionary<V>
    {
        private readonly Dictionary<V, Dictionary<string, double>> entryDictionary =
            new Dictionary<V, Dictionary<string, double>>();

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

        public event Action<V> EntryAdded;

        protected virtual void OnEntryAdded(V entry)
        {
            var handler = EntryAdded;
            if (handler != null) handler(entry);
        }

        public event Action<V> EntryRemoved;

        protected virtual void OnEntryRemoved(V entry)
        {
            var handler = EntryRemoved;
            if (handler != null) handler(entry);
        }

        /// <summary>
        ///     Add a single element with a single tag
        /// </summary>
        /// <param name="value"> The object to add  </param>
        /// <param name="tag"> The string to identify it in search </param>
        /// <param name="weight"></param>
        public void Add(V value, string tag, double weight = 1)
        {
            Add(value, tag.AsSingleton(), weight);
        }

        /// <summary>
        ///     Add a list of elements with a single tag
        /// </summary>
        /// <param name="values"> List of objects to add  </param>
        /// <param name="tag"> The string to identify it in search </param>
        /// <param name="weight"></param>
        public void Add(IEnumerable<V> values, string tag, double weight = 1)
        {
            foreach (var value in values)
                Add(value, tag, weight);
        }

        /// <summary>
        ///     Add a single element with a number of tags
        /// </summary>
        /// <param name="value"> The object to add  </param>
        /// <param name="tags"> The list of strings to identify it in search </param>
        /// <param name="weight"></param>
        public void Add(V value, IEnumerable<string> tags, double weight = 1)
        {
            Dictionary<string, double> keys;
            if (!entryDictionary.TryGetValue(value, out keys))
            {
                keys = new Dictionary<string, double>();
                entryDictionary[value] = keys;
                OnEntryAdded(value);
            }
            foreach (var tag in tags.Select(x => x.ToLower()))
                keys[tag] = weight;
        }

        /// <summary>
        ///     Add a coordinated list of objects and strings.
        /// </summary>
        /// <param name="values"> The objects to add. Must have the same cardinality as the second parameter</param>
        /// <param name="tags"> The list of strings to identify it in search. Must have the same cardinality as the first parameter </param>
        /// <param name="weight"></param>
        public void Add(IEnumerable<V> values, IEnumerable<string> tags, double weight = 1)
        {
            var tagList = tags as IList<string> ?? tags.ToList();
            foreach (var value in values)
                Add(value, tagList, weight);
        }

        /// <summary>
        ///     Remove an element from the search
        /// </summary>
        /// <param name="value"> The object to remove </param>
        /// <param name="tag">The tag to remove for the given value </param>
        public bool Remove(V value, string tag)
        {
            Dictionary<string, double> keys;
            return entryDictionary.TryGetValue(value, out keys) && keys.Remove(tag)
                && (keys.Any() || Remove(value));
        }

        /// <summary>
        ///     Remove an element from the search
        /// </summary>
        /// <param name="value"> The object to remove </param>
        public bool Remove(V value)
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
        public int Remove(Func<V, bool> removeCondition)
        {
            var removals = entryDictionary.Keys.Where(removeCondition).ToList();
            foreach (var ele in removals)
                Remove(ele);
            return removals.Count;
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="valueCondition"></param>
        /// <param name="removeTagCondition"></param>
        /// <returns></returns>
        public int Remove(Func<V, bool> valueCondition, Func<string, bool> removeTagCondition)
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
        public int Remove(V value, IEnumerable<string> tags)
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
        ///     Get the elements with a given tag
        /// </summary>
        /// <param name="tag"> The tag to match </param>
        /// <returns> The elements with the given tag </returns>
        public IEnumerable<V> ByTag(string tag)
        {
            return entryDictionary.Where(kv => kv.Value.ContainsKey(tag)).Select(kv => kv.Key);
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public bool Contains(V a)
        {
            return entryDictionary.Keys.Any(x => Equals(x, a));
        }

        #region Regex Searching
        /// <summary>
        /// Check if key matches with query string. The query string could
        /// contains multiple sub query strings which are seperated with 
        /// space character. The function returns true if the key sequentially
        /// matches with each sub query strings. E.g., 
        /// "Autodesk.Geometry.Point.ByCoordinate" matches with query string
        /// "geometry point by".
        /// </summary>
        /// <param name="key"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        private static bool MatchWithQuerystring(string key, Regex pattern)
        {
            return pattern.IsMatch(key);
        }

        private static string SanitizeQuery(string query)
        {
            return query.Trim()
                .Replace("\\", "\\\\")
                .Replace(".", "\\.")
                .Replace("*", "\\*");
        }

        private static string[] SplitOnWhiteSpace(string s)
        {
            return s.Split(null);
        }

        private static Regex MakePattern(string[] subPatterns)
        {
            var pattern = "(.*)" + string.Join("(.*)", subPatterns) + "(.*)";
            return new Regex(pattern);
        }

        private static bool ContainsSpecialCharacters(string element)
        {
            return element.Contains("*") || element.Contains(".") || element.Contains(" ")
                || element.Contains("\\");
        }
        #endregion

        private static IEnumerable<Func<string, bool>> SearchMethodsForQuery(
            string query)
        {
            yield return kv => kv.Contains(query);

            if (!ContainsSpecialCharacters(query))
                yield break;
            var regexPattern = MakePattern(SplitOnWhiteSpace(SanitizeQuery(query)));
            yield return kv => MatchWithQuerystring(kv, regexPattern);
        }

        /// <summary>
        ///     Search for elements in the dictionary.
        /// </summary>
        /// <param name="query">The search query</param>
        public IEnumerable<V> Search(string query)
        {
            return
                entryDictionary.AsParallel()
                    
                    //Begin the search by "flattening" the entryDictionary.
                    .SelectMany(
                        entryAndTags =>
                            //For each entry in the entry dictionary, get all the tag/weight pairs.
                            entryAndTags.Value.AsParallel().Select(
                                //Package together the search tag, the weight associated with the tag, and the entry.
                                tagAndWeight =>
                                    new
                                    {
                                        Tag = tagAndWeight.Key,
                                        Weight = tagAndWeight.Value,
                                        Entry = entryAndTags.Key
                                    }))

                    //Group together matching search tags. Each search tag now has an enumerable sequence of entries and associated weight
                    .GroupBy(
                        tagWeightAndEntry => tagWeightAndEntry.Tag,
                        tagWeightAndEntry =>
                            new { tagWeightAndEntry.Entry, tagWeightAndEntry.Weight })

                    //For each grouping, we calculate a "match closeness" based on the difference between the search tag and the query
                    .Select(
                        tagAndEntries =>
                            new
                            {
                                MatchCloseness = JaroWinkler.StringDistance(tagAndEntries.Key, query),
                                EntriesAndWeights = tagAndEntries
                            })
                    
                    //We combine the match closeness with each individual weight to calculate the total weight associated with each entry.
                    .SelectMany(
                        closenessAndEntries =>
                            closenessAndEntries.EntriesAndWeights.Select(
                                entryAndWeight =>
                                    new
                                    {
                                        entryAndWeight.Entry,
                                        Weight =
                                    entryAndWeight.Weight*closenessAndEntries.MatchCloseness
                                    }))
                    
                    //Filter out all entries with a weight of approximately zero
                    .Where(entryAndWeight => Math.Abs(entryAndWeight.Weight) >= double.Epsilon)
                    
                    //Group together all results consisting of the same entry.
                    .GroupBy(
                        entryAndWeight => entryAndWeight.Entry,
                        entryAndWeight => entryAndWeight.Weight)
                    
                    //We remove all duplicates, using the largest available weight
                    .Select(
                        entryAndWeights =>
                            new { Entry = entryAndWeights.Key, Weight = entryAndWeights.Max() })
                    
                    //Sort results in descending order by weight.
                    .OrderByDescending(entryAndWeight => entryAndWeight.Weight)
                    
                    //Select only the entries, stripping out the weights.
                    .Select(entryAndWeight => entryAndWeight.Entry);
        }


        public static class JaroWinkler
        {
            /// <summary>
            /// Gets or sets the current value of the threshold used for adding the Winkler bonus.
            /// Set to a negative value to get the Jaro distance. The default value is 0.7.
            /// </summary>
            private const float THRESHOLD = 0.7f;

            private static int[] Matches(String s1, String s2)
            {
                string max, min;

                if (s1.Length > s2.Length)
                {
                    max = s1;
                    min = s2;
                }
                else
                {
                    max = s2;
                    min = s1;
                }

                var range = Math.Max(max.Length/2 - 1, 0);
                var matchIndexes = new int[min.Length];

                for (var i = 0; i < matchIndexes.Length; i++)
                    matchIndexes[i] = -1;

                var matchFlags = new bool[max.Length];
                var matches = 0;

                for (var mi = 0; mi < min.Length; mi++)
                {
                    var c1 = min[mi];
                    for (int xi = Math.Max(mi - range, 0),
                        xn = Math.Min(mi + range + 1, max.Length);
                         xi < xn;
                         xi++)
                    {
                        if (matchFlags[xi] || c1 != max[xi]) continue;

                        matchIndexes[mi] = xi;
                        matchFlags[xi] = true;
                        matches++;
                        break;
                    }
                }

                var ms1 = new char[matches];
                var ms2 = new char[matches];

                for (int i = 0, si = 0; i < min.Length; i++)
                {
                    if (matchIndexes[i] != -1)
                    {
                        ms1[si] = min[i];
                        si++;
                    }
                }

                for (int i = 0, si = 0; i < max.Length; i++)
                {
                    if (matchFlags[i])
                    {
                        ms2[si] = max[i];
                        si++;
                    }
                }

                var transpositions = ms1.Where((t, mi) => t != ms2[mi]).Count();

                var prefix = 0;
                for (var mi = 0; mi < min.Length; mi++)
                {
                    if (s1[mi] == s2[mi])
                    {
                        prefix++;
                    }
                    else
                    {
                        break;
                    }
                }

                return new[] { matches, transpositions/2, prefix, max.Length };
            }

            public static float StringDistance(String s1, String s2)
            {
                var mtp = Matches(s1, s2);
                var m = (float)mtp[0];

                if (Math.Abs(m) < 0.0001)
                    return 0f;

                float j = ((m/s1.Length + m/s2.Length + (m - mtp[1])/m))/3;
                float jw = j < THRESHOLD ? j : j + Math.Min(0.1f, 1f/mtp[3])*mtp[2]*(1 - j);
                return jw;
            }
        }
    }
}