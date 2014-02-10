using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using NUnit.Framework;
using DSCore;
using List = DSCore.List;

namespace DSCoreNodesTests
{
    [TestFixture]
    internal static class ListTests
    {
        [Test]
        public static void UniqueInList()
        {
            Assert.AreEqual(
                new ArrayList { 1, 2, 3, 4, 5 },
                List.UniqueItems(new ArrayList { 1, 1, 2, 3, 4, 4, 5, 4, 2, 1, 3 }));
        }

        [Test]
        public static void ListContains()
        {
            Assert.IsTrue(List.ContainsItem(new ArrayList { 1, 2, 3, 4, 5 }, 4));
            Assert.IsFalse(List.ContainsItem(new ArrayList { 1, 2 }, 3));
            Assert.IsFalse(List.ContainsItem(new ArrayList(), 0));
        }

        [Test]
        public static void ReverseList()
        {
            Assert.AreEqual(new ArrayList { 5, 4, 3, 2, 1 }, List.Reverse(new List<int> { 1, 2, 3, 4, 5 }));
        }

        [Test]
        public static void CreateList()
        {
            Assert.AreEqual(new ArrayList { 1, 2, 3, 4, 5 }, List.Create(1, 2, 3, 4, 5));
        }

        [Test]
        public static void SortList()
        {
            Assert.AreEqual(
                new ArrayList { 1, 2, 3, 4, 5 },
                List.Sort(new ArrayList { 2, 3, 5, 4, 1 }));
        }

        //[Test]
        //public static void SortListByKey()
        //{
        //    Assert.AreEqual(
        //        new ArrayList { "", " ", "  ", "   " },
        //        List.SortByKey(
        //            new ArrayList { "  ", " ", "   ", "" },
        //            new Func<string, int>(s => s.Length)));
        //}

        //[Test]
        //public static void SortListByComparison()
        //{
        //    Assert.AreEqual(
        //        new ArrayList { 5, 4, 3, 2, 1 },
        //        List.SortByComparison(
        //            new ArrayList { 3, 1, 2, 5, 4},
        //            new Func<int, int, int>((i, i1) => i1 - i)));
        //}

        [Test]
        public static void ListMinimumValue()
        {
            Assert.AreEqual(0, List.MinimumItem(new ArrayList { 8, 4, 0, 66, 10 }));
        }

        //[Test]
        //public static void ListMinimumByKey()
        //{
        //    Assert.AreEqual(10, List.MinimumItemByKey(new ArrayList { 8, 10, 5, 7, 1, 2 }, new Func<int, int>(i => 10 - i)));
        //}

        [Test]
        public static void ListMaximumValue()
        {
            Assert.AreEqual(66, List.MaximumItem(new List<int> { 8, 4, 0, 66, 10 }));
        }

        //[Test]
        //public static void ListMaximumByKey()
        //{
        //    Assert.AreEqual(1, List.MaximumItemByKey(new List<int> { 8, 10, 5, 7, 1, 2 }, new Func<int, int>(i => 10 - i)));
        //}

        //[Test]
        //public static void FilterList()
        //{
        //    Assert.AreEqual(new List<int> { 0, 1, 2, 3 }, List.Filter(Enumerable.Range(0, 10).ToList(), new Func<int, bool>(i => i < 4)));
        //}

        //[Test]
        //public static void FilterOutList()
        //{
        //    Assert.AreEqual(new List<int> { 0, 1, 2, 3 }, List.FilterOut(Enumerable.Range(0, 10).ToList(), new Func<int, bool>(i => i > 3)));
        //}

        //[Test]
        //public static void TrueForAllInList()
        //{
        //    Assert.IsTrue(List.TrueForAllItems(new List<int> { 0, 1, 2, 3, 4, 5 }, new Func<int, bool>(x => x < 10)));

        //    //Test short circuit
        //    Assert.IsFalse(List.TrueForAllItems(new List<int> { 10, 0 }, new Func<int, bool>(x => 10/x != 1)));
        //}

        //[Test]
        //public static void TrueForAnyInList()
        //{
        //    Assert.IsFalse(List.TrueForAnyItems(new List<int> { 0, 1, 2, 3, 4, 5 }, new Func<int, bool>(x => x >= 10)));

        //    //Test short circuit
        //    Assert.IsTrue(List.TrueForAnyItems(new List<int> { 10, 0 }, new Func<int, bool>(x => 10/x == 1)));
        //}

        [Test]
        public static void SplitList()
        {
            Assert.AreEqual(
                new object[] { 0, new List<int> { 1, 2, 3, 4, 5 } },
                List.Deconstruct(new List<int> { 0, 1, 2, 3, 4, 5 }));
        }

        [Test]
        public static void AddToList()
        {
            Assert.AreEqual(new ArrayList { 0, 1, 2 }, List.AddItemToFront(0, new ArrayList { 1, 2 }));
        }

        [Test]
        public static void TakeValuesFromList()
        {
            Assert.AreEqual(new List<int> { 0, 1 }, List.TakeItems(new List<int> { 0, 1, 2 }, 2));
        }

