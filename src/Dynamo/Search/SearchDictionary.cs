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
using System.Text.RegularExpressions;

namespace Dynamo.Search
{
   public class SearchDictionary<V>
   {

      private Dictionary<string, HashSet<V>> tagDict = new Dictionary<string, HashSet<V>>();
      private Dictionary<V, HashSet<string>> symbolDict = new Dictionary<V, HashSet<string>>();

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
       
      public void Remove(V value, string tag)
      {
         this.tagDict[tag].Remove(value);
         this.symbolDict[value].Remove(tag);
      }

      public void Remove(string tag)
       {
           if (this.tagDict.ContainsKey(tag))
           {
               var elems = tagDict[tag];
               tagDict.Remove(tag);
               foreach (var elem in elems)
               {
                   symbolDict[elem].Remove(tag);
               }
           }
       }

      public void Remove(Predicate<V> removeCondition)
       {
           var removeSet = new HashSet<V>();
           // remove from tagDict and keep track of which elements were removed
           foreach (var pair in tagDict )
           {
               foreach (var ele in pair.Value)
               {
                   if (removeCondition(ele)) removeSet.Add(ele);
               }
               pair.Value.RemoveWhere(removeCondition);
           }
           // remove from symbol dictionary
           foreach (var ele in removeSet)
           {
               symbolDict.Remove(ele);
           }
       }

      public void Remove(V value, IEnumerable<string> tags)
      {
         foreach (string tag in tags)
            this.Remove(value, tag);
      }

      public HashSet<V> Filter(string search)
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

      public List<V> Search(string search, int numResults = 10)
      {
          var searchDict = new Dictionary<V, double>();

          foreach (var pair in this.tagDict)
          {
              // does the key have an internal match with the search?
              var pattern = ".*(" + Regex.Escape(search) + ").*";
              var matches = Regex.Matches(pair.Key.ToLower(), pattern, RegexOptions.IgnoreCase);
              if (matches.Count > 0)
              {
                  // it has a match, how close is it to matching the entire string?
                  var matchCloseness = Math.Max(((double)(pair.Key.Length - search.Length)) / pair.Key.Length, 0);
                  foreach (var ele in pair.Value)
                  {
                      double weight = matchCloseness;
                      // search elements have a weight associated with them
                      var @base = ele as SearchElementBase;
                      if (@base != null)
                          weight *= @base.Weight;

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
          }

          return searchDict.OrderBy(x => x.Value).Select(x => x.Key).ToList().GetRange(0, Math.Min(numResults, searchDict.Count));

      }
   }
}
