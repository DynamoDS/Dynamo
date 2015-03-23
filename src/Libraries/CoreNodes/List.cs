#region
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Autodesk.DesignScript.Runtime;

#endregion

namespace DSCore
{
    /// <summary>
    ///     Methods for creating and manipulating Lists.
    /// </summary>
    public static class List
    {
        /// <summary>
        ///     An Empty List.
        /// </summary>
        /// <returns name="list">Empty list.</returns>
        /// <search>empty list, emptylist,[]</search>
        public static IList Empty
        {
            get { return new ArrayList(); }
        }

        /// <summary>
        ///     Creates a new list containing all unique items in the given list.
        /// </summary>
        /// <param name="list">List to filter duplicates out of.</param>
        /// <returns name="list">Filtered list.</returns>
        /// <search>removes,duplicates,remove duplicates,cull duplicates,distinct</search>
        public static IList UniqueItems(IList list)
        {
            return list.Cast<object>().Distinct(DistinctComparer.Instance).ToList();
        }

        /// <summary>
        ///     Determines if the given list contains the given item.
        /// </summary>
        /// <param name="list">List to search in.</param>
        /// <param name="item">Item to look for.</param>
        /// <returns name="bool">Whether list contains the given item.</returns>
        /// <search>item,search,in</search>
        public static bool ContainsItem(IList list, object item)
        {
            return list.Contains(item);
        }

        /// <summary>
        ///     Creates a new list containing the items of the given list but in reverse order.
        /// </summary>
        /// <param name="list">List to be reversed.</param>
        /// <returns name="list">New list.</returns>
        /// <search>flip</search>
        public static IList Reverse(IList list)
        {
            return list.Cast<object>().Reverse().ToList();
        }

        /// <summary>
        ///     Creates a new list containing the given items.
        /// </summary>
        /// <param name="items">Items to be stored in the new list.</param>
        public static IList __Create(IList items)
        {
            return items;
        }

        /// <summary>
        ///     Build sublists from a list using DesignScript range syntax.
        /// </summary>
        /// <param name="list">The list from which to create sublists.</param>
        /// <param name="ranges">
        ///     The index ranges of the sublist elements.
        ///     Ex. \"{0..3,5,2}\"
        /// </param>
        /// <param name="offset">
        ///     The offset to apply to the sublist.
        ///     Ex. the range \"0..3\" with an offset of 2 will yield
        ///     {0,1,2,3}{2,3,4,5}{4,5,6,7}...
        /// </param>
        /// <param name="keepIncomplete">
        ///     Determines if ranges where some indices are out of bounds are kept.
        ///     If true (default): All ranges are kept, out of bounds indices are ommitted.
        ///     If false: Any ranges with out of bounds indices are ommitted.</param>
        /// <returns name="lists">Sublists of the given list.</returns>
        /// <search>sublists,build sublists,subset,</search>
        public static IList Sublists(IList list, IList ranges, int offset)
        {
            var result = new ArrayList();
            int len = list.Count;

            if (offset <= 0)
                throw new ArgumentException("Must be greater than zero.", "offset");

            for (int start = 0; start < len; start += offset)
            {
                var row = new List<object>();

                foreach (object item in ranges)
                {
                    List<int> subrange;

                    if (item is ICollection)
                        subrange = ((IList)item).Cast<int>().Select(Convert.ToInt32).ToList();
                    else
                        subrange = new List<int> { Convert.ToInt32(item) };

                    row.AddRange(
                        subrange.Where(idx => start + idx < len).Select(idx => list[start + idx]));
                }

                if (row.Count > 0)
                    result.Add(row);
            }

            return result;
        }

        /// <summary>
        ///     Sorts a list using the built-in natural ordering.
        /// </summary>
        /// <param name="list">List to be sorted.</param>
        /// <returns name="list">Sorted list.</returns>
        /// <search>sort,order,sorted</search>
        public static IList Sort(IEnumerable<object> list)
        {
            return list.OrderBy(x => x, new ObjectComparer()).ToList();
        }

        /// <summary>
        ///     Returns the minimum value from a list.
        /// </summary>
        /// <param name="list">List to take the minimum value from.</param>
        /// <returns name="min">Minimum value from the list.</returns>
        /// <search>least,smallest,find min</search>
        public static object MinimumItem(IEnumerable<object> list)
        {
            return list.Min<object, object>(DoubleConverter);
        }

