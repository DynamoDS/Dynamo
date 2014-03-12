using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Runtime;

namespace DSCore
{
    /// <summary>
    ///     Methods for creating and manipulating Lists.
    /// </summary>
    public static class List
    {
        /// <summary>
        ///     Creates a new list containing all unique items in the given list.
        /// </summary>
        /// <param name="list">List to filter duplicates out of.</param>
        public static IList UniqueItems(
            [ArbitraryDimensionArrayImport] IList list)
        {
            return list.Cast<object>().Distinct().ToList();
        }

        /// <summary>
        ///     Determines if the given list contains the given item.
        /// </summary>
        /// <param name="list">List to search in.</param>
        /// <param name="item">Item to look for.</param>
        public static bool ContainsItem(
            [ArbitraryDimensionArrayImport] IList list, 
            object item)
        {
            return list.Contains(item);
        }

        /// <summary>
        ///     Creates a new list containing the items of the given list but in reverse order.
        /// </summary>
        /// <param name="list">List to be reversed.</param>
        /// <search>reverse,list</search>
        public static IList Reverse(
            [ArbitraryDimensionArrayImport] IList list)
        {
            return list.Cast<object>().Reverse().ToList();
        }

        /*
        /// <summary>
        ///     Creates a new list containing the given items.
        /// </summary>
        /// <param name="items">Items to be stored in the new list.</param>
        public static IList Create(
            [ArbitraryDimensionArrayImport] params object[] items)
        {
            return items.ToList();
        }
        */

        /// <summary>
        ///     Build sublists from a list using DesignScript range syntax.
        /// </summary>
        /// <param name="list">The list from which to create sublists.</param>
        /// <param name="ranges">The index ranges of the sublist elements.
        /// Ex. \"{0..3,5,2}\"</param>
        /// <param name="offset">The offset to apply to the sublist.
        /// Ex. the range \"0..3\" with an offset of 2 will yield
        /// {0,1,2,3}{2,3,4,5}{4,5,6,7}...</param>
        /// <returns></returns>
        public static IList Sublists(
            [ArbitraryDimensionArrayImport] IList list,
            [ArbitraryDimensionArrayImport] IList ranges,
            int offset)
        {
            var result = new List<object>();
            int len = list.Count;

            for (int start = 0; start < len; start += offset)
            {
                var row = new List<object>();
                
                foreach (object item in ranges)
                {
                    IList subrange = null;

                    if (item is ICollection)
                        subrange = (IList)item;
                    else
                        subrange = new List<object>{item};

                    // skip subrange if exceeds the list
                    if (start + (int)subrange.Cast<object>().Max() >= len)
                        continue;
                    
                    foreach (int idx in subrange)
                    {
                        if (start + idx < len)
                            row.Add(list[start + idx]);
                    }
                }

                if (row.Count > 0)
                    result.Add(row.ToArray());
            }

            return result;
        }

        /// <summary>
        ///     Sorts a list using the built-in natural ordering.
        /// </summary>
        /// <param name="list">List to be sorted.</param>
        public static IList Sort(
            [ArbitraryDimensionArrayImport] IList list)
        {
            return list.Cast<object>().OrderBy(x => x).ToList();
        }

        /// <summary>
        ///     Returns the minimum value from a list.
        /// </summary>
        /// <param name="list">List to take the minimum value from.</param>
        public static object MinimumItem(
            [ArbitraryDimensionArrayImport] IList list)
        {
            return list.Cast<object>().Min();
        }

        /// <summary>
        ///     Returns the maximum value from a list.
        /// </summary>
        /// <param name="list">List to take the maximum value from.</param>
        /// <search>lizzard</search>
        public static object MaximumItem(
            [ArbitraryDimensionArrayImport] IList list)
        {
            return list.Cast<object>().Max();
        }

