using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using List = DSCore.List;

namespace DSCoreNodesTests
{
    [TestFixture]
    internal static class ListTests
    {
        #region UniqueInList
        private sealed class Point
        {
            private bool Equals(Point other)
            {
                return X == other.X && Y == other.Y;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (X*397) ^ Y;
                }
            }

            public readonly int X;
            public readonly int Y;

            public Point(int x, int y)
            {
                X = x;
                Y = y;
            }

            public Point() { }

            public override bool Equals(object obj)
            {
                return !ReferenceEquals(null, obj)
                    && (ReferenceEquals(this, obj)
                        || obj.GetType() == GetType() && Equals((Point)obj));
            }
        }

        [Test]
        public static void UniqueInListWithPoints()
        {
            var pt1 = new Point();
            var pt2 = new Point(1, 0);
            var pt3 = new Point(1, 0);

            Assert.AreEqual(
                new ArrayList { pt1, pt2 },
                List.UniqueItems(new ArrayList { pt1, pt2, pt1, pt3 }));
        }

        [Test]
        public static void UniqueInList()
        {
            Assert.AreEqual(
                new ArrayList { 1, 2, 3, 4, 5 },
                List.UniqueItems(new ArrayList { 1, 1.0, 2, 3, 4, 4.0, 5, 4, 2, 1, 3 }));
        }

        [Test]
        public static void UniqueInStringList()
        {
            Assert.AreEqual(new ArrayList { "foo", "bar" },
                List.UniqueItems(new ArrayList { "foo", "bar", "foo", "bar" }));
        }

        [Test]
        public static void UniqueInCharList()
        {
            Assert.AreEqual(new ArrayList { 'a', 'b', 'c' },
                List.UniqueItems(new ArrayList { 'a', 'b', 'c', 'a', 'b', 'c' }));
        }

        [Test]
        public static void UniqueInBooleanList()
        {
            Assert.AreEqual(new ArrayList { true },
                List.UniqueItems(new ArrayList { true, true, true, true, true}));
        }

        [Test]
        public static void UniqueInNullList()
        {
            Assert.AreEqual(new ArrayList { null },
                List.UniqueItems(new ArrayList { null, null, null, null }));
        }

        [Test]
        public static void UniqueInCombineList()
        {
            Assert.AreEqual(new ArrayList { true, null, 'a', "foo"},
                List.UniqueItems(new ArrayList { true, true, null, null, 'a', 'a', "foo", "foo" }));
        }
        #endregion

        [Test]
        [Category("UnitTests")]
        public static void ListContainsInSublists()
        {
            Assert.IsTrue(List.Contains(new ArrayList { 1, 2, 3, 4, 5 }, 4));
            Assert.IsTrue(List.Contains(new ArrayList { 1, new ArrayList { 2, 3 }, 4, 5 }, 2));
            Assert.IsFalse(List.Contains(new ArrayList (), 0));
        }

        [Test]
        [Category("UnitTests")]
        public static void ListContainsNumbers()
        {
            var list = new[] { 1, 2, 3, 4, 5 };
            foreach(var l in list)
            {
                Assert.IsTrue(List.Contains(list, (double)(l / 3.0) * 3));
            }
        }

        [Test]
        [Category("UnitTests")]
        public static void ListIsHomogeneous()
        {
            Assert.IsTrue(List.IsHomogeneous(new ArrayList { 1, 2, 4, 8 }));
            Assert.IsFalse(List.IsHomogeneous(new ArrayList { "string", true, 4, 8 }));
        }

        [Test]
        [Category("UnitTests")]
        public static void ListIsRectangular()
        {
            Assert.IsTrue(List.IsRectangular(new ArrayList { new ArrayList { 1, 2, 3 }, new ArrayList { 4, 5, 6 } }));
            Assert.IsFalse(List.IsRectangular(new ArrayList()));
            Assert.IsFalse(List.IsRectangular(new ArrayList { 1, 2, 4, 8 }));
        }

        [Test]
        [Category("UnitTests")]
        public static void ListIsUniformDepth()
        {
            Assert.IsTrue(List.IsUniformDepth(new ArrayList()));
            Assert.IsTrue(List.IsUniformDepth(new ArrayList { new ArrayList { 1, 2, 3 }, new ArrayList { 4, 5, 6, 7 } }));
            Assert.IsFalse(List.IsUniformDepth(new ArrayList { new ArrayList { 1, 2, 3, new ArrayList { 0, 8, 2 } }, new ArrayList { 4, 5, 6, 7 } }));
        }

        [Test]
        [Category("UnitTests")]
        public static void ListSetDifference()
        {
            Assert.AreEqual(new ArrayList { 1 }, List.SetDifference(new List<object> { 1, 2, 3 }, new List<object> { 2, 3 }));
            Assert.AreEqual(new ArrayList { 9 }, List.SetDifference(new List<object> { 9 }, new List<object> { 0, 1, 2 }));
            Assert.AreEqual(new ArrayList { 9 }, List.SetDifference(new List<object> { 9, 9, 8 }, new List<object> { 8, 7 }));
        }

