using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Dynamo.Utilities;
using NUnit.Framework;

namespace DynamoUtilitiesTests
{
    /// <summary>
    /// Tests for <see cref="ObservableHashSet{T}"/>. The concurrency tests guard the
    /// DYN-9493 crash: the set is mutated on the scheduler thread while being enumerated
    /// on the UI thread, which used to throw "Collection was modified; enumeration
    /// operation may not execute." from HashSet.Enumerator.MoveNext().
    /// </summary>
    [TestFixture]
    public class ObservableHashSetTests
    {
        [Test, Category("UnitTests")]
        public void WhenEnumeratingWhileAddRangeMutatesFromAnotherThreadThenDoesNotThrow()
        {
            // Arrange
            var set = new ObservableHashSet<int>();
            const int iterations = 50000;
            Exception caught = null;

            // Act - one thread hammers the set with AddRange/RemoveWhere while another
            // thread enumerates it. On master this reliably throws InvalidOperationException.
            var writer = Task.Run(() =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    set.AddRange(new[] { i, i + 1, i + 2 });
                    set.RemoveWhere(x => x < i);
                }
            });

            var reader = Task.Run(() =>
            {
                try
                {
                    for (int i = 0; i < iterations; i++)
                    {
                        int sum = 0;
                        foreach (var item in set)
                        {
                            sum += item;
                        }
                    }
                }
                catch (InvalidOperationException ex)
                {
                    caught = ex;
                }
            });

            Task.WaitAll(writer, reader);

            // Assert
            Assert.IsNull(caught, "Enumerating the set while it was mutated on another thread threw: " + caught);
        }

        [Test, Category("UnitTests")]
        public void WhenCountReadWhileMutatedFromAnotherThreadThenDoesNotThrow()
        {
            // Arrange
            var set = new ObservableHashSet<int>();
            const int iterations = 50000;
            Exception caught = null;

            // Act
            var writer = Task.Run(() =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    set.Add(i);
                    set.RemoveWhere(x => x % 2 == 0);
                }
            });

            long checksum = 0;
            var reader = Task.Run(() =>
            {
                try
                {
                    for (int i = 0; i < iterations; i++)
                    {
                        // Consume both reads so the calls genuinely exercise Count/Any
                        // under contention rather than being dead code.
                        checksum += set.Count;
                        checksum += set.Any(x => x > 0) ? 1 : 0;
                    }
                }
                catch (InvalidOperationException ex)
                {
                    caught = ex;
                }
            });

            Task.WaitAll(writer, reader);

            // Assert
            Assert.IsNull(caught, "Reading Count/Any while the set was mutated on another thread threw: " + caught);
            Assert.GreaterOrEqual(checksum, 0);
        }

        [Test, Category("UnitTests")]
        public void WhenSetMutatedAfterGetEnumeratorThenEnumeratorSeesSnapshot()
        {
            // Arrange
            var set = new ObservableHashSet<int>();
            set.AddRange(new[] { 1, 2, 3 });

            // Act - grab the enumerator, then mutate the set. A snapshot enumerator must
            // remain stable (and not throw) regardless of the later mutation.
            var enumerator = set.GetEnumerator();
            set.Add(4);
            set.RemoveWhere(x => x == 1);

            var observed = new List<int>();
            while (enumerator.MoveNext())
            {
                observed.Add(enumerator.Current);
            }

            // Assert
            CollectionAssert.AreEquivalent(new[] { 1, 2, 3 }, observed);
        }

        [Test, Category("UnitTests")]
        public void WhenAddNewItemThenCollectionChangedRaisedWithItem()
        {
            // Arrange
            var set = new ObservableHashSet<int>();
            NotifyCollectionChangedEventArgs args = null;
            set.CollectionChanged += (s, e) => args = e;

            // Act
            set.Add(42);

            // Assert
            Assert.IsNotNull(args);
            Assert.AreEqual(NotifyCollectionChangedAction.Add, args.Action);
            CollectionAssert.Contains(args.NewItems, 42);
        }

        [Test, Category("UnitTests")]
        public void WhenAddDuplicateItemThenCollectionChangedNotRaised()
        {
            // Arrange
            var set = new ObservableHashSet<int>();
            set.Add(42);
            int raised = 0;
            set.CollectionChanged += (s, e) => raised++;

            // Act - adding an item already present is a no-op on the underlying set.
            set.Add(42);

            // Assert
            Assert.AreEqual(0, raised, "CollectionChanged should not fire for a duplicate Add.");
        }

        [Test, Category("UnitTests")]
        public void WhenRemoveWhereMatchesNothingThenCollectionChangedNotRaised()
        {
            // Arrange
            var set = new ObservableHashSet<int>();
            set.AddRange(new[] { 1, 2, 3 });
            int raised = 0;
            set.CollectionChanged += (s, e) => raised++;

            // Act
            set.RemoveWhere(x => x > 100);

            // Assert
            Assert.AreEqual(0, raised, "CollectionChanged should not fire when RemoveWhere removes nothing.");
        }

        [Test, Category("UnitTests")]
        public void WhenRemoveWhereMatchesItemsThenResetRaised()
        {
            // Arrange
            var set = new ObservableHashSet<int>();
            set.AddRange(new[] { 1, 2, 3 });
            NotifyCollectionChangedEventArgs args = null;
            set.CollectionChanged += (s, e) => args = e;

            // Act
            set.RemoveWhere(x => x == 2);

            // Assert
            Assert.IsNotNull(args);
            Assert.AreEqual(NotifyCollectionChangedAction.Reset, args.Action);
        }

        [Test, Category("UnitTests")]
        public void WhenAddRangeContainsNewAndDuplicateItemsThenOnlyNewItemsReported()
        {
            // Arrange
            var set = new ObservableHashSet<int>();
            set.Add(1);
            NotifyCollectionChangedEventArgs args = null;
            set.CollectionChanged += (s, e) => args = e;

            // Act - 1 is already present, 2 and 3 are new.
            set.AddRange(new[] { 1, 2, 3 });

            // Assert
            Assert.IsNotNull(args);
            Assert.AreEqual(NotifyCollectionChangedAction.Add, args.Action);
            CollectionAssert.AreEquivalent(new[] { 2, 3 }, args.NewItems.Cast<int>().ToList());
        }

        [Test, Category("UnitTests")]
        public void WhenAddRangeAddsNoNewItemsThenCollectionChangedNotRaised()
        {
            // Arrange
            var set = new ObservableHashSet<int>();
            set.AddRange(new[] { 1, 2, 3 });
            int raised = 0;
            set.CollectionChanged += (s, e) => raised++;

            // Act
            set.AddRange(new[] { 1, 2, 3 });

            // Assert
            Assert.AreEqual(0, raised, "CollectionChanged should not fire when AddRange adds no new items.");
        }
    }
}