        /// <summary>
        ///     Filters a sequence by lookng up corresponding indices in a separate list of
        ///     booleans.
        /// </summary>
        /// <param name="list">List to filter.</param>
        /// <param name="mask">List of booleans representing a mask.</param>
        /// <search>filter,boolean,bool,mask,dispatch</search>
        [MultiReturn("in", "var[]")]
        [MultiReturn("out", "var[]")]
        public static Dictionary<string, object> FilterByBoolMask(
            [ArbitraryDimensionArrayImport] IList list,
            [ArbitraryDimensionArrayImport] IList mask)
        {
            var result = FilterByMaskHelper(list.Cast<object>(), mask.Cast<object>());

            return new Dictionary<string, object>
            {
                { "in", result.Item1 },
                { "out", result.Item2 }
            };
        }

        private static Tuple<ArrayList, ArrayList> FilterByMaskHelper(IEnumerable<object> list, IEnumerable<object> mask)
        {
            var inList = new ArrayList();
            var outList = new ArrayList();

            foreach (var p in list.Zip(mask, (item, flag) => new { item, flag }))
            {
                if (p.flag is IList && p.item is IList)
                {
                    var recur = FilterByMaskHelper(
                        (p.item as IList).Cast<object>(),
                        (p.flag as IList).Cast<object>());
                    inList.Add(recur.Item1);
                    outList.Add(recur.Item2);
                }
                else
                {
                    if ((bool)p.flag)
                        inList.Add(p.item);
                    else
                        outList.Add(p.item);
                }
            }

            return Tuple.Create(inList, outList);
        }

        /// <summary>
        ///     Given a list, produces the first item in the list, and a new list containing all items
        ///     except the first.
        /// </summary>
        /// <param name="list">List to be split.</param>
        [MultiReturn("first", "var")]
        [MultiReturn("rest", "var[]")]
        public static Dictionary<string, object> Deconstruct(
            [ArbitraryDimensionArrayImport] IList list)
        {
            return new Dictionary<string, object>
            {
                { "first", list[0] }, 
                { "rest", list.Cast<object>().Skip(1).ToList() }
            };
        }

        /// <summary>
        ///     Produces a new list by adding an item to the beginning of a given list.
        /// </summary>
        /// <param name="item">Item to be added.</param>
        /// <param name="list">List to add on to.</param>
        public static IList AddItemToFront(
            object item,
            [ArbitraryDimensionArrayImport] IList list)
        {
            var newList = new ArrayList { item };
            newList.AddRange(list);
            return newList;
        }

        /// <summary>
        ///     Fetches the given amount of items from the start of the given list.
        /// </summary>
        /// <param name="list">List to take from.</param>
        /// <param name="amount">
        ///     Amount of items to take. If negative, items are taken from the end of the list.
        /// </param>
        public static IList TakeItems(
            [ArbitraryDimensionArrayImport] IList list,
            int amount)
        {
            var genList = list.Cast<object>();
            return (amount < 0 ? genList.Skip(list.Count + amount) : genList.Take(amount)).ToList();
        }

        /// <summary>
        ///     Removes the given amount of items from the start of the given list.
        /// </summary>
        /// <param name="list">List to remove items from.</param>
        /// <param name="amount">
        ///     Amount of items to remove. If negative, items are removed from the end of the list.
        /// </param>
        public static IList DropItems(
            [ArbitraryDimensionArrayImport] IList list,
            int amount)
        {
            var genList = list.Cast<object>();
            return (amount < 0 ? genList.Take(list.Count + amount) : genList.Skip(amount)).ToList();
        }

        /// <summary>
        ///     Shifts indices in the given list to the right by the given amount.
        /// </summary>
        /// <param name="list">List to be shifted.</param>
        /// <param name="amount">
        ///     Amount to shift indices by. If negative, indices will be shifted to the left.
        /// </param>
        public static IList ShiftIndices(
            [ArbitraryDimensionArrayImport] IList list,
            int amount)
        {
            if (amount == 0)
                return list;

            var genList = list.Cast<object>();
            return
                (amount < 0
                    ? genList.Skip(-amount).Concat(genList.Take(-amount))
                    : genList.Skip(list.Count - amount).Concat(genList.Take(list.Count - amount))).ToList();
        }

        /// <summary>
        ///     Gets an item from the given list that's located at the specified index.
        /// </summary>
        /// <param name="list">List to fetch an item from.</param>
        /// <param name="index">Index of the item to be fetched.</param>
        public static object GetItemAtIndex(
            [ArbitraryDimensionArrayImport] IList list,
            int index)
        {
            return list[index];
        }