        [Test]
        [Category("UnitTests")]
        public static void ListSetIntersection()
        {
            Assert.AreEqual(new ArrayList { 2, 3 }, List.SetIntersection(new List<object> { 1, 2, 3 }, new List<object> { 2, 3 }));
            Assert.AreEqual(new ArrayList(), List.SetIntersection(new List<object> { 9 }, new List<object> { 0, 1, 2 }));
            Assert.AreEqual(new ArrayList { 8 }, List.SetIntersection(new List<object> { 9, 9, 8 }, new List<object> { 8, 7 }));
        }

        [Test]
        [Category("UnitTests")]
        public static void ListSetUnion()
        {
            Assert.AreEqual(new ArrayList { 1, 2, 3 }, List.SetUnion(new List<object> { 1, 2, 3 }, new List<object> { 2, 3 }));
            Assert.AreEqual(new ArrayList { 9, 0, 1, 2 }, List.SetUnion(new List<object> { 9 }, new List<object> { 0, 1, 2 }));
            Assert.AreEqual(new ArrayList { 8, 7 }, List.SetUnion(new List<object>(), new List<object> { 8, 7 }));
        }

        [Test]
        [Category("UnitTests")]
        public static void ListIndexOf()
        {
            Assert.AreEqual(1, List.IndexOf(new ArrayList { "x", "y", 1 }, "y"));
            Assert.AreEqual(-1, List.IndexOf(new ArrayList { 3, 4, 6, 8 }, 9));
        }

        [Test]
        [Category("UnitTests")]
        public static void ListCountFalse()
        {
            Assert.AreEqual(0, List.CountFalse(new ArrayList { 1,3,6, true }));
            Assert.AreEqual(2, List.CountFalse(new ArrayList { 2, new ArrayList { false }, false, true }));
        }

        [Test]
        [Category("UnitTests")]
        public static void ListCountTrue()
        {
            Assert.AreEqual(1, List.CountTrue(new ArrayList { 1, 3, 6, true }));
            Assert.AreEqual(2, List.CountTrue(new ArrayList { 2, new ArrayList { true }, false, true }));
        }

        [Test]
        public static void FlattenListCompletely()
        {
            Assert.AreEqual(
                new ArrayList { 0, 1, 2, 3, 4, 5, 6, 7, 8 },
                List.Flatten(
                    new List<object>
                    {
                        new List<object> { 0, 1, 2 },
                        new List<object> { 3, new List<object> { 4 }, 5 },
                        new List<object> { 6, 7, 8 }
                    }));
        }

