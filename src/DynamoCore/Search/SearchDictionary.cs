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
            foreach (string tag in tags.Select(x => x.ToLower()))
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
            foreach (V ele in removals)
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
        /// Search for elements in the dictionary based on the query
        /// </summary>
        /// <param name="query">The query</param>
        public IEnumerable<V> Search(string query)
        {
            return //Begin the search by "flattening" the entryDictionary.
                entryDictionary.AsParallel()
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

                    //Start by grouping together matching search tags. Each search tag now has an enumerable sequence of entries and associated weight
                    .GroupBy(
                        tagWeightAndEntry => tagWeightAndEntry.Tag,
                        tagWeightAndEntry => new { tagWeightAndEntry.Entry, tagWeightAndEntry.Weight })
                    .Select(
                        tagAndEntries =>
                            //For each grouping, we calculate a "match closeness" based on the difference between the search tag and the query
                            new
                            {
                                MatchCloseness = JaroWinkler.StringDistance(tagAndEntries.Key, query),
                                EntriesAndWeights = tagAndEntries
                            })
                    .SelectMany(
                        closenessAndEntries =>
                            closenessAndEntries.EntriesAndWeights.Select(
                                entryAndWeight =>
                                    //We combine the match closeness with each individual weight to calculate the total weight associated with each entry.
                                    new
                                    {
                                        entryAndWeight.Entry,
                                        Weight = entryAndWeight.Weight*closenessAndEntries.MatchCloseness
                                    }))
                    //Filter out all entries with a weight of approximately zero
                    .Where(entryAndWeight => !(Math.Abs(entryAndWeight.Weight) < double.Epsilon))
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

        public static class JaroWinkler
        {
            public static double StringDistance(string firstWord, string secondWord)
            {
                const double prefixAdustmentScale = 0.1;

                if ((firstWord == null) || (secondWord == null)) 
                    return 0.0;

                double dist = GetJaroDistance(firstWord, secondWord);
                int prefixLength = GetPrefixLength(firstWord, secondWord);
                return dist + prefixLength * prefixAdustmentScale * (1.0 - dist);
            }

            private static double GetJaroDistance(string firstWord, string secondWord)
            {
                const double defaultMismatchScore = 0;

                if ((firstWord == null) || (secondWord == null)) 
                    return defaultMismatchScore;

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

            private static StringBuilder GetCommonCharacters(string firstWord, string secondWord, int distanceSep)
            {
                if ((firstWord == null) || (secondWord == null)) 
                    return null;

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
                        if (copy[j] != ch)
                            continue;

                        foundIt = true;
                        returnCommons.Append(ch);
                        copy[j] = '#';
                    }
                }

                return returnCommons;
            }

            private static int GetPrefixLength(string firstWord, string secondWord)
            {
                const int minPrefixTestLength = 4;

                if ((firstWord == null) || (secondWord == null)) 
                    return minPrefixTestLength;
                
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
        }
    }
}