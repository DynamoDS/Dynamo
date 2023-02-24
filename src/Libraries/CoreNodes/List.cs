using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using DSCore.Properties;
using ProtoCore.Utils;


namespace DSCore
{

    #region public methods
    /// <summary>
    ///     Methods for creating and manipulating Lists.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public static class List
    {
        /// <summary>
        ///     Returns an Empty List.
        /// </summary>
        /// <returns name="list">Empty list.</returns>
        /// <search>empty list, emptylist,[]</search>
        [IsVisibleInDynamoLibrary(true)]
        public static IList Empty
        {
            get { return new ArrayList(); }
        }

        /// <summary>
        ///     Creates a new list containing all unique items in the given list.
        /// </summary>
        /// <param name="list">List to filter duplicates out of.</param>
        /// <returns name="list">Filtered list.</returns>
        /// <search>removes,duplicates,remove duplicates,cull duplicates,distinct,listcontains</search>
        [IsVisibleInDynamoLibrary(true)]
        public static IList UniqueItems(IList list)
        {
            return list.Cast<object>().Distinct(DistinctComparer.Instance).ToList();
        }

        /// <summary>
        ///     Determines if the given list contains the given item. This function searches through the sublists contained in it.
        /// </summary>
        /// <param name="list">List to search in</param>
        /// <param name="item">Item to look for</param>
        /// <returns name="bool">True if list contains item, false if it doesn’t</returns>
        /// <search>item,search,in,listcontains</search>
        [IsVisibleInDynamoLibrary(true)]
        public static bool Contains(IList list, [ArbitraryDimensionArrayImport] object item)
        {
            bool result = false;
            foreach (var obj in list)
            {
                if (obj is IList) result = result || Contains((IList)obj, item);
            }
            // After checking all sublists, check if the current list contains the item
            return result || (IndexInList(list, item) >= 0);
        }

        /// <summary>
        ///     Check if the items in the list are of the same type.
        /// </summary>
        /// <param name="list">List to be checked if it's homogeneous.</param>
        /// <returns name="bool">Whether the list is homogeneous.</returns>
        /// <search>homogeneous,allequal,same,type</search>
        [IsVisibleInDynamoLibrary(true)]
        public static bool IsHomogeneous(IList list)
        {
            if (list.Count > 0)
            {
                var firstItem = list[0];
                foreach (var obj in list)
                {
                    if (obj.GetType() != firstItem.GetType())
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        ///     Check if the number of items in all rows of the list are the same.
        /// </summary>
        /// <param name="list">List to be checked if the rows have the same number of items.</param>
        /// <returns name="bool">Whether the list has the same number of items in all rows.</returns>
        /// <search>rectangular,isrectangular,same,sublist,row</search>
        [IsVisibleInDynamoLibrary(true)]
        public static bool IsRectangular(IList list)
        {
            int count = -1;
            if (list.Count <= 0) return false;

            foreach (var obj in list)
            {
                if (!(obj is IList)) return false; // All items must be in a list in order to do a comparison
                if (count == -1) count = ((IList)obj).Count; // If the count has not yet been initialized, assign the value
                else if (count != ((IList)obj).Count) return false; // Otherwise, check if they have the same count. If not, return false
            }
            return true; //if all items have the same count
        }

        /// <summary>
        ///     Check if the items in the list have the same depth.
        /// </summary>
        /// <param name="list">List to be checked if the items have the same depth.</param>
        /// <returns name="bool">Whether the depth of the list is uniform.</returns>
        /// <search>depth,uniform,isuniformdepth,sublist,jagged</search>
        [IsVisibleInDynamoLibrary(true)]
        public static bool IsUniformDepth(IList list)
        {
            int depth = -1;
            if (list.Count > 0) depth = GetDepth(list[0]);
            foreach (var obj in list)
            {
                if (GetDepth(obj) != depth) return false;
            }
            return true;
        }

        /// <summary>
        ///     Returns a new list that includes objects in List1 but excludes objects in List2.
        /// </summary>
        /// <param name="list1">List of objects to be included in the new list</param>
        /// <param name="list2">List of objects to be excluded in the new list</param>
        /// <returns name="list">The new list that contains objects in List1 but not in List2</returns>
        /// <search>difference,setdifference,set</search>
        [IsVisibleInDynamoLibrary(true)]
        public static IList SetDifference(IList<object> list1, IList<object> list2)
        {
            return Enumerable.Except(list1, list2).ToList();
        }

        /// <summary>
        ///     Returns a new list that includes objects that are present in both List1 and List2.
        /// </summary>
        /// <param name="list1">List of objects to be compared with list2</param>
        /// <param name="list2">List of objects to be compared with list1</param>
        /// <returns name="list">The new list that contains objects that are in both List1 and List2</returns>
        /// <search>intersection,setintersection,set,overlap</search>
        [IsVisibleInDynamoLibrary(true)]
        public static IList SetIntersection(IList<object> list1, IList<object> list2)
        {
            return Enumerable.Intersect(list1, list2).ToList();
        }

        /// <summary>
        ///     Returns a new list that includes objects that are present in either List1 or List2.
        /// </summary>
        /// <param name="list1">List of objects to be included</param>
        /// <param name="list2">List of objects to be included to List1</param>
        /// <returns name="list">The new list that contains objects that are either in List1 or List2</returns>
        /// <search>union,setunion,set</search>
        [IsVisibleInDynamoLibrary(true)]
        public static IList SetUnion(IList<object> list1, IList<object> list2)
        {
            return Enumerable.Union(list1, list2).ToList();
        }

        /// <summary>
        ///     Returns the index of the element in the given list. Match between given list and target element must be a strict match (i.e. int to int, double to double, string to string, object to object etc.)
        /// </summary>
        /// <param name="list">The list to find the element in.</param>
        /// <param name="element">The element whose index is to be returned.</param>
        /// <returns name="int">The index of the element in the list. Invalid index -1 will be returned if strict match not found.</returns>
        /// <search>index,indexof</search>
        [IsVisibleInDynamoLibrary(true)]
        public static int IndexOf(IList list, object element)
        {
            return list.IndexOf(element);
        }

        /// <summary>
        ///     Returns the number of false boolean values in the given list.
        /// </summary>
        /// <param name="list">The list find the false boolean values.</param>
        /// <returns name="int">The number of false boolean values in the list.</returns>
        /// <search>false,count</search>
        [IsVisibleInDynamoLibrary(true)]
        public static int CountFalse(IList list)
        {
            return CountBool(list, false);
        }

        /// <summary>
        ///     Returns the number of true boolean values in the given list.
        /// </summary>
        /// <param name="list">The list find the true boolean values.</param>
        /// <returns name="int">The number of true boolean values in the list.</returns>
        /// <search>true,count</search>
        [IsVisibleInDynamoLibrary(true)]
        public static int CountTrue(IList list)
        {
            return CountBool(list, true);
        }

        /// <summary>
        ///     Inserts an element into a list at specified index.
        /// </summary>
        /// <param name="list">The list the element will be inserted to</param>
        /// <param name="element">The element to be inserted</param>
        /// <param name="index">Specifies the location in the list of the element to be inserted</param>
        /// <returns name="list">List with the element inserted</returns>
        /// <search>insert,add</search>
        [IsVisibleInDynamoLibrary(true)]
        public static IList Insert(IList list, [ArbitraryDimensionArrayImport] object element, int index)
        {
            list.Insert(index, element);
            return list;
        }

        /// <summary>
        ///     Reorders the input list based on the given list of indices.
        /// </summary>
        /// <param name="list">The list to be reordered</param>
        /// <param name="indices">The indices used to reorder the items in the list</param>
        /// <returns name="list">Reordered list</returns>
        /// <search>reorder,index,indices</search>
        [IsVisibleInDynamoLibrary(true)]
        public static IList Reorder(IList list, IList indices)
        {
            // Note: slightly different behaviour from Builtin - invalid input indices will be ignored, 
            // and will return a list instead of null if the indices are invalid
            List<object> newList = new List<object>();
            for (int i = 0; i < indices.Count; i++)
            {
                int index;
                if ((int.TryParse(indices[i].ToString(), out index) && (index >= 0) && (index < list.Count)))
                {
                    newList.Add(list[index]);
                }
            }
            return newList;
        }

        /// <summary>
        ///     Sorts a list by the items and return their indices.
        /// </summary>
        /// <param name="list">List of items to be sorted</param>
        /// <returns name="int[]">The indices of the items in the sorted list</returns>
        /// <search>sort,index,value</search>
        [IsVisibleInDynamoLibrary(true)]
        public static IEnumerable SortIndexByValue(List<double> list)
        {
            List<Tuple<int, double>> tupleList = new List<Tuple<int, double>>();
            for (int i = 0; i < list.Count; i++)
            {
                tupleList.Add(new Tuple<int, double>(i, list[i]));
            }
            tupleList = tupleList.OrderBy(x => x.Item2).ToList();
            IEnumerable<int> newList = tupleList.OrderBy(x => x.Item2).Select(y => y.Item1);
            return newList;
        }

        /// <summary>
        ///     Returns multidimensional list according the rank given.
        /// </summary>
        /// <param name="list">The list whose depth is to be normalized according to the rank.</param>
        /// <param name="rank">The rank the list is to be normalized to. Default value is 1.</param>
        /// <returns name="list">The list with the normalized rank.</returns>
        /// <search>depth,normalize</search>
        [IsVisibleInDynamoLibrary(true)]
        public static IList NormalizeDepth(IList list, int rank = 1)
        {
            if (rank <= 1)
            {
                return Flatten(list);
            }
            else
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] is IList)
                    {
                        list[i] = NormalizeDepth((IList)list[i], rank - 1);
                    }
                    else
                    {
                        list[i] = IncreaseDepth(new List<object>() { list[i] }, rank - 1);
                    }
                }
            }
            return list;
        }

