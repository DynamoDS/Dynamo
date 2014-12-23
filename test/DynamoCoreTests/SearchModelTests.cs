using System;
using System.Linq;
using System.Threading;
using Dynamo.Search;
using Dynamo.Search.SearchElements;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    internal class SearchModelTests
    {
        private static SearchModel search;

        [SetUp]
        public void Init()
        {
           search = new SearchModel();
        }

        #region Refactoring

        [Test]
        [Category("UnitTests")]
        public void CanRefactorCustomNodeName()
        {
            var nodeName = "TheNoodle";
            var catName = "TheCat";
            var descr = "TheCat";
            var path = @"C:\turtle\graphics.dyn";
            var guid1 = Guid.NewGuid();
            var dummyInfo1 = new CustomNodeInfo(guid1, nodeName, catName, descr, path);

            search.Add(dummyInfo1);

            Assert.AreEqual(1, search.SearchDictionary.NumElements);

            var newNodeName = "TheTurtle";
            var newInfo = new CustomNodeInfo(guid1, newNodeName, catName, descr, path);
            search.Refactor(newInfo);

            Assert.AreEqual(1, search.SearchDictionary.NumElements);

            // search for new name
            var results = search.Search(newNodeName).ToList();

            // results are correct
            Assert.AreEqual(1, results.Count());
            var res1 = results[0];
            Assert.IsAssignableFrom(typeof(CustomNodeSearchElement), res1);
            var node1 = res1 as CustomNodeSearchElement;
            Assert.AreEqual(node1.Guid, guid1);

            // search for old name
            var results1 = search.Search(nodeName);

            // results are correct
            Assert.AreEqual(0, results1.Count());
        }

        [Test]
        [Category("UnitTests")]
        public void CanRefactorCustomNodeDescription()
        {
            var nodeName = "TheNoodle";
            var catName = "TheCat";
            var descr = "Cool description, man";
            var path = @"C:\turtle\graphics.dyn";
            var guid1 = Guid.NewGuid();
            var dummyInfo1 = new CustomNodeInfo(guid1, nodeName, catName, descr, path);

            search.Add(dummyInfo1);

            Assert.AreEqual(1, search.SearchDictionary.NumElements);

            // search for name
            var results = search.Search(nodeName).ToList();

            // results are correct
            Assert.AreEqual(1, results.Count());
            var res1 = results[0];
            Assert.IsAssignableFrom(typeof(CustomNodeSearchElement), res1);
            var node1 = res1 as CustomNodeSearchElement;
            Assert.AreEqual(node1.Guid, guid1);
            Assert.AreEqual(node1.Description, descr);

            // refactor description
            const string newDescription = "Tickle me elmo";
            var newInfo = new CustomNodeInfo(guid1, nodeName, catName, newDescription, path);
            search.Refactor(newInfo);

            // num elements is unchanged
            Assert.AreEqual(1, search.SearchDictionary.NumElements);

            // search for name
            var results1 = search.Search(nodeName).ToList();

            // description is updated
            Assert.AreEqual(1,results1.Count());
            var res2 = results1[0];
            Assert.IsAssignableFrom(typeof(CustomNodeSearchElement), res2);
            var node2 = res2 as CustomNodeSearchElement;
            Assert.AreEqual( guid1, node2.Guid );
            Assert.AreEqual( newDescription, node2.Description);

        }

        [Test]
        [Category("UnitTests")]
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

            search.Add(dummyInfo1);
            search.Add(dummyInfo2);

            Assert.AreEqual(2, search.SearchDictionary.NumElements);

            // refactor one of the nodes with newNodeName
            var newNodeName = "TheTurtle";
            var newInfo = new CustomNodeInfo(guid1, newNodeName, catName, descr, path);
            search.Refactor(newInfo);

            // num elements is unchanged
            Assert.AreEqual(2, search.SearchDictionary.NumElements);

            // search for new name
            var results = search.Search(newNodeName).ToList();

            // results are correct - only one result
            Assert.AreEqual(1, results.Count());
            var res1 = results[0];
            Assert.IsAssignableFrom(typeof(CustomNodeSearchElement), res1);
            var node1 = res1 as CustomNodeSearchElement;
            Assert.AreEqual(node1.Guid, guid1);

            // search for old name
            results = search.Search(nodeName).ToList();

            // results are correct - the first nodes are returned
            Assert.AreEqual(1, results.Count());
            var res2 = results[0];
            Assert.IsAssignableFrom(typeof(CustomNodeSearchElement), res2);
            var node2 = res2 as CustomNodeSearchElement;
            Assert.AreEqual(node2.Guid, guid2);
        }

        #endregion

        #region Obtaining Stored Categories

        [Test]
        [Category("UnitTests")]
        public void GetCategoryByNameWithValidInput()
        {
            const string catName = "Category.Child";
            search.AddCategory(catName);
            Assert.IsTrue(search.ContainsCategory(catName));
            Assert.AreEqual(1, search.BrowserRootCategories.Count(x => x.Name == "Category"));
            var nestedCat = search.GetCategoryByName("Category.Child");
            Assert.NotNull(nestedCat);
        }

        [Test]
        [Category("UnitTests")]
        public void GetCategoryByNameWithInvalidInput()
        {
            const string catName = "Category.Child";
            search.AddCategory(catName);
            Assert.IsTrue(search.ContainsCategory(catName));
            Assert.AreEqual(1, search.BrowserRootCategories.Count(x => x.Name == "Category"));
            var nestedCat = search.GetCategoryByName("Toonces.The.Cat");
            Assert.IsNull(nestedCat);
        }

        [Test]
        [Category("UnitTests")]
        public void ContainsCategoryWithValidInput()
        {
            const string catName = "Category.Child";
            search.AddCategory(catName);
            Assert.IsTrue(search.ContainsCategory(catName));
        }

        [Test]
        [Category("UnitTests")]
        public void ContainsCategoryWithInvalidInput()
        {
            const string catName = "Category.Child";
            search.AddCategory(catName);
            Assert.IsFalse(search.ContainsCategory("Toonces.The.Cat"));
        }


        [Test]
        [Category("UnitTests")]
        public void TryGetSubCategoryWithValidInput()
        {
            const string catName = "Category";
            var cat = search.AddCategory(catName);
            cat.Items.Add(new BrowserInternalElement("Child",cat));
            Assert.IsNotNull(search.TryGetSubCategory(cat, "Child"));
        }

        [Test]
        [Category("UnitTests")]
        public void TryGetSubCategoryWithInvalidInput()
        {
            const string catName = "Category";
            var cat = search.AddCategory(catName);
            cat.Items.Add(new BrowserInternalElement("Child", cat));
            Assert.IsNull(search.TryGetSubCategory(cat, "Purple"));
        }

        #endregion

        #region Search

        [Test]
        [Category("UnitTests")]
        public void CanSearchForPartOfTextAndGetResult()
        {
            const string catName = "Category.Child";
            search.AddCategory(catName);
            Assert.IsTrue(search.ContainsCategory(catName));
            Assert.AreEqual(1, search.BrowserRootCategories.Count(x => x.Name == "Category"));
            var nestedCat = search.GetCategoryByName("Category.Child");
            Assert.NotNull(nestedCat);
        }

        [Test]
        [Category("UnitTests")]
        public void CanDuplicateAddedNodesInSearch()
        {
            const string catName = "Category.Child.Thing.That";
            const string nodeName = "what is this";
            for (var i = 0; i < 100; i++)
            {
                search.Add(new CustomNodeInfo(Guid.NewGuid(), nodeName, catName, "des", ""));
            }
            search.MaxNumSearchResults = 100;
            var results = search.Search(nodeName).ToList();
            Assert.AreEqual(100, results.Count());
            Assert.AreEqual(nodeName, results[0].Name);
        }

        [Test]
        [Category("UnitTests")]
        public void CanAddMultiplyNestedCategory()
        {
            const string catName = "Category.Child.Thing.That";
            search.AddCategory(catName);
            Assert.True(search.ContainsCategory(catName));
        }

        [Test]
        [Category("UnitTests")]
        public void CanAddAndRemoveMultiplyNestedCategory()
        {
            const string catName = "Category.Child.Thing.That";
            search.AddCategory(catName);
            Assert.True(search.ContainsCategory(catName));
            search.RemoveCategory(catName);
            Assert.False(search.ContainsCategory(catName));
        }

        [Test]
        [Category("UnitTests")]
        public void CanRemoveRootAndRestOfChildrenOfNestedCategory()
        {
            const string catName = "Category.Child.Thing.That";
            search.AddCategory(catName);
            Assert.True(search.ContainsCategory(catName));
            search.RemoveCategory("Category");
            Assert.False(search.ContainsCategory(catName));
        }

        [Test]
        [Category("UnitTests")]
        public void CanAddMultiplyNestedCategoryMultipleTimes()
        {
            const string catName = "Category.Child.Thing.That";
            search.AddCategory(catName);
            search.AddCategory(catName);
            search.AddCategory(catName);
            search.AddCategory(catName);
            Assert.True(search.ContainsCategory(catName));
        }

        [Test]
        [Category("UnitTests")]
        public void CanDuplicateAddedNodesInBrowser()
        {
            const string catName = "Category.Child.Thing.That";
            const string nodeName = "what is this";
            for (var i = 0; i < 100; i++)
            {
                search.Add(new CustomNodeInfo(Guid.NewGuid(), nodeName, catName, "des", ""));
            }

            var nestedCat = search.GetCategoryByName(catName);
            Assert.AreEqual(100, nestedCat.Items.Count());
            Assert.AreEqual(nodeName, nestedCat.Items[0].Name);
        }

        [Test]
        [Category("UnitTests")]
        public void DoNotGetResultsWhenNoElementsMatch()
        {
            const string catName = "Category.Child.Thing.That";
            const string nodeName = "what is this";
            search.Add(new CustomNodeInfo(Guid.NewGuid(), nodeName, catName, "des", ""));

            var results = search.Search("frog");
            Assert.AreEqual(0, results.Count());
        }

        [Test]
        [Category("UnitTests")]
        public void GetResultsWhenTheresIsPartialMatch()
        {
            const string catName = "Category.Child.Thing.That";
            const string nodeName = "what is this";
            search.Add(new CustomNodeInfo(Guid.NewGuid(), nodeName, catName, "des", ""));

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
            search.Add(new CustomNodeInfo(Guid.NewGuid(), nodeName1, catName, "des", ""));
            search.Add(new CustomNodeInfo(Guid.NewGuid(), nodeName2, catName, "des", ""));

            var results = search.Search("wh").ToList();
            Assert.AreEqual(2, results.Count());
            Assert.AreEqual(nodeName1, results[0].Name);
            Assert.AreEqual(nodeName2, results[1].Name);
        }

        [Test]
        [Category("UnitTests")]
        public void SearchingForACategoryReturnsAllItsChildren()
        {
            const string catName = "Category.Child";
            search.AddCategory(catName);
            search.Add(new CustomNodeInfo(Guid.NewGuid(), "what", catName, "des", ""));
            search.Add(new CustomNodeInfo(Guid.NewGuid(), "where", catName, "des", ""));
            search.Add(new CustomNodeInfo(Guid.NewGuid(), "where", catName, "des", ""));
            var results = search.Search("Category.Child");
            Assert.AreEqual(3, results.Count());
        }

        #endregion

        #region Split categories

        [Test]
        [Category("UnitTests")]
        public void CanSplitCategoryNameWithValidInput()
        {
            var split = SearchModel.SplitCategoryName("this is a root category");
            Assert.AreEqual(1, split.Count());
            Assert.AreEqual("this is a root category", split[0] );

            split = SearchModel.SplitCategoryName("this is a root category.and");
            Assert.AreEqual(2, split.Count());
            Assert.AreEqual("this is a root category", split[0] );
            Assert.AreEqual("and", split[1]);

            split = SearchModel.SplitCategoryName("this is a root category.and.this is a sub");
            Assert.AreEqual(3, split.Count());
            Assert.AreEqual("this is a root category", split[0]);
            Assert.AreEqual("and", split[1]);
            Assert.AreEqual("this is a sub", split[2]);

            split = SearchModel.SplitCategoryName("this is a root category.and.this is a sub. with noodles");
            Assert.AreEqual(4, split.Count());
            Assert.AreEqual("this is a root category", split[0]);
            Assert.AreEqual("and", split[1]);
            Assert.AreEqual("this is a sub", split[2]);
            Assert.AreEqual(" with noodles", split[3]);

            split = SearchModel.SplitCategoryName("this is a root category.");
            Assert.AreEqual(1,split.Count());
            Assert.AreEqual("this is a root category", split[0] );
        }

        [Test]
        [Category("UnitTests")]
        public void CanSplitCategoryNameWithInvalidInput()
        {
            var split = SearchModel.SplitCategoryName("");
            Assert.AreEqual(0, split.Count());

            split = SearchModel.SplitCategoryName("this is a root category.");
            Assert.AreEqual(1, split.Count());
            Assert.AreEqual("this is a root category", split[0]);

            split = SearchModel.SplitCategoryName(".this is a root category.");
            Assert.AreEqual(1, split.Count());
            Assert.AreEqual("this is a root category", split[0]);

            split = SearchModel.SplitCategoryName("...");
            Assert.AreEqual(0, split.Count());
        }

        #endregion

        #region Add Nodes

        /// <summary>
        /// Helper method for custom node adding and removing
        /// </summary>
        public static void AssertAddAndRemoveCustomNode(SearchModel searchModel, string nodeName, string catName, string descr = "Bla",
                                                 string path = "Bla")
        {
            var dummyInfo = new CustomNodeInfo(Guid.NewGuid(), nodeName, catName, descr, path);

            searchModel.Add(dummyInfo);

            var res = searchModel.Search(nodeName).ToList();
            Assert.AreNotEqual(0, res.Count());
            Assert.AreEqual(res[0].Name, nodeName);
            Assert.IsTrue(searchModel.ContainsCategory(catName));

            searchModel.RemoveNodeAndEmptyParentCategory(nodeName);
            res = searchModel.Search(nodeName).ToList();

            Assert.AreEqual(0, res.Count());
            Assert.IsFalse(searchModel.ContainsCategory(catName));
        }

        [Test]
        [Category("UnitTests")]
        public void CannotAddCustomNodesWithSameGuids()
        {
            var nodeName = "TheNoodle";
            var catName = "TheCat";
            var descr = "TheCat";
            var path = @"C:\turtle\graphics.dyn";
            var guid1 = Guid.NewGuid();
            var dummyInfo1 = new CustomNodeInfo(guid1, nodeName, catName, descr, path);
            var dummyInfo2 = new CustomNodeInfo(guid1, nodeName, catName, descr, path);

            search.Add(dummyInfo1);
            search.Add(dummyInfo2);

            Assert.AreEqual(1, search.SearchDictionary.NumElements);

            var results = search.Search(nodeName).ToList();

            Assert.AreEqual(1, results.Count());

            var res1 = results[0];

            Assert.IsAssignableFrom(typeof(CustomNodeSearchElement), res1);

            var node1 = res1 as CustomNodeSearchElement;

            Assert.AreEqual(node1.Guid, guid1);

        }

        [Test]
        [Category("UnitTests")]
        public void CanRemoveNodeAndCategoryByFunctionId()
        {
            var nodeName = "TheNoodle";
            var catName = "TheCat";
            var descr = "TheCat";
            var path = @"C:\turtle\graphics.dyn";
            var guid1 = Guid.NewGuid();
            var dummyInfo1 = new CustomNodeInfo(guid1, nodeName, catName, descr, path);

            // add custom node
            search.Add(dummyInfo1);

            // confirm it's in the dictionary
            Assert.AreEqual(1, search.SearchDictionary.NumElements);

            // remove custom node
            search.RemoveNodeAndEmptyParentCategory(guid1);

            // it's gone
            Assert.AreEqual(0, search.SearchDictionary.NumElements);
            var results = search.Search(nodeName);
            Assert.AreEqual(0, results.Count());

        }

        [Test]
        [Category("UnitTests")]
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

            search.Add(dummyInfo1);
            search.Add(dummyInfo2);

            Assert.AreEqual(2, search.SearchDictionary.NumElements);

            var results = search.Search(nodeName).ToList();

            Assert.AreEqual(2, results.Count());

            var res1 = results[0];
            var res2 = results[1];

            Assert.IsAssignableFrom(typeof(CustomNodeSearchElement), res1);
            Assert.IsAssignableFrom(typeof(CustomNodeSearchElement), res2);

            var node1 = res1 as CustomNodeSearchElement;
            var node2 = res2 as CustomNodeSearchElement;

            Assert.AreEqual(node1.Guid, guid1);
            Assert.AreEqual(node2.Guid, guid2);

        }

        [Test]
        [Category("UnitTests")]
        public void CanAddCustomNodeWithSinglyNestedCategoryValidInput()
        {
            var nodeName = "TheNode";
            var catName = "TheCat";
            AssertAddAndRemoveCustomNode(search, nodeName, catName);
        }

        [Test]
        [Category("UnitTests")]
        public void CanAddCustomNodeWithDoublyNestedCategoryValidInput()
        {
            var nodeName = "TheNode";
            var catName = "TheCat.TheInnerCat";
            AssertAddAndRemoveCustomNode(search, nodeName, catName);
        }
        #endregion

        #region Add Categories

        [Test]
        [Category("UnitTests")]
        public void AddingARootCategoryMultipleTimesOnlyCreatesOneCategory()
        {
            const string catName = "Category";

            for (var i = 0; i < 10; i++)
            {
                search.TryAddRootCategory(catName);
            }
            Assert.IsTrue(search.ContainsCategory(catName));
            Assert.AreEqual(1, search.BrowserRootCategories.Count(x => x.Name == catName));
        }

        [Test]
        [Category("UnitTests")]
        public void AddingANestedCategoryMultipleTimesDoeNotDuplicateParentCategories()
        {
            const string catName = "Category.Child";

            for (var i = 0; i < 10; i++)
            {
                search.AddCategory(catName);
            }
            Assert.IsTrue(search.ContainsCategory(catName));
            Assert.AreEqual(1, search.BrowserRootCategories.Count(x => x.Name == "Category"));
            var nestedCat = (BrowserInternalElement)search.GetCategoryByName("Category.Child");
            Assert.NotNull(nestedCat);
            Assert.AreEqual(1, nestedCat.Parent.Items.Count());
        }

        [Test]
        [Category("UnitTests")]
        public void CanAddCategory()
        {
            var root = search.TryAddRootCategory("Peter");
            var leafCat = new BrowserInternalElement("Boyer", root);
            root.Items.Add(leafCat);

            Assert.Contains( leafCat, root.Items );
            Assert.Contains(root, search.BrowserRootCategories);
            
        }


        [Test]
        [Category("UnitTests")]
        public void CanAddCategoryWithDelimiters()
        {
            search.AddCategory("Peter.Boyer");
            Assert.IsTrue(search.ContainsCategory("Peter.Boyer"));
        }

        #endregion

        #region Remove Categories

        [Test]
        [Category("UnitTests")]
        public void CanRemoveRootCategoryWithInternalElements()
        {
            var root = (BrowserRootElement)search.TryAddRootCategory("Peter");
            var leafCat = new BrowserInternalElement("Boyer", root);
            root.Items.Add(leafCat);

            Assert.Contains( leafCat, root.Items );
            Assert.Contains(root, search.BrowserRootCategories);

            search.RemoveCategory("Peter");
            Assert.False(search.BrowserRootCategories.Contains(root));
        }

        [Test]
        [Category("UnitTests")]
        public void CanRemoveCategoryWithDelimiters()
        {
            search.AddCategory("Peter.Boyer");

            Assert.IsTrue(search.ContainsCategory("Peter.Boyer"));

            search.RemoveCategory("Peter.Boyer");
            Assert.IsNull(search.GetCategoryByName("Peter.Boyer"));

        }

        [Test]
        [Category("UnitTests")]
        public void CanRunRemoveCategoryIfCategoryDoesntExist()
        {
            var search = new SearchModel();
            search.AddCategory("Peter.Boyer");

            search.RemoveCategory("Peter.Rabbit");
            Assert.IsNull(search.GetCategoryByName("Peter.Rabbit"));

        }

        #endregion

        #region Remove Nodes

        [Test]
        [Category("UnitTests")]
        public void CanTryToRemoveElementFromSearchWithNonexistentName()
        {
            search.RemoveNodeAndEmptyParentCategory("NonExistentName");

            var results = search.Search("NonExistentName");
            Assert.AreEqual(0, results.Count());
        }

        [Test]
        [Category("UnitTests")]
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

            search.Add(dummyInfo1);
            search.Add(dummyInfo2);

            Assert.AreEqual(2, search.SearchDictionary.NumElements);

            search.RemoveNodeAndEmptyParentCategory(guid2);

            Assert.AreEqual(1, search.SearchDictionary.NumElements);

            var results = search.Search(nodeName).ToList();

            Assert.AreEqual(1, results.Count());

            var res1 = results[0];
            Assert.IsAssignableFrom(typeof(CustomNodeSearchElement), res1);
            var node1 = res1 as CustomNodeSearchElement;
            Assert.AreEqual(node1.Guid, guid1);
        }

        [Test]
        [Category("UnitTests")]
        public void CanRemoveElementCustomNodeByNameWithNestedCategory()
        {

            search.Add(new CustomNodeInfo(Guid.NewGuid(), "Peter", "Turnip.Greens", "des", ""));

            var results = search.Search("Peter");
            Assert.AreEqual(1, results.Count());

            search.RemoveNodeAndEmptyParentCategory("Peter");
            results = search.Search("Peter");

            Assert.AreEqual(0, results.Count());
        }

        [Test]
        [Category("UnitTests")]
        public void CanRemoveElementCustomNodeByNameWithSingleCategory()
        {
            search.Add(new CustomNodeInfo(Guid.NewGuid(), "Peter", "Greens", "des", ""));

            var results = search.Search("Peter");
            Assert.AreEqual(1, results.Count());

            search.RemoveNodeAndEmptyParentCategory("Peter");
            results = search.Search("Peter");

            Assert.AreEqual(0, results.Count());
        }

        #endregion
    }
}
