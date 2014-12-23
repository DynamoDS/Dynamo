//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text.RegularExpressions;

//using Dynamo.Search.SearchElements;

//namespace Dynamo.Search
//{
//    /// <summary>
//    ///     A dictionary of objects for search
//    /// </summary>
//    public class SearchDictionary<V>
//    {
//        private readonly Dictionary<V, HashSet<string>> _symbolDictionary = new Dictionary<V, HashSet<string>>();
//        private readonly Dictionary<string, HashSet<V>> _tagDictionary = new Dictionary<string, HashSet<V>>();

//        /// <summary>
//        ///     The number of tags in the dicitionary
//        /// </summary>
//        public int NumTags
//        {
//            get
//            {
//                return _tagDictionary.Count;
//            }
//        }

//        /// <summary>
//        ///     The number of elements in the dicitionary
//        /// </summary>
//        public int NumElements
//        {
//            get
//            {
//                return _symbolDictionary.Count;
//            }
//        }

//        /// <summary>
//        ///     Add a single element with a single tag
//        /// </summary>
//        /// <param name="value"> The object to add  </param>
//        /// <param name="tag"> The string to identify it in search </param>
//        public void Add(V value, string tag)
//        {
//            tag = tag.ToLower();

//            if (_tagDictionary.ContainsKey(tag))
//                _tagDictionary[tag].Add(value);
//            else
//                _tagDictionary[tag] = new HashSet<V> {value};

//            if (_symbolDictionary.ContainsKey(value))
//                _symbolDictionary[value].Add(tag);
//            else
//                _symbolDictionary[value] = new HashSet<string> {tag};
//        }

//        /// <summary>
//        ///     Add a list of elements with a single tag
//        /// </summary>
//        /// <param name="values"> List of objects to add  </param>
//        /// <param name="tag"> The string to identify it in search </param>
//        public void Add(IEnumerable<V> values, string tag)
//        {
//            tag = tag.ToLower();

//            if (_tagDictionary.ContainsKey(tag))
//                _tagDictionary[tag].UnionWith(values);
//            else
//                _tagDictionary[tag] = new HashSet<V>(values);

//            foreach (V val in values)
//            {
//                if (_symbolDictionary.ContainsKey(val))
//                    _symbolDictionary[val].Add(tag);
//                else
//                    _symbolDictionary[val] = new HashSet<string> {tag};
//            }
//        }

//        /// <summary>
//        ///     Add a single element with a number of tags
//        /// </summary>
//        /// <param name="value"> The object to add  </param>
//        /// <param name="tags"> The list of strings to identify it in search </param>
//        public void Add(V value, IEnumerable<string> tags)
//        {
//            tags = tags.Select(x => x.ToLower());

//            foreach (string tag in tags)
//                Add(value, tag);
//        }

//        /// <summary>
//        ///     Add a coordinated list of objects and strings.
//        /// </summary>
//        /// <param name="values"> The objects to add. Must have the same cardinality as the second parameter</param>
//        /// <param name="tags"> The list of strings to identify it in search. Must have the same cardinality as the first parameter </param>
//        public void Add(IEnumerable<V> values, IEnumerable<string> tags)
//        {
//            tags = tags.Select(x => x.ToLower());

//            foreach (string tag in tags)
//                Add(values, tag);
//        }

//        /// <summary>
//        ///     Remove an element from the search
//        /// </summary>
//        /// <param name="value"> The object to remove </param>
//        /// <param name="tag"> The tags to remove for the given value </param>
//        public void Remove(V value, string tag)
//        {
//            tag = tag.ToLower();

//            _tagDictionary[tag].Remove(value);
//            _symbolDictionary[value].Remove(tag);
//        }

//        /// <summary>
//        ///     Remove an element from the search
//        /// </summary>
//        /// <param name="value"> The object to remove </param>
//        public void Remove(V value)
//        {
//            _symbolDictionary.Remove(value);
            
//            foreach (var set in _tagDictionary)
//            {
//                set.Value.Remove(value);
//            }
//        }

//        /// <summary>
//        ///     Remove an element from the search
//        /// </summary>
//        /// <param name="tag"> The tag for which to remove elements </param>
//        public void Remove(string tag)
//        {
//            tag = tag.ToLower();

//            if (_tagDictionary.ContainsKey(tag))
//            {
//                HashSet<V> elems = _tagDictionary[tag];
//                _tagDictionary.Remove(tag);
//                foreach (V elem in elems)
//                {
//                    _symbolDictionary[elem].Remove(tag);
//                }
//            }
//        }

//        /// <summary>
//        ///     Remove elements from search based on a predicate
//        /// </summary>
//        /// <param name="removeCondition"> The predicate with which to test.  True results in removal. </param>
//        public void Remove(Predicate<V> removeCondition)
//        {
//            var removeSet = new HashSet<V>();
//            // remove from _tagDictionary and keep track of which elements were removed
//            foreach (var pair in _tagDictionary)
//            {
//                foreach (V ele in pair.Value)
//                {
//                    if (removeCondition(ele)) removeSet.Add(ele);
//                }
//                pair.Value.RemoveWhere(removeCondition);
//            }
//            // remove from symbol dictionary
//            foreach (V ele in removeSet)
//            {
//                _symbolDictionary.Remove(ele);
//            }
//        }

