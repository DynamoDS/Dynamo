using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamo.Elements
{
   class SearchDictionary<V>
   {
      private Dictionary<string, HashSet<V>> tagDict = new Dictionary<string, HashSet<V>>();

      public void Add(string tag, V value)
      {
         if (tagDict.ContainsKey(tag))
         {
            tagDict[tag].Add(value);
         }
         else
         {
            tagDict[tag] = new HashSet<V>() { value };
         }
      }

      public void Add(string tag, IEnumerable<V> values)
      {
         if (tagDict.ContainsKey(tag))
         {
            tagDict[tag].UnionWith(values);
         }
         else
         {
            tagDict[tag] = new HashSet<V>(values);
         }
      }

      public void Add(IEnumerable<string> tags, V value)
      {
         foreach (var tag in tags)
         {
            this.Add(tag, value);
         }
      }

      public void Add(IEnumerable<string> tags, IEnumerable<V> value)
      {
         foreach (var tag in tags)
         {
            this.Add(tag, value);
         }
      }

      public HashSet<V> SearchWithSubString(string search)
      {
         HashSet<V> result = new HashSet<V>();

         foreach (var word in search.Split(new char[] { ' ' }).Where(x => x.Length > 0))
         {
            foreach (var pair in tagDict)
            {
               if (pair.Key.ToLower().Contains(word))
                  result.UnionWith(pair.Value);
            }
         }
         return result;
      }
   }
}