        /// <summary>
        ///     Creates a new list containing the items of the given list but in reverse order.
        /// </summary>
        /// <param name="list">List to be reversed.</param>
        /// <returns name="list">Reversed list</returns>
        /// <search>flip,listcontains</search>
        [IsVisibleInDynamoLibrary(true)]
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
        /// <returns name="lists">type: var[]..[]</returns>
        /// <search>sublists,build sublists,subset,</search>
        [IsVisibleInDynamoLibrary(true)]
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
        /// <param name="list">List to be sorted</param>
        /// <returns name="list">Sorted list</returns>
        /// <search>sort,order,sorted</search>
        [IsVisibleInDynamoLibrary(true)]
        public static IList Sort(IEnumerable<object> list)
        {
            return list.OrderBy(x => x, new ObjectComparer()).ToList();
        }

        /// <summary>
        ///     Returns the minimum value from a list.
        /// </summary>
        /// <param name="list">List of comparable items to take the minimum value from</param>
        /// <returns name="item">Minimum item from the list.</returns>
        /// <search>least,smallest,find min</search>
        [IsVisibleInDynamoLibrary(true)]
        public static object MinimumItem(IEnumerable<object> list)
        {
            return list.Min<object, object>(DoubleConverter);
        }

        /// <summary>
        ///     Returns the maximum value from a list.
        /// </summary>
        /// <param name="list">List of comparable items to take the maximum value from</param>
        /// <returns name="item">Maximum item from the list.</returns>
        /// <search>greatest,largest,biggest,find max</search>
        [IsVisibleInDynamoLibrary(true)]
        public static object MaximumItem(IEnumerable<object> list)
        {
            return list.Max<object, object>(DoubleConverter);
        }