//        /// <summary>
//        ///     Remove elements from search
//        /// </summary>
//        /// <param name="value"> The object to remove </param>
//        /// <param name="tags"> The list of tags to remove. </param>
//        public void Remove(V value, IEnumerable<string> tags)
//        {
//            tags = tags.Select(x => x.ToLower());

//            foreach (string tag in tags)
//                Remove(value, tag);
//        }

//        /// <summary>
//        ///     Get the elements with a given tag
//        /// </summary>
//        /// <param name="tag"> The tag to match </param>
//        /// <returns> The elements with the given tag </returns>
//        public HashSet<V> ByTag(string tag)
//        {
//            tag = tag.ToLower();

//            return _tagDictionary[tag];
//        }

//        public bool Contains(V a)
//        {
//            return this._symbolDictionary.Keys.Any(x => x.Equals(a));
//        }

//        /// <summary>
//        ///     Filter the elements in the SearchDictionary, based on whether there is a string
//        ///     in the tag matching the query
//        /// </summary>
//        /// <param name="query"> The query </param>
//        public HashSet<V> Filter(string query)
//        {
//            query = query.ToLower();

//            var result = new HashSet<V>();

//            foreach (var pair in _tagDictionary)
//            {
//                if (pair.Key.Contains(query))
//                {
//                    result.UnionWith(pair.Value);
//                }
//            }

//            return result;
//        }

//        /// <summary>
//        /// Check if key matches with query string. The query string could
//        /// contains multiple sub query strings which are seperated with 
//        /// space character. The function returns true if the key sequentially
//        /// matches with each sub query strings. E.g., 
//        /// "Autodesk.Geometry.Point.ByCoordinate" matches with query string
//        /// "geometry point by".
//        /// </summary>
//        /// <param name="key"></param>
//        /// <param name="pattern"></param>
//        /// <returns></returns>
//        private bool MatchWithQuerystring(string key, string pattern)
//        {
//            return Regex.IsMatch(key, pattern);
//        }

//        private string SanitizeQuery(string query)
//        {
//            return query.Trim()
//                        .Replace("\\", "\\\\")
//                        .Replace(".", "\\.")
//                        .Replace("*", "\\*");
//        }

//        private string[] SplitOnWhiteSpace(string s)
//        {
//            return s.Split(null);
//        }

//        private string MakePattern(string[] subPatterns)
//        {
//            return "(.*)" + String.Join("(.*)", subPatterns) + "(.*)";
//        }

//        private bool ContainsSpecialCharacters(string element)
//        {

//            return element.Contains("*") || element.Contains(".") || element.Contains(" ")
//                || element.Contains("\\");

//        }

//        /// <summary>
//        /// Search for elements in the dictionary based on the query
//        /// </summary>
//        /// <param name="query"> The query </param>
//        /// <param name="numResults"> The max number of results to return </param>
//        /// <param name="minResultsForTolerantSearch">Minimum number of results in the original search strategy to justify doing more tolerant search</param>
//        public IEnumerable<V> Search(string query, int numResults = 10, int minResultsForTolerantSearch = 0)
//        {
//            var searchDict = new Dictionary<V, double>();


//            query = query.ToLower();

//            // do containment check
//            foreach (var pair in _tagDictionary.Where(x => x.Key.Contains(query)))
//            {
//                ComputeWeightAndAddToDictionary(query, pair, searchDict );
//            }

//            // if you don't have enough results and the query contains special characters, do fuzzy search
//            if (searchDict.Count <= minResultsForTolerantSearch && ContainsSpecialCharacters(query))
//            {
//                var regexPattern = MakePattern( SplitOnWhiteSpace( SanitizeQuery(query) ) );

//                foreach (var pair in _tagDictionary.Where(x => MatchWithQuerystring(x.Key, regexPattern)))
//                {
//                    ComputeWeightAndAddToDictionary( query, pair, searchDict );
//                }
//            }

//            return searchDict
//                .OrderByDescending(x => x.Value)
//                .Select(x => x.Key)
//                .Take(Math.Min(numResults, searchDict.Count));
//        }

//        private static void ComputeWeightAndAddToDictionary(string query, 
//            KeyValuePair<string, HashSet<V>> pair, Dictionary<V, double> searchDict )
//        {
//            // it has a match, how close is it to matching the entire string?
//            double matchCloseness = ((double)query.Length) / pair.Key.Length;

//            foreach (V ele in pair.Value)
//            {
//                double weight = matchCloseness;
//                // search elements have a weight associated with them
//                var @base = ele as SearchElementBase;

//                // ignore elements which should not be search for
//                if (@base.Searchable == false)
//                    continue;

//                if (@base != null)
//                    weight *= @base.Weight;

//                // we may have seen V before
//                if (searchDict.ContainsKey(ele))
//                {
//                    // if we have, update its weight if better than the current one
//                    if (searchDict[ele] < weight) searchDict[ele] = weight;
//                }
//                else
//                {
//                    // if we haven't seen it, add it to the dictionary for this search
//                    searchDict.Add(ele, weight);
//                }
//            }
//        }

//    }
//}