        [Test]
        public static void DropValuesFromList()
        {
            Assert.AreEqual(new List<int> { 2 }, List.DropItems(new List<int> { 0, 1, 2 }, 2));
        }

        [Test]
        public static void ShiftListIndices()
        {
            Assert.AreEqual(new List<int> { 2, 0, 1 }, List.ShiftIndices(new List<int> { 0, 1, 2 }, 1));
            Assert.AreEqual(new List<int> { 1, 2, 0 }, List.ShiftIndices(new List<int> { 0, 1, 2 }, -1));
        }

        [Test]
        public static void GetFromList()
        {
            Assert.AreEqual(2, List.GetItemAtIndex(new List<int> { 0, 1, 2, 3 }, 2));
        }

        [Test]
        public static void TakeListSlice()
        {
            var list = new ArrayList(Enumerable.Range(0, 10).ToList());

            Assert.AreEqual(list, List.Slice(list));
            Assert.AreEqual(new ArrayList { 0, 1, 2, 3, 4 }, List.Slice(list, count: 5));
            Assert.AreEqual(new ArrayList { 1, 2, 3, 4, 5 }, List.Slice(list, 1, 5));
            Assert.AreEqual(new ArrayList { 0, 2, 4, 6, 8 }, List.Slice(list, step: 2));
        }

        [Test]
        public static void RemoveValueFromList()
        {
            Assert.AreEqual(new List<int> { 0, 1, 3, 4 }, List.RemoveItemAtIndex(new List<int> { 0, 1, 2, 3, 4 }, 2));
        }

        [Test]
        public static void RemoveMultipleValuesFromList()
        {
            Assert.AreEqual(
                new List<int> { 0, 4 },
                List.RemoveItemsAtIndices(new List<int> { 0, 1, 2, 3, 4 }, new List<int> { 1, 2, 3 }));
        }

        [Test]
        public static void DropEveryNthValueFromList()
        {
            Assert.AreEqual(
                new List<int> { 1, 2, 4, 5, 7 },
                List.DropEveryNthItem(new List<int> { 0, 1, 2, 3, 4, 5, 6, 7 }, 3, 1));
        }

        [Test]
        public static void TakeEveryNthValueFromList()
        {
            Assert.AreEqual(
                new List<int> { 3, 6 },
                List.TakeEveryNthItem(new List<int> { 0, 1, 2, 3, 4, 5, 6, 7}, 3, 1));
        }

        [Test]
        public static void EmptyList()
        {
            Assert.AreEqual(0, List.Empty.Count);
        }

        [Test]
        public static void IsEmptyList()
        {
            Assert.IsTrue(List.IsEmpty(List.Empty));
            Assert.IsFalse(List.IsEmpty(new ArrayList { 1 }));
        }

        [Test]
        public static void ListCount()
        {
            Assert.AreEqual(0, List.Count(List.Empty));
            Assert.AreEqual(3, List.Count(new ArrayList { 0, 1, 2 }));
        }

        [Test]
        public static void JoinLists()
        {
            Assert.AreEqual(
                new ArrayList { 0, 1, 2, 3, 4 },
                List.Join(new ArrayList { 0, 1 }, new ArrayList { 2 }, new ArrayList(), new ArrayList { 3, 4 }));
        }

        [Test]
        public static void FirstInList()
        {
            Assert.AreEqual(0, List.FirstItem(new ArrayList { 0, 1, 2, 3 }));
        }

        [Test]
        public static void RestOfList()
        {
            Assert.AreEqual(new List<int> { 1 }, List.RestOfItems(new List<int> { 0, 1 }));
        }

        [Test]
        public static void PartitionList()
        {
            Assert.AreEqual(
                new ArrayList { new ArrayList { 0, 1 }, new ArrayList { 2, 3 } },
                List.Chop(new ArrayList { 0, 1, 2, 3 }, 2));
        }