        /// <summary>
        ///     Filters a sequence by looking up corresponding indices in a separate list of
        ///     booleans.
        /// </summary>
        /// <param name="list">List to filter.</param>
        /// <param name="mask">List of booleans representing a mask.</param>
        /// <returns name="in">Items whose mask index is true.</returns>
        /// <returns name="out">Items whose mask index is false.</returns>
        /// <search>filter,in,out,mask,dispatch,bool filter,boolfilter,bool filter</search>
        [MultiReturn(new[] { "in", "out" })]
        [IsVisibleInDynamoLibrary(true)]
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

        /// <summary>
        ///     Given a list, produces the first item in the list, and a new list containing all items
        ///     except the first.
        /// </summary>
        /// <param name="list">List to be split.</param>
        /// <returns name="first">First item in the list (type: var[]..[]) </returns>
        /// <returns name="rest">Rest of the list (type: var[]..[]) </returns>
        /// <search>first,rest,list split,listcontains</search>
        [MultiReturn(new[] { "first", "rest" })]
        [IsVisibleInDynamoLibrary(true)]
        public static IDictionary Deconstruct(IList list)
        {
            return new Dictionary<string, object>
            {
                { "first", list[0] },
                { "rest", list.Cast<object>().Skip(1).ToList() }
            };
        }

