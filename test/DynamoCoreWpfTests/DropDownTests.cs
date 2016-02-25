using System.Collections.Generic;

using SystemTestServices;

using CoreNodeModels;

using Dynamo.Nodes;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    public class DropDownTests : SystemTestBase
    {
        [Test]
        public void Save_NothingSelected()
        {
            Assert.AreEqual(
                "-1",
                DSDropDownBase.SaveSelectedIndexImpl(-1, TestList()));
        }

        [Test]
        public void Save_NothingInList()
        {
            Assert.AreEqual("-1", DSDropDownBase.SaveSelectedIndexImpl(5, new List<DynamoDropDownItem>()));
        }

        [Test]
        public void Save_SelectedIndex()
        {
            Assert.AreEqual(
                "2:banana:blueberry",
                DSDropDownBase.SaveSelectedIndexImpl(2, TestList()));
        }

        [Test]
        public void Load_NothingSelected()
        {
            Assert.AreEqual(-1, DSDropDownBase.ParseSelectedIndexImpl("-1", TestList()));
        }

        [Test]
        public void Load_Selection()
        {
            Assert.AreEqual(2, DSDropDownBase.ParseSelectedIndexImpl("2:banana:blueberry", TestList()));
        }

        [Test]
        public void Load_SelectionIndexOnly()
        {
            Assert.AreEqual(2, DSDropDownBase.ParseSelectedIndexImpl("2", TestList()));
        }

        [Test]
        public void Load_SelectionIndexOutOfRange()
        {
            Assert.AreEqual(-1, DSDropDownBase.ParseSelectedIndexImpl("12", TestList()));
        }

        [Test]
        public void Load_SelectionIndexNoNameMatch()
        {
            Assert.AreEqual(-1, DSDropDownBase.ParseSelectedIndexImpl("2:foo", TestList()));
        }

        private static List<DynamoDropDownItem> TestList()
        {
            var items = new List<DynamoDropDownItem>
            {
                new DynamoDropDownItem("cat", "cat"),
                new DynamoDropDownItem("dog", "dog"),
                new DynamoDropDownItem("banana:blueberry", "stuff"),
                new DynamoDropDownItem("!@#$%%%^&*()", "craziness")
            };

            return items;
        }

    }
}
