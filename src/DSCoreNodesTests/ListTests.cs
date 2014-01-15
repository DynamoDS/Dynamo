using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DSCoreNodes;
using List = DSCoreNodes.List;

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
            Assert.AreEqual(0, List.MinimumValue(new List<int> { 8, 4, 0, 66, 10 }));
        }

        [Test]
        public static void ListMinimumByKey()
        {
            Assert.AreEqual(10, List.MinimumValueByKey(new List<int> { 8, 10, 5, 7, 1, 2 }, i => 10 - i));
        }

        [Test]
        public static void ListMaximumValue()
        {
            Assert.AreEqual(66, List.MaximumValue(new List<int> { 8, 4, 0, 66, 10 }));
        }

        [Test]
        public static void ListMaximumByKey()
        {
            Assert.AreEqual(1, List.MaximumValueByKey(new List<int> { 8, 10, 5, 7, 1, 2 }, i => 10 - i));
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
            Assert.IsTrue(List.TrueForAll(x => x < 10, new List<int> { 0, 1, 2, 3, 4, 5 }));

            //Test short circuit
            Assert.IsFalse(
                List.TrueForAll(
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
            Assert.IsFalse(List.TrueForAny(x => x >= 10, new List<int> { 0, 1, 2, 3, 4, 5 }));

            //Test short circuit
            Assert.IsTrue(
                List.TrueForAny(
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
                List.SplitList(new List<int> { 0, 1, 2, 3, 4, 5 }));
        }

        [Test]
        public static void AddToList()
        {
            Assert.AreEqual(new ArrayList { 0, 1, 2 }, List.AddToList(0, new ArrayList { 1, 2 }));
        }

        [Test]
        public static void TakeValuesFromList()
        {
            Assert.AreEqual(new List<int> { 0, 1 }, List.TakeFromList(new List<int> { 0, 1, 2 }, 2));
        }

        [Test]
        public static void DropValuesFromList()
        {
            Assert.AreEqual(new List<int> { 2 }, List.DropFromList(new List<int> { 0, 1, 2 }, 2));
        }

        [Test]
        public static void ShiftListIndices()
        {
            Assert.AreEqual(new List<int> { 2, 0, 1 }, List.ShiftListIndices(new List<int> { 0, 1, 2 }, 1));
            Assert.AreEqual(new List<int> { 1, 2, 0 }, List.ShiftListIndices(new List<int> { 0, 1, 2 }, -1));
        }

        [Test]
        public static void GetFromList()
        {
            Assert.AreEqual(2, List.GetFromList(new List<int> { 0, 1, 2, 3 }, 2));
        }

        [Test]
        public static void TakeListSlice()
        {
            var list = new ArrayList(Enumerable.Range(0, 10).ToList());

            Assert.AreEqual(list, List.SliceList(list));
            Assert.AreEqual(new ArrayList { 0, 1, 2, 3, 4 }, List.SliceList(list, count: 5));
            Assert.AreEqual(new ArrayList { 1, 2, 3, 4, 5 }, List.SliceList(list, 1, 5));
            Assert.AreEqual(new ArrayList { 0, 2, 4, 6, 8 }, List.SliceList(list, step: 2));
        }

        [Test]
        public static void RemoveValueFromList()
        {
            Assert.AreEqual(new List<int> { 0, 1, 3, 4 }, List.RemoveFromList(new List<int> { 0, 1, 2, 3, 4 }, 2));
        }

        [Test]
        public static void RemoveMultipleValuesFromList()
        {
            Assert.AreEqual(
                new List<int> { 0, 4 },
                List.RemoveFromList(new List<int> { 0, 1, 2, 3, 4 }, new List<int> { 1, 2, 3 }));
        }

        [Test]
        public static void DropEveryNthValueFromList()
        {
            Assert.AreEqual(
                new List<int> { 1, 2, 4, 5, 7 },
                List.DropEveryNth(new List<int> { 0, 1, 2, 3, 4, 5, 6, 7 }, 3, 1));
        }

        [Test]
        public static void TakeEveryNthValueFromList()
        {
            Assert.AreEqual(
                new List<int> { 3, 6 },
                List.TakeEveryNth(new List<int> { 0, 1, 2, 3, 4, 5, 6, 7}, 3, 1));
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
            Assert.AreEqual(0, List.First(new ArrayList { 0, 1, 2, 3 }));
        }

        [Test]
        public static void RestOfList()
        {
            Assert.AreEqual(new List<int> { 1 }, List.Rest(new List<int> { 0, 1 }));
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
            Assert.AreEqual(new List<object> { 1, 1, 1, 1 }, List.Repeat(1, 4));
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
