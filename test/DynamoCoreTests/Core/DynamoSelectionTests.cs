using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Dynamo.Selection;
using DynamoUtilities;
using NUnit.Framework;

namespace Dynamo.Tests.Core
{
    internal class SelectableDummy : ISelectable {
        public bool IsSelected { get; set; }
        /// <summary>
        /// Select object.
        /// </summary>
        public void Select() { IsSelected = true; }

        /// <summary>
        /// Deselect object.
        /// </summary>
        public void Deselect() { IsSelected = false; }
    }

    [TestFixture]
    public class DynamoSelectionTests
    {
        [Test]
        [Category("UnitTests")]
        public void SelectionPropertyTest()
        {
            IEnumerable<ISelectable> enumerable = new List<ISelectable>();
            SmartObservableCollection<ISelectable> smartCollection = new SmartObservableCollection<ISelectable>(new List<ISelectable>());

            DynamoSelection.Instance.Selection = smartCollection;

            Assert.AreEqual(smartCollection, DynamoSelection.Instance.Selection);

            DynamoSelection.DestroyInstance();
        }

        [Test]
        [Category("UnitTests")]
        public void SmartCollectionConstructorTest()
        {
            IEnumerable<int> enumerable = new List<int>() { 1 };
            SmartObservableCollection<int> smartCollection = new SmartObservableCollection<int>(enumerable);

            Assert.AreEqual(1, smartCollection.Count);
            Assert.IsTrue(smartCollection.Contains(1));

            var list = new List<int>() { 1, 2 };
            smartCollection = new SmartObservableCollection<int>(list);

            Assert.AreEqual(2, smartCollection.Count);
            Assert.IsTrue(smartCollection.Contains(1));
            Assert.IsTrue(smartCollection.Contains(2));
        }

        [Test]
        [Category("UnitTests")]
        public void SelectionAddRangeTest()
        {
            List<ISelectable> objects = new List<ISelectable>();
            objects.Add(new SelectableDummy());
            objects.Add(new SelectableDummy());
            objects.Add(new SelectableDummy());

            int eventCounter = 0;
            NotifyCollectionChangedEventHandler handler = (object sender, NotifyCollectionChangedEventArgs e) => 
            {
                Assert.AreEqual(NotifyCollectionChangedAction.Add, e.Action);
                Assert.AreEqual(3, e.NewItems.Count);
                eventCounter++;
            };

            DynamoSelection.Instance.Selection.CollectionChanged += handler;
            DynamoSelection.Instance.Selection.AddRange(objects);

            foreach(var item in objects)
            {
                Assert.IsTrue(item.IsSelected);
            }
            Assert.AreEqual(3, DynamoSelection.Instance.Selection.Count);
            Assert.AreEqual(1, eventCounter);
            DynamoSelection.Instance.Selection.CollectionChanged -= handler;
            DynamoSelection.Instance.Selection.Clear();
        }

        [Test]
        [Category("UnitTests")]
        public void SelectionRemoveRangeTest()
        {
            List<ISelectable> objects = new List<ISelectable>();
            objects.Add(new SelectableDummy());
            objects.Add(new SelectableDummy());
            objects.Add(new SelectableDummy());

            DynamoSelection.Instance.Selection.AddRange(objects);

            foreach (var item in objects)
            {
                Assert.IsTrue(item.IsSelected);
            }

            int eventCounter = 0;
            NotifyCollectionChangedEventHandler handler = (object sender, NotifyCollectionChangedEventArgs e) =>
            {
                Assert.AreEqual(NotifyCollectionChangedAction.Remove, e.Action);
                Assert.AreEqual(3, e.OldItems.Count);
                eventCounter++;
            };

            DynamoSelection.Instance.Selection.CollectionChanged += handler;
            DynamoSelection.Instance.Selection.RemoveRange(objects);

            foreach (var item in objects)
            {
                Assert.IsFalse(item.IsSelected);
            }
            Assert.AreEqual(0, DynamoSelection.Instance.Selection.Count);

            Assert.AreEqual(1, eventCounter);
            DynamoSelection.Instance.Selection.CollectionChanged -= handler;
            DynamoSelection.Instance.Selection.Clear();
        }

