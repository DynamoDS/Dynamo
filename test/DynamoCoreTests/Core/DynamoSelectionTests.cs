using System.Collections.Generic;
using Dynamo.Selection;
using DynamoUtilities;
using NUnit.Framework;

namespace Dynamo.Tests.Core
{
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
    }
}