        /// <summary>
        ///     Gets a single sub-list from the given list, based on starting index, amount of items
        ///     to take, and a step amount.
        /// </summary>
        /// <param name="list">List to take a slice of.</param>
        /// <param name="start">Index to start the slice from.</param>
        /// <param name="count">Number of items to take in the slice.</param>
        /// <param name="step">
        ///     Amount the indices of the items are separate by in the original list.
        /// </param>
        /// <search>list,sub,sublist,slice</search>
        public static IList Slice(
            [ArbitraryDimensionArrayImport] IList list,
            int? start = null,
            int? count = null,
            int step = 1)
        {
            #region Disabled python-like slicing capability

            /*
            if (step == 0)
                throw new ArgumentException("Cannot slice a list with step of 0.", @"step");

            int _start = start ?? (step < 0 ? -1 : 0);
            int _end = end ?? (step < 0 ? -list.Count - 1 : list.Count);

            if (_start < -list.Count || _start >= list.Count)
                throw new ArgumentException("Cannot slice a list starting at a negative index.", "start");
            _start = _start >= 0 ? _start : _start + list.Count;

            if (_end < -list.Count - 1)
                throw new ArgumentException("Ending index out of range.", "end");

            if (_end > list.Count)
                _end = list.Count;

            _end = _end >= 0 ? _end : _end + list.Count;

            IList result = new ArrayList();

            if (step > 0)
            {
                if (_start > _end)
                    return result;
                for (int i = _start; i < _end; i += step)
                    result.Add(list[i]);
            }
            else
            {
                if (_end > _start)
                    return result;
                for (int i = _start; i > end; i += step)
                    result.Add(list[i]);
            }

            return result;
            */

            #endregion

            IList result = new ArrayList();

            int _start = start ?? 0;
            int end = count == null ? list.Count : _start + (int)count;

            for (int i = start ?? 0; i < end; i += step)
                result.Add(list[i]);

            return result;
        }

        /// <summary>
        ///     Removes an item from the given list at the specified index.
        /// </summary>
        /// <param name="list">List to remove an item from.</param>
        /// <param name="indices">Index or indices of the item(s) to be removed.</param>
        public static IList RemoveItemAtIndex(
            [ArbitraryDimensionArrayImport] IList list,
            [ArbitraryDimensionArrayImport] object indices)
        {
            if (indices is ICollection)
                return list.Cast<object>().Where((_, i) => !((IList)indices).Contains(i)).ToList();
            else
                return list.Cast<object>().Where((_, i) => i != (int)indices).ToList();
        }

        /// <summary>
        ///     Removes items from the given list at indices that are multiples
        ///     of the given value, after the given offset.
        /// </summary>
        /// <param name="list">List to remove items from/</param>
        /// <param name="n">Indices that are multiples of this argument will be removed.</param>
        /// <param name="offset">
        ///     Amount of items to be ignored from the start of the list.
        /// </param>
        public static IList DropEveryNthItem(
            [ArbitraryDimensionArrayImport] IList list,
            int n,
            int offset = 0)
        {
            return list.Cast<object>().Skip(offset).Where((_, i) => (i + 1)%n != 0).ToList();
        }

        /// <summary>
        ///     Fetches items from the given list at indices that are multiples
        ///     of the given value, after the given offset.
        /// </summary>
        /// <param name="list">List to take items from.</param>
        /// <param name="n">
        ///     Indices that are multiples of this number (after the offset)
        ///     will be fetched.
        /// </param>
        /// <param name="offset">
        ///     Amount of items to be ignored from the start of the list.
        /// </param>
        public static IList TakeEveryNthItem(
            [ArbitraryDimensionArrayImport] IList list,
            int n,
            int offset = 0)
        {
            return list.Cast<object>().Skip(offset).Where((_, i) => (i + 1)%n == 0).ToList();
        }

        /// <summary>
        ///     An Empty List.
        /// </summary>
        public static IList Empty
        {
            get { return new ArrayList(); }
        }

        /// <summary>
        ///     Determines if the given list is empty.
        /// </summary>
        /// <param name="list">List to check for items.</param>
        public static bool IsEmpty(
            [ArbitraryDimensionArrayImport] IList list)
        {
            return list.Count == 0;
        }

