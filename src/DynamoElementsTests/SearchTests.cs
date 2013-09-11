using System;
using System.Linq;
using Dynamo.Nodes.Search;
using Dynamo.Search.SearchElements;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    internal class SearchTests
    {
        private static SearchViewModel _search;

        [SetUp]
        public void Init()
        {
           _search = new SearchViewModel();
        }

        #region Obtaining Stored Categories

        [Test]
        public void GetCategoryByNameWithValidInput()
        {
            const string catName = "Category.Child";
            _search.AddCategory(catName);
            Assert.IsTrue(_search.ContainsCategory(catName));
            Assert.AreEqual(1, _search.BrowserRootCategories.Count(x => x.Name == "Category"));
            var nestedCat = _search.GetCategoryByName("Category.Child");
            Assert.NotNull(nestedCat);
        }

        [Test]
        public void GetCategoryByNameWithInvalidInput()
        {
            const string catName = "Category.Child";
            _search.AddCategory(catName);
            Assert.IsTrue(_search.ContainsCategory(catName));
            Assert.AreEqual(1, _search.BrowserRootCategories.Count(x => x.Name == "Category"));
            var nestedCat = _search.GetCategoryByName("Toonces.The.Cat");
            Assert.IsNull(nestedCat);
        }

        [Test]
        public void ContainsCategoryWithValidInput()
        {
            const string catName = "Category.Child";
            _search.AddCategory(catName);
            Assert.IsTrue(_search.ContainsCategory(catName));
        }

        [Test]
        public void ContainsCategoryWithInvalidInput()
        {
            const string catName = "Category.Child";
            _search.AddCategory(catName);
            Assert.IsFalse(_search.ContainsCategory("Toonces.The.Cat"));
        }


        [Test]
        public void TryGetSubCategoryWithValidInput()
        {
            const string catName = "Category";
            var cat = _search.AddCategory(catName);
            cat.Items.Add(new BrowserInternalElement("Child",cat));
            Assert.IsNotNull(_search.TryGetSubCategory(cat, "Child"));
        }

        [Test]
        public void TryGetSubCategoryWithInvalidInput()
        {
            const string catName = "Category";
            var cat = _search.AddCategory(catName);
            cat.Items.Add(new BrowserInternalElement("Child", cat));
            Assert.IsNull(_search.TryGetSubCategory(cat, "Purple"));
        }

        #endregion

        #region Search

        [Test]
        public void CanSearchForPartOfTextAndGetResult()
        {
            const string catName = "Category.Child";
            _search.AddCategory(catName);
            Assert.IsTrue(_search.ContainsCategory(catName));
            Assert.AreEqual(1, _search.BrowserRootCategories.Count(x => x.Name == "Category"));
            var nestedCat = _search.GetCategoryByName("Category.Child");
            Assert.NotNull(nestedCat);
        }

        [Test]
        public void DoNotDuplicateAddedNodesInSearch()
        {
            const string catName = "Category.Child.Thing.That";
            const string nodeName = "what is this";
            for (var i = 0; i < 100; i++)
            {
                _search.Add(nodeName, catName, "des", Guid.NewGuid());
            }
            _search.SearchAndUpdateResultsSync(nodeName);
            Assert.AreEqual(1, _search.SearchResults.Count);
            Assert.AreEqual(nodeName, _search.SearchResults[0].Name);
        }

        [Test]
        public void CanAddMultiplyNestedCategory()
        {
            const string catName = "Category.Child.Thing.That";
            _search.AddCategory(catName);
            Assert.True(_search.ContainsCategory(catName));
        }

        [Test]
        public void CanAddAndRemoveMultiplyNestedCategory()
        {
            const string catName = "Category.Child.Thing.That";
            _search.AddCategory(catName);
            Assert.True(_search.ContainsCategory(catName));
            _search.RemoveCategory(catName);
            Assert.False(_search.ContainsCategory(catName));
        }

        [Test]
        public void CanRemoveRootAndRestOfChildrenOfNestedCategory()
        {
            const string catName = "Category.Child.Thing.That";
            _search.AddCategory(catName);
            Assert.True(_search.ContainsCategory(catName));
            _search.RemoveCategory("Category");
            Assert.False(_search.ContainsCategory(catName));
        }

        [Test]
        public void CanAddMultiplyNestedCategoryMultipleTimes()
        {
            const string catName = "Category.Child.Thing.That";
            _search.AddCategory(catName);
            _search.AddCategory(catName);
            _search.AddCategory(catName);
            _search.AddCategory(catName);
            Assert.True(_search.ContainsCategory(catName));
        }

        [Test]
        public void DoNotDuplicateAddedNodesInBrowser()
        {
            const string catName = "Category.Child.Thing.That";
            const string nodeName = "what is this";
            for (var i = 0; i < 100; i++)
            {
                _search.Add(nodeName, catName, "des", Guid.NewGuid());
            }

            var nestedCat = _search.GetCategoryByName(catName);
            Assert.AreEqual(1, nestedCat.Items.Count);
            Assert.AreEqual(nodeName, nestedCat.Items[0].Name);
        }

        [Test]
        public void DoNotGetResultsWhenNoElementsMatch()
        {
            const string catName = "Category.Child.Thing.That";
            const string nodeName = "what is this";
            _search.Add(nodeName, catName, "des", Guid.NewGuid());

            _search.SearchAndUpdateResultsSync("frog");
            Assert.AreEqual(0, _search.SearchResults.Count);
        }

        [Test]
        public void GetResultsWhenTheresIsPartialMatch()
        {
            const string catName = "Category.Child.Thing.That";
            const string nodeName = "what is this";
            _search.Add(nodeName, catName, "des", Guid.NewGuid());

            _search.SearchAndUpdateResultsSync("hi");
            Assert.AreEqual(1, _search.SearchResults.Count);
        }

        [Test]
        public void ResultsAreOrderProperlyForPartialMatch()
        {
            const string catName = "Category.Child.Thing.That";
            const string nodeName1 = "what is this";
            const string nodeName2 = "where is this";
            _search.Add(nodeName1, catName, "des", Guid.NewGuid());
            _search.Add(nodeName2, catName, "des", Guid.NewGuid());

            _search.SearchAndUpdateResultsSync("wh");
            Assert.AreEqual(2, _search.SearchResults.Count);
            Assert.AreEqual(nodeName1, _search.SearchResults[0].Name);
            Assert.AreEqual(nodeName2, _search.SearchResults[1].Name);
        }

        [Test]
        public void SearchingForACategoryReturnsAllItsChildren()
        {
            const string catName = "Category.Child";
            _search.AddCategory(catName);
            _search.Add("what", catName, "des", Guid.NewGuid());
            _search.Add("where", catName, "des", Guid.NewGuid());
            _search.Add("why", catName, "des", Guid.NewGuid());
            _search.SearchAndUpdateResultsSync("Category.Child");
            Assert.AreEqual(3, _search.SearchResults.Count);
        }

        #endregion

        #region Split categories

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

        #endregion

        #region Add Nodes

        /// <summary>
        /// Helper method for custom node adding and removing
        /// </summary>
        public void AssertAddAndRemoveCustomNode(SearchViewModel search, string nodeName, string catName, string descr = "Bla",
                                                 string path = "Bla")
        {
            var dummyInfo = new CustomNodeInfo(Guid.NewGuid(), nodeName, catName, descr, path);

            search.Add(dummyInfo);

            search.SearchAndUpdateResultsSync(nodeName);
            Assert.AreNotEqual(0, search.SearchResults.Count);
            Assert.AreEqual(search.SearchResults[0].Name, nodeName);
            Assert.IsTrue(search.ContainsCategory(catName));

            search.RemoveNodeAndEmptyParentCategory(nodeName);
            search.SearchAndUpdateResultsSync(nodeName);

            Assert.AreEqual(0, search.SearchResults.Count);
            Assert.IsFalse(search.ContainsCategory(catName));

        }

        [Test]
        public void CanAddCustomNodeWithSinglyNestedCategoryValidInput()
        {
            var nodeName = "TheNode";
            var catName = "TheCat";
            AssertAddAndRemoveCustomNode(_search, nodeName, catName);
        }

        [Test]
        public void CanAddCustomNodeWithDoublyNestedCategoryValidInput()
        {
            var nodeName = "TheNode";
            var catName = "TheCat.TheInnerCat";
            AssertAddAndRemoveCustomNode(_search, nodeName, catName);
        }
        #endregion

        #region Add Categories

        [Test]
        public void AddingARootCategoryMultipleTimesOnlyCreatesOneCategory()
        {
            const string catName = "Category";

            for (var i = 0; i < 10; i++)
            {
                _search.TryAddRootCategory(catName);
            }
            Assert.IsTrue(_search.ContainsCategory(catName));
            Assert.AreEqual(1, _search.BrowserRootCategories.Count(x => x.Name == catName));
        }

        [Test]
        public void AddingANestedCategoryMultipleTimesDoeNotDuplicateParentCategories()
        {
            const string catName = "Category.Child";

            for (var i = 0; i < 10; i++)
            {
                _search.AddCategory(catName);
            }
            Assert.IsTrue(_search.ContainsCategory(catName));
            Assert.AreEqual(1, _search.BrowserRootCategories.Count(x => x.Name == "Category"));
            var nestedCat = (BrowserInternalElement)_search.GetCategoryByName("Category.Child");
            Assert.NotNull(nestedCat);
            Assert.AreEqual(1, nestedCat.Parent.Items.Count);
        }

        [Test]
        public void CanAddCategory()
        {
            var root = _search.TryAddRootCategory("Peter");
            var leafCat = new BrowserInternalElement("Boyer", root);
            root.Items.Add(leafCat);

            Assert.Contains( leafCat, root.Items );
            Assert.Contains(root, _search.BrowserRootCategories);
            
        }


        [Test]
        public void CanAddCategoryWithDelimiters()
        {
            _search.AddCategory("Peter.Boyer");
            Assert.IsTrue(_search.ContainsCategory("Peter.Boyer"));
        }

        #endregion

        #region Remove Categories

        [Test]
        public void CanRemoveRootCategoryWithInternalElements()
        {
            var root = (BrowserRootElement)_search.TryAddRootCategory("Peter");
            var leafCat = new BrowserInternalElement("Boyer", root);
            root.Items.Add(leafCat);

            Assert.Contains( leafCat, root.Items );
            Assert.Contains(root, _search.BrowserRootCategories);

            _search.RemoveCategory("Peter");
            Assert.False(_search.BrowserRootCategories.Contains(root));
        }

        [Test]
        public void CanRemoveCategoryWithDelimiters()
        {
            _search.AddCategory("Peter.Boyer");

            Assert.IsTrue(_search.ContainsCategory("Peter.Boyer"));

            _search.RemoveCategory("Peter.Boyer");
            Assert.IsNull(_search.GetCategoryByName("Peter.Boyer"));

        }

        [Test]
        public void CanRunRemoveCategoryIfCategoryDoesntExist()
        {
            var search = new SearchViewModel();
            search.AddCategory("Peter.Boyer");

            search.RemoveCategory("Peter.Rabbit");
            Assert.IsNull(search.GetCategoryByName("Peter.Rabbit"));

        }

        #endregion

        #region Remove Nodes

        [Test]
        public void CanTryToRemoveElementFromSearchWithNonexistentName()
        {
            _search.RemoveNodeAndEmptyParentCategory("NonExistentName");

            _search.SearchAndUpdateResultsSync("NonExistentName");
            Assert.AreEqual(0, _search.SearchResults.Count);
        }

        [Test]
        public void CanRemoveElementCustomNodeByNameWithNestedCategory()
        {
            _search.Add("Peter", "Turnip.Greens", "A description", System.Guid.NewGuid());

            _search.SearchAndUpdateResultsSync("Peter");
            Assert.AreEqual(1, _search.SearchResults.Count);

            _search.RemoveNodeAndEmptyParentCategory("Peter");
            _search.SearchAndUpdateResultsSync("Peter");

            Assert.AreEqual(0, _search.SearchResults.Count);
        }

        [Test]
        public void CanRemoveElementCustomNodeByNameWithSingleCategory()
        {
            _search.Add("Peter", "Greens", "A description", System.Guid.NewGuid());

            _search.SearchAndUpdateResultsSync("Peter");
            Assert.AreEqual(1, _search.SearchResults.Count);

            _search.RemoveNodeAndEmptyParentCategory("Peter");
            _search.SearchAndUpdateResultsSync("Peter");

            Assert.AreEqual(0, _search.SearchResults.Count);
        }

        #endregion
    }
}
