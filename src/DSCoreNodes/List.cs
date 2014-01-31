using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms.Layout;
using System.Windows.Input;

namespace DSCore
{
    /// <summary>
    ///     Methods for creating and manipulating Lists.
    /// </summary>
    public class List
    {
        private List()
        {
        }

        /// <summary>
        ///     Creates a new list containing the elements of the given list but in reverse order.
        /// </summary>
        /// <param name="list">List to be reversed.</param>
        public static IList Reverse(IList list)
        {
            return list.Cast<object>().Reverse().ToList();
        }

        /// <summary>
        ///     Creates a new list containing the given elements.
        /// </summary>
        /// <param name="elements">Elements to be stored in the new list.</param>
        public static IList Create(params object[] elements)
        {
            return elements.ToList();
        }

        /// <summary>
        ///     Sorts a list using the built-in natural ordering.
        /// </summary>
        /// <param name="list">List to be sorted.</param>
        public static IList Sort(IList list)
        {
            return list.Cast<object>().OrderBy(x => x).ToList();
        }

        /// <summary>
        ///     Sorts a list using a key projection. A projection is created for each element,
        ///     and that value is used to order the original elements.
        /// </summary>
        /// <param name="list">List to be sorted.</param>
        /// <param name="keyProjector">
        ///     Function that consumes an element from the list and produces an orderable value.
        /// </param>
        /// <returns></returns>
        public static IList SortByKey(IList list, Delegate keyProjector)
        {
            return list.Cast<object>().OrderBy(x => keyProjector.DynamicInvoke(x)).ToList();
        }

        /// <summary>
        ///     Sorts a list using a comparison function. Given two elements from the list, the comparison
        ///     function determines which element should appear first in the sorted list.
        /// </summary>
        /// <param name="list">List to be sorted.</param>
        /// <param name="comparison">
        ///     Function that consumes two elements from the list and produces a value determining the order
        ///     of the two elements as follows: a value less than zero if the first element should appear
        ///     before the second, zero if the values are considered the same, and a value greater than
        ///     zero if the second element should appear before the first.
        /// </param>
        public static IList SortByComparison(IList list, Delegate comparison)
        {
            var rtn = list.Cast<object>().ToList();
            rtn.Sort((x, y) => (int)comparison.DynamicInvoke(x, y));
            return rtn;
        }

        /// <summary>
        ///     Returns the minimum value from a list.
        /// </summary>
        /// <typeparam name="T">Type of the contents of the list.</typeparam>
        /// <param name="list">List to take the minimum value from.</param>
        public static object MinimumItem(IEnumerable list)
        {
            return list.Cast<object>().Min();
        }

        /// <summary>
        ///     Returns the minimum value from a list using a key projection. The minimum
        ///     value is the element in the list that the key projection produces the smallest
        ///     value for.
        /// </summary>
        /// <typeparam name="T">Type of the contents of the list.</typeparam>
        /// <typeparam name="TKey">Type that the Key Projection produces.</typeparam>
        /// <param name="list">List to take the minimum value from.</param>
        /// <param name="keyProjector">
        ///     Function that consumes an element from the list and produces an orderable value.
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
        ///     Returns the maximum value from a list.
        /// </summary>
        /// <typeparam name="T">Type of the contents of the list.</typeparam>
        /// <param name="list">List to take the maximum value from.</param>
        public static object MaximumItem(IList list)
        {
            return list.Cast<object>().Max();
        }

        /// <summary>
        ///     Returns the maximum value from a list using a key projection. The maximum
        ///     value is the element in the list that the key projection produces the largest
        ///     value for.
        /// </summary>
        /// <typeparam name="T">Type of the contents of the list.</typeparam>
        /// <typeparam name="TKey">Type that the Key Projection produces.</typeparam>
        /// <param name="list">List to take the maximum value from.</param>
        /// <param name="keyProjector">
        ///     Function that consumes an element from the list and produces an orderable value.
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
        ///     Creates a new list containing all the elements of an old list for which
        ///     the given predicate function returns True.
        /// </summary>
        /// <typeparam name="T">Type of the contents of the list.</typeparam>
        /// <param name="list">List to be filtered.</param>
        /// <param name="predicate">
        ///     Function to be applied to all elements in the list. All elements that make the
        ///     predicate produce True will be stored in the output list.
        /// </param>
        public static IList Filter(IList list, Delegate predicate)
        {
            return list.Cast<object>().Where(x => (bool)predicate.DynamicInvoke(x)).ToList();
        }