        /// <summary>
        ///     Gets the number of items stored in the given list.
        /// </summary>
        /// <param name="list">List to get the item count of.</param>
        public static int Count(
            [ArbitraryDimensionArrayImport] IList list)
        {
            return list.Count;
        }

        /// <summary>
        ///     Concatenates all given lists into a single list.
        /// </summary>
        /// <param name="lists">Lists to join into one.</param>
        public static IList Join(
            [ArbitraryDimensionArrayImport] params IList[] lists)
        {
            var result = new ArrayList();
            foreach (IList list in lists)
                result.AddRange(list);
            return result;
        }

        /// <summary>
        ///     Gets the first item in a list.
        /// </summary>
        /// <param name="list">List to get the first item from.</param>
        public static object FirstItem(
            [ArbitraryDimensionArrayImport] IList list)
        {
            return list[0];
        }

        /// <summary>
        ///     Removes the first item from the given list.
        /// </summary>
        /// <param name="list">List to get the rest of.</param>
        public static IList RestOfItems(
            [ArbitraryDimensionArrayImport] IList list)
        {
            return list.Cast<object>().Skip(1).ToList();
        }

        /// <summary>
        ///     Creates a list of lists out of an existing list with each sub-list containing
        ///     the given amount of items.
        /// </summary>
        /// <param name="list">List to chop up.</param>
        /// <param name="subLength">Length of each new sub-list.</param>
        public static IList Chop(
            [ArbitraryDimensionArrayImport] IList list, 
            int subLength)
        {
            if (list.Count < subLength)
                return list;

            var finalList = new ArrayList();
            var currList = new ArrayList();
            int count = 0;

            foreach (object v in list)
            {
                count++;
                currList.Add(v);

                if (count == subLength)
                {
                    finalList.Add(currList);
                    currList = new ArrayList();
                    count = 0;
                }
            }

            if (currList.Count > 0)
                finalList.Add(currList);

            return finalList;
        }

        /// <summary>
        ///     Create a diagonal lists of lists from top left to lower right.
        /// </summary>
        /// <param name="flatList">A list</param>
        /// <param name="subLength">Length of each new sub-list.</param>
        public static IList DiagonalRight(
            [ArbitraryDimensionArrayImport] IList list,
            int subLength)
        {
            var flatList = list.Cast<IList<object>>().SelectMany(i => i).ToArray();

            if (flatList.Count() < subLength)
                return list;

            var finalList = new ArrayList();
            var currList = new ArrayList();

            var startIndices = new List<int>();

            //get indices along 'side' of array
            for (int i = subLength; i < flatList.Count(); i += subLength)
                startIndices.Add(i);

            startIndices.Reverse();

            //get indices along 'top' of array
            for (int i = 0; i < subLength; i++)
                startIndices.Add(i);

            foreach (int start in startIndices)
            {
                int index = start;

                while (index < flatList.Count())
                {
                    var currentRow = (int)System.Math.Ceiling((index + 1)/(double)subLength);
                    currList.Add(flatList[index]);
                    index += subLength + 1;

                    //ensure we are skipping a row to get the next index
                    var nextRow = (int)System.Math.Ceiling((index + 1) / (double)subLength);
                    if (nextRow > currentRow + 1 || nextRow == currentRow)
                        break;
                }
                finalList.Add(currList);
                currList = new ArrayList();
            }

            if (currList.Count > 0)
                finalList.Add(currList);

            return finalList;
        }