        /// <summary>
        ///     Returns the maximum value from a list.
        /// </summary>
        /// <param name="list">List to take the maximum value from.</param>
        /// <returns name="max">Maximum value from the list.</returns>
        /// <search>greatest,largest,biggest,find max</search>
        public static object MaximumItem(IEnumerable<object> list)
        {
            return list.Max<object, object>(DoubleConverter);
        }

        /// <summary>
        /// Converts integer to double, else returns the input object.
        /// </summary>
        private static object DoubleConverter(object obj)
        {
            if (obj is int)
                return Convert.ToDouble(obj);
            return obj;
        }

        /// <summary>
        ///     Filters a sequence by lookng up corresponding indices in a separate list of
        ///     booleans.
        /// </summary>
        /// <param name="list">List to filter.</param>
        /// <param name="mask">List of booleans representing a mask.</param>
        /// <returns name="in">Items whose mask index is true.</returns>
        /// <returns name="out">Items whose mask index is false.</returns>
        /// <search>filter,in,out,mask,dispatch,bool filter,boolfilter,bool filter</search>
        [MultiReturn(new[] { "in", "out" })]
        public static Dictionary<string, object> FilterByBoolMask(IList list, IList mask)
        {
            Tuple<ArrayList, ArrayList> result = FilterByMaskHelper(
                list.Cast<object>(),
                mask.Cast<object>());

            return new Dictionary<string, object>
            {
                { "in", result.Item1 },
                { "out", result.Item2 }
            };
        }

        private static Tuple<ArrayList, ArrayList> FilterByMaskHelper(
            IEnumerable<object> list, IEnumerable<object> mask)
        {
            var inList = new ArrayList();
            var outList = new ArrayList();

            foreach (var p in list.Zip(mask, (item, flag) => new { item, flag }))
            {
                if (p.flag is IList && p.item is IList)
                {
                    Tuple<ArrayList, ArrayList> recur =
                        FilterByMaskHelper(
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
        /// <returns name="first">First item in the list.</returns>
        /// <returns name="rest">Rest of the list.</returns>
        /// <search>first,rest,list split</search>
        [MultiReturn(new[] { "first", "rest" })]
        public static IDictionary Deconstruct(IList list)
        {
            return new Dictionary<string, object>
            {
                { "first", list[0] },
                { "rest", list.Cast<object>().Skip(1).ToList() }
            };
        }

        /// <summary>
        ///     Adds an item to the beginning of a list.
        /// </summary>
        /// <param name="item">Item to be added.</param>
        /// <param name="list">List to add on to.</param>
        /// <returns name="list">New list.</returns>
        /// <search>insert,add,item,front,start,begin</search>
        public static IList AddItemToFront([ArbitraryDimensionArrayImport] object item, IList list)
        {
            var newList = new ArrayList { item };
            newList.AddRange(list);
            return newList;
        }

        /// <summary>
        ///     Adds an item to the end of a list.
        /// </summary>
        /// <param name="item">Item to be added.</param>
        /// <param name="list">List to add on to.</param>
        /// <search>insert,add,item,end</search>
        public static IList AddItemToEnd(object item, IList list)
        {
            return new ArrayList(list) //Clone original list
            {
                item //Add item to the end of cloned list.
            };
        }

        /// <summary>
        ///     Fetches an amount of items from the start of the list.
        /// </summary>
        /// <param name="list">List to take from.</param>
        /// <param name="amount">
        ///     Amount of items to take. If negative, items are taken from the end of the list.
        /// </param>
        /// <returns name="list">List of extracted items.</returns>
        /// <search>get,sub,sublist,extract</search>
        public static IList TakeItems(IList list, int amount)
        {
            IEnumerable<object> genList = list.Cast<object>();
            return (amount < 0 ? genList.Skip(list.Count + amount) : genList.Take(amount)).ToList();
        }

        /// <summary>
        ///     Removes an amount of items from the start of the list.
        /// </summary>
        /// <param name="list">List to remove items from.</param>
        /// <param name="amount">
        ///     Amount of items to remove. If negative, items are removed from the end of the list.
        /// </param>
        /// <returns name="list">List of remaining items.</returns>
        /// <search>drop,remove,shorten</search>
        public static IList DropItems(IList list, int amount)
        {
            IEnumerable<object> genList = list.Cast<object>();
            return (amount < 0 ? genList.Take(list.Count + amount) : genList.Skip(amount)).ToList();
        }

        /// <summary>
        ///     Shifts indices in the list to the right by the given amount.
        /// </summary>
        /// <param name="list">List to be shifted.</param>
        /// <param name="amount">
        ///     Amount to shift indices by. If negative, indices will be shifted to the left.
        /// </param>
        /// <returns name="list">Shifted list.</returns>
        /// <search>shift,slide,offset</search>
        public static IList ShiftIndices(IList list, int amount)
        {
            if (amount == 0)
                return list;

            IEnumerable<object> genList = list.Cast<object>();
            return
                (amount < 0
                    ? genList.Skip(-amount).Concat(genList.Take(-amount))
                    : genList.Skip(list.Count - amount).Concat(genList.Take(list.Count - amount)))
                    .ToList();
        }

        /// <summary>
        ///     Gets an item from the given list that's located at the specified index.
        /// </summary>
        /// <param name="list">List to fetch an item from.</param>
        /// <param name="index">Index of the item to be fetched.</param>
        /// <returns name="item">Item in the list at the given index.</returns>
        /// <search>get,item,index,fetch,at,getfrom,get from,extract</search>
        public static object GetItemAtIndex(IList list, int index)
        {
            return list[index];
        }

        /// <summary>
        ///     Replace an item from the given list that's located at the specified index.
        /// </summary>
        /// <param name="list">List to replace an item in.</param>
        /// <param name="index">Index of the item to be replaced.</param>
        /// <param name="item">The item to insert.</param>
        /// <returns name="list">A new list with the item replaced.</returns>
        /// <search>replace,switch</search>
        public static IList ReplaceItemAtIndex(IList list, int index, [ArbitraryDimensionArrayImport] object item)
        {
            if (index < 0)
            {
                index = list.Count + index;
            }

            if (index >= list.Count || index < 0)
            {
                throw new IndexOutOfRangeException();
            }

            // copy the list, insert and return
            var newList = new ArrayList(list);
            newList[index] = item;
            return newList;
        }

        /// <summary>
        ///     Gets a single sub-list from the given list, based on starting index, ending index,
        ///     and a step amount.
        /// </summary>
        /// <param name="list">List to take a slice of.</param>
        /// <param name="start">Index to start the slice from.</param>
        /// <param name="end">Index to end the slice at.</param>
        /// <param name="step">
        ///     Amount the indices of the items are separate by in the original list.
        /// </param>
        /// <returns name="items">Items in the slice of the given list.</returns>
        /// <search>list,sub,sublist,subrange,get sublist</search>
        public static IList Slice(IList list, int? start = null, int? end = null, int step = 1)
        {
            if (step == 0)
                throw new ArgumentException("Cannot slice a list with step of 0.", @"step");

            int _start = start ?? (step < 0 ? -1 : 0);
            int _end = end ?? (step < 0 ? -list.Count - 1 : list.Count);

            _start = _start >= 0 ? _start : _start + list.Count;

            if (_start < 0)
                _start = 0;

            _end = _end >= 0 ? _end : _end + list.Count;

            if (_end > list.Count)
                _end = list.Count;

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
                for (int i = _start; i > _end; i += step)
                    result.Add(list[i]);
            }

            return result;
        }

        /// <summary>
        ///     Removes an item from the given list at the specified index.
        /// </summary>
        /// <param name="list">List to remove an item or items from.</param>
        /// <param name="indices">Index or indices of the item(s) to be removed.</param>
        /// <returns name="list">List with items removed.</returns>
        /// <search>index,indices,cull,remove,item</search>
        public static IList RemoveItemAtIndex(IList list, int[] indices)
        {
            return list.Cast<object>().Where((_, i) => !indices.Contains(i)).ToList();
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
        /// <returns name="list">List with items removed.</returns>
        /// <search>nth,remove,cull,every</search>
        public static IList DropEveryNthItem(IList list, int n, int offset = 0)
        {
            return list.Cast<object>().Where((_, i) => (i + 1 - offset)%n != 0).ToList();
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
        /// <returns name="items">Items from the list.</returns>
        /// <search>fetch,take,every,nth</search>
        public static IList TakeEveryNthItem(IList list, int n, int offset = 0)
        {
            return list.Cast<object>().Where((_, i) => (i + 1 - offset)%n == 0).ToList();
        }

        /// <summary>
        ///     Determines if the given list is empty.
        /// </summary>
        /// <param name="list">List to check for items.</param>
        /// <returns name="bool">Whether the list is empty.</returns>
        /// <search>test,is,empty,null,count</search>
        public static bool IsEmpty(IList list)
        {
            return list.Count == 0;
        }

        /// <summary>
        ///     Gets the number of items stored in the given list.
        /// </summary>
        /// <param name="list">List to get the item count of.</param>
        /// <returns name="count">List length.</returns>
        /// <search>listlength,list length,count,size,sizeof</search>
        public static int Count(IList list)
        {
            return list.Count;
        }

        /// <summary>
        ///     Concatenates all given lists into a single list.
        /// </summary>
        /// <param name="lists">Lists to join into one.</param>
        /// <returns name="list">Joined list.</returns>
        /// <search>join lists,merge,concatenate</search>
        public static IList Join(params IList[] lists)
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
        /// <returns name="item">First item in the list.</returns>
        /// <search>get,fetch,first,item,start</search>
        public static object FirstItem(IList list)
        {
            return list[0];
        }

        /// <summary>
        ///     Removes the first item from the given list.
        /// </summary>
        /// <param name="list">List to get the rest of.</param>
        /// <returns name="rest">Rest of the list.</returns>
        /// <search>get,fetch,rest,end,rest of list</search>
        public static IList RestOfItems(IList list)
        {
            return list.Cast<object>().Skip(1).ToList();
        }

        /// <summary>
        ///     Chop a list into a set of lists each containing the given amount of items.
        /// </summary>
        /// <param name="list">List to chop up.</param>
        /// <param name="subLength">Length of each new sub-list.</param>
        /// <returns name="lists">List of lists.</returns>
        /// <search>sublists,build sublists,slices,partitions,cut</search>
        public static IList Chop(IList list, int subLength)
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
        ///     List elements along each diagonal in the matrix from the top left to the lower right.
        /// </summary>
        /// <param name="list">A flat list</param>
        /// <param name="subLength">Length of each new sub-list.</param>
        /// <returns name="diagonals">Lists of elements along matrix diagonals.</returns>
        /// <search>diagonal,right,matrix,get diagonals,diagonal sublists, </search>
        public static IList DiagonalRight([ArbitraryDimensionArrayImport] IList list, int subLength)
        {
            object[] flatList;

            try
            {
                flatList = list.Cast<ArrayList>().SelectMany(i => i.Cast<object>()).ToArray();
            }
            catch
            {
                flatList = list.Cast<object>().ToArray();
            }

            if (flatList.Count() < subLength)
                return list;

            var finalList = new List<List<object>>();

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
                var currList = new List<object>();

                while (index < flatList.Count())
                {
                    var currentRow = (int)System.Math.Ceiling((index + 1)/(double)subLength);
                    currList.Add(flatList[index]);
                    index += subLength + 1;

                    //ensure we are skipping a row to get the next index
                    var nextRow = (int)System.Math.Ceiling((index + 1)/(double)subLength);
                    if (nextRow > currentRow + 1 || nextRow == currentRow)
                        break;
                }
                finalList.Add(currList);
            }

            return finalList;
        }

        /// <summary>
        ///     List elements along each diagonal in the matrix from the top right to the lower left.
        /// </summary>
        /// <param name="list">A flat list.</param>
        /// <param name="rowLength">Length of each new sib-list.</param>
        /// <returns name="diagonals">Lists of elements along matrix diagonals.</returns>
        /// <search>diagonal,left,matrix,get diagonals,diagonal sublists</search>
        public static IList DiagonalLeft(IList list, int rowLength)
        {
            object[] flatList;

            try
            {
                flatList = list.Cast<ArrayList>().SelectMany(i => i.Cast<object>()).ToArray();
            }
            catch (Exception)
            {
                flatList = list.Cast<object>().ToArray();
            }

            if (flatList.Count() < rowLength)
                return list;

            var finalList = new List<List<object>>();

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
                var currList = new List<object>();

                while (index < flatList.Count())
                {
                    var currentRow = (int)System.Math.Ceiling((index + 1)/(double)rowLength);
                    currList.Add(flatList.ElementAt(index));
                    index += rowLength - 1;

                    //ensure we are skipping a row to get the next index
                    var nextRow = (int)System.Math.Ceiling((index + 1)/(double)rowLength);
                    if (nextRow > currentRow + 1 || nextRow == currentRow)
                        break;
                }
                finalList.Add(currList);
            }

            return finalList;
        }


        /// <summary>
        ///     Swaps rows and columns in a list of lists.
        /// </summary>
        /// <param name="lists">A list of lists to be transposed.</param>
        /// <returns name="lists">A list of transposed lists.</returns>
        /// <search>transpose,flip matrix,matrix,swap,rows,columns</search>
        public static IList Transpose(IList lists)
        {
            if (lists.Count == 0 || !lists.Cast<object>().Any(x => x is IList))
                return lists;

            IEnumerable<IList> ilists = lists.Cast<IList>();
            int maxLength = ilists.Max(subList => subList.Count);
            List<ArrayList> transposedList =
                Enumerable.Range(0, maxLength).Select(i => new ArrayList()).ToList();

            foreach (IList sublist in ilists)
            {
                for (int i = 0; i < sublist.Count; i++)
                    transposedList[i].Add(sublist[i]);
            }

            return transposedList;
        }


        /// <summary>
        ///     Creates a list containing the given item the given number of times.
        /// </summary>
        /// <param name="item">The item to repeat.</param>
        /// <param name="amount">The number of times to repeat.</param>
        /// <returns name="list">List of repeated items.</returns>
        /// <search>repeat,repeated,duplicate,list of item,fill list,copies</search>
        public static IList OfRepeatedItem([ArbitraryDimensionArrayImport] object item, int amount)
        {
            return Enumerable.Repeat(item, amount).ToList();
        }

        /// <summary>
        ///     Creates a new list by concatenining copies of a given list.
        /// </summary>
        /// <param name="list">List to repeat.</param>
        /// <param name="amount">Number of times to repeat.</param>
        /// <returns name="list">List of repeated lists.</returns>
        /// <search>repeat,repeated,duplicate,repeated list,concat list</search>
        public static IList Cycle(IList list, int amount)
        {
            var result = new ArrayList();
            while (amount > 0)
            {
                result.AddRange(list);
                amount--;
            }
            return result;
        }

        // Here for backwards compatibility until we have a good way to migrate
        // changes in DSFunction nodes. --SJE
        [IsVisibleInDynamoLibrary(false)]
        public static IList Repeat(IList list, int amount)
        {
            return Cycle(list, amount);
        }

        /// <summary>
        ///     Retrieves the last item in a list.
        /// </summary>
        /// <param name="list">List to get the last item of.</param>
        /// <returns name="last">Last item in the list.</returns>
        /// <search>get,fetch,last,item,end of list</search>
        public static object LastItem(IList list)
        {
            if (list.Count == 0)
                throw new ArgumentException("Cannot get the last item in an empty list.", "list");

            return list[list.Count - 1];
        }

        /// <summary>
        ///     Shuffles a list, randomizing the order of its items.
        /// </summary>
        /// <param name="list">List to shuffle.</param>
        /// <returns name="list">Randomized list.</returns>
        /// <search>random,randomize,shuffle,jitter,randomness</search>
        public static IList Shuffle(IList list)
        {
            var rng = new Random();
            return list.Cast<object>().OrderBy(_ => rng.Next()).ToList();
        }

        /// <summary>
        ///     Produces all permutations of the given length of a given list.
        /// </summary>
        /// <param name="list">List to permute.</param>
        /// <param name="length">Length of each permutation.</param>
        /// <returns name="perm">Permutations of the list of the given length.</returns>
        /// <search>permutation,permutations</search>
        public static IList Permutations(IList list, int? length = null)
        {
            return
                GetPermutations(list.Cast<object>(), length ?? list.Count)
                    .Select(x => x.ToList())
                    .ToList();
        }

        /// <summary>
        ///     Produces all combinations of the given length of a given list.
        /// </summary>
        /// <param name="list">List to generate combinations of.</param>
        /// <param name="length">Length of each combination.</param>
        /// <param name="replace">
        ///     Whether or not items are removed once selected for combination, defaults
        ///     to false.
        /// </param>
        /// <returns name="comb">Combinations of the list of the given length.</returns>
        /// <search>combo</search>
        public static IList Combinations(IList list, int length, bool replace = false)
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
            IList<T> enumerable = items as IList<T> ?? items.ToList();
            foreach (T item in enumerable)
            {
                if (count == 1)
                    yield return Singleton(item);
                else
                {
                    IEnumerable<IEnumerable<T>> combs =
                        GetCombinations(enumerable.Skip(replace ? i : i + 1), count - 1, replace);
                    foreach (var result in combs)
                        yield return Singleton(item).Concat(result);
                }

                ++i;
            }
        }

        private static IEnumerable<IEnumerable<T>> GetPermutations<T>(
            IEnumerable<T> items, int count)
        {
            int i = 0;
            IList<T> enumerable = items as IList<T> ?? items.ToList();
            foreach (T item in enumerable)
            {
                if (count == 1)
                    yield return Singleton(item);
                else
                {
                    IEnumerable<IEnumerable<T>> perms =
                        GetPermutations(
                            enumerable.Take(i).Concat(enumerable.Skip(i + 1)),
                            count - 1);
                    foreach (var result in perms)
                        yield return Singleton(item).Concat(result);
                }

                ++i;
            }
        }

        /// <summary>
        ///     Flattens a nested list of lists by a certain amount.
        /// </summary>
        /// <param name="list">List to flatten.</param>
        /// <param name="amt">Layers of nesting to remove.</param>
        public static IList Flatten(IList list, int amt)
        {
            return Flatten(list, amt, new List<object>());
        }

        private static IList Flatten(IList list, int amt, IList acc)
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
        #endregion

        #region UniqueItems Comparer
        private class DistinctComparer : IEqualityComparer<object>
        {
            internal static readonly IEqualityComparer<object> Instance = new DistinctComparer();

            bool IEqualityComparer<object>.Equals(object x, object y)
            {
                // If both x and y are null, we can't compare null == null. 
                // See: http://stackoverflow.com/questions/4730648/c-sharp-nullable-equality-operations-why-does-null-null-resolve-as-false
                if (ReferenceEquals(x, null) && ReferenceEquals(y, null))
                    return true;

                return Eq(x as dynamic, y as dynamic);
            }

            // Bypass hash code check, use Equals instead.
            public int GetHashCode(object obj)
            {
                return -1;
            }

            private static bool Eq(IList x, IList y)
            {
                return x.Cast<object>().Zip(y.Cast<object>(), Equals).All(b => b);
            }

            private static bool Eq(IConvertible x, IConvertible y)
            {
                try
                {
                    switch (x.GetTypeCode())
                    {
                        case TypeCode.Boolean:
                            if (y.GetTypeCode() == TypeCode.Boolean)
                                return Convert.ToBoolean(x).Equals(Convert.ToBoolean(y));
                            else
                                return false;

                        case TypeCode.Char:
                            if (y.GetTypeCode() == TypeCode.Char)
                                return Convert.ToChar(x).Equals(Convert.ToChar(y));
                            else
                                return false;

                        case TypeCode.String:
                            if (y.GetTypeCode() == TypeCode.String)
                                return Convert.ToString(x).Equals(Convert.ToString(y));
                            else
                                return false;
                        default:
                            return Convert.ToDouble(x).Equals(Convert.ToDouble(y));
                    }
                }
                catch
                {
                    return false;
                }
            }

            private static bool Eq(object x, object y)
            {
                return x.Equals(y);
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

    /// <summary>
    ///     Implements Compare function for two objects using following rule.
    ///     1. Numbers are assumed to be smallest, then bool, string and pointers.
    ///     2. If the two objects are IComparable and of the same type, then use
    ///     it's native comparison mechanism.
    ///     3. If both inputs are value type, but one of them is bool, bool is bigger
    ///     4. Otherwise Convert them all to double and compare.
    ///     5. Else If only one is value type, then value type object is smaller
    ///     6. Else If only one is string, then the string is smaller than other
    ///     7. Else don't know how to compare, so best campare them based on HashCode.
    /// </summary>
    internal class ObjectComparer : IComparer<object>
    {
        public int Compare(object x, object y)
        {
            Type xType = x.GetType();
            Type yType = y.GetType();

            //Same type and are comparable, use it's own compareTo method.
            if (xType == yType && typeof(IComparable).IsAssignableFrom(xType))
                return ((IComparable)x).CompareTo(y);

            //Both are value type, can be converted to Double, use double comparison
            if (xType.IsValueType && yType.IsValueType)
            {
                //Bool is bigger than other value type.
                if (xType == typeof(bool))
                    return 1;

                //Other value type is smaller than bool
                if (yType == typeof(bool))
                    return -1;

                if (typeof(IConvertible).IsAssignableFrom(xType)
                    && typeof(IConvertible).IsAssignableFrom(yType))
                { 
                    return Convert.ToDouble(x).CompareTo(Convert.ToDouble(y));
                }
            }

            //Value Type object will be smaller, if x is value type it is smaller
            if (xType.IsValueType)
                return -1;
            //if y is value type x is bigger
            if (yType.IsValueType)
                return 1;

            //Next higher order object is string
            if (xType == typeof(string))
                return -1;
            if (yType == typeof(string))
                return 1;

            //No idea, return based on hash code.
            return x.GetHashCode() - y.GetHashCode();
        }
    }
}