        /// <summary>
        ///     Sort list based on its keys
        /// </summary>
        /// <param name="list">list to be sorted</param>
        /// <param name="keys">list of keys, keys have to be sortable (e.g. numbers,strings)   </param>
        /// <returns name="sortedList">type: var[]..[]</returns>
        /// <returns name="sortedKeys">type: var[]..[]</returns>
        /// <search>sort;key</search>
        [MultiReturn("sortedList", "sortedKeys")]
        [IsVisibleInDynamoLibrary(true)]
        public static IDictionary SortByKey(IList list, IList keys)
        {
            if (list == null || keys == null)
                return null;

            var containsSublists = keys.Cast<object>().Any(key => key is IList || key is ICollection);
            if (containsSublists)
            {
                throw new ArgumentException(Resources.InvalidKeysErrorMessage);
            }

            if (list.Count != keys.Count)
            {
                throw new ArgumentException(Resources.InvalidKeysLenghtErrorMessage);
            }

            var pairs = list.Cast<object>()
                    .Zip(keys.Cast<object>(), (item, key) => new { item, key });

            var numberKeyPairs = pairs.Where(pair => pair.key is double || pair.key is int || pair.key is float || pair.key is long);
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

            var sortedList = sortedPairs.Select(x => x.item).ToList();
            var sortedKeys = sortedPairs.Select(x => x.key).ToList();

            return new Dictionary<object, object>
            {
                { "sortedList", sortedList },
                { "sortedKeys", sortedKeys }
            };
        }

        /// <summary>
        ///     Group items into sub-lists based on their like key values
        /// </summary>
        /// <param name="list">List of items to group as sublists</param>
        /// <param name="keys">Key values, one per item in the input list, used for grouping the items</param>
        /// <returns name="groups">list of sublists, with items grouped by like key values</returns>
        /// <returns name="uniqueKeys">key value corresponding to each group</returns>
        /// <search>list;group;groupbykey;</search>
        [MultiReturn(new[] { "groups", "unique keys" })]
        [IsVisibleInDynamoLibrary(true)]
        public static IDictionary GroupByKey(IList list, IList keys)
        {
            if (list.Count != keys.Count)
            {
                throw new ArgumentException(Resources.InvalidKeysLenghtErrorMessage);
            }

            var containsSublists = keys.Cast<object>().Any(key => key is IList || key is ICollection);
            if (containsSublists)
            {
                throw new ArgumentException(Resources.InvalidKeysErrorMessage);
            }

            var groups =
                list.Cast<object>().Zip(keys.Cast<object>(), (item, key) => new { item, key })
                    .GroupBy(x => x.key)
                    .Select(x => x.Select(y => y.item).ToList());

            var uniqueItems = keys.Cast<object>().Distinct().ToList();

            return new Dictionary<object, object>
            {
                { "groups", groups },
                { "unique keys", uniqueItems }
            };
        }

        /// <summary>
        ///     Adds an item to the beginning of a list.
        /// </summary>
        /// <param name="item">Item to be added. Item could be an object or a list.</param>
        /// <param name="list">List to add on to.</param>
        /// <returns name="list">List with added items</returns>
        /// <search>insert,add,item,front,start,begin</search>
        [IsVisibleInDynamoLibrary(true)]
        public static IList AddItemToFront([ArbitraryDimensionArrayImport] object item, IList list)
        {
            var newList = new ArrayList { item };
            newList.AddRange(list);
            return newList;
        }

        /// <summary>
        ///     Adds an item to the end of a list.
        /// </summary>
        /// <param name="item">Item to be added.Item could be an object or a list.</param>
        /// <param name="list">List to add on to.</param>
        /// <returns name="list">List with added items</returns>
        /// <search>insert,add,item,end</search>
        [IsVisibleInDynamoLibrary(true)]
        public static IList AddItemToEnd([ArbitraryDimensionArrayImport] object item, IList list)
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
        [IsVisibleInDynamoLibrary(true)]
        public static IList TakeItems(IList list, int amount)
        {
            IEnumerable<object> genList = list.Cast<object>();
            return (amount < 0 ? genList.Skip(list.Count + amount) : genList.Take(amount)).ToList();
        }

        /// <summary>
        ///     Removes an amount of items from the start of the list. If the amount is a negative value,
        ///     items are removed from the end of the list.
        /// </summary>
        /// <param name="list">List to remove items from.</param>
        /// <param name="amount">
        ///     Amount of items to remove. If negative, items are removed from the end of the list.
        /// </param>
        /// <returns name="list">List of remaining items.</returns>
        /// <search>drop,remove,shorten</search>
        [IsVisibleInDynamoLibrary(true)]
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
        /// <search>shift,offset</search>
        [IsVisibleInDynamoLibrary(true)]
        public static IList ShiftIndices(IList list, int amount)
        {
            var count = list.Count;
            if (count > 0 && System.Math.Abs(amount) > count)
            {
                amount = amount % count;
            }
            if (amount == 0) return list;

            IEnumerable<object> genList = list.Cast<object>();
            return
                (amount < 0
                    ? genList.Skip(-amount).Concat(genList.Take(-amount))
                    : genList.Skip(list.Count - amount).Concat(genList.Take(list.Count - amount)))
                    .ToList();
        }

