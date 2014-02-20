using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSCore
{
    /// <summary>
    /// Utility methods for sorting by keys. These should be suppressed from becoming nodes, instead
    /// they will be wrapped by DS implementations that accept a key mapping function.
    /// </summary>
    public class Sorting
    {
        public static object minByKey(IList list, IList keys)
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

        public static object maxByKey(IList list, IList keys)
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

        public static IList sortByKey(IList list, IList keys)
        {
            return
                list.Cast<object>()
                    .Zip(keys.Cast<IComparable>(), (item, key) => new {item, key})
                    .OrderBy(x => x.key)
                    .Select(x => x.item)
                    .ToList();
        }
    }
}