        /// <summary>
        ///     Create a diagonal lists of lists from top right to lower left.
        /// </summary>
        /// <param name="flatList">A list.</param>
        /// <param name="rowLength">Length of each new sib-list.</param>
        public static IList DiagonalLeft(
            [ArbitraryDimensionArrayImport] IList list,
            int rowLength)
        {
            var flatList = list.Cast<IList<object>>().SelectMany(i => i).ToArray();

            if (flatList.Count() < rowLength)
                return list;

            var finalList = new ArrayList();

            var startIndices = new List<int>();

            //get indices along 'top' of array
            for (int i = 0; i < rowLength; i++)
                startIndices.Add(i);

            //get indices along 'side' of array
            for (int i = rowLength - 1 + rowLength; i < flatList.Count(); i += rowLength)
                startIndices.Add(i);

            foreach (int start in startIndices)
            {
                int index = start;
                var currList = new ArrayList();

                while (index < flatList.Count())
                {
                    var currentRow = (int)System.Math.Ceiling((index + 1)/(double)rowLength);
                    currList.Add(flatList.ElementAt(index));
                    index += rowLength - 1;

                    //ensure we are skipping a row to get the next index
                    var nextRow = (int)System.Math.Ceiling((index + 1) / (double)rowLength);
                    if (nextRow > currentRow + 1 || nextRow == currentRow)
                        break;
                }
                finalList.Add(currList);
            }

            return finalList;
        }

        /*

        /// <summary>
        ///     Swaps rows and columns in a list of lists.
        /// </summary>
        /// <param name="lists">A list of lists to be transposed.</param>
        public static IList Transpose(IList lists)
        {
            if (lists.Count == 0)
                return lists;

            var genList = lists.Cast<IList>();

            // ReSharper disable PossibleMultipleEnumeration
            var argList =
                genList.First().Cast<object>().Select(x => new ArrayList { x } as IList).ToList();

            var query =
                genList.Skip(1)
                       .SelectMany(
                           list => list.Cast<object>().Zip(argList, (o, objs) => new { o, objs }));
            // ReSharper restore PossibleMultipleEnumeration

            foreach (var pair in query)
                pair.objs.Add(pair.o);

            return argList;
        }
        
        */
        
        /// <summary>
        ///     Creates a list containing the given item the given number of times.
        /// </summary>
        /// <param name="item">The item to repeat.</param>
        /// <param name="amount">The number of times to repeat.</param>
        public static IList OfRepeatedItem(object item, int amount)
        {
            return Enumerable.Repeat(item, amount).ToList();
        }

        /// <summary>
        ///     Creates a new list by concatenining copies of a given list.
        /// </summary>
        /// <param name="list">List to repeat.</param>
        /// <param name="amount">Number of times to repeat.</param>
        public static IList Repeat(
            [ArbitraryDimensionArrayImport] IList list,
            int amount)
        {
            var result = new ArrayList();
            while (amount > 0)
            {
                result.AddRange(list);
                amount--;
            }
            return result;
        }

        /// <summary>
        ///     Retrieves the last item in a list.
        /// </summary>
        /// <param name="list">List to get the last item of.</param>
        public static object LastItem(
            [ArbitraryDimensionArrayImport] IList list)
        {
            if (list.Count == 0)
                throw new ArgumentException("Cannot get the last item in an empty list.", "list");

            return list[list.Count - 1];
        }

        /// <summary>
        ///     Shuffles a list, randomizing the order of its items.
        /// </summary>
        /// <param name="list">List to shuffle.</param>
        public static IList Shuffle(
            [ArbitraryDimensionArrayImport] IList list)
        {
            var rng = new Random();
            return list.Cast<object>().OrderBy(_ => rng.Next()).ToList();
        }
        
        /// <summary>
        ///     Produces all permutations of the given length of a given list.
        /// </summary>
        /// <param name="list">List to permute.</param>
        /// <param name="length">Length of each permutation.</param>
        public static IList Permutations(
            [ArbitraryDimensionArrayImport] IList list,
            int? length = null)
        {
            return
                GetPermutations(list.Cast<object>(), length ?? list.Count)
                    .Select(x => x.ToList())
                    .ToList();
        }

        /// <summary>
        ///     Produces all combination of the given length of a given list.
        /// </summary>
        /// <param name="list">List to generate combinations of.</param>
        /// <param name="length">Length of each combination.</param>
        /// <param name="replace">
        ///     Whether or not items are removed once selected for combination, defaults
        ///     to false.
        /// </param>
        public static IList Combinations(
            [ArbitraryDimensionArrayImport] IList list,
            int length,
            bool replace = false)
        {
            return
                GetCombinations(list.Cast<object>(), length, replace)
                    .Select(x => x.ToList())
                    .ToList();
        }

        #region Combinatorics Helpers