        //TODO: This could be combined with Filter into a multi-output node
        /// <summary>
        ///     Creates a new list containing all the elements of an old list for which
        ///     the given predicate function returns False.
        /// </summary>
        /// <typeparam name="T">Type of the contents of the list.</typeparam>
        /// <param name="list">List to be filtered.</param>
        /// <param name="predicate">
        ///     Function to be applied to all elements in the list. All elements that make the
        ///     predicate produce False will be stored in the output list.
        /// </param>
        public static IList FilterOut(IList list, Delegate predicate)
        {
            return list.Cast<object>().Where(x => !(bool)predicate.DynamicInvoke(x)).ToList();
        }

        /*

        /// <summary>
        ///     Reduces a list of values into a new value using a reduction function.
        /// </summary>
        /// <typeparam name="T">Type of the contents of the list.</typeparam>
        /// <typeparam name="TState">Type of the reduced output.</typeparam>
        /// <param name="list">List to be reduced.</param>
        /// <param name="seed">
        ///     Starting value for the reduction. If the list being reduced is
        ///     empty, this will immediately be returned.
        /// </param>
        /// <param name="reducer">
        ///     A function that consumes an element in the list and a reduction state. It must produce
        ///     a new reduction state by combining the element with the current reduction state.
        /// </param>
        public static TState Reduce<T, TState>(IEnumerable<T> list, TState seed, Func<T, TState, TState> reducer)
        {
            return list.Aggregate(seed, (a, x) => reducer(x, a));
        }

        /// <summary>
        ///     Produces a new list by applying a projection function to each element of the input list(s) and
        ///     storing the result.
        /// </summary>
        /// <param name="projection">
        ///     Function that consumes an element from each input list and produces a value that is stored
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
        ///     Produces a new list by applying a projection function to all combinations of elements from the
        ///     input lists and storing the result.
        /// </summary>
        /// <param name="projection">
        ///     Function that consumes an element from each input list and produces a value that is stored
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
        ///     Applies a function to each element of the input list(s). Does not accumulate results.
        /// </summary>
        /// <param name="action">
        ///     Function that consumed an element from each input list. Return value is ignored.
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
         
        */

        /// <summary>
        ///     Determines if the given predicate function returns True when applied to all of the
        ///     elements in the given list.
        /// </summary>
        /// <typeparam name="T">Type of the contents of the list.</typeparam>
        /// <param name="predicate">
        ///     Function to be applied to all elements in the list, returns a boolean value.
        /// </param>
        /// <param name="list">List to be tested.</param>
        public static bool TrueForAllItems(IList list, Delegate predicate)
        {
            return list.Cast<object>().All(x => (bool)predicate.DynamicInvoke(x));
        }

        /// <summary>
        ///     Determines if the given predicate function returns True when applied to any of the
        ///     elements in the given list.
        /// </summary>
        /// <typeparam name="T">Type of the contents of the list.</typeparam>
        /// <param name="predicate">
        ///     Function to be applied to all elements in the list, returns a boolean value.
        /// </param>
        /// <param name="list">List to be tested.</param>
        public static bool TrueForAnyItems(IList list, Delegate predicate)
        {
            return list.Cast<object>().Any(x => (bool)predicate.DynamicInvoke(x));
        }

        /// <summary>
        ///     Given a list, produces the first item in the list, and a new list containing all items
        ///     except the first.
        /// </summary>
        /// <typeparam name="T">Type of the contents of the list.</typeparam>
        /// <param name="list">List to be split.</param>
        public static object[] Deconstruct(IList list)
        {
            return new[] { list[0], list.Cast<object>().Skip(1).ToList() };
        }

        /// <summary>
        ///     Produces a new list by adding an item to the beginning of a given list.
        /// </summary>
        /// <param name="o">Item to be added.</param>
        /// <param name="list">List to add on to.</param>
        public static IList AddItemToFront(object o, IList list)
        {
            var newList = new ArrayList { o };
            newList.AddRange(list);
            return newList;
        }

        /// <summary>
        ///     Fetches the given amount of elements from the start of the given list.
        /// </summary>
        /// <typeparam name="T">Type of the contents of the list.</typeparam>
        /// <param name="list">List to take from.</param>
        /// <param name="amount">
        ///     Amount of elements to take. If negative, elements are taken from the end of the list.
        /// </param>
        public static IList TakeItems(IList list, int amount)
        {
            var genList = list.Cast<object>();
            return (amount < 0 ? genList.Skip(list.Count + amount) : genList.Take(amount)).ToList();
        }

