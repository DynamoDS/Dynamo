//Copyright © Autodesk, Inc. 2012. All rights reserved.
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Dynamo.Search
{
   public class SearchDictionary<V>
   {
      private Dictionary<string, HashSet<V>> tagDict = new Dictionary<string, HashSet<V>>();
      private Dictionary<V, HashSet<string>> symbolDict = new Dictionary<V, HashSet<string>>();

      private Dictionary<string, V> nameDict = new Dictionary<string, V>();

      public void AddName(V value, string name)
      {
          if ( !nameDict.ContainsKey(name) )
              nameDict.Add(name, value);
      }

      public void Add(V value, string tag)
      {

         if (tagDict.ContainsKey(tag))
            this.tagDict[tag].Add(value);
         else
            this.tagDict[tag] = new HashSet<V>() { value };

         if (this.symbolDict.ContainsKey(value))
            this.symbolDict[value].Add(tag);
         else
            this.symbolDict[value] = new HashSet<string>() { tag };
      }

      public void Add(IEnumerable<V> values, string tag)
      {
         if (this.tagDict.ContainsKey(tag))
            this.tagDict[tag].UnionWith(values);
         else
            this.tagDict[tag] = new HashSet<V>(values);

         foreach (V val in values)
         {
            if (this.symbolDict.ContainsKey(val))
               this.symbolDict[val].Add(tag);
            else
               this.symbolDict[val] = new HashSet<string>() { tag };
         }
      }

      public void Add(V value, IEnumerable<string> tags)
      {
         foreach (var tag in tags)
            this.Add(value, tag);
      }

      public void Add(IEnumerable<V> values, IEnumerable<string> tags)
      {
         foreach (var tag in tags)
            this.Add(values, tag);
      }

      public HashSet<V> Search(string search)
      {
          var result = new HashSet<V>();

          foreach (var word in search.Split(new char[] { ' ' }).Where(x => x.Length > 0))
          {
              foreach (var pair in this.tagDict)
              {
                  if (pair.Key.ToLower().StartsWith(word))
                      result.UnionWith(pair.Value);
              }
          }

          return result;

       }

      public void Remove(V value, string tag)
      {
         this.tagDict[tag].Remove(value);
         this.symbolDict[value].Remove(tag);
      }

      public void Remove(V value, IEnumerable<string> tags)
      {
         foreach (string tag in tags)
            this.Remove(value, tag);
      }

      public List<V> FuzzySearch(string search, int numResults = 10)
      {

          var searchDict = new List<KeyValuePair<int, V>>();

          foreach (var pair in this.nameDict)
          {
              int levDist = LevenshteinDistance(search, pair.Key.ToLower());
              searchDict.Add(new KeyValuePair<int, V>(levDist, pair.Value));
          }

          return searchDict.OrderBy(x => x.Key).Select(x => x.Value).ToList().GetRange(0, numResults);

      }

      public List<V> FuzzySearchSymbols(string search, int numResults = 10)
      {

          var searchDict = new List<KeyValuePair<int, V>>();

          foreach (var pair in this.symbolDict)
          {
              var dist = int.MaxValue;

              foreach (var keyword in pair.Value)
              {
                  int levDist = LevenshteinDistance(search, keyword.ToLower());
                  if (levDist < dist)
                      dist = levDist;
              }
              searchDict.Add(new KeyValuePair<int, V>(dist, pair.Key));
          }

          return searchDict.OrderBy(x => x.Key).Select(x => x.Value).ToList().GetRange(0, numResults);

      }

      public int LevenshteinDistance(string source, string target)
      {
          if (String.IsNullOrEmpty(source))
          {
              if (String.IsNullOrEmpty(target)) return 0;
              return target.Length;
          }
          if (String.IsNullOrEmpty(target)) return source.Length;

          if (source.Length > target.Length)
          {
              var temp = target;
              target = source;
              source = temp;
          }

          var m = target.Length;
          var n = source.Length;
          var distance = new int[2, m + 1];

          for (var j = 1; j <= m; j++) distance[0, j] = j;

          var currentRow = 0;
          for (var i = 1; i <= n; ++i)
          {
              currentRow = i & 1;
              distance[currentRow, 0] = i;
              var previousRow = currentRow ^ 1;
              for (var j = 1; j <= m; j++)
              {
                  var cost = (target[j - 1] == source[i - 1] ? 0 : 1);
                  distance[currentRow, j] = Math.Min(Math.Min(
                                          distance[previousRow, j] + 1,
                                          distance[currentRow, j - 1] + 1),
                                          distance[previousRow, j - 1] + cost);
              }
          }
          return distance[currentRow, m];
      }
   }
}
