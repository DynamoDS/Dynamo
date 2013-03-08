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

namespace Dynamo.Nodes
{
   class SearchDictionary<V>
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

      public HashSet<V> Search(string search)
      {
         HashSet<V> result = new HashSet<V>();

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
   }
}
