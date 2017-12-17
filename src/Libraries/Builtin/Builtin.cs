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
            private static List<object> Flatten(IList list, int amt, List<object> acc)
            {
                if (amt == 0)
                {
                    foreach (object item in list)
                        acc.Add(item);
                }
                else
                {
                    foreach (object item in list)
                    {
                        if (item is IList)
                            acc = Flatten(item as IList, amt - 1, acc);
                        else
                            acc.Add(item);
                    }
                }
                return acc;
            }

            private static int GetDepth(object list)
            {
                if (!(list is IList)) return 0;

                int depth = 1;
                foreach (var obj in (IList)list) // If it is a list, check if it contains a sublist
                {
                    if (obj is IList) // If it contains a sublist
                    {
                        int d = 1 + GetDepth((IList)obj);
                        depth = (depth > d) ? depth : d; // Get the maximum depth among all items
                    }
                }
                return depth;
            }

            // The indexing syntax a["foo"] or b[1] are syntactic sugar for these methods.

            public static object ValueAtIndex(Dictionary dictionary, [ArbitraryDimensionArrayImport] IList key)
            {
                var flatKeyList = Flatten(key, GetDepth(key), new List<object>());
                var values = new List<object>();
                try
                {
                    if (flatKeyList.Count == 1)
                    {
                        return dictionary.ValueAtKey((string) flatKeyList[0]);
                    }
                    values.AddRange(flatKeyList.Select(k => dictionary.ValueAtKey((string) k)));
                }
                catch (InvalidCastException e)
                {
                    throw new InvalidCastException("Non-string keys are not supported in Dictionaries");
                }
                return values;
            }

            public static object ValueAtIndex(IList list, [ArbitraryDimensionArrayImport] IList index)
            {
                var flatIndexList = Flatten(index, GetDepth(index), new List<object>());

                var count = list.Count;
                var values = new List<object>();
                try
                {
                    if (flatIndexList.Count == 1)
                    {

                        var idx = Convert.ToInt32(flatIndexList[0]);
                        while (idx < 0)
                        {
                            if (count == 0) break;
                            idx += count;
                        }
                        return list[idx];
                    }
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
                }
                catch (InvalidCastException e)
                {
                    throw new InvalidCastException("Non-numeric keys are not supported in Lists");
                }
                return values;
            }
        }
    }
}