        [Test]
        [Category("UnitTests")]
        public static void FlattenListByAmount()
        {
            List<object> testList = new List<object>
                    {
                        new List<object> { 0, 1, 2 },
                        new List<object> { 3, new List<object> { 4 }, 5 },
                        new List<object> { 6, 7, 8 }
                    };
            
            Assert.AreEqual(
                new ArrayList { 0, 1, 2, 3, new List<object> { 4 }, 5, 6, 7, 8 },
                List.Flatten(testList, 1));
            Assert.AreEqual(
                new ArrayList { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, List.Flatten(testList, 2));
        }

        [Test]
        [Category("UnitTests")]
        public static void InsertToList()
        {
            Assert.AreEqual(new ArrayList { 1, 9, 2, 3, 4, 5 }, List.Insert(new ArrayList { 1, 2, 3, 4, 5 }, 9, 1));
        }

        [Test]
        [Category("UnitTests")]
        public static void ReorderList()
        {
            Assert.AreEqual(new ArrayList { 4, 8, 10, 6, 2 }, List.Reorder(new ArrayList { 2, 4, 6, 8, 10 }, new ArrayList { 1, 3, 4, 2, 0 }));
        }

        [Test]
        [Category("UnitTests")]
        public static void SortIndexByValue()
        {
            Assert.AreEqual(new ArrayList { 4, 2, 3, 1, 0 }, List.SortIndexByValue(new List<double> { 8.0, 4.1, 2.0, 4.0, 0.0 }));
        }
        
        [Test]
        [Category("UnitTests")]
        public static void NormalizeDepthWithRank()
        {
            List<object> testList = new List<object> { 1, 2, new ArrayList { 3, 4, 5 } };
            Assert.AreEqual(
                new ArrayList {
                    new ArrayList { 1 },
                    new ArrayList { 2 },
                    new ArrayList { 3, 4, 5 }
                },
                List.NormalizeDepth(testList, 2));
            Assert.AreEqual(
                new ArrayList { 1, 2, 3, 4, 5 }, List.NormalizeDepth(testList, 1));
        }

        [Test]
        [Category("UnitTests")]
        public static void ReplaceItemAtIndex()
        {
            Assert.AreEqual(List.ReplaceItemAtIndex(new ArrayList { 5, 4, 3, 2, 1 }, 0, 20), new ArrayList { 20, 4, 3, 2, 1 });
            Assert.AreEqual(List.ReplaceItemAtIndex(new ArrayList { 5, 4, 3, 2, 1 }, -1, 20), new ArrayList { 5, 4, 3, 2, 20 });

            Assert.Throws<IndexOutOfRangeException>(() => List.ReplaceItemAtIndex(new ArrayList { 5, 4, 3, 2, 1 }, 12, 20));
        }

        [Test]
        [Category("UnitTests")]
        public static void ReverseList()
        {
            Assert.AreEqual(new ArrayList { 5, 4, 3, 2, 1 }, List.Reverse(new List<int> { 1, 2, 3, 4, 5 }));
        }


        /*
        [Test]
        public static void CreateList()
        {
            Assert.AreEqual(new ArrayList { 1, 2, 3, 4, 5 }, List.Create(1, 2, 3, 4, 5));
        }
         * */

        [Test]
        [Category("UnitTests")]
        public static void SortList()
        {
            var sorted = Enumerable.Range(1, 5).ToList();

            Assert.AreEqual(sorted, List.Sort(List.Shuffle(sorted).Cast<object>()));
        }

        [Test]
        [Category("UnitTests")]
        public static void SortMixedList1()
        {
            Assert.AreEqual(
                new List<object> { 1, 2, 3.5, 4.002, 5 },
                List.Sort(new List<object> { 2, 3.5, 5, 4.002, 1 }));
        }

        [Test]
        [Category("UnitTests")]
        public static void SortMixedList2()
        {
            var obj = new object();
            Assert.AreEqual(
                new ArrayList { 1, 2, 3.5, 4.002, 5, false, true, "aaa", "bb", obj },
                List.Sort(new List<object> { obj, 2, 3.5, "bb", 5, 4.002, true, 1, false, "aaa" }));
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
        [Category("UnitTests")]
        public static void ListMinimumValue()
        {
            Assert.AreEqual(0, List.MinimumItem(new List<object> { 8, 4, 0, 66, 10 }));
        }

        [Test]
        [Category("UnitTests")]
        public static void ListMinimumValueMixed()
        {
            Assert.AreEqual(0, List.MinimumItem(new List<object> { 8.5, 4, 0, 6.6, 10.2 }));
        }

        //[Test]
        //public static void ListMinimumByKey()
        //{
        //    Assert.AreEqual(10, List.MinimumItemByKey(new ArrayList { 8, 10, 5, 7, 1, 2 }, new Func<int, int>(i => 10 - i)));
        //}

        [Test]
        [Category("UnitTests")]
        public static void ListMaximumValue()
        {
            Assert.AreEqual(66, List.MaximumItem(new List<object> { 8, 4, 0, 66, 10 }));
        }

        [Test]
        [Category("UnitTests")]
        public static void ListMaximumValueMixed()
        {
            Assert.AreEqual(66, List.MaximumItem(new List<object> { 8.223, 4, 0.64, 66, 10.2 }));
        }

        [Test]
        [Category("UnitTests")]
        public static void FilterListByMask()
        {
            Assert.AreEqual(
                new Dictionary<string, object>
                {
                    { "in", new List<int> { 1, 3, 5 } },
                    { "out", new List<int> { 2, 4, 6 } }
                },
                List.FilterByBoolMask(
                    new List<int> { 1, 2, 3, 4, 5, 6 },
                    new List<bool> { true, false, true, false, true, false }));

            Assert.AreEqual(
                new Dictionary<string, object>
                {
                    { "in", new List<object> { 1, 3, new List<object> { 5, 7 } } },
                    { "out", new List<object> { 2, 4, new List<object> { 6, 8 } } }
                },
                List.FilterByBoolMask(
                    new List<object> { 1, 2, 3, 4, new List<object> { 5, 6, 7, 8 } },
                    new List<object>
                    {
                        true,
                        false,
                        true,
                        false,
                        new List<object> { true, false, true, false }
                    }));
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
        [Category("UnitTests")]
        public static void SplitList()
        {
            var results = List.Deconstruct(new List<int> { 0, 1, 2, 3, 4, 5 });

            // Explicitly test each aspect of the returned value.
            Assert.IsNotNull(results);
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("first", results.Keys.Cast<string>().First());
            Assert.AreEqual("rest", results.Keys.Cast<string>().ElementAt(1));
            Assert.AreEqual(0, results["first"]);

            var rest = results["rest"] as List<object>;
            Assert.IsNotNull(rest);
            Assert.AreEqual(new List<object> { 1, 2, 3, 4, 5 }, rest);
        }

        [Test]
        [Category("UnitTests")]
        public static void AddToList()
        {
            Assert.AreEqual(new ArrayList { 0, 1, 2 }, List.AddItemToFront(0, new ArrayList { 1, 2 }));
        }

        [Test]
        [Category("UnitTests")]
        public static void TakeValuesFromList()
        {
            Assert.AreEqual(new List<int> { 0, 1 }, List.TakeItems(new List<int> { 0, 1, 2 }, 2));
        }

        [Test]
        [Category("UnitTests")]

        public static void DropValuesFromList()
        {
            Assert.AreEqual(new List<int> { 2 }, List.DropItems(new List<int> { 0, 1, 2 }, 2));
        }

        [Test]
        [Category("UnitTests")]
        public static void ShiftListIndices()
        {
            Assert.AreEqual(new List<int> { 2, 0, 1 }, List.ShiftIndices(new List<int> { 0, 1, 2 }, 1));
            Assert.AreEqual(new List<int> { 1, 2, 0 }, List.ShiftIndices(new List<int> { 0, 1, 2 }, -1));
            Assert.AreEqual(new List<int> { 2, 0, 1 }, List.ShiftIndices(new List<int> { 0, 1, 2 }, 4));
            Assert.AreEqual(new List<int> { 1, 2, 0 }, List.ShiftIndices(new List<int> { 0, 1, 2 }, -4));
        }

        [Test]
        [Category("UnitTests")]
        public static void GetFromList()
        {
            Assert.AreEqual(2, List.GetItemAtIndex(new List<int> { 0, 1, 2, 3 }, 2));
        }

        [Test]
        [Category("UnitTests")]
        public static void TakeListSlice()
        {
            var list = new ArrayList(Enumerable.Range(0, 10).ToList());
            var reversed = list.Cast<object>().Reverse().ToList();

            Assert.AreEqual(list, List.Slice(list));

            Assert.AreEqual(new ArrayList { 0, 1, 2, 3, 4 }, List.Slice(list, end: 5));
            Assert.AreEqual(new ArrayList { 1, 2, 3, 4 }, List.Slice(list, 1, 5));
            Assert.AreEqual(new ArrayList { 0, 2, 4, 6, 8 }, List.Slice(list, step: 2));
            Assert.AreEqual(reversed, List.Slice(list, step: -1));
            Assert.AreEqual(new ArrayList { 0, 1, 2, 3, 4 }, List.Slice(list, -11, -5));
            Assert.AreEqual(reversed, List.Slice(list, -1, -11, -1));
        }

        [Test]
        [Category("UnitTests")]
        public static void RemoveValueFromList()
        {
            Assert.AreEqual(new List<int> { 0, 1, 3, 4 }, List.RemoveItemAtIndex(new List<int> { 0, 1, 2, 3, 4 }, new[]{2}));
        }

        [Test]
        [Category("UnitTests")]
        public static void RemoveMultipleValuesFromList()
        {
            Assert.AreEqual(
                new List<int> { 0, 4 },
                List.RemoveItemAtIndex(new List<int> { 0, 1, 2, 3, 4 }, new[] { 1, 2, 3 }));
        }

        [Test]
        [Category("UnitTests")]
        public static void RemoveMultipleValuesFromNestedList()
        {
            var strings = new List<string> { "one", "two" };
            Assert.AreEqual(
                new List<object> { 0, 4 },
                List.RemoveItemAtIndex(new List<object> { 0, 1, strings, new List<object> { 1, strings }, 4 }, new[] { 1, 2, 3 }));
        }

        [Test]
        [Category("UnitTests")]
        public static void DropEveryNthValueFromList()
        {
            var list = Enumerable.Range(1, 12).ToList();
            const int n = 3;

            Assert.AreEqual(new List<int> { 1, 2, 4, 5, 7, 8, 10, 11 }, List.DropEveryNthItem(list, n));
            Assert.AreEqual(new List<int> { 2, 3, 5, 6, 8, 9, 11, 12 }, List.DropEveryNthItem(list, n, 1));
            Assert.AreEqual(new List<int> { 1, 3, 4, 6, 7, 9, 10, 12 }, List.DropEveryNthItem(list, n, 2));
            Assert.AreEqual(List.DropEveryNthItem(list, n), List.DropEveryNthItem(list, n, 3));
        }

        [Test]
        [Category("UnitTests")]
        public static void TakeEveryNthValueFromList()
        {
            var list = Enumerable.Range(1, 12).ToList();
            const int n = 3;

            Assert.AreEqual(new List<int> { 3, 6, 9, 12 }, List.TakeEveryNthItem(list, n));
            Assert.AreEqual(new List<int> { 1, 4, 7, 10 }, List.TakeEveryNthItem(list, n, 1));
            Assert.AreEqual(new List<int> { 2, 5, 8, 11 }, List.TakeEveryNthItem(list, n, 2));
            Assert.AreEqual(List.TakeEveryNthItem(list, n), List.TakeEveryNthItem(list, n, 3));
        }

        [Test]
        [Category("UnitTests")]
        public static void EmptyList()
        {
            Assert.AreEqual(0, List.Empty.Count);
        }

        [Test]
        [Category("UnitTests")]
        public static void IsEmptyList()
        {
            Assert.IsTrue(List.IsEmpty(List.Empty));
            Assert.IsFalse(List.IsEmpty(new ArrayList { 1 }));
        }

        [Test]
        [Category("UnitTests")]
        public static void ListCount()
        {
            Assert.AreEqual(0, List.Count(List.Empty));
            Assert.AreEqual(3, List.Count(new ArrayList { 0, 1, 2 }));
        }

        [Test]
        [Category("UnitTests")]
        public static void ListAnyTrue()
        {
            var oneTrue = new List<object> {true};
            var oneFalse = new List<object> {false};
            var subListsAllTrue = new List<object> {true, true, new List<object> {true, new List<object> {true, new List<object> {true, true, new List<object> {true}, true}}}, true};
            var subListsAllFalse = new List<object> {false, false, new List<object> {false, new List<object> {false, new List<object> {false, false, new List<object> {false}, false}}}, false};
            var subListsOneTrue = new List<object> {false, false, new List<object> {false, new List<object> {false, new List<object> {false, false, new List<object> {false}, false}}}, true};
            var subListsOneFalse = new List<object> {true, true, new List<object> {true, new List<object> {true, new List<object> {true, false, new List<object> {true}, true}}}, true};

            Assert.IsFalse(List.AnyTrue(List.Empty));
            Assert.IsTrue(List.AnyTrue(oneTrue));
            Assert.IsFalse(List.AnyTrue(oneFalse));
            Assert.IsTrue(List.AnyTrue(subListsAllTrue));
            Assert.IsFalse(List.AnyTrue(subListsAllFalse));
            Assert.IsTrue(List.AnyTrue(subListsOneTrue));
            Assert.IsTrue(List.AnyTrue(subListsOneFalse));
        }

        [Test]
        [Category("UnitTests")]
        public static void ListAnyFalse()
        {
            var oneTrue = new List<object> {true};
            var oneFalse = new List<object> {false};
            var subListsAllTrue = new List<object> {true, true, new List<object> {true, new List<object> {true, new List<object> {true, true, new List<object> {true}, true}}}, true};
            var subListsAllFalse = new List<object> {false, false, new List<object> {false, new List<object> {false, new List<object> {false, false, new List<object> {false}, false}}}, false};
            var subListsOneTrue = new List<object> {false, false, new List<object> {false, new List<object> {false, new List<object> {false, false, new List<object> {false}, false}}}, true};
            var subListsOneFalse = new List<object> {true, true, new List<object> {true, new List<object> {true, new List<object> {true, false, new List<object> {true}, true}}}, true};

            Assert.IsFalse(List.AnyFalse(List.Empty));
            Assert.IsFalse(List.AnyFalse(oneTrue));
            Assert.IsTrue(List.AnyFalse(oneFalse));
            Assert.IsFalse(List.AnyFalse(subListsAllTrue));
            Assert.IsTrue(List.AnyFalse(subListsAllFalse));
            Assert.IsTrue(List.AnyFalse(subListsOneTrue));
            Assert.IsTrue(List.AnyFalse(subListsOneFalse));
        }

        [Test]
        [Category("UnitTests")]
        public static void JoinLists()
        {
            Assert.AreEqual(
                new ArrayList { 0, 1, 2, 3, 4 },
                List.Join(new ArrayList { 0, 1 }, new ArrayList { 2 }, new ArrayList(), new ArrayList { 3, 4 }));
        }

        [Test]
        [Category("UnitTests")]
        public static void FirstInList()
        {
            Assert.AreEqual(0, List.FirstItem(new ArrayList { 0, 1, 2, 3 }));
        }

        [Test]
        [Category("UnitTests")]
        public static void RestOfList()
        {
            Assert.AreEqual(new List<int> { 1 }, List.RestOfItems(new List<int> { 0, 1 }));
        }

        [Test]
        [Category("UnitTests")]
        public static void PartitionList()
        {
            Assert.AreEqual(
                new ArrayList { new ArrayList { 0, 1 }, new ArrayList { 2, 3 } },
                List.Chop(new ArrayList { 0, 1, 2, 3 }, new List<int> { 2 }));
        }

        [Test]
        [Category("UnitTests")]
        public static void ListDiagonalRight()
        {
            Assert.AreEqual(
                new List<List<int>> {
                    new List<int> { 15 }, 
                    new List<int> { 10, 16 }, 
                    new List<int> { 5, 11, 17, },
                    new List<int> { 0, 6, 12, 18 },
                    new List<int> { 1, 7, 13, 19 },
                    new List<int> { 2, 8, 14 },
                    new List<int> { 3, 9 },
                    new List<int> { 4 }
                },
                List.DiagonalRight(Enumerable.Range(0, 20).ToList(), 5));
        }

        [Test]
        [Category("UnitTests")]
        public static void ListDiagonalLeft()
        {
            Assert.AreEqual(
                new List<List<int>> {
                    new List<int> { 0 }, 
                    new List<int> { 1, 5 }, 
                    new List<int> { 2, 6, 10, },
                    new List<int> { 3, 7, 11, 15 },
                    new List<int> { 4, 8, 12, 16 },
                    new List<int> { 9, 13, 17 },
                    new List<int> { 14, 18 },
                    new List<int> { 19 }
                },
                List.DiagonalLeft(Enumerable.Range(0, 20).ToList(), 5));
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
        public static void TransposeJaggedListOfLists()
        {
            Assert.AreEqual(
                new List<IList> { new ArrayList { 0, 3, 6 }, new ArrayList { 1, 4, 7 }, new ArrayList { 2, 5, 8 }, new ArrayList { null, 6, null} },
                List.Transpose(
                    new List<IList<object>>
                    {
                        new List<object> { 0, 1, 2 },
                        new List<object> { 3, 4, 5, 6 },
                        new List<object> { 6, 7, 8 }
                    }));
        }

        [Test]
        [Category("UnitTests")]
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

        [Test]
        [Category("UnitTests")]
        public static void LastInList()
        {
            Assert.AreEqual(4, List.LastItem(Enumerable.Range(0, 5).ToList()));
        }

        [Test]
        [Category("UnitTests")]
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
        [Category("UnitTests")]
        public static void ListPermutations()
        {
            var check = List.Permutations(new ArrayList { "A", "B", "C", "D" }, 2);

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
        [Category("UnitTests")]
        public static void ListCombinations()
        {
            var input = new ArrayList { "A", "B", "C", "D" };

            var check = List.Combinations(input, 2);

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

        [Test]
        [Category("UnitTests")]
        public static void Sublists()
        {
            List<int> input = Enumerable.Range(0, 10).ToList();

            Assert.AreEqual(
                new ArrayList
                {
                    new ArrayList { 0, 2 },
                    new ArrayList { 1, 3 },
                    new ArrayList { 2, 4 },
                    new ArrayList { 3, 5 },
                    new ArrayList { 4, 6 },
                    new ArrayList { 5, 7 },
                    new ArrayList { 6, 8 },
                    new ArrayList { 7, 9 },
                    new ArrayList { 8 },
                    new ArrayList { 9 }
                },
                List.Sublists(input, new ArrayList { 0, 2 }, 1));

            Assert.AreEqual(
                new ArrayList
                {
                    new ArrayList { 2, 1 },
                    new ArrayList { 5, 4 },
                    new ArrayList { 8, 7 },
                },
                List.Sublists(input, new ArrayList { 2, 1 }, 3));

            Assert.AreEqual(
                new ArrayList
                {
                    new ArrayList { 0, 5, 2, 3, 4 },
                    new ArrayList { 5, 7, 8, 9 }
                },
                List.Sublists(input, new ArrayList { 0, 5, new ArrayList { 2, 3, 4 } }, 5));


        }

        [Test]
        [Category("UnitTests")]
        public static void FirstIndexOf()
        {
            List<int> input = Enumerable.Range(0, 10).ToList();

            int index = List.FirstIndexOf(input, 3);
            Assert.AreEqual(index, 3);

            index = List.FirstIndexOf(input, 21);
            Assert.AreEqual(index, -1);
        }

        [Test]
        [Category("UnitTests")]
        public static void AllIndicesOf()
        {
            List<int> input = new List<int> { 1, 2, 3, 1, 2, 3 };

            var indices = List.AllIndicesOf(input, 3).Cast<int>();
            Assert.IsTrue(indices.SequenceEqual(new [] {2, 5}));

            indices = List.AllIndicesOf(input, 21).Cast<int>();
            Assert.IsEmpty(indices);
        }

        [Test]
        [Category("UnitTests")]
        public static void AllIndicesOfNullTest()
        {
            var input = new List<object> { true, false, null };

            var indices = List.AllIndicesOf(input, true);
            Assert.True(indices.Count == 1);
            Assert.AreEqual(0, indices[0]);

            indices = List.AllIndicesOf(input, false);
            Assert.True(indices.Count == 1);
            Assert.AreEqual(1, indices[0]);

            indices = List.AllIndicesOf(input, null);
            Assert.True(indices.Count == 1);
            Assert.AreEqual(2, indices[0]);
        }

        [Test]
        [Category("UnitTests")]
        public static void CleanNullsPreserveIndices()
        {
            var input = new ArrayList
            {
                new ArrayList {1, null, 2, null, null},
                new ArrayList {null, null, 3, 4, null},
                new ArrayList {null, null},
                new ArrayList {1, 2}
            };

            var output = List.Clean(input);

            var expected = new ArrayList
            {
                new ArrayList {1, null, 2},
                new ArrayList {null, null, 3, 4},
                null,
                new ArrayList {1, 2}
            };

            Assert.AreEqual(expected, output);
        }

        [Test]
        [Category("UnitTests")]
        public static void CleanNullsChangeIndices()
        {
            var input = new ArrayList
            {
                new ArrayList {1, null, 2, null, null},
                new ArrayList {null, null, 3, 4, null},
                new ArrayList {null, null},
                new ArrayList {1, 2}
            };

            var output = List.Clean(input, false);

            var expected = new ArrayList
            {
                new ArrayList {1, 2},
                new ArrayList {3, 4},
                new ArrayList {1, 2}
            };

            Assert.AreEqual(expected, output);
        }

        [Test]
        [Category("UnitTests")]
        public static void CleanNullsChangeIndicesEdgeCases()
        {
            // 1D array of nulls
            var input = new ArrayList { null };
            var output = List.Clean(input, false);
            var expected = new ArrayList();
            Assert.AreEqual(expected, output);

            // list is null itself
            input = null;
            output = List.Clean(input, false);
            expected = null;
            Assert.AreEqual(expected, output);

            // nested array of nulls
            input = new ArrayList { new ArrayList { null } };
            output = List.Clean(input, false);
            expected = new ArrayList();
            Assert.AreEqual(expected, output);

            // empty list
            input = new ArrayList();
            output = List.Clean(input, false);
            expected = new ArrayList();
            Assert.AreEqual(expected, output);

            // nested empty list
            input = new ArrayList { new ArrayList() };
            output = List.Clean(input, false);
            expected = new ArrayList();
            Assert.AreEqual(expected, output);
        }

        [Test]
        [Category("UnitTests")]
        public static void CleanNullsPreserveIndicesEdgeCases()
        {
            // 1D array of nulls
            var input = new ArrayList { null };
            var output = List.Clean(input);
            Assert.AreEqual(null, output);

            // list is null itself
            input = null;
            output = List.Clean(input);
            Assert.AreEqual(null, output);

            // nested array of nulls
            input = new ArrayList { new ArrayList { null } };
            output = List.Clean(input);
            var expected = new ArrayList { null };
            Assert.AreEqual(expected, output);

            // empty list
            input = new ArrayList();
            output = List.Clean(input);
            expected = new ArrayList();
            Assert.AreEqual(expected, output);

            // nested empty list
            input = new ArrayList { new ArrayList() };
            output = List.Clean(input);
            expected = new ArrayList { new ArrayList() };
            Assert.AreEqual(expected, output);
        }

        [Test]
        [Category("UnitTests")]
        public static void Chop1()
        {
            var list = new ArrayList { 1, 2, 3, 4, 5 };
            var lengths = new List<int> { 3, 2 };

            var output = List.Chop(list, lengths);
            var expected = new ArrayList { new ArrayList { 1, 2, 3 }, new ArrayList { 4, 5 } };
            Assert.AreEqual(expected, output);
        }

        [Test]
        [Category("UnitTests")]
        public static void Chop2()
        {
            var list = new ArrayList { 1, 2, 3, 4, 5 };
            var lengths = new List<int> { 0, 2 };

            var output = List.Chop(list, lengths);
            var expected = new ArrayList { new ArrayList { }, new ArrayList { 1, 2 }, new ArrayList { 3, 4 }, new ArrayList { 5 } };
            Assert.AreEqual(expected, output);
        }

        [Test]
        [Category("UnitTests")]
        public static void Chop3()
        {
            var list = new ArrayList { 1, 2, 3, 4, 5 };
            var lengths = new List<int> { -1, -1 };

            var output = List.Chop(list, lengths);
            var expected = new ArrayList { 1, 2, 3, 4, 5 };
            Assert.AreEqual(expected, output);
        }

        [Test]
        [Category("UnitTests")]
        public static void Chop4()
        {
            var list = new ArrayList { 1, 2, 3 };
            var lengths = new List<int> { 2, 5 };

            var output = List.Chop(list, lengths);
            var expected = new ArrayList { new ArrayList { 1, 2 }, new ArrayList { 3 } };
            Assert.AreEqual(expected, output);
        }

        [Test]
        [Category("UnitTests")]
        public static void Chop5()
        {
            var list = new ArrayList { 1, "a", 3 };
            var lengths = new List<int> { 2, 1 };

            var output = List.Chop(list, lengths);
            var expected = new ArrayList { new ArrayList { 1, "a" }, new ArrayList { 3 } };
            Assert.AreEqual(expected, output);
        }

        [Test]
        [Category("UnitTests")]
        public static void Chop6()
        {
            var list = new ArrayList { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var lengths = new List<int> { 1, 0, 3 };

            var output = List.Chop(list, lengths);
            var expected = new ArrayList { new ArrayList { 1 }, new ArrayList {}, new ArrayList { 2, 3, 4 },
                new ArrayList { 5, 6, 7 }, new ArrayList { 8, 9, 10 } };
            Assert.AreEqual(expected, output);
        }

        [Test]
        [Category("UnitTests")]
        public static void SortByKey1()
        {
            var list = new ArrayList { "item1", "item2" };
            var keys = new ArrayList { "key2", "key1" };

            var result = List.SortByKey(list, keys);
            var expected = new Dictionary<string, object>
            {
                { "sortedList", new object[] { "item2", "item1" } },
                { "sortedKeys", new object[] { "key1", "key2" } }
            };

            Assert.AreEqual(expected, result);
        }

        [Test]
        [Category("UnitTests")]
        public static void SortByKey2()
        {
            var list = new ArrayList { "item1", "item2" };
            var keys = new ArrayList { "key1" };

            Assert.Throws<ArgumentException>(() => List.SortByKey(list, keys));
        }

        [Test]
        [Category("UnitTests")]
        public static void SortByKey3()
        {
            var list = new ArrayList { "item1" };
            var keys = new ArrayList { "key1", "key2" };

            Assert.Throws<ArgumentException>(() => List.SortByKey(list, keys));
        }

        [Test]
        [Category("UnitTests")]
        public static void SortByKey4()
        {
            var list = new ArrayList { "Zack",
        "Ian",
        "Neal",
        "Colin",
        "Matt" };
            var keys = new ArrayList {"Kron",
        "Keough",
        "Burnham",
        "McCrone",
        "Jezyk" };

            var result = List.SortByKey(list, keys);
            var expected = new Dictionary<string, object>
            {
                { "sortedList", new object[] { "Neal", "Matt", "Ian", "Zack", "Colin" } },
                { "sortedKeys", new object[] { "Burnham", "Jezyk", "Keough", "Kron", "McCrone" } }
            };

            Assert.AreEqual(expected, result);
        }

        [Test]
        [Category("UnitTests")]
        public static void SortByKey5()
        {
            var list = new ArrayList { "Zack",
        "Ian",
        "Neal",
        "Anna"};
            var keys = new ArrayList {-3,
        1.6,
        "abc",
        5};

            var result = List.SortByKey(list, keys);
            var expected = new Dictionary<string, object>
            {
                { "sortedList", new object[] { "Zack", "Ian", "Anna", "Neal" } },
                { "sortedKeys", new object[] { -3, 1.6, 5, "abc" } }
            };

            Assert.AreEqual(expected, result);
        }

        [Test]
        [Category("UnitTests")]
        public static void SortByKey6()
        {
            var list = new ArrayList { 1, 2, 3 };
            var keys = new ArrayList { 1.21, 1.20, 1.2001 };

            var result = List.SortByKey(list, keys);
            var expected = new Dictionary<string, object>
            {
                { "sortedList", new object[] { 2, 3, 1 }},
                { "sortedKeys", new object[] { 1.20, 1.2001, 1.21 } }
            };

            Assert.AreEqual(expected, result);
        }

        [Test]
        [Category("UnitTests")]
        public static void SortByKey7()
        {
            var list = new ArrayList { 1, 2, 3 };
            var keys = new ArrayList { new object[] { true, true }, false };

            Assert.Throws<ArgumentException>(() => List.SortByKey(list, keys));
        }

        [Test]
        [Category("UnitTests")]
        public static void GroupByKey1()
        {
            var list = new ArrayList { "a", "b", "c" };
            var keys = new ArrayList { "key1", "key2", "key1" };

            var result = List.GroupByKey(list, keys);
            var expected = new Dictionary<string, object>
            {
                { "groups", new object[] { new object[] { "a", "c" }, new object[] { "b" } }},
                { "unique keys", new object[] { "key1", "key2" } }
            };

            Assert.AreEqual(expected, result);
        }

        [Test]
        [Category("UnitTests")]
        public static void GroupByKey2()
        {
            var list = new ArrayList {"San Francisco",
        "Springfield",
        "Fresno",
        "Berkeley",
        "Fall River",
        "Waltham",
        "Sacramento" };
            var keys = new ArrayList { "California",
        "Massachusetts",
        "California",
        "California",
        "Massachusetts",
        "Massachusetts",
        "California" };

            var result = List.GroupByKey(list, keys);
            var expected = new Dictionary<string, object>
            {
                { "groups", 
                    new object[]
                    {
                        new object[] { "San Francisco", "Fresno",  "Berkeley", "Sacramento"}, 
                        new object[] { "Springfield", "Fall River", "Waltham" }
                    }},
                { "unique keys", new object[] { "California", "Massachusetts" } }
            };

            Assert.AreEqual(expected, result);
        }

        [Test]
        [Category("UnitTests")]
        public static void GroupByKey3()
        {
            var list = new ArrayList { "item1", "item2", "item1", "item3" };
            var keys = new ArrayList { "key1", "key2", "key1" };

            Assert.Throws<ArgumentException>(() => List.GroupByKey(list, keys));
        }

        [Test]
        [Category("UnitTests")]
        public static void GroupByKey4()
        {
            var list = new ArrayList { "item1" };
            var keys = new ArrayList { "key1", "key2" };

            Assert.Throws<ArgumentException>(() => List.GroupByKey(list, keys));
        }
    }
}
