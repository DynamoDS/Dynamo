using System;
using System.Collections;
using System.Linq;
using Autodesk.DesignScript.Runtime;

namespace DSCore
{
    /// <summary>
    /// Utility methods for sorting by keys. These should be suppressed from becoming nodes, instead
    /// they will be wrapped by DS implementations that accept a key mapping function.
    /// </summary>
    public static class Sorting
    {
        [IsVisibleInDynamoLibrary(false)]
        public static object minByKey(
            [ArbitraryDimensionArrayImport] IList list,
            [ArbitraryDimensionArrayImport] IList keys)
        {
            object min = null;
            IComparable minKey = null;

            foreach (var pair in list.Cast<object>().Zip(keys.Cast<IComparable>(), (item, key) => new {item, key}))
            {
                if (min == null || minKey == null || pair.key.CompareTo(minKey) < 0)
                {
                    min = pair.item;
                    minKey = pair.key;
                }
            }

            return min;
        }

        [IsVisibleInDynamoLibrary(false)]
        public static object maxByKey(
            [ArbitraryDimensionArrayImport] IList list,
            [ArbitraryDimensionArrayImport] IList keys)
        {
            object max = null;
            IComparable maxKey = null;

            foreach (var pair in list.Cast<object>().Zip(keys.Cast<IComparable>(), (item, key) => new { item, key }))
            {
                if (max == null || maxKey == null || pair.key.CompareTo(maxKey) > 0)
                {
                    max = pair.item;
                    maxKey = pair.key;
                }
            }

            return max;
        }

        [IsVisibleInDynamoLibrary(false)]
        public static IList sortByKey(
            [ArbitraryDimensionArrayImport] IList list,
            [ArbitraryDimensionArrayImport] IList keys)
        {
            return
                list.Cast<object>()
                    .Zip(keys.Cast<IComparable>(), (item, key) => new {item, key})
                    .OrderBy(x => x.key)
                    .Select(x => x.item)
                    .ToList();
        }

        //public static IList groupByKey(IList list, IList keys)
        //{
        //    return
        //        list.Cast<object>().Zip(keys.Cast<object>(), (item, key) => new { item, key })
        //            .GroupBy(x => x.key)
        //            .Select(x => x.Select(y => y.item).ToList())
        //            .ToList();
        //}
    }
}