        /// <summary>
        /// Returns an item from the given list that's located at the specified index.
        /// </summary>
        /// <param name="list">List to fetch an item from.</param>
        /// <param name="index">Index of the item to be fetched.</param>
        /// <returns name="item">Item in the list at the given index.</returns>
        /// <search>get,item,index,fetch,at,getfrom,get from,extract</search>
        [IsVisibleInDynamoLibrary(true)]
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
        [IsVisibleInDynamoLibrary(true)]
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
        ///     Returns a single sub-list from the given list, based on starting index, ending index,
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
        [IsVisibleInDynamoLibrary(true)]
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
        [IsVisibleInDynamoLibrary(true)]
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
        [IsVisibleInDynamoLibrary(true)]
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
        [IsVisibleInDynamoLibrary(true)]
        public static IList TakeEveryNthItem(IList list, int n, int offset = 0)
        {
            return list.Cast<object>().Where((_, i) => (i + 1 - offset)%n == 0).ToList();
        }

        /// <summary>
        ///     Determines if the given list is empty.
        /// </summary>
        /// <param name="list">List to be checked if it is empty</param>
        /// <returns name="bool">True if list is empty, false if it isnt</returns>
        /// <search>test,is,empty,null,count</search>
        [IsVisibleInDynamoLibrary(true)]
        public static bool IsEmpty(IList list)
        {
            return list.Count == 0;
        }

        /// <summary>
        ///     Determines if all items in the given list is a boolean and has a true value.
        /// </summary>
        /// <param name="list">List to be checked on whether all items are true.</param>
        /// <returns name="bool">True if all items from list are true, false if one or more items is not true</returns>
        /// <search>test,all,true,istrue</search>
        [IsVisibleInDynamoLibrary(true)]
        public static bool AllTrue(IList list)
        {
            bool result = true;
            foreach (var obj in list)
            {
                if (obj is IList)
                {
                    result = result && AllTrue((IList)obj);
                }
                else
                {
                    if ((obj is bool) && (bool)obj) continue;
                    else return false;
                }
            }
            return result;
        }

        /// <summary>
        ///     Determines if all items in the given list is a boolean and has a false value.
        /// </summary>
        /// <param name="list">List to be checked on whether all items are false.</param>
        /// <returns name="bool">True if all items from list are false, false if one or more items is not false</returns>
        /// <search>test,all,false,isfalse</search>
        [IsVisibleInDynamoLibrary(true)]
        public static bool AllFalse(IList list)
        {
            bool result = true;
            foreach (var obj in list)
            {
                if (obj is IList)
                {
                    result = result && AllFalse((IList)obj);
                }
                else
                {
                    if ((obj is bool) && !(bool)obj) continue;
                    else return false;
                }
            }
            return result;
        }

        /// <summary>
        ///     Determines if any item in the given list is a boolean and has a true value.
        /// </summary>
        /// <param name="list">List to be checked on whether any item is true.</param>
        /// <returns name="bool">Whether any item is true.</returns>
        /// <search>test,any,true,istrue</search>
        [IsVisibleInDynamoLibrary(true)]
        public static bool AnyTrue(IList list)
        {
            bool result = false;
            foreach (object obj in list)
            {
                if (obj is IList subList)
                {
                    result = AnyTrue(subList);
                }
                else if (obj is bool boolObj && boolObj)
                {
                    result = true;
                }
                if (result) break;
            }
            return result;
        }

        /// <summary>
        ///     Determines if any item in the given list is a boolean and has a false value.
        /// </summary>
        /// <param name="list">List to be checked on whether any item is false.</param>
        /// <returns name="bool">Whether any item is false.</returns>
        /// <search>test,any,false,isfalse</search>
        [IsVisibleInDynamoLibrary(true)]
        public static bool AnyFalse(IList list)
        {
            bool result = false;
            foreach (object obj in list)
            {
                if (obj is IList subList)
                {
                    result = AnyFalse(subList);
                }
                else if(obj is bool boolObj && !boolObj)
                {
                    result = true;
                }
                if (result) break;
            }
            return result;
        }