        [Test]
        [Category("UnitTests")]
        public void SelectionSetCollectionTest()
        {
            List<ISelectable> objects = new List<ISelectable>();
            objects.Add(new SelectableDummy());
            objects.Add(new SelectableDummy());
            objects.Add(new SelectableDummy());

            DynamoSelection.Instance.Selection.AddRange(objects);

            foreach (var item in objects)
            {
                Assert.IsTrue(item.IsSelected);
            }

            List<ISelectable> newObjects = new List<ISelectable>();
            newObjects.Add(new SelectableDummy());

            int eventCounter = 0;
            NotifyCollectionChangedEventHandler handler = (object sender, NotifyCollectionChangedEventArgs e) =>
            {
                eventCounter++;

                if (eventCounter == 1)
                {
                    Assert.AreEqual(NotifyCollectionChangedAction.Remove, e.Action);
                    Assert.AreEqual(e.OldItems.Count, objects.Count);
                }

                if (eventCounter == 2)
                {
                    Assert.AreEqual(NotifyCollectionChangedAction.Add, e.Action);
                    Assert.AreEqual(newObjects.Count, e.NewItems.Count);
                }
            };

            DynamoSelection.Instance.Selection.CollectionChanged += handler;

            DynamoSelection.Instance.Selection.SetCollection(newObjects);

            foreach (var item in objects)
            {
                Assert.IsFalse(item.IsSelected);
            }

            foreach (var item in newObjects)
            {
                Assert.IsTrue(item.IsSelected);
            }

            Assert.AreEqual(newObjects.Count, DynamoSelection.Instance.Selection.Count);

            Assert.AreEqual(2, eventCounter);
            DynamoSelection.Instance.Selection.CollectionChanged -= handler;
            DynamoSelection.Instance.Selection.Clear();
        }

        [Test]
        [Category("UnitTests")]
        public void SelectionDeferTest()
        {
            List<ISelectable> objects = new List<ISelectable>();
            objects.Add(new SelectableDummy());
            objects.Add(new SelectableDummy());
            objects.Add(new SelectableDummy());
            objects.Add(new SelectableDummy());
            objects.Add(new SelectableDummy());

            int deferredEventCounter = 0;
            NotifyCollectionChangedEventHandler deferredHandler = (object sender, NotifyCollectionChangedEventArgs e) =>
            {
                deferredEventCounter++;

                if (deferredEventCounter == 1)
                {
                    Assert.AreEqual(NotifyCollectionChangedAction.Remove, e.Action);
                    Assert.AreEqual(1, e.OldItems.Count);
                }

                if (deferredEventCounter == 2)
                {
                    Assert.AreEqual(NotifyCollectionChangedAction.Add, e.Action);
                    Assert.AreEqual(2, e.NewItems.Count);
                }
            };

            int eventCounter = 0;
            NotifyCollectionChangedEventHandler handler = (object sender, NotifyCollectionChangedEventArgs e) =>
            {
                Assert.AreEqual(NotifyCollectionChangedAction.Reset, e.Action);
                eventCounter++;
            };

            DynamoSelection.Instance.Selection.Add(objects[0]);
            DynamoSelection.Instance.Selection.Add(objects[1]);

            DynamoSelection.Instance.Selection.CollectionChanged += handler;
            DynamoSelection.Instance.Selection.CollectionChangedDuringDeferredReset += deferredHandler;

            using (DynamoSelection.Instance.Selection.DeferCollectionReset())
            {
                DynamoSelection.Instance.Selection.Add(objects[2]);
                DynamoSelection.Instance.Selection.Add(objects[3]);
                DynamoSelection.Instance.Selection.Add(objects[4]);

                DynamoSelection.Instance.Selection.Remove(objects[4]);
                DynamoSelection.Instance.Selection.Remove(objects[1]);
            }

            Assert.AreEqual(3, DynamoSelection.Instance.Selection.Count);
            Assert.AreEqual(1, eventCounter);

            Assert.IsTrue(objects[0].IsSelected);
            Assert.IsTrue(objects[2].IsSelected);
            Assert.IsTrue(objects[3].IsSelected);

            Assert.IsFalse(objects[1].IsSelected);
            Assert.IsFalse(objects[4].IsSelected);

            DynamoSelection.Instance.Selection.CollectionChanged -= handler;
            DynamoSelection.Instance.Selection.CollectionChangedDuringDeferredReset -= deferredHandler;
            DynamoSelection.Instance.Selection.Clear();
        }
    }
}