        /// <summary>
        ///     Removes the given amount of elements from the start of the given list.
        /// </summary>
        /// <typeparam name="T">Type of the contents of the list.</typeparam>
        /// <param name="list">List to remove elements from.</param>
        /// <param name="amount">
        ///     Amount of elements to remove. If negative, elements are removed from the end of the list.
        /// </param>
        public static IList DropItems(IList list, int amount)
        {
            var genList = list.Cast<object>();
            return (amount < 0 ? genList.Take(list.Count + amount) : genList.Skip(amount)).ToList();
        }

        /// <summary>
        ///     Shifts indices in the given list to the right by the given amount.
        /// </summary>
        /// <typeparam name="T">Type of the contents of the list.</typeparam>
        /// <param name="list">List to be shifted.</param>
        /// <param name="amount">
        ///     Amount to shift indices by. If negative, indices will be shifted to the left.
        /// </param>
        public static IList ShiftIndices(IList list, int amount)
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
        ///     Gets an element from the given list that's located at the specified index.
        /// </summary>
        /// <param name="list">List to fetch an element from.</param>
        /// <param name="index">Index of the element to be fetched.</param>
        public static object GetItemAtIndex(IList list, int index)
        {
            return list[index];
        }

        /// <summary>
        ///     Gets a single sub-list from the given list, based on starting index, amount of elements
        ///     to take, and a step amount.
        /// </summary>
        /// <param name="list">List to take a slice of.</param>
        /// <param name="start">Index to start the slice from.</param>
        /// <param name="count">Number of elements to take in the slice.</param>
        /// <param name="step">
        ///     Amount the indices of the elements are separate by in the original list.
        /// </param>
        public static IList Slice(IList list, int? start = null, int? count = null, int step = 1)
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
        ///     Removes an element from the given list at the specified index.
        /// </summary>
        /// <typeparam name="T">Type of the contents of the list.</typeparam>
        /// <param name="list">List to remove an element from.</param>
        /// <param name="index">Index of the element to be removed.</param>
        public static IList RemoveItemAtIndex(IList list, int index)
        {
            return list.Cast<object>().Where((_, i) => i != index).ToList();
        }

        /// <summary>
        ///     Removes elements from the given list at the specified indices.
        /// </summary>
        /// <typeparam name="T">Type of the contents of the list.</typeparam>
        /// <param name="list">List to remove elements from.</param>
        /// <param name="indices">Indices of the elements to be removed.</param>
        public static IList RemoveItemsAtIndices(IList list, IList indices)
        {
            var idxs = new HashSet<int>(indices.Cast<int>());
            return list.Cast<object>().Where((_, i) => !idxs.Contains(i)).ToList();
        }

        /// <summary>
        ///     Removes elements from the given list at indices that are multiples
        ///     of the given value, after the given offset.
        /// </summary>
        /// <typeparam name="T">Type of the contents of the list.</typeparam>
        /// <param name="list">List to remove elements from/</param>
        /// <param name="n">Indices that are multiples of this argument will be removed.</param>
        /// <param name="offset">
        ///     Amount of elements to be ignored from the start of the list.
        /// </param>
        public static IList DropEveryNthItem(IList list, int n, int offset = 0)
        {
            return list.Cast<object>().Skip(offset).Where((_, i) => (i + 1)%n != 0).ToList();
        }

        /// <summary>
        ///     Fetches elements from the given list at indices that are multiples
        ///     of the given value, after the given offset.
        /// </summary>
        /// <typeparam name="T">Type of the contents of the list.</typeparam>
        /// <param name="list">List to take elements from.</param>
        /// <param name="n">
        ///     Indices that are multiples of this number (after the offset)
        ///     will be fetched.
        /// </param>
        /// <param name="offset">
        ///     Amount of elements to be ignored from the start of the list.
        /// </param>
        public static IList TakeEveryNthItem(IList list, int n, int offset = 0)
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
        /// <param name="list">List to check for elements.</param>
        public static bool IsEmpty(IList list)
        {
            return list.Count == 0;
        }

        /// <summary>
        ///     Gets the number of elements stored in the given list.
        /// </summary>
        /// <param name="list">List to get the element count of.</param>
        public static int Count(IList list)
        {
            return list.Count;
        }

