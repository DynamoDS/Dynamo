using Dynamo.Nodes.Search;
using Dynamo.Search;
using NUnit.Framework;

namespace Dynamo.Tests
{

    [TestFixture]
    internal class SearchTests
    {
        [Test]
        public void CanAddCategory()
        {
            var model = new SearchViewModel();
            var root = model.AddRootCategory("Peter");
            var leafCat = new BrowserInternalElement("Boyer", root);
            root.Items.Add(leafCat);

            Assert.Contains(leafCat, root.Items );
            Assert.Contains( root, model.BrowserRootCategories );
            
        }

        [Test]
        public void CanRemoveRootCategoryWithInternalElements()
        {
            var model = new SearchViewModel();
            var root = model.AddRootCategory("Peter");
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
            var root = model.AddRootCategory("Peter");
            var leaf = new BrowserInternalElement("Boyer", root);
            root.AddChild(leaf);

            Assert.Contains(leaf, root.Items);
            Assert.Contains(root, model.BrowserRootCategories);

            model.RemoveCategory("Peter.Boyer");
            Assert.True( model.BrowserRootCategories.Contains(root) );
            Assert.False( root.Items.Contains(leaf) );

        }

        [Test]
        public void CanRunRemoveCategoryIfCategoryDoesntExist()
        {
            var model = new SearchViewModel();
            var root = model.AddRootCategory("Peter");
            var leaf = new BrowserInternalElement("Boyer", root);
            root.AddChild(leaf);

            Assert.Contains(leaf, root.Items);
            Assert.Contains(root, model.BrowserRootCategories);

            model.RemoveCategory("Peter.Rabbit");
            Assert.True(model.BrowserRootCategories.Contains(root));
            Assert.True(root.Items.Contains(leaf));

        }

        [Test]
        public void CanAddCategoryWithDelimiters()
        {
            var model = new SearchViewModel();
            var leaf = model.AddCategory("Peter.Boyer");

            model.RemoveCategory("Peter.Boyer");

        }

        [Test]
        public void CanTryToRemoveElementFromSearchWithNonexistentName()
        {
            var model = new SearchViewModel();
            model.Remove("NonExistentName");

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

            model.Remove("Peter");
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

            model.Remove("Peter");
            model.SearchAndUpdateResultsSync("Peter");

            Assert.AreEqual(0, model.SearchResults.Count);
        }

    }
}
