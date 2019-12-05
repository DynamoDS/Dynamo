using System;
using System.Linq;
using Dynamo.Search;
using Dynamo.Search.SearchElements;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    internal class SearchModelTests
    {
        private static NodeSearchModel search;

        [SetUp]
        public void Init()
        {
            search = new NodeSearchModel();
        }

        #region Refactoring

        [Test]
        [Category("UnitTests")]
        public void CanRefactorCustomNodeName()
        {
            const string nodeName = "TheNoodle";
            const string catName = "TheCat";
            const string descr = "TheCat";
            const string path = @"C:\turtle\graphics.dyn";
            var guid1 = Guid.NewGuid();
            var dummyInfo1 = new CustomNodeInfo(guid1, nodeName, catName, descr, path);
            var dummySearch1 = new CustomNodeSearchElement(null, dummyInfo1);

            search.Add(dummySearch1);

            Assert.AreEqual(1, search.NumElements);

            const string newNodeName = "TheTurtle";
            var newInfo = new CustomNodeInfo(guid1, newNodeName, catName, descr, path);
            dummySearch1.SyncWithCustomNodeInfo(newInfo);
            search.Update(dummySearch1);

            Assert.AreEqual(1, search.NumElements);

            // search for new name
            var results = search.Search(newNodeName).ToList();

            // results are correct
            Assert.AreEqual(1, results.Count());
            var res1 = results[0];
            Assert.IsInstanceOf<CustomNodeSearchElement>(res1);
            Assert.AreEqual(guid1, ((CustomNodeSearchElement)res1).ID);

            // search for old name
            var results1 = search.Search(nodeName);

            // results are correct
            Assert.AreEqual(0, results1.Count());
        }

        [Test]
        [Category("UnitTests")]
        public void CanRefactorCustomNodeDescription()
        {
            const string nodeName = "TheNoodle";
            const string catName = "TheCat";
            const string descr = "TheCat";
            const string path = @"C:\turtle\graphics.dyn";
            var guid1 = Guid.NewGuid();
            var dummyInfo1 = new CustomNodeInfo(guid1, nodeName, catName, descr, path);
            var dummySearch1 = new CustomNodeSearchElement(null, dummyInfo1);

            search.Add(dummySearch1);

            Assert.AreEqual(1, search.NumElements);

            // search for name
            var results = search.Search(nodeName).ToList();

            // results are correct
            Assert.AreEqual(1, results.Count());
            var res1 = results[0];
            Assert.IsInstanceOf<CustomNodeSearchElement>(res1);
            Assert.AreEqual(guid1, ((CustomNodeSearchElement)res1).ID);

            // refactor description
            const string newDescription = "Tickle me elmo";
            var newInfo = new CustomNodeInfo(guid1, nodeName, catName, newDescription, path);
            dummySearch1.SyncWithCustomNodeInfo(newInfo);
            search.Update(dummySearch1);

            // num elements is unchanged
            Assert.AreEqual(1, search.NumElements);

            // search for name
            var results1 = search.Search(nodeName).ToList();

            // description is updated
            Assert.AreEqual(1, results1.Count());
            var res2 = results1[0];
            Assert.IsInstanceOf<CustomNodeSearchElement>(res2);
            Assert.AreEqual(guid1, ((CustomNodeSearchElement)res2).ID);
            Assert.AreEqual(newDescription, res2.Description);
        }

        [Test]
        [Category("UnitTests")]
        public void CanRefactorCustomNodeWhilePreservingDuplicates()
        {
            const string nodeName = "TheNoodle";
            const string catName = "TheCat";
            const string descr = "TheCat";
            const string path = @"C:\turtle\graphics.dyn";
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();

            //all properties are the same except guid.
            var dummyInfo1 = new CustomNodeInfo(guid1, nodeName, catName, descr, path);
            var dummyInfo2 = new CustomNodeInfo(guid2, nodeName, catName, descr, path);

            var dummySearch1 = new CustomNodeSearchElement(null, dummyInfo1);
            var dummySearch2 = new CustomNodeSearchElement(null, dummyInfo2);

            search.Add(dummySearch1);
            search.Add(dummySearch2);

            Assert.AreEqual(2, search.NumElements);

            // refactor one of the nodes with newNodeName
            const string newNodeName = "TheTurtle";
            var newInfo = new CustomNodeInfo(guid1, newNodeName, catName, descr, path);
            dummySearch1.SyncWithCustomNodeInfo(newInfo);
            search.Update(dummySearch1);

            // num elements is unchanged
            Assert.AreEqual(2, search.NumElements);

            // search for new name
            var results = search.Search(newNodeName).ToList();

            // results are correct - only one result
            Assert.AreEqual(1, results.Count());
            var res1 = results[0];
            Assert.IsInstanceOf<CustomNodeSearchElement>(res1);
            Assert.AreSame(dummySearch1, res1);

            // search for old name
            results = search.Search(nodeName).ToList();

            // results are correct - the first nodes are returned
            Assert.AreEqual(1, results.Count());
            var res2 = results[0];
            Assert.IsInstanceOf<CustomNodeSearchElement>(res2);
            Assert.AreSame(dummySearch2, res2);
        }

        #endregion

        #region Search

        [Test]
        [Category("UnitTests")]
        public void CanDuplicateAddedNodesInSearch()
        {
            const string catName = "Category.Child.Thing.That";
            const string nodeName = "what is this";
            for (var i = 0; i < 100; i++)
            {
                search.Add(
                    new CustomNodeSearchElement(
                        null,
                        new CustomNodeInfo(Guid.NewGuid(), nodeName, catName, "des", "")));
            }
            var results = search.Search(nodeName).ToList();
            Assert.AreEqual(nodeName, results[0].Name);
        }

        [Test]
        [Category("UnitTests")]
        public void CanDuplicateAddedNodesInBrowser()
        {
            const string catName = "Category.Child.Thing.That";
            const string nodeName = "what is this";
            for (var i = 0; i < 100; i++)
            {
                search.Add(
                    new CustomNodeSearchElement(
                        null,
                        new CustomNodeInfo(Guid.NewGuid(), nodeName, catName, "des", "")));
            }

            var categorized = SearchCategoryUtil.CategorizeSearchEntries(search.SearchEntries, x => x.Categories);
            Assert.AreEqual(1, categorized.SubCategories.Count());

            categorized = categorized.SubCategories.First();
            Assert.AreEqual("Category", categorized.Name);
            Assert.AreEqual(1, categorized.SubCategories.Count());

            categorized = categorized.SubCategories.First();
            Assert.AreEqual("Child", categorized.Name);
            Assert.AreEqual(1, categorized.SubCategories.Count());

            categorized = categorized.SubCategories.First();
            Assert.AreEqual("Thing", categorized.Name);
            Assert.AreEqual(1, categorized.SubCategories.Count());

            categorized = categorized.SubCategories.First();
            Assert.AreEqual("That", categorized.Name);
            Assert.AreEqual(0, categorized.SubCategories.Count());
            Assert.AreEqual(100, categorized.Entries.Count());
            Assert.AreEqual(nodeName, categorized.Entries.First().Name);
        }

        [Test]
        [Category("UnitTests")]
        public void DoNotGetResultsWhenNoElementsMatch()
        {
            const string catName = "Category.Child.Thing.That";
            const string nodeName = "what is this";
            search.Add(
                new CustomNodeSearchElement(
                    null,
                    new CustomNodeInfo(Guid.NewGuid(), nodeName, catName, "des", "")));

            var results = search.Search("frog");
            Assert.AreEqual(0, results.Count());
        }

        [Test]
        [Category("UnitTests")]
        public void GetResultsWhenTheresIsPartialMatch()
        {
            const string catName = "Category.Child.Thing.That";
            const string nodeName = "what is this";
            search.Add(
                new CustomNodeSearchElement(
                    null,
                    new CustomNodeInfo(Guid.NewGuid(), nodeName, catName, "des", "")));

            var results = search.Search("hi");
            Assert.AreEqual(1, results.Count());
        }

        [Test]
        [Category("UnitTests")]
        public void ResultsAreOrderProperlyForPartialMatch()
        {
            const string catName = "Category.Child.Thing.That";
            const string nodeName1 = "what is this";
            const string nodeName2 = "where is this";
            search.Add(
                new CustomNodeSearchElement(
                    null,
                    new CustomNodeInfo(Guid.NewGuid(), nodeName1, catName, "des", "")));
            search.Add(
                new CustomNodeSearchElement(
                    null,
                    new CustomNodeInfo(Guid.NewGuid(), nodeName2, catName, "des", "")));

            var results = search.Search("wh").ToList();
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(nodeName1, results[0].Name);
            Assert.AreEqual(nodeName2, results[1].Name);
        }

        [Test]
        [Category("UnitTests")]
        public void SearchingForACategoryReturnsAllItsChildren()
        {
            const string catName = "Category.Child";
            search.Add(
                new CustomNodeSearchElement(
                    null,
                    new CustomNodeInfo(Guid.NewGuid(), "what", catName, "des", "")));
            search.Add(
                new CustomNodeSearchElement(
                    null,
                    new CustomNodeInfo(Guid.NewGuid(), "where", catName, "des", "")));
            search.Add(
                new CustomNodeSearchElement(
                    null,
                    new CustomNodeInfo(Guid.NewGuid(), "who", catName, "des", "")));
            var results = search.Search("Category.Child");
            Assert.AreEqual(3, results.Count());
        }

        #endregion

        #region Split categories

        [Test]
        [Category("UnitTests")]
        public void CanSplitCategoryNameWithValidInput()
        {
            var split = NodeSearchElement.SplitCategoryName("this is a root category").ToList();
            Assert.AreEqual(1, split.Count);
            Assert.AreEqual("this is a root category", split[0]);

            split = NodeSearchElement.SplitCategoryName("this is a root category.and").ToList();
            Assert.AreEqual(2, split.Count);
            Assert.AreEqual("this is a root category", split[0]);
            Assert.AreEqual("and", split[1]);

            split = NodeSearchElement.SplitCategoryName("this is a root category.and.this is a sub").ToList();
            Assert.AreEqual(3, split.Count);
            Assert.AreEqual("this is a root category", split[0]);
            Assert.AreEqual("and", split[1]);
            Assert.AreEqual("this is a sub", split[2]);

            split = NodeSearchElement.SplitCategoryName("this is a root category.and.this is a sub. with noodles").ToList();
            Assert.AreEqual(4, split.Count);
            Assert.AreEqual("this is a root category", split[0]);
            Assert.AreEqual("and", split[1]);
            Assert.AreEqual("this is a sub", split[2]);
            Assert.AreEqual(" with noodles", split[3]);

            split = NodeSearchElement.SplitCategoryName("this is a root category.").ToList();
            Assert.AreEqual(1, split.Count);
            Assert.AreEqual("this is a root category", split[0]);
        }

        [Test]
        [Category("UnitTests")]
        public void CanSplitCategoryNameWithInvalidInput()
        {
            var split = NodeSearchElement.SplitCategoryName("").ToList();
            Assert.AreEqual(0, split.Count);

            split = NodeSearchElement.SplitCategoryName("this is a root category.").ToList();
            Assert.AreEqual(1, split.Count);
            Assert.AreEqual("this is a root category", split[0]);

            split = NodeSearchElement.SplitCategoryName(".this is a root category.").ToList();
            Assert.AreEqual(1, split.Count);
            Assert.AreEqual("this is a root category", split[0]);

            split = NodeSearchElement.SplitCategoryName("...").ToList();
            Assert.AreEqual(0, split.Count);
        }

        #endregion

        #region Add Nodes

        /// <summary>
        /// Helper method for custom node adding and removing
        /// </summary>
        public static void AssertAddAndRemoveCustomNode(
            NodeSearchModel searchModel, string nodeName, string catName, string descr = "Bla",
            string path = "Bla")
        {
            var dummyInfo = new CustomNodeInfo(Guid.NewGuid(), nodeName, catName, descr, path);
            var dummySearch = new CustomNodeSearchElement(null, dummyInfo);
            searchModel.Add(dummySearch);

            var res = searchModel.Search(nodeName).ToList();
            Assert.AreNotEqual(0, res.Count());
            Assert.AreEqual(res[0].Name, nodeName);

            searchModel.Remove(dummySearch);
            res = searchModel.Search(nodeName).ToList();
            Assert.AreEqual(0, res.Count());
        }

        [Test]
        [Category("UnitTests")]
        public void CanRemoveNodeAndCategoryByFunctionId()
        {
            const string nodeName = "TheNoodle";
            const string catName = "TheCat";
            const string descr = "TheCat";
            const string path = @"C:\turtle\graphics.dyn";
            var guid1 = Guid.NewGuid();
            var dummyInfo1 = new CustomNodeInfo(guid1, nodeName, catName, descr, path);
            var dummySearch1 = new CustomNodeSearchElement(null, dummyInfo1);

            // add custom node
            search.Add(dummySearch1);
            // confirm it's in the dictionary
            Assert.AreEqual(1, search.NumElements);

            // remove custom node
            search.Remove(dummySearch1);
            // it's gone
            Assert.AreEqual(0, search.NumElements);

            var results = search.Search(nodeName);
            Assert.AreEqual(0, results.Count());
        }

        [Test]
        [Category("UnitTests")]
        public void CanAddDuplicateCustomNodeWithDifferentGuidsAndGetBothInResults()
        {
            const string nodeName = "TheNoodle";
            const string catName = "TheCat";
            const string descr = "TheCat";
            const string path = @"C:\turtle\graphics.dyn";
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            var dummyInfo1 = new CustomNodeInfo(guid1, nodeName, catName, descr, path);
            var dummySearch1 = new CustomNodeSearchElement(null, dummyInfo1);
            var dummyInfo2 = new CustomNodeInfo(guid2, nodeName, catName, descr, path);
            var dummySearch2 = new CustomNodeSearchElement(null, dummyInfo2);

            search.Add(dummySearch1);
            search.Add(dummySearch2);
            Assert.AreEqual(2, search.NumElements);

            var results = search.Search(nodeName).ToList();
            Assert.AreEqual(2, results.Count());

            var resultIds = results.Cast<CustomNodeSearchElement>().Select(x => x.ID).ToList();
            Assert.IsTrue(resultIds.Contains(guid1));
            Assert.IsTrue(resultIds.Contains(guid2));

        }

        [Test]
        [Category("UnitTests")]
        public void CanAddCustomNodeWithSinglyNestedCategoryValidInput()
        {
            const string nodeName = "TheNode";
            const string catName = "TheCat";
            AssertAddAndRemoveCustomNode(search, nodeName, catName);
        }

        [Test]
        [Category("UnitTests")]
        public void CanAddCustomNodeWithDoublyNestedCategoryValidInput()
        {
            const string nodeName = "TheNode";
            const string catName = "TheCat.TheInnerCat";
            AssertAddAndRemoveCustomNode(search, nodeName, catName);
        }
        #endregion

        [Test]
        [Category("UnitTests")]
        public void ProcessNodeCategoryTests()
        {
            SearchElementGroup group = SearchElementGroup.None;
            string category = null;
            Assert.AreEqual(null, search.ProcessNodeCategory(category, ref group));
            Assert.AreEqual(SearchElementGroup.None, group);

            group = SearchElementGroup.None;
            category = "";
            Assert.AreEqual("", search.ProcessNodeCategory(category, ref group));
            Assert.AreEqual(SearchElementGroup.None, group);

            group = SearchElementGroup.None;
            category = "Builtin Functions";
            Assert.AreEqual("Builtin Functions", search.ProcessNodeCategory(category, ref group));
            Assert.AreEqual(SearchElementGroup.Action, group);

            group = SearchElementGroup.None;
            category = "Core.Evaluate";
            Assert.AreEqual("Core.Evaluate", search.ProcessNodeCategory(category, ref group));
            Assert.AreEqual(SearchElementGroup.Action, group);

            group = SearchElementGroup.None;
            category = "Core.List.Create";
            Assert.AreEqual("Core.List", search.ProcessNodeCategory(category, ref group));
            Assert.AreEqual(SearchElementGroup.Create, group);
        }

        #region Remove Nodes

        [Test]
        [Category("UnitTests")]
        public void CanTryToRemoveElementFromSearchWithNonexistentName()
        {
            search.Remove(new CustomNodeSearchElement(null, new CustomNodeInfo(Guid.NewGuid(), "", "", "", "")));

            var results = search.Search("NonExistentName");
            Assert.AreEqual(0, results.Count());
        }

        [Test]
        [Category("UnitTests")]
        public void CanRemoveSingleCustomNodeByIdWhereThereAreDuplicatesWithDifferentIds()
        {
            const string nodeName = "TheNoodle";
            const string catName = "TheCat";
            const string descr = "TheCat";
            const string path = @"C:\turtle\graphics.dyn";
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            var dummyInfo1 = new CustomNodeInfo(guid1, nodeName, catName, descr, path);
            var dummySearch1 = new CustomNodeSearchElement(null, dummyInfo1);
            var dummyInfo2 = new CustomNodeInfo(guid2, nodeName, catName, descr, path);
            var dummySearch2 = new CustomNodeSearchElement(null, dummyInfo2);

            search.Add(dummySearch1);
            search.Add(dummySearch2);
            Assert.AreEqual(2, search.NumElements);

            search.Remove(dummySearch2);
            Assert.AreEqual(1, search.NumElements);

            var results = search.Search(nodeName).ToList();
            Assert.AreEqual(1, results.Count());

            var res1 = results[0];
            Assert.IsInstanceOf<CustomNodeSearchElement>(res1);
            var node1 = (CustomNodeSearchElement)res1;
            Assert.AreEqual(node1.ID, guid1);
        }

        [Test]
        [Category("UnitTests")]
        public void CanRemoveElementCustomNodeByNameWithNestedCategory()
        {
            var element = new CustomNodeSearchElement(
                null,
                new CustomNodeInfo(Guid.NewGuid(), "Peter", "Turnip.Greens", "des", ""));
            search.Add(element);

            var results = search.Search("Peter");
            Assert.AreEqual(1, results.Count());

            search.Remove(element);

            results = search.Search("Peter");
            Assert.AreEqual(0, results.Count());
        }

        [Test]
        [Category("UnitTests")]
        public void CanRemoveElementCustomNodeByNameWithSingleCategory()
        {
            var element = new CustomNodeSearchElement(
                   null,
                   new CustomNodeInfo(Guid.NewGuid(), "Peter", "Greens", "des", ""));
            search.Add(element);

            var results = search.Search("Peter");
            Assert.AreEqual(1, results.Count());

            search.Remove(element);

            results = search.Search("Peter");
            Assert.AreEqual(0, results.Count());
        }

        #endregion
    }
}
