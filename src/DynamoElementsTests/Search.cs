using System;
using System.Linq;
using System.Threading;
using Dynamo.Nodes.Search;
using Dynamo.Search.SearchElements;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    internal class Search
    {
        private static SearchViewModel _search;

        [SetUp]
        public void Init()
        {
           _search = new SearchViewModel();
        }

        #region Refactoring

        [Test]
        public void CanRefactorCustomNodeName()
        {
            var nodeName = "TheNoodle";
            var catName = "TheCat";
            var descr = "TheCat";
            var path = @"C:\turtle\graphics.dyn";
            var guid1 = Guid.NewGuid();
            var dummyInfo1 = new CustomNodeInfo(guid1, nodeName, catName, descr, path);

            _search.Add(dummyInfo1);

            Assert.AreEqual(1, _search.SearchDictionary.NumElements);

            var newNodeName = "TheTurtle";
            var newInfo = new CustomNodeInfo(guid1, newNodeName, catName, descr, path);
            _search.Refactor(newInfo);

            Assert.AreEqual(1, _search.SearchDictionary.NumElements);

            // search for new name
            _search.SearchAndUpdateResultsSync(newNodeName);

            // results are correct
            Assert.AreEqual(1, _search.SearchResults.Count);
            var res1 = _search.SearchResults[0];
            Assert.IsAssignableFrom(typeof(CustomNodeSearchElement), res1);
            var node1 = res1 as CustomNodeSearchElement;
            Assert.AreEqual(node1.Guid, guid1);

            // search for old name
            _search.SearchAndUpdateResultsSync(nodeName);

            // results are correct
            Assert.AreEqual(0, _search.SearchResults.Count);
        }

        [Test]
        public void CanRefactorCustomNodeDescription()
        {
            var nodeName = "TheNoodle";
            var catName = "TheCat";
            var descr = "Cool description, man";
            var path = @"C:\turtle\graphics.dyn";
            var guid1 = Guid.NewGuid();
            var dummyInfo1 = new CustomNodeInfo(guid1, nodeName, catName, descr, path);

            _search.Add(dummyInfo1);

            Assert.AreEqual(1, _search.SearchDictionary.NumElements);

            // search for name
            _search.SearchAndUpdateResultsSync(nodeName);

            // results are correct
            Assert.AreEqual(1, _search.SearchResults.Count);
            var res1 = _search.SearchResults[0];
            Assert.IsAssignableFrom(typeof(CustomNodeSearchElement), res1);
            var node1 = res1 as CustomNodeSearchElement;
            Assert.AreEqual(node1.Guid, guid1);
            Assert.AreEqual(node1.Description, descr);

            // refactor description
            const string newDescription = "Tickle me elmo";
            var newInfo = new CustomNodeInfo(guid1, nodeName, catName, newDescription, path);
            _search.Refactor(newInfo);

            // num elements is unchanged
            Assert.AreEqual(1, _search.SearchDictionary.NumElements);

            // search for name
            _search.SearchAndUpdateResultsSync(nodeName);

            // description is updated
            Assert.AreEqual(1, _search.SearchResults.Count);
            var res2 = _search.SearchResults[0];
            Assert.IsAssignableFrom(typeof(CustomNodeSearchElement), res2);
            var node2 = res2 as CustomNodeSearchElement;
            Assert.AreEqual( guid1, node2.Guid );
            Assert.AreEqual( newDescription, node2.Description);

        }

        [Test]
        public void CanRefactorCustomNodeWhilePreservingDuplicates()
        {
            var nodeName = "TheNoodle";
            var catName = "TheCat";
            var descr = "TheCat";
            var path = @"C:\turtle\graphics.dyn";
            var guid1 = Guid.NewGuid();
            var dummyInfo1 = new CustomNodeInfo(guid1, nodeName, catName, descr, path);
            var guid2 = Guid.NewGuid();
            var dummyInfo2 = new CustomNodeInfo(guid2, nodeName, catName, descr, path);

            _search.Add(dummyInfo1);
            _search.Add(dummyInfo2);

            Assert.AreEqual(2, _search.SearchDictionary.NumElements);

            // refactor one of the nodes with newNodeName
            var newNodeName = "TheTurtle";
            var newInfo = new CustomNodeInfo(guid1, newNodeName, catName, descr, path);
            _search.Refactor(newInfo);

            // num elements is unchanged
            Assert.AreEqual(2, _search.SearchDictionary.NumElements);

            // search for new name
            _search.SearchAndUpdateResultsSync(newNodeName);

            // results are correct - only one result
            Assert.AreEqual(1, _search.SearchResults.Count);
            var res1 = _search.SearchResults[0];
            Assert.IsAssignableFrom(typeof(CustomNodeSearchElement), res1);
            var node1 = res1 as CustomNodeSearchElement;
            Assert.AreEqual(node1.Guid, guid1);

            // search for old name
            _search.SearchAndUpdateResultsSync(nodeName);

            // results are correct - the first nodes are returned
            Assert.AreEqual(1, _search.SearchResults.Count);
            var res2 = _search.SearchResults[0];
            Assert.IsAssignableFrom(typeof(CustomNodeSearchElement), res2);
            var node2 = res2 as CustomNodeSearchElement;
            Assert.AreEqual(node2.Guid, guid2);
        }

        #endregion

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
        public void PopulateSearchTextWithSelectedResultReturnsExpectedResult()
        {
            var catName = "Animals";
            var descr = "";
            var path = "";

            _search.Add(new CustomNodeInfo(Guid.NewGuid(), "xyz", catName, descr, path));
            _search.Add(new CustomNodeInfo(Guid.NewGuid(), "abc", catName, descr, path));
            _search.Add(new CustomNodeInfo(Guid.NewGuid(), "cat", catName, descr, path));
            _search.Add(new CustomNodeInfo(Guid.NewGuid(), "dog", catName, descr, path));
            _search.Add(new CustomNodeInfo(Guid.NewGuid(), "frog", catName, descr, path));
            _search.Add(new CustomNodeInfo(Guid.NewGuid(), "Noodle", catName, descr, path));

            _search.SearchAndUpdateResultsSync("xy");
            _search.PopulateSearchTextWithSelectedResult();
            Assert.AreEqual("xyz",_search.SearchText);

            _search.SearchAndUpdateResultsSync("ood");
            _search.PopulateSearchTextWithSelectedResult();
            Assert.AreEqual("Noodle", _search.SearchText);

            _search.SearchAndUpdateResultsSync("do");
            _search.PopulateSearchTextWithSelectedResult();
            Assert.AreEqual("dog", _search.SearchText);

        }

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
        public void CanDuplicateAddedNodesInSearch()
        {
            const string catName = "Category.Child.Thing.That";
            const string nodeName = "what is this";
            for (var i = 0; i < 100; i++)
            {
                _search.Add(new CustomNodeInfo(Guid.NewGuid(), nodeName, catName, "des", ""));
            }
            _search.MaxNumSearchResults = 100;
            _search.SearchAndUpdateResultsSync(nodeName);
            Assert.AreEqual(100, _search.SearchResults.Count);
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
        public void CanDuplicateAddedNodesInBrowser()
        {
            const string catName = "Category.Child.Thing.That";
            const string nodeName = "what is this";
            for (var i = 0; i < 100; i++)
            {
                _search.Add(new CustomNodeInfo(Guid.NewGuid(), nodeName, catName, "des", ""));
            }

            var nestedCat = _search.GetCategoryByName(catName);
            Assert.AreEqual(100, nestedCat.Items.Count);
            Assert.AreEqual(nodeName, nestedCat.Items[0].Name);
        }

        [Test]
        public void DoNotGetResultsWhenNoElementsMatch()
        {
            const string catName = "Category.Child.Thing.That";
            const string nodeName = "what is this";
            _search.Add(new CustomNodeInfo(Guid.NewGuid(), nodeName, catName, "des", ""));

            _search.SearchAndUpdateResultsSync("frog");
            Assert.AreEqual(0, _search.SearchResults.Count);
        }

        [Test]
        public void GetResultsWhenTheresIsPartialMatch()
        {
            const string catName = "Category.Child.Thing.That";
            const string nodeName = "what is this";
            _search.Add(new CustomNodeInfo(Guid.NewGuid(), nodeName, catName, "des", ""));

            _search.SearchAndUpdateResultsSync("hi");
            Assert.AreEqual(1, _search.SearchResults.Count);
        }

        [Test]
        public void ResultsAreOrderProperlyForPartialMatch()
        {
            const string catName = "Category.Child.Thing.That";
            const string nodeName1 = "what is this";
            const string nodeName2 = "where is this";
            _search.Add(new CustomNodeInfo(Guid.NewGuid(), nodeName1, catName, "des", ""));
            _search.Add(new CustomNodeInfo(Guid.NewGuid(), nodeName2, catName, "des", ""));

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
            _search.Add(new CustomNodeInfo(Guid.NewGuid(), "what", catName, "des", ""));
            _search.Add(new CustomNodeInfo(Guid.NewGuid(), "where", catName, "des", ""));
            _search.Add(new CustomNodeInfo(Guid.NewGuid(), "where", catName, "des", ""));
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
        public void CannotAddCustomNodesWithSameGuids()
        {
            var nodeName = "TheNoodle";
            var catName = "TheCat";
            var descr = "TheCat";
            var path = @"C:\turtle\graphics.dyn";
            var guid1 = Guid.NewGuid();
            var dummyInfo1 = new CustomNodeInfo(guid1, nodeName, catName, descr, path);
            var dummyInfo2 = new CustomNodeInfo(guid1, nodeName, catName, descr, path);

            _search.Add(dummyInfo1);
            _search.Add(dummyInfo2);

            Assert.AreEqual(1, _search.SearchDictionary.NumElements);

            _search.SearchAndUpdateResultsSync(nodeName);

            Assert.AreEqual(1, _search.SearchResults.Count);

            var res1 = _search.SearchResults[0];

            Assert.IsAssignableFrom(typeof(CustomNodeSearchElement), res1);

            var node1 = res1 as CustomNodeSearchElement;

            Assert.AreEqual(node1.Guid, guid1);

        }

        [Test]
        public void CanRemoveNodeAndCategoryByFunctionId()
        {
            var nodeName = "TheNoodle";
            var catName = "TheCat";
            var descr = "TheCat";
            var path = @"C:\turtle\graphics.dyn";
            var guid1 = Guid.NewGuid();
            var dummyInfo1 = new CustomNodeInfo(guid1, nodeName, catName, descr, path);

            // add custom node
            _search.Add(dummyInfo1);

            // confirm it's in the dictionary
            Assert.AreEqual(1, _search.SearchDictionary.NumElements);

            // remove custom node
            _search.RemoveNodeAndEmptyParentCategory(guid1);

            // it's gone
            Assert.AreEqual(0, _search.SearchDictionary.NumElements);
            _search.SearchAndUpdateResultsSync(nodeName);
            Assert.AreEqual(0, _search.SearchResults.Count);

        }

        [Test]
        public void CanAddDuplicateCustomNodeWithDifferentGuidsAndGetBothInResults()
        {
            var nodeName = "TheNoodle";
            var catName = "TheCat";
            var descr = "TheCat";
            var path = @"C:\turtle\graphics.dyn";
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            var dummyInfo1 = new CustomNodeInfo(guid1, nodeName, catName, descr, path);
            var dummyInfo2 = new CustomNodeInfo(guid2, nodeName, catName, descr, path);

            _search.Add(dummyInfo1);
            _search.Add(dummyInfo2);

            Assert.AreEqual(2, _search.SearchDictionary.NumElements);

            _search.SearchAndUpdateResultsSync(nodeName);

            Assert.AreEqual(2, _search.SearchResults.Count);

            var res1 = _search.SearchResults[0];
            var res2 = _search.SearchResults[1];

            Assert.IsAssignableFrom(typeof(CustomNodeSearchElement), res1);
            Assert.IsAssignableFrom(typeof(CustomNodeSearchElement), res2);

            var node1 = res1 as CustomNodeSearchElement;
            var node2 = res2 as CustomNodeSearchElement;

            Assert.AreEqual(node1.Guid, guid1);
            Assert.AreEqual(node2.Guid, guid2);

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
        public void CanRemoveSingleCustomNodeByIdWhereThereAreDuplicatesWithDifferentIds()
        {
            var nodeName = "TheNoodle";
            var catName = "TheCat";
            var descr = "TheCat";
            var path = @"C:\turtle\graphics.dyn";
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            var dummyInfo1 = new CustomNodeInfo(guid1, nodeName, catName, descr, path);
            var dummyInfo2 = new CustomNodeInfo(guid2, nodeName, catName, descr, path);

            _search.Add(dummyInfo1);
            _search.Add(dummyInfo2);

            Assert.AreEqual(2, _search.SearchDictionary.NumElements);

            _search.RemoveNodeAndEmptyParentCategory(guid2);

            Assert.AreEqual(1, _search.SearchDictionary.NumElements);

            _search.SearchAndUpdateResultsSync(nodeName);

            Assert.AreEqual(1, _search.SearchResults.Count);

            var res1 = _search.SearchResults[0];
            Assert.IsAssignableFrom(typeof(CustomNodeSearchElement), res1);
            var node1 = res1 as CustomNodeSearchElement;
            Assert.AreEqual(node1.Guid, guid1);
        }

        [Test]
        public void CanRemoveElementCustomNodeByNameWithNestedCategory()
        {

            _search.Add(new CustomNodeInfo(Guid.NewGuid(), "Peter", "Turnip.Greens", "des", ""));

            _search.SearchAndUpdateResultsSync("Peter");
            Assert.AreEqual(1, _search.SearchResults.Count);

            _search.RemoveNodeAndEmptyParentCategory("Peter");
            _search.SearchAndUpdateResultsSync("Peter");

            Assert.AreEqual(0, _search.SearchResults.Count);
        }

        [Test]
        public void CanRemoveElementCustomNodeByNameWithSingleCategory()
        {
            _search.Add(new CustomNodeInfo(Guid.NewGuid(), "Peter", "Greens", "des", ""));

            _search.SearchAndUpdateResultsSync("Peter");
            Assert.AreEqual(1, _search.SearchResults.Count);

            _search.RemoveNodeAndEmptyParentCategory("Peter");
            _search.SearchAndUpdateResultsSync("Peter");

            Assert.AreEqual(0, _search.SearchResults.Count);
        }

        #endregion
    }
}
