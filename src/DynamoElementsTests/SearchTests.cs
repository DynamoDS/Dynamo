using System;
using Dynamo.Nodes.Search;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using NUnit.Framework;

namespace Dynamo.Tests
{

    [TestFixture]
    internal class SearchTests
    {

        [Test]
        public void AddingARootCategoryTwiceOnlyCreatesOneCategory()
        {
            Assert.Inconclusive("Not completed.");
        }

        [Test]
        public void AddingANestedCategoryDoeNotDuplicateAnyParentCategories()
        {
            Assert.Inconclusive("Not completed.");
        }

        [Test]
        public void GetCategoryByNameWithValidInput()
        {
            Assert.Inconclusive("Not completed.");
        }

        [Test]
        public void GetCategoryByNameWithInvalidInput()
        {
            Assert.Inconclusive("Not completed.");
        }

        [Test]
        public void ContainsCategoryWithValidInput()
        {
            Assert.Inconclusive("Not completed.");
        }

        [Test]
        public void ContainsCategoryWithInvalidInput()
        {
            Assert.Inconclusive("Not completed.");
        }

        [Test]
        public void SearchingForACategoryReturnsAllItsChildren()
        {
            Assert.Inconclusive("Not completed.");
        }

        [Test]
        public void TryGetSubCategoryWithValidInput()
        {
            Assert.Inconclusive("Not completed.");
        }

        [Test]
        public void TryGetSubCategoryWithInvalidInput()
        {
            Assert.Inconclusive("Not completed.");
        }

        [Test]
        public void CanSplitCategoryNameWithValidInput()
        {
            var split = SearchViewModel.SplitCategoryName("this is a root category");
            Assert.AreEqual(1, split.Count);
            Assert.AreEqual("this is a root category", split[0] );

            split = SearchViewModel.SplitCategoryName("this is a root category.and");
            Assert.AreEqual(2, split.Count);
            Assert.AreEqual("this is a root category", split[0] );
            Assert.AreEqual("and", split[1]);

            split = SearchViewModel.SplitCategoryName("this is a root category.and.this is a sub");
            Assert.AreEqual(3, split.Count);
            Assert.AreEqual("this is a root category", split[0]);
            Assert.AreEqual("and", split[1]);
            Assert.AreEqual("this is a sub", split[2]);

            split = SearchViewModel.SplitCategoryName("this is a root category.and.this is a sub. with noodles");
            Assert.AreEqual(4, split.Count);
            Assert.AreEqual("this is a root category", split[0]);
            Assert.AreEqual("and", split[1]);
            Assert.AreEqual("this is a sub", split[2]);
            Assert.AreEqual(" with noodles", split[3]);

            split = SearchViewModel.SplitCategoryName("this is a root category.");
            Assert.AreEqual(1,split.Count);
            Assert.AreEqual("this is a root category", split[0] );
        }

        [Test]
        public void CanSplitCategoryNameWithInvalidInput()
        {
            var split = SearchViewModel.SplitCategoryName("");
            Assert.AreEqual(0, split.Count);

            split = SearchViewModel.SplitCategoryName("this is a root category.");
            Assert.AreEqual(1, split.Count);
            Assert.AreEqual("this is a root category", split[0]);

            split = SearchViewModel.SplitCategoryName(".this is a root category.");
            Assert.AreEqual(1, split.Count);
            Assert.AreEqual("this is a root category", split[0]);

            split = SearchViewModel.SplitCategoryName("...");
            Assert.AreEqual(0, split.Count);
        }

        /// <summary>
        /// Helper method for custom node adding and removing
        /// </summary>
        public void AssertAddAndRemoveCustomNode(SearchViewModel model, string nodeName, string catName, string descr = "Bla",
                                                 string path = "Bla")
        {
            var dummyInfo = new CustomNodeInfo(Guid.NewGuid(), nodeName, catName, descr, path);

            model.Add(dummyInfo);

            model.SearchAndUpdateResultsSync(nodeName);
            Assert.AreNotEqual(0, model.SearchResults.Count);
            Assert.AreEqual(model.SearchResults[0].Name, nodeName);
            Assert.IsTrue(model.ContainsCategory(catName));

            model.RemoveNodeAndEmptyParentCategory(nodeName);
            model.SearchAndUpdateResultsSync(nodeName);

            Assert.AreEqual(0, model.SearchResults.Count);
            Assert.IsFalse(model.ContainsCategory(catName));

        }

