using System.Collections.Generic;
using Dynamo.Selection;
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
            SmartCollection<ISelectable> smartCollection = new SmartCollection<ISelectable>(new List<ISelectable>());

            DynamoSelection.Instance.Selection = smartCollection;

            Assert.AreEqual(smartCollection, DynamoSelection.Instance.Selection);
        }

        [Test]
        [Category("UnitTests")]
        public void SmartCollectionConstructorTest()
        {
            IEnumerable<int> enumerable = new List<int>() { 1 };
            SmartCollection<int> smartCollection = new SmartCollection<int>(enumerable);

            Assert.AreEqual(1, smartCollection.Count);
            Assert.IsTrue(smartCollection.Contains(1));

            var list = new List<int>() { 1, 2 };
            smartCollection = new SmartCollection<int>(list);

            Assert.AreEqual(2, smartCollection.Count);
            Assert.IsTrue(smartCollection.Contains(1));
            Assert.IsTrue(smartCollection.Contains(2));
        }
    }
}