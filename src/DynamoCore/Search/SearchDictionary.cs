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
        public IEnumerable<V> SearchEntries { get { return entryDictionary.Keys; } } 

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
            get
            {
                return entryDictionary.Count;
            }
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
        public void Add(V value, string tag, double weight=1)
        {
            Add(value, tag.AsSingleton(), weight);
        }

        /// <summary>
        ///     Add a list of elements with a single tag
        /// </summary>
        /// <param name="values"> List of objects to add  </param>
        /// <param name="tag"> The string to identify it in search </param>
        /// <param name="weight"></param>
        public void Add(IEnumerable<V> values, string tag, double weight=1)
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
        public void Add(V value, IEnumerable<string> tags, double weight=1)
        {
            Dictionary<string, double> keys;
            if (!entryDictionary.TryGetValue(value, out keys))
            {
                keys = new Dictionary<string, double>();
                entryDictionary[value] = keys;
            }
            foreach (var tag in tags.Select(x => x.ToLower()))
                keys[tag] = weight;
            OnEntryAdded(value);
        }

        /// <summary>
        ///     Add a coordinated list of objects and strings.
        /// </summary>
        /// <param name="values"> The objects to add. Must have the same cardinality as the second parameter</param>
        /// <param name="tags"> The list of strings to identify it in search. Must have the same cardinality as the first parameter </param>
        /// <param name="weight"></param>
        public void Add(IEnumerable<V> values, IEnumerable<string> tags, double weight=1)
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
                        tagWeightAndEntry => new { tagWeightAndEntry.Entry, tagWeightAndEntry.Weight })

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
                                        Weight = entryAndWeight.Weight*closenessAndEntries.MatchCloseness
                                    }))
                    
                    //Filter out all entries with a weight of approximately zero
                    .Where(entryAndWeight => Math.Abs(entryAndWeight.Weight) >= double.Epsilon)
                    
                    //Group together all results consisting of the same entry.
                    .GroupBy(entryAndWeight => entryAndWeight.Entry, entryAndWeight => entryAndWeight.Weight)
                    
                    //We remove all duplicates, using the largest available weight
                    .Select(
                        entryAndWeights => new { Entry = entryAndWeights.Key, Weight = entryAndWeights.Max() })
                    
                    //Sort results in descending order by weight.
                    .OrderByDescending(entryAndWeight => entryAndWeight.Weight)
                    
                    //Select only the entries, stripping out the weights.
                    .Select(entryAndWeight => entryAndWeight.Entry);
        }


        //http://stackoverflow.com/questions/19123506/jaro-winkler-distance-algorithm-in-c-sharp
        public static class JaroWinkler
        {
            /* The Winkler modification will not be applied unless the 
             * percent match was at or above the mWeightThreshold percent 
             * without the modification. 
             * Winkler's paper used a default value of 0.7
             */
            private const double M_WEIGHT_THRESHOLD = 0.7;

            /* Size of the prefix to be concidered by the Winkler modification. 
             * Winkler's paper used a default value of 4
             */
            private const int M_NUM_CHARS = 4;


            /// <summary>
            /// Returns the Jaro-Winkler distance between the specified  
            /// strings. The distance is symmetric and will fall in the 
            /// range 0 (perfect match) to 1 (no match). 
            /// </summary>
            /// <param name="aString1">First String</param>
            /// <param name="aString2">Second String</param>
            /// <returns></returns>
            public static double StringDistance(string aString1, string aString2)
            {
                return 1.0 - Proximity(aString1, aString2);
            }


            /// <summary>
            /// Returns the Jaro-Winkler distance between the specified  
            /// strings. The distance is symmetric and will fall in the 
            /// range 0 (no match) to 1 (perfect match). 
            /// </summary>
            /// <param name="aString1">First String</param>
            /// <param name="aString2">Second String</param>
            /// <returns></returns>
            private static double Proximity(string aString1, string aString2)
            {
                int lLen1 = aString1.Length;
                int lLen2 = aString2.Length;
                if (lLen1 == 0)
                    return lLen2 == 0 ? 1.0 : 0.0;

                int lSearchRange = Math.Max(0, Math.Max(lLen1, lLen2) / 2 - 1);

                var lMatched1 = new bool[lLen1];
                for (int i = 0; i < lMatched1.Length; i++)
                {
                    lMatched1[i] = false;
                }

                var lMatched2 = new bool[lLen2];
                for (int i = 0; i < lMatched2.Length; i++)
                {
                    lMatched2[i] = false;
                }

                int lNumCommon = 0;
                for (int i = 0; i < lLen1; ++i)
                {
                    int lStart = Math.Max(0, i - lSearchRange);
                    int lEnd = Math.Min(i + lSearchRange + 1, lLen2);
                    for (int j = lStart; j < lEnd; ++j)
                    {
                        if (lMatched2[j]) continue;
                        if (aString1[i] != aString2[j])
                            continue;
                        lMatched1[i] = true;
                        lMatched2[j] = true;
                        ++lNumCommon;
                        break;
                    }
                }
                if (lNumCommon == 0) return 0.0;

                int lNumHalfTransposed = 0;
                int k = 0;
                for (int i = 0; i < lLen1; ++i)
                {
                    if (!lMatched1[i]) continue;
                    while (!lMatched2[k]) ++k;
                    if (aString1[i] != aString2[k])
                        ++lNumHalfTransposed;
                    ++k;
                }
                // System.Diagnostics.Debug.WriteLine("numHalfTransposed=" + numHalfTransposed);
                int lNumTransposed = lNumHalfTransposed / 2;

                // System.Diagnostics.Debug.WriteLine("numCommon=" + numCommon + " numTransposed=" + numTransposed);
                double lNumCommonD = lNumCommon;
                double lWeight = (lNumCommonD / lLen1
                                 + lNumCommonD / lLen2
                                 + (lNumCommon - lNumTransposed) / lNumCommonD) / 3.0;

                if (lWeight <= M_WEIGHT_THRESHOLD) return lWeight;
                int lMax = Math.Min(M_NUM_CHARS, Math.Min(aString1.Length, aString2.Length));
                int lPos = 0;
                while (lPos < lMax && aString1[lPos] == aString2[lPos])
                    ++lPos;
                if (lPos == 0) return lWeight;
                return lWeight + 0.1 * lPos * (1.0 - lWeight);
            }
        }
    }
}