        /// <summary>
        ///     Returns the number of items stored in the given list.
        /// </summary>
        /// <param name="list">List to get the item count of.</param>
        /// <returns name="int">List length.</returns>
        /// <search>listlength,list length,count,size,sizeof</search>
        [IsVisibleInDynamoLibrary(true)]
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
        [IsLacingDisabled]
        [IsVisibleInDynamoLibrary(true)]
        public static IList Join(params IList[] lists)
        {
            var result = new ArrayList();
            foreach (IList list in lists)
                result.AddRange(list);
            return result;
        }

        /// <summary>
        ///     Returns the first item in a list.
        /// </summary>
        /// <param name="list">List to get the first item from.</param>
        /// <returns name="item">First item in the list.</returns>
        /// <search>get,fetch,first,item,start</search>
        [IsVisibleInDynamoLibrary(true)]
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
        [IsVisibleInDynamoLibrary(true)]
        public static IList RestOfItems(IList list)
        {
            return list.Cast<object>().Skip(1).ToList();
        }

        /// <summary>
        ///     Chop a list into a set of consecutive sublists with the specified lengths. List division begins at the top of the list.
        /// </summary>
        /// <param name="list">List to chop into sublists</param>
        /// <param name="lengths">Lengths of consecutive sublists to be created from the input list</param>
        /// <returns name="lists">Sublists created from the list</returns>
        /// <search>sublists,build sublists,slices,partitions,cut,listcontains,chop</search>
        [IsVisibleInDynamoLibrary(true)]
        public static IList Chop(IList list, List<int> lengths)
        {
            var finalList = new ArrayList();
            var currList = new ArrayList();
            int count = 0;
            int lengthIndex = 0;

            // If there are not any lengths more than 0,
            // we return incoming list.
            if (lengths.All(x => x <= 0))
            {
                return list;
            }

            int i = 0;
            while (i < list.Count)
            {
                // If number of items in current list equals length in list of lengths,
                // we should add current list in final list.
                // Or if length in list of lengths <= 0, we should process this length and move further.
                if (count == lengths[lengthIndex] || lengths[lengthIndex] <= 0)
                {
                    finalList.Add(currList);
                    currList = new ArrayList();
                    if (lengthIndex < lengths.Count - 1)
                    {
                        lengthIndex++;
                    }
                    else if(lengths[lengthIndex] <= 0)
                    {
                        break;
                    }
                    count = 0;
                }
                if (lengths[lengthIndex] > 0)
                {
                    currList.Add(list[i]);
                    count++;
                    i++;
                }
            }

            if (currList.Count > 0)
                finalList.Add(currList);

            return finalList;
        }

        /// <summary>
        ///     List elements along each diagonal in the matrix from the lower left to the top right.
        /// </summary>
        /// <param name="list">A flat list</param>
        /// <param name="subLength">Length of each new sub-list.</param>
        /// <returns name="diagonals">Lists of elements along matrix diagonals.</returns>
        /// <search>diagonal,right,matrix,get diagonals,diagonal sublists</search>
        [IsVisibleInDynamoLibrary(true)]
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
        ///     List elements along each diagonal in the matrix from the top left to the lower right.
        /// </summary>
        /// <param name="list">A flat list.</param>
        /// <param name="rowLength">Length of each new sib-list.</param>
        /// <returns name="diagonals">Lists of elements along matrix diagonals.</returns>
        /// <search>diagonal,left,matrix,get diagonals,diagonal sublists</search>
        [IsVisibleInDynamoLibrary(true)]
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
        ///     If there are some rows that are shorter than others,
        ///     null values are inserted as place holders in the resultant 
        ///     array such that it is always rectangular.
        /// </summary>
        /// <param name="lists">List of lists to be transposed</param>
        /// <returns name="lists">List of transposed lists</returns>
        /// <search>transpose,flip matrix,matrix,swap,rows,columns</search>
        [IsVisibleInDynamoLibrary(true)]
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
                for (int i = 0; i < transposedList.Count; i++)
                {
                    transposedList[i].Add(i < sublist.Count ? sublist[i] : null);
                }
            }

            return transposedList;
        }

        /// <summary>
        /// Cleans data of nulls and empty lists from a given list of arbitrary dimension
        /// </summary>
        /// <param name="list">List containing nulls and empty sublists to clean</param>
        /// <param name="preserveIndices">Provide an option to preserve the indices of the data
        /// <returns name="list">List of transposed lists</returns>
        /// such that non-trailing nulls may not be filtered out</param>
        /// <returns>A list cleaned of nulls and empty lists</returns>
        [IsVisibleInDynamoLibrary(true)]
        public static IList Clean(IList list, bool preserveIndices = true)
        {
            if (list == null)
                return null;

            if (list.Count == 0)
                return list;

            var culledList = new List<object>();
            if (preserveIndices)
            {
                // if list contains only nulls or is empty, e.g. {null, ...} OR {} 
                if (list.Cast<object>().All(el => el == null))
                    return null;

                int j = list.Count - 1;
                while (j >= 0 && list[j] == null)
                    j--;

                for (int i = 0; i <= j; i++)
                {
                    var subList = list[i] as IList;
                    if (subList != null)
                    {
                        var val = Clean(subList);
                        culledList.Add(val);
                    }
                    else
                    {
                        culledList.Add(list[i]);    
                    }
                }
            }
            else
            {
                // if list contains only nulls or is empty, e.g. {null, ...} OR {} 
                if (list.Cast<object>().All(el => el == null))
                    return new List<object>();

                foreach (var el in list)
                {
                    var subList = el as IList;
                    if (subList != null)
                    {
                        var val = Clean(subList, false);
                        if (!List.IsEmpty(val))
                            culledList.Add(val);
                    }
                    else if (el != null)
                    {
                        culledList.Add(el);
                    }
                }
            }
            return culledList;
        }


        /// <summary>
        ///     Creates a list containing the given item the given number of times.
        /// </summary>
        /// <param name="item">The item to repeat.</param>
        /// <param name="amount">The number of times to repeat.</param>
        /// <returns name="list">List of repeated items.</returns>
        /// <search>repeat,repeated,duplicate,list of item,fill list,copies,listcontains</search>
        [IsVisibleInDynamoLibrary(true)]
        public static IList OfRepeatedItem([ArbitraryDimensionArrayImport] object item, int amount)
        {
            return Enumerable.Repeat(item, amount).ToList();
        }

        /// <summary>
        ///     Creates a new list by concatenating copies of a given list.
        /// </summary>
        /// <param name="list">List to repeat.</param>
        /// <param name="amount">Number of times to repeat.</param>
        /// <returns name="list">List of repeated lists of type: var[]..[]</returns>
        /// <search>repeat,repeated,duplicate,repeated list,concat list</search>
        [IsVisibleInDynamoLibrary(true)]
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
        /// <param name="list">List to get the last item of</param>
        /// <returns name="item">Last item in the list</returns>
        /// <search>get,fetch,last,item,end of list</search>
        [IsVisibleInDynamoLibrary(true)]
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
        [IsVisibleInDynamoLibrary(true)]
        public static IList Shuffle(IList list)
        {
            return list.Cast<object>().OrderBy(_ => mRandom.Next()).ToList();
        }

        /// <summary>
        ///     Shuffles a list, randomizing the order of its items based on an intial seed value.
        /// </summary>
        /// <param name="list">List to shuffle.</param>
        /// <param name="seed">Seed value for the random number generator.</param>
        /// <returns name="list">Randomized list.</returns>
        /// <search>random,randomize,shuffle,jitter,randomness,seed</search>
        [IsVisibleInDynamoLibrary(true)]
        public static IList Shuffle(IList list, int seed)
        {
            var rng = new Random(seed);
            return list.Cast<object>().OrderBy(_ => rng.Next()).ToList();
        }

        /// <summary>
        ///     Produces all permutations of the given length of a given list.
        /// </summary>
        /// <param name="list">List to permute.</param>
        /// <param name="length">Length of each permutation.</param>
        /// <returns name="permutations">Permutations of the list of the given length (type: var[]..[]) </returns>
        /// <search>permutation,permutations</search>
        [IsVisibleInDynamoLibrary(true)]
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
        /// <param name="list">List to generate combinations of</param>
        /// <param name="length">Length of each combination</param>
        /// <param name="replace">
        ///     Whether or not items are removed once selected for combination, defaults
        ///     to false.
        /// </param>
        /// <returns name="lists">Combinations of the list of the given length</returns>
        /// <search>combo</search>
        [IsVisibleInDynamoLibrary(true)]
        public static IList Combinations(IList list, int length, bool replace = false)
        {
            return
                GetCombinations(list.Cast<object>(), length, replace)
                    .Select(x => x.ToList())
                    .ToList();
        }

        /// <summary>
        ///     Given an item, returns the zero-based index of its first occurrence 
        ///     in the list. If the item cannot be found in the list, -1 is returned.
        /// </summary>
        /// <param name="list">
        ///     List to search in. If this argument is null, -1 is returned.
        /// </param>
        /// <param name="item">Item to look for.</param>
        /// <returns>Zero-based index of the item in the list, or -1 if it is not found.
        /// </returns>
        [IsVisibleInDynamoLibrary(true)]
        public static int FirstIndexOf(IList list, object item)
        {
            if (list == null)
                return -1;

            int index = list.IndexOf(item);
            return index;
        }

        /// <summary>
        ///     Given an item, returns the zero-based indices of all its occurrences
        ///     in the list. If the item cannot be found, an empty list is returned.
        /// </summary>
        /// <param name="list">
        ///     List to search in. If this argument is null, an empty list is returned.
        /// </param>
        /// <param name="item">Item to look for.</param>
        /// <returns name="indices">A list of zero-based indices of all occurrences of the item if 
        /// found, or an empty list if the item does not exist in the list.</returns>
        [IsVisibleInDynamoLibrary(true)]
        public static IList AllIndicesOf(IList list, object item)
        {
            if (list == null)
                return new List<int> { }; 

            var indices = Enumerable.Range(0, list.Count).Where(i => list[i] != null ? list[i].Equals(item) : item == null).ToList();
            return indices;
        }

        /// <summary>
        ///     Flattens a nested list of lists by a certain amount.
        /// </summary>
        /// <param name="list">List to flatten.</param>
        /// <param name="amount">Layers of list nesting to remove (-1 will remove all list nestings)</param>
        /// <returns name="list">Flattened list by amount</returns>
        /// <search>flatten,completely</search>
        [IsVisibleInDynamoLibrary(true)]
        public static IList Flatten(IList list, int amount = -1)
        {
            if (amount < 0)
            {
                return Flatten(list, GetDepth(list), new List<object>());
            }
            return Flatten(list, amount, new List<object>());
        }
        #endregion

        #region private helper methods

        private static readonly Random mRandom = new Random();

        /// <summary>
        ///     An alternative to using IList.Contains which uses Enumerable.SequenceEqual to check if
        ///     the item is contained in the list if the item is an array. Returns the index if found, 
        ///     -1 if not found.
        /// </summary>
        /// <param name="list">The list to check if it contains the item.</param>
        /// <param name="item">The item that needs to be found.</param>
        /// <returns name="index">Index of the item in the list.</returns>
        private static int IndexInList(IList list, object item)
        {
            for (int index = 0; index < list.Count; index++)
            {
                if (MathUtils.IsNumber(list[index]) && MathUtils.IsNumber(item))
                {
                    if (MathUtils.Equals(Convert.ToDouble(list[index]), Convert.ToDouble(item)))
                        return index;
                }
                if (list[index] is ArrayList && item is ArrayList)
                {
                    if (((ArrayList)list[index]).Cast<object>().SequenceEqual<object>(((ArrayList)item).Cast<object>())) return index;
                }
                else
                {
                    // This method of comparing is used in IList.Contains
                    if (list[index].Equals(item)) return index;
                }
            }
            return -1;
        }

        /// <summary>
        ///     Obtain the maximum depth of a given list.
        /// </summary>
        /// <param name="list">The input list to obtain the depth from.</param>
        /// <returns name="depth">Depth of the given list.</returns>
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

        /// <summary>
        ///     Returns the number of the specified boolean values in the given list.
        /// </summary>
        /// <param name="list">The list find the boolean values.</param>
        /// <param name="value">The boolean value to be found</param>
        /// <returns name="int">The number of the specified boolean value in the list.</returns>
        private static int CountBool(IList list, bool value)
        {
            int count = 0;
            foreach (var obj in list)
            {
                if ((obj is bool) && ((bool)obj == value)) count++;
                else if ((obj is IList))
                {
                    count += CountBool((IList)obj, value);
                }
            }
            return count;
        }

        /// <summary>
        ///     Increase the depth of a given list by a specified amount. Depth is increased
        ///     by creating a new list to contain the given list.
        /// </summary>
        /// <param name="list">The list whose depth is to be increased.</param>
        /// <param name="amt">The amount the depth is to be increased by.</param>
        /// <returns name="list">The new list whose depth is increased by amt.</returns>
        private static IList IncreaseDepth(IList list, int amt)
        {
            if (amt <= 1)
            {
                return list;
            }
            else
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (!(list[i] is IList))
                    {
                        list[i] = new List<object>() { list[i] };
                    }
                    list[i] = IncreaseDepth((IList)list[i], amt - 1);
                }
            }
            return list;
        }

        /// <summary>
        /// Converts integer to double, else returns the input object.
        /// </summary>
        private static object DoubleConverter(object obj)
        {
            if (obj is int || obj is long)
                return Convert.ToDouble(obj);
            return obj;
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