        [Test]
        public void CanAddCustomNodeWithSinglyNestedCategoryValidInput()
        {
            var model = new SearchViewModel();
            var nodeName = "TheNode";
            var catName = "TheCat";
            AssertAddAndRemoveCustomNode(model, nodeName, catName);
        }

        [Test]
        public void CanAddCustomNodeWithDoublyNestedCategoryValidInput()
        {
            var model = new SearchViewModel();
            var nodeName = "TheNode";
            var catName = "TheCat.TheInnerCat";
            AssertAddAndRemoveCustomNode(model, nodeName, catName);
        }

        [Test]
        public void CanAddCategory()
        {
            var model = new SearchViewModel();
            var root = model.TryAddRootCategory("Peter");
            var leafCat = new BrowserInternalElement("Boyer", root);
            root.Items.Add(leafCat);

            Assert.Contains( leafCat, root.Items );
            Assert.Contains( root, model.BrowserRootCategories );
            
        }

        [Test]
        public void CanRemoveRootCategoryWithInternalElements()
        {
            var model = new SearchViewModel();
            var root = (BrowserRootElement) model.TryAddRootCategory("Peter");
            var leafCat = new BrowserInternalElement("Boyer", root);
            root.Items.Add(leafCat);

            Assert.Contains( leafCat, root.Items );
            Assert.Contains( root, model.BrowserRootCategories );

            model.RemoveCategory("Peter");
            Assert.False( model.BrowserRootCategories.Contains(root) );
        }

        [Test]
        public void CanRemoveCategoryWithDelimiters()
        {
            var model = new SearchViewModel();
            var cat = model.AddCategory("Peter.Boyer");

            Assert.IsTrue(model.ContainsCategory("Peter.Boyer"));

            model.RemoveCategory("Peter.Boyer");
            Assert.IsNull( model.GetCategoryByName("Peter.Boyer") );

        }

        [Test]
        public void CanRunRemoveCategoryIfCategoryDoesntExist()
        {
            var model = new SearchViewModel();
            var cat = model.AddCategory("Peter.Boyer");

            model.RemoveCategory("Peter.Rabbit");
            Assert.IsNull(model.GetCategoryByName("Peter.Rabbit"));

        }

        [Test]
        public void CanAddCategoryWithDelimiters()
        {
            var model = new SearchViewModel();
            model.AddCategory("Peter.Boyer");

            model.RemoveCategory("Peter.Boyer");

        }

        [Test]
        public void CanTryToRemoveElementFromSearchWithNonexistentName()
        {
            var model = new SearchViewModel();
            model.RemoveNodeAndEmptyParentCategory("NonExistentName");

            model.SearchAndUpdateResultsSync("NonExistentName");
            Assert.AreEqual(0, model.SearchResults.Count);
        }

        [Test]
        public void CanRemoveElementCustomNodeByNameWithNestedCategory()
        {
            var model = new SearchViewModel();
            model.Add("Peter", "Turnip.Greens", "A description", System.Guid.NewGuid());

            model.SearchAndUpdateResultsSync("Peter");
            Assert.AreEqual(1, model.SearchResults.Count);

            model.RemoveNodeAndEmptyParentCategory("Peter");
            model.SearchAndUpdateResultsSync("Peter");

            Assert.AreEqual(0, model.SearchResults.Count);
        }

        [Test]
        public void CanRemoveElementCustomNodeByNameWithSingleCategory()
        {
            var model = new SearchViewModel();
            model.Add("Peter", "Greens", "A description", System.Guid.NewGuid());

            model.SearchAndUpdateResultsSync("Peter");
            Assert.AreEqual(1, model.SearchResults.Count);

            model.RemoveNodeAndEmptyParentCategory("Peter");
            model.SearchAndUpdateResultsSync("Peter");

            Assert.AreEqual(0, model.SearchResults.Count);
        }

    }
}
