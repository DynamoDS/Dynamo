using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Autodesk.DesignScript.Runtime;

namespace DesignScript
{
    namespace Builtin
    {
        [IsVisibleInDynamoLibrary(false)]
        public static class Get
        {
            
            // The indexing syntax a["foo"] or b[1] are syntactic sugar for these methods.

            public static object ValueAtIndex(Dictionary dictionary, string key)
            {
                return dictionary.ValueAtKey(key);
            }

            public static object ValueAtIndex(IList list, [ArbitraryDimensionArrayImport] IList index)
            {
                object[] flatIndexList;

                try
                {
                    flatIndexList = index.Cast<ArrayList>().SelectMany(i => i.Cast<object>()).ToArray();
                }
                catch
                {
                    flatIndexList = index.Cast<object>().ToArray();
                }

                var count = list.Count;
                if (flatIndexList.Length == 1)
                {
                    var idx = Convert.ToInt32(flatIndexList[0]);
                    while (idx < 0)
                    {
                        if (count == 0) break;
                        idx += count;
                    }
                    return list[idx];
                }

                var values = new List<object>();
                foreach (var flatIndex in flatIndexList)
                {
                    var idx = Convert.ToInt32(flatIndex);
                    while (idx < 0)
                    {
                        if (count == 0) break;
                        idx += count;
                    }
                    values.Add(list[idx]);
                }

                return values;
            }
        }
    }
}
