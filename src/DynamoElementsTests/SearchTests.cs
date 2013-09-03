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
            var model = new SearchViewModel();
            var root = model.TryAddRootCategory("Peter");
            var leafCat = new BrowserInternalElement("Boyer", root);
            root.Items.Add(leafCat);

            Assert.Contains( leafCat, root.Items );
            Assert.Contains( root, model.BrowserRootCategories );
            
        }


        [Test]
        public void CanAddCategoryWithDelimiters()
        {
            var model = new SearchViewModel();
            model.AddCategory("Peter.Boyer");

            model.RemoveCategory("Peter.Boyer");

        }

        #endregion

        #region Remove Categories

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

        #endregion

        #region Remove Nodes

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

        #endregion
    }
}
