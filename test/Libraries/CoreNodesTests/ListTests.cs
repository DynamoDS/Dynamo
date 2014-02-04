using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public static void ReverseList()
        {
            Assert.AreEqual(new List<int> { 5, 4, 3, 2, 1 }, List.Reverse(new List<int> { 1, 2, 3, 4, 5 }).ToList());
        }

        [Test]
        public static void ListMinimumValue()
        {
            Assert.AreEqual(0, List.MinimumItem(new List<int> { 8, 4, 0, 66, 10 }));
        }

        [Test]
        public static void ListMinimumByKey()
        {
            Assert.AreEqual(10, List.MinimumItemByKey(new List<int> { 8, 10, 5, 7, 1, 2 }, i => 10 - i));
        }

        [Test]
        public static void ListMaximumValue()
        {
            Assert.AreEqual(66, List.MaximumItem(new List<int> { 8, 4, 0, 66, 10 }));
        }

        [Test]
        public static void ListMaximumByKey()
        {
            Assert.AreEqual(1, List.MaximumItemByKey(new List<int> { 8, 10, 5, 7, 1, 2 }, i => 10 - i));
        }

        [Test]
        public static void FilterList()
        {
            Assert.AreEqual(new List<int> { 0, 1, 2, 3 }, List.Filter(Enumerable.Range(0, 10), i => i < 4));
        }

        [Test]
        public static void FilterOutList()
        {
            Assert.AreEqual(new List<int> { 0, 1, 2, 3 }, List.FilterOut(Enumerable.Range(0, 10), i => i > 3));
        }

        [Test]
        public static void TrueForAllInList()
        {
            Assert.IsTrue(List.TrueForAllItems(x => x < 10, new List<int> { 0, 1, 2, 3, 4, 5 }));

            //Test short circuit
            Assert.IsFalse(
                List.TrueForAllItems(
                    x => x < 10,
                    new List<int> { 10, 0 }.Select(
                        delegate(int x, int i)
                        {
                            if (i == 1)
                                Assert.Fail();

                            return x;
                        })));
        }

        [Test]
        public static void TrueForAnyInList()
        {
            Assert.IsFalse(List.TrueForAnyItems(x => x >= 10, new List<int> { 0, 1, 2, 3, 4, 5 }));

            //Test short circuit
            Assert.IsTrue(
                List.TrueForAnyItems(
                    x => x >= 10,
                    new List<int> { 10, 0 }.Select(
                        delegate(int x, int i)
                        {
                            if (i == 1)
                                Assert.Fail();

                            return x;
                        })));
        }

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
            Assert.AreEqual(0, List.Empty().Count);
        }

        [Test]
        public static void IsEmptyList()
        {
            Assert.IsTrue(List.IsEmpty(List.Empty()));
            Assert.IsFalse(List.IsEmpty(new ArrayList { 1 }));
        }

        [Test]
        public static void ListCount()
        {
            Assert.AreEqual(0, List.Count(List.Empty()));
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

        [Test]
        public static void FlattenListCompletely()
        {
            Assert.AreEqual(
                new ArrayList { 0, 1, 2, 3, 4, 5, 6, 7, 8 },
                List.FlattenCompletely(
                    new List<object>
                    {
                        new List<object> { 0, 1, 2 },
                        new List<object> { 3, new List<object> { 4 }, 5 },
                        new List<object> { 6, 7, 8 }
                    }));
        }

        [Test]
        public static void FlattenList()
        {
            Assert.AreEqual(
                new ArrayList { 0, 1, 2, 3, new List<object> { 4 }, 5, 6, 7, 8 },
                List.Flatten(
                    new List<object>
                    {
                        new List<object> { 0, 1, 2 },
                        new List<object> { 3, new List<object> { 4 }, 5 },
                        new List<object> { 6, 7, 8 }
                    },
                    1));
        }
    }
}