        [Test]
        public static void ListDiagonalRight()
        {
            var result = List.DiagonalRight(new ArrayList { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 2);
            Console.WriteLine(result);

            Assert.Inconclusive();
        }

        [Test]
        public static void ListDiagonalLeft()
        {
            Assert.AreEqual(
                new ArrayList { new ArrayList { 0 }, new ArrayList { 1, 2 }, new ArrayList { 3 }},
                List.DiagonalLeft(new ArrayList { 0, 1, 2, 3}, 2));
        }

        [Test]
        public static void TransposeListOfLists()
        {
            Assert.AreEqual(
                new List<IList> { new ArrayList { 0, 3, 6 }, new ArrayList { 1, 4, 7 }, new ArrayList { 2, 5, 8 } },
                List.Transpose(
                    new List<IList<object>>
                    {
                        new List<object> { 0, 1, 2 },
                        new List<object> { 3, 4, 5 },
                        new List<object> { 6, 7, 8 }
                    }));
        }

        [Test]
        public static void RepeatObject()
        {
            Assert.AreEqual(new List<object> { 1, 1, 1, 1 }, List.OfRepeatedItem(1, 4));
        }

        //[Test]
        //public static void FlattenListCompletely()
        //{
        //    Assert.AreEqual(
        //        new ArrayList { 0, 1, 2, 3, 4, 5, 6, 7, 8 },
        //        List.FlattenCompletely(
        //            new List<object>
        //            {
        //                new List<object> { 0, 1, 2 },
        //                new List<object> { 3, new List<object> { 4 }, 5 },
        //                new List<object> { 6, 7, 8 }
        //            }));
        //}

        //[Test]
        //public static void FlattenList()
        //{
        //    Assert.AreEqual(
        //        new ArrayList { 0, 1, 2, 3, new List<object> { 4 }, 5, 6, 7, 8 },
        //        List.Flatten(
        //            new List<object>
        //            {
        //                new List<object> { 0, 1, 2 },
        //                new List<object> { 3, new List<object> { 4 }, 5 },
        //                new List<object> { 6, 7, 8 }
        //            },
        //            1));
        //}

        [Test]
        public static void LastInList()
        {
            Assert.AreEqual(4, List.LastItem(Enumerable.Range(0, 5).ToList()));
        }

        [Test]
        public static void ShuffleList()
        {
            var numbers = Enumerable.Range(0, 100).ToList();
            var numberSet = new HashSet<int>(numbers);
            Assert.True(List.Shuffle(numbers).Cast<int>().All(numberSet.Contains));
        }

        //[Test]
        //public static void GroupListByKey()
        //{
        //    Assert.AreEqual(
        //        new ArrayList
        //        {
        //            new ArrayList { "a", "b", "c" },
        //            new ArrayList { "aa", "bb", "cc" },
        //            new ArrayList { "aaa", "bbb", "ccc" }
        //        },
        //        List.GroupByKey(
        //            new ArrayList { "a", "aa", "aaa", "b", "bb", "bbb", "c", "cc", "ccc" },
        //            new Func<string, int>(s => s.Length)));
        //}

        //[Test]
        //public static void MapList()
        //{
        //    Assert.AreEqual(
        //        new ArrayList { 1, 2, 3 },
        //        List.Map(new ArrayList { 0, 1, 2 }, new Func<int, int>(i => i + 1)));
        //}

        //[Test]
        //public static void CombineLists()
        //{
        //    var aList = new ArrayList { 1, 2, 3 };
        //    Assert.AreEqual(
        //        new ArrayList { 2, 4, 6 },
        //        List.Combine(new Func<int, int, int>((i, j) => i + j), aList, aList));
        //}

        //[Test]
        //public static void ReduceList()
        //{
        //    Assert.AreEqual(
        //        10,
        //        List.Reduce(
        //            new Func<int, int, int>((i, j) => i + j),
        //            0,
        //            Enumerable.Range(0, 5).ToList()));

        //    Assert.AreEqual(
        //        20,
        //        List.Reduce(
        //            new Func<int, int, int, int>((i, j, k) => i + j + k),
        //            0,
        //            Enumerable.Range(0, 5).ToList(),
        //            Enumerable.Range(0, 5).ToList()));
        //}

        [Test]
        public static void ListPermutations()
        {
            var check = List.Permutations(new ArrayList { "A", "B", "C", "D" }, 2);

            Console.WriteLine(string.Join("\n", check.Cast<IList>().Select(
                lst => string.Join("", lst.Cast<string>()))));

            Assert.AreEqual(
                new ArrayList
                {
                    new ArrayList { "A", "B" },
                    new ArrayList { "A", "C" },
                    new ArrayList { "A", "D" },
                    new ArrayList { "B", "A" },
                    new ArrayList { "B", "C" },
                    new ArrayList { "B", "D" },
                    new ArrayList { "C", "A" },
                    new ArrayList { "C", "B" },
                    new ArrayList { "C", "D" },
                    new ArrayList { "D", "A" },
                    new ArrayList { "D", "B" },
                    new ArrayList { "D", "C" }
                },
                check);
        }

        [Test]
        public static void ListCombinations()
        {
            var input = new ArrayList { "A", "B", "C", "D" };

            var check = List.Combinations(input, 2);

            Console.WriteLine(string.Join("\n", check.Cast<IList>().Select(
                lst => string.Join("", lst.Cast<string>()))));

            Assert.AreEqual(
                new ArrayList
                {
                    new ArrayList { "A", "B" },
                    new ArrayList { "A", "C" },
                    new ArrayList { "A", "D" },
                    new ArrayList { "B", "C" },
                    new ArrayList { "B", "D" },
                    new ArrayList { "C", "D" }
                },
                check);

            check = List.Combinations(input, 2, true);

            Assert.AreEqual(
                new ArrayList
                {
                    new ArrayList { "A", "A" },
                    new ArrayList { "A", "B" },
                    new ArrayList { "A", "C" },
                    new ArrayList { "A", "D" },
                    new ArrayList { "B", "B" },
                    new ArrayList { "B", "C" },
                    new ArrayList { "B", "D" },
                    new ArrayList { "C", "C" },
                    new ArrayList { "C", "D" },
                    new ArrayList { "D", "D" }
                },
                check);
        }
    }
}