        private static IEnumerable<T> Singleton<T>(T t)
        {
            yield return t;
        }

        private static IEnumerable<IEnumerable<T>> GetCombinations<T>(
            IEnumerable<T> items, int count, bool replace)
        {
            int i = 0;
            foreach (var item in items)
            {
                if (count == 1)
                    yield return Singleton(item);
                else
                {
                    foreach (var result in GetCombinations(items.Skip(replace ? i : i + 1), count - 1, replace))
                        yield return Singleton(item).Concat(result);
                }

                ++i;
            }
        }

        private static IEnumerable<IEnumerable<T>> GetPermutations<T>(
            IEnumerable<T> items, int count)
        {
            int i = 0;
            foreach (var item in items)
            {
                if (count == 1)
                    yield return Singleton(item);
                else
                {
                    var perms = GetPermutations(items.Take(i).Concat(items.Skip(i + 1)), count - 1);
                    foreach (var result in perms)
                        yield return Singleton(item).Concat(result);
                }

                ++i;
            }
        }

        #endregion

        /* Disabled Higher-order functions
         
        /// <summary>
        ///     Flattens a nested list of lists into a single list containing no
        ///     sub-lists.
        /// </summary>
        /// <param name="list">List to flatten.</param>
        public static IList FlattenCompletely(IList list)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Flattens a nested list of lists by a certain amount.
        /// </summary>
        /// <param name="list">List to flatten.</param>
        /// <param name="amt">Layers of nesting to remove.</param>
        /// s
        public static IList Flatten(IList list, int amt)
        {
            throw new NotImplementedException();
        }
        
        
        /// <summary>
        ///     Returns the minimum value from a list using a key projection. The minimum
        ///     value is the item in the list that the key projection produces the smallest
        ///     value for.
        /// </summary>
        /// <param name="list">List to take the minimum value from.</param>
        /// <param name="keyProjector">
        ///     Function that consumes an item from the list and produces an orderable value.
        /// </param>
        public static object MinimumItemByKey(IList list, Delegate keyProjector)
        {
            if (list.Count == 0)
                throw new ArgumentException("Cannot take the minimum value of an empty list.", "list");

            object min = list[0];
            var minProjection = (IComparable)keyProjector.DynamicInvoke(min);

            foreach (var item in list.Cast<object>().Skip(1))
            {
                var projection = (IComparable)keyProjector.DynamicInvoke(item);
                if (projection.CompareTo(minProjection) < 0)
                {
                    min = item;
                    minProjection = projection;
                }
            }

            return min;
        }
         
        /// <summary>
        ///     Returns the maximum value from a list using a key projection. The maximum
        ///     value is the item in the list that the key projection produces the largest
        ///     value for.
        /// </summary>
        /// <param name="list">List to take the maximum value from.</param>
        /// <param name="keyProjector">
        ///     Function that consumes an item from the list and produces an orderable value.
        /// </param>
        public static object MaximumItemByKey(IList list, Delegate keyProjector)
        {
            if (list.Count == 0)
                throw new ArgumentException("Cannot take the maximum value of an empty list.", "list");

            object max = list[0];
            var maxProjection = (IComparable)keyProjector.DynamicInvoke(max);

            foreach (var item in list.Cast<object>().Skip(1))
            {
                var projection = (IComparable)keyProjector.DynamicInvoke(item);
                if (projection.CompareTo(maxProjection) > 0)
                {
                    max = item;
                    maxProjection = projection;
                }
            }

            return max;
        }

        /// <summary>
        ///     Creates a new list containing all the items of an old list for which
        ///     the given predicate function returns True.
        /// </summary>
        /// <param name="list">List to be filtered.</param>
        /// <param name="predicate">
        ///     Function to be applied to all items in the list. All items that make the
        ///     predicate produce True will be stored in the output list.
        /// </param>
        public static IList Filter(IList list, Delegate predicate)
        {
            return list.Cast<object>().Where(x => (bool)predicate.DynamicInvoke(x)).ToList();
        }

        //TODO: This could be combined with Filter into a multi-output node
        /// <summary>
        ///     Creates a new list containing all the items of an old list for which
        ///     the given predicate function returns False.
        /// </summary>
        /// <param name="list">List to be filtered.</param>
        /// <param name="predicate">
        ///     Function to be applied to all items in the list. All items that make the
        ///     predicate produce False will be stored in the output list.
        /// </param>
        public static IList FilterOut(IList list, Delegate predicate)
        {
            return list.Cast<object>().Where(x => !(bool)predicate.DynamicInvoke(x)).ToList();
        }

        /// <summary>
        ///     Sorts a list using a key projection. A projection is created for each item,
        ///     and that value is used to order the original items.
        /// </summary>
        /// <param name="list">List to be sorted.</param>
        /// <param name="keyProjector">
        ///     Function that consumes an item from the list and produces an orderable value.
        /// </param>
        /// <returns></returns>
        public static IList SortByKey(IList list, Delegate keyProjector)
        {
            return list.Cast<object>().OrderBy(x => keyProjector.DynamicInvoke(x)).ToList();
        }

        /// <summary>
        ///     Sorts a list using a comparison function. Given two items from the list, the comparison
        ///     function determines which item should appear first in the sorted list.
        /// </summary>
        /// <param name="list">List to be sorted.</param>
        /// <param name="comparison">
        ///     Function that consumes two items from the list and produces a value determining the order
        ///     of the two items as follows: a value less than zero if the first item should appear
        ///     before the second, zero if the values are considered the same, and a value greater than
        ///     zero if the second item should appear before the first.
        /// </param>
        public static IList SortByComparison(IList list, Delegate comparison)
        {
            var rtn = list.Cast<object>().ToList();
            rtn.Sort((x, y) => (int)comparison.DynamicInvoke(x, y));
            return rtn;
        }
        
        /// <summary>
        ///     Reduces a list of values into a new value using a reduction function.
        /// </summary>
        /// <param name="list">List to be reduced.</param>
        /// <param name="seed">
        ///     Starting value for the reduction. If the list being reduced is
        ///     empty, this will immediately be returned.
        /// </param>
        /// <param name="reducer">
        ///     A function that consumes an item in the list and a reduction state. It must produce
        ///     a new reduction state by combining the item with the current reduction state.
        /// </param>
        public static TState Reduce<T, TState>(IEnumerable<T> list, TState seed, Func<T, TState, TState> reducer)
        {
            return list.Aggregate(seed, (a, x) => reducer(x, a));
        }

        /// <summary>
        ///     Produces a new list by applying a projection function to each item of the input list(s) and
        ///     storing the result.
        /// </summary>
        /// <param name="projection">
        ///     Function that consumes an item from each input list and produces a value that is stored
        ///     in the output list.
        /// </param>
        /// <param name="lists">Lists to be combined/mapped into a new list.</param>
        public static IList<object> Map(Function.MapDelegate projection, params IEnumerable<object>[] lists)
        {
            if (!lists.Any())
                throw new ArgumentException("Need at least one list to map.");

            IEnumerable<List<object>> argList = lists[0].Select(x => new List<object> { x });

            foreach (var pair in
                lists.Skip(1).SelectMany(list => list.Zip(argList, (o, objects) => new { o, objects })))
                pair.objects.Add(pair.o);

            return argList.Select(x => projection(x.ToArray())).ToList();
        }

        /// <summary>
        ///     Produces a new list by applying a projection function to all combinations of items from the
        ///     input lists and storing the result.
        /// </summary>
        /// <param name="projection">
        ///     Function that consumes an item from each input list and produces a value that is stored
        ///     in the output list.
        /// </param>
        /// <param name="lists">Lists to take the cartesion product of.</param>
        public static IList<object> CartesianProduct(
            Function.MapDelegate projection,
            params IEnumerable<object>[] lists)
        {
            if (!lists.Any())
                throw new ArgumentException("Need at least one list to map.");

            throw new NotImplementedException();
        }

        /// <summary>
        ///     Applies a function to each item of the input list(s). Does not accumulate results.
        /// </summary>
        /// <param name="action">
        ///     Function that consumed an item from each input list. Return value is ignored.
        /// </param>
        /// <param name="lists">Lists to be iterated over.</param>
        public static void ForEach(Function.MapDelegate action, params IEnumerable<object>[] lists)
        {
            if (!lists.Any())
                throw new ArgumentException("Need at least one list to iterate over.");

            IEnumerable<List<object>> argList = lists[0].Select(x => new List<object> { x });

            foreach (var pair in
                lists.Skip(1).SelectMany(list => list.Zip(argList, (o, objects) => new { o, objects })))
                pair.objects.Add(pair.o);

            foreach (var args in argList)
                action(args.ToArray());
        }

        /// <summary>
        ///     Determines if the given predicate function returns True when applied to all of the
        ///     items in the given list.
        /// </summary>
        /// <param name="predicate">
        ///     Function to be applied to all items in the list, returns a boolean value.
        /// </param>
        /// <param name="list">List to be tested.</param>
        public static bool TrueForAllItems(IList list, Delegate predicate)
        {
            return list.Cast<object>().All(x => (bool)predicate.DynamicInvoke(x));
        }

        /// <summary>
        ///     Determines if the given predicate function returns True when applied to any of the
        ///     items in the given list.
        /// </summary>
        /// <param name="predicate">
        ///     Function to be applied to all items in the list, returns a boolean value.
        /// </param>
        /// <param name="list">List to be tested.</param>
        public static bool TrueForAnyItems(IList list, Delegate predicate)
        {
            return list.Cast<object>().Any(x => (bool)predicate.DynamicInvoke(x));
        }

        /// <summary>
        ///     Applies a key mapping function to each item in the given list, and produces
        ///     a new list of lists where each sublist contains items for which the key
        ///     mapping function produced the same result.
        /// </summary>
        /// <param name="list">List to group into sublists.</param>
        /// <param name="keyProjector">Function that produces grouping values.</param>
        public static IList GroupByKey(IList list, Delegate keyProjector)
        {
            return
                list.Cast<object>()
                    .GroupBy(x => keyProjector.DynamicInvoke(x))
                    .Select(x => x.ToList() as IList)
                    .ToList();
        }

        /// <summary>
        ///     Applies a function to each item in the given list, and constructs a new
        ///     list containing the results of each function application.
        /// </summary>
        /// <param name="list">List to map.</param>
        /// <param name="mapFunc">Function to apply to each element.</param>
        public static IList Map(IList list, Delegate mapFunc)
        {
            return list.Cast<object>().Select(x => mapFunc.DynamicInvoke(x)).ToList();
        }

        private static IEnumerable<object> GetArgs(IEnumerable<IList> lists, int index)
        {
            return lists.Where(argList => index < argList.Count).Select(x => x[index]);
        }

        /// <summary>
        ///     Combines multiple lists into a single list by taking items at corresponding
        ///     indices and applying a function to them, and using the results to construct
        ///     a new list.
        /// </summary>
        /// <param name="combinator">Function to apply to an element from each list.</param>
        /// <param name="lists">Lists to combine.</param>
        public static IList Combine(Delegate combinator, params IList[] lists)
        {
            var result = new ArrayList();

            int i = 0;
            while (true)
            {
                var args = GetArgs(lists, i).ToArray();

                if (!args.Any()) break;

                result.Add(combinator.DynamicInvoke(args));
                i++;
            }

            return result;
        }

        /// <summary>
        ///     Reduces multiple lists into a single value by taking items at corresponding
        ///     indices and applying a function to them and a "reduced value". The result
        ///     is then used as a new reduced value for the next application. When all lists
        ///     are empty, the final reduced value is returned.
        /// </summary>
        /// <param name="reductor">
        ///     Reductor function: consumes an item from each list and a reduced value, and
        ///     produces a new reduced value.
        /// </param>
        /// <param name="initial">The initial reduced value.</param>
        /// <param name="lists">Lists to reduce.</param>
        public static object Reduce(Delegate reductor, object initial, params IList[] lists)
        {
            int i = 0;
            while (true)
            {
                var args = GetArgs(lists, i);

                if (!args.Any()) break;

                initial = reductor.DynamicInvoke(args.Concat(new[] { initial }).ToArray());
                i++;
            }

            return initial;
        }
        */
    }


    //TODO
    /*
    public class BuildSublists : NodeModel
    {
        
    }
    */
}
