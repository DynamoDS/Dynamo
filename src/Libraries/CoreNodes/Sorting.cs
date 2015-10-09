using System;
using System.Collections;
using System.Linq;
using Autodesk.DesignScript.Runtime;

namespace DSCore
{
    // ReSharper disable InconsistentNaming
    /// <summary>
    /// Utility methods for sorting by keys. These should be suppressed from becoming nodes, instead
    /// they will be wrapped by DS implementations that accept a key mapping function.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public static class Sorting
    {
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

        public static IList sortByKey(
            [ArbitraryDimensionArrayImport] IList list,
            [ArbitraryDimensionArrayImport] IList keys)
        {
            var pairs = list.Cast<object>()
                    .Zip(keys.Cast<object>(), (item, key) => new { item, key });

            var numberKeyPairs = pairs.Where(pair => pair.key is double || pair.key is int || pair.key is float);
            // We don't use Except, because Except doesn't return duplicates.
            var keyPairs = pairs.Where(
                pair =>
                    !numberKeyPairs.Any(
                        numberPair => numberPair.item == pair.item && numberPair.key == pair.key));

            // Sort.
            numberKeyPairs = numberKeyPairs.OrderBy(pair => Convert.ToDouble(pair.key));
            keyPairs = keyPairs.OrderBy(pair => pair.key);

            // First items with number keys, then items with letter keys.
            var sortedPairs = numberKeyPairs.Concat(keyPairs);

            var sortedList = sortedPairs.Select(x => x.item);
            var sortedKeys = sortedPairs.Select(x => x.key);

            return new ArrayList { sortedList, sortedKeys };
        }

        public static IList groupByKey(IList list, IList keys)
        {
            return
                list.Cast<object>().Zip(keys.Cast<object>(), (item, key) => new { item, key })
                    .GroupBy(x => x.key)
                    .Select(x => x.Select(y => y.item).ToList())
                    .ToList();
        }

        public static IList uniqueItems(IList list)
        {
            return list.Cast<object>().Distinct().ToList();
        }
    }
    // ReSharper restore InconsistentNaming
}