        /// <summary>
        ///     Concatenates all given lists into a single list.
        /// </summary>
        /// <param name="lists">Lists to join into one.</param>
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
        public static object FirstItem(IList list)
        {
            return list[0];
        }

        /// <summary>
        ///     Removes the first item from the given list.
        /// </summary>
        /// <typeparam name="T">Type of the contents of the list.</typeparam>
        /// <param name="list">List to get the rest of.</param>
        public static IList RestOfItems(IList list)
        {
            return list.Cast<object>().Skip(1).ToList();
        }

        /// <summary>
        ///     Creates a list of lists out of an existing list with each sub-list containing
        ///     the given amount of elements.
        /// </summary>
        /// <param name="list">List to chop up.</param>
        /// <param name="subLength">Length of each new sub-list.</param>
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
        ///     Create a diagonal lists of lists from top left to lower right.
        /// </summary>
        /// <param name="list">A list</param>
        /// <param name="subLength">Length of each new sub-list.</param>
        public static IList DiagonalRight(IList list, int subLength)
        {
            if (list.Count < subLength)
                return list;

            var finalList = new ArrayList();
            var currList = new ArrayList();

            var startIndices = new List<int>();

            //get indices along 'side' of array
            for (int i = subLength; i < list.Count; i += subLength)
                startIndices.Add(i);

            startIndices.Reverse();

            //get indices along 'top' of array
            for (int i = 0; i < subLength; i++)
                startIndices.Add(i);

            foreach (int start in startIndices)
            {
                int index = start;

                while (index < list.Count)
                {
                    var currentRow = (int)System.Math.Ceiling((index + 1)/(double)subLength);
                    currList.Add(list[index]);
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
        /// <param name="list">A list.</param>
        /// <param name="subLength">Length of each new sib-list.</param>
        public static IList DiagonalLeft(IList list, int subLength)
        {
            if (list.Count < subLength)
                return list;

            var finalList = new ArrayList();

            var startIndices = new List<int>();

            //get indices along 'top' of array
            for (int i = 0; i < subLength; i++)
                startIndices.Add(i);

            //get indices along 'side' of array
            for (int i = subLength - 1 + subLength; i < list.Count; i += subLength)
                startIndices.Add(i);

            foreach (int start in startIndices)
            {
                int index = start;
                var currList = new ArrayList();

                while (index < list.Count)
                {
                    var currentRow = (int)System.Math.Ceiling((index + 1)/(double)subLength);
                    currList.Add(list[index]);
                    index += subLength - 1;

                    //ensure we are skipping a row to get the next index
                    var nextRow = (int)System.Math.Ceiling((index + 1) / (double)subLength);
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
        public static IList Transpose(IList lists)
        {
            if (lists.Count == 0)
                return lists;

            var genList = lists.Cast<IList>();

            var argList =
                genList.First().Cast<object>().Select(x => new ArrayList { x } as IList).ToList();

            var query =
                genList.Skip(1)
                       .SelectMany(
                           list => list.Cast<object>().Zip(argList, (o, objs) => new { o, objs }));

            foreach (var pair in query)
                pair.objs.Add(pair.o);

            return argList;
        }

        /// <summary>
        ///     Creates a list containing the given item the given number of times.
        /// </summary>
        /// <param name="thing">The thing to repeat.</param>
        /// <param name="amount">The number of times to repeat.</param>
        public static IList OfRepeatedItem(object thing, int amount)
        {
            return Enumerable.Repeat(thing, amount).ToList();
        }

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
        ///     Retrieves the last item in a list.
        /// </summary>
        /// <param name="list">List to get the last item of.</param>
        public static object LastItem(IList list)
        {
            if (list.Count == 0)
                throw new ArgumentException("Cannot get the last item in an empty list.", "list");

            return list[list.Count - 1];
        }

        /// <summary>
        ///     Shuffles a list, randomizing the order of its elements.
        /// </summary>
        /// <param name="list">List to shuffle.</param>
        public static IList Shuffle(IList list)
        {
            var rng = new Random();
            return list.Cast<object>().OrderBy(_ => rng.Next()).ToList();
        }

        /// <summary>
        ///     Applies a key mapping function to each item in the given list, and produces
        ///     a new list of lists where each sublist contains elements for which the key
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
    }


    //TODO
    /*
    public class BuildSublists : NodeModel
    {
        
    }
    */
}
