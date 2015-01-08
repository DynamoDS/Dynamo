using System.Linq;
using Dynamo.Search;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    class CategoryBuilderTests
    {
        // TODO(Vladimir): no need in these tests.
#if false
        private CategoryBuilder browserCatBuilder;
        private CategoryBuilder addonCatBuilder;

        [SetUp]
        public void Init()
        {
            browserCatBuilder = new CategoryBuilder(new SearchModel(), false);
            addonCatBuilder = new CategoryBuilder(new SearchModel(), true);
        }

        #region Obtaining Stored Categories

        [Test]
        [Category("UnitTests")]
        public void GetCategoryByNameWithValidInput()
        {
            const string catName = "Category.Child";
            addonCatBuilder.AddCategory(catName);
            Assert.IsTrue(addonCatBuilder.ContainsCategory(catName));
            Assert.AreEqual(1, addonCatBuilder.RootCategories.Count(x => x.Name == "Category"));
            var nestedCat = addonCatBuilder.GetCategoryByName("Category.Child");
            Assert.NotNull(nestedCat);
        }

        [Test]
        [Category("UnitTests")]
        public void GetCategoryByNameWithInvalidInput()
        {
            const string catName = "Category.Child";
            addonCatBuilder.AddCategory(catName);
            Assert.IsTrue(addonCatBuilder.ContainsCategory(catName));
            Assert.AreEqual(1, addonCatBuilder.RootCategories.Count(x => x.Name == "Category"));
            var nestedCat = addonCatBuilder.GetCategoryByName("Toonces.The.Cat");
            Assert.IsNull(nestedCat);
        }

        [Test]
        [Category("UnitTests")]
        public void ContainsCategoryWithValidInput()
        {
            const string catName = "Category.Child";
            addonCatBuilder.AddCategory(catName);
            Assert.IsTrue(addonCatBuilder.ContainsCategory(catName));
        }

        [Test]
        [Category("UnitTests")]
        public void ContainsCategoryWithInvalidInput()
        {
            const string catName = "Category.Child";
            addonCatBuilder.AddCategory(catName);
            Assert.IsFalse(addonCatBuilder.ContainsCategory("Toonces.The.Cat"));
        }

        [Test]
        [Category("UnitTests")]
        public void TryGetSubCategoryWithValidInput()
        {
            const string catName = "Category";
            var cat = addonCatBuilder.AddCategory(catName);
            cat.Items.Add(new BrowserInternalElement("Child", cat));
            Assert.IsNotNull(addonCatBuilder.TryGetSubCategory(cat, "Child"));
        }

        [Test]
        [Category("UnitTests")]
        public void TryGetSubCategoryWithInvalidInput()
        {
            const string catName = "Category";
            var cat = addonCatBuilder.AddCategory(catName);
            cat.Items.Add(new BrowserInternalElement("Child", cat));
            Assert.IsNull(addonCatBuilder.TryGetSubCategory(cat, "Purple"));
        }

        [Test]
        [Category("UnitTests")]
        public void ContainsClassWithValidInput()
        {
            browserCatBuilder.AddCategory("TopCategory.SubCategory.SomeClass");
            Assert.IsNotNull(browserCatBuilder.GetCategoryByName("TopCategory.SubCategory.Classes.SomeClass"));
            Assert.IsTrue(browserCatBuilder.ContainsClass("TopCategory.SubCategory", "SomeClass"));
        }

        [Test]
        [Category("UnitTests")]
        public void ContainsClassWithInvalidInput()
        {
            Assert.IsNull(browserCatBuilder.GetCategoryByName("TopCategory.SubCategory.Classes"));
            Assert.IsFalse(browserCatBuilder.ContainsClass("TopCategory.SubCategory", "NotRealClass"));

            browserCatBuilder.AddCategory("TopCategory.SubCategory.SomeClass");
            Assert.IsNotNull(browserCatBuilder.GetCategoryByName("TopCategory.SubCategory.Classes.SomeClass"));
            Assert.IsFalse(browserCatBuilder.ContainsClass("TopCategory.SubCategory", "NeededClass"));
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
                addonCatBuilder.TryAddRootCategory(catName);
            }
            Assert.IsTrue(addonCatBuilder.ContainsCategory(catName));
            Assert.AreEqual(1, addonCatBuilder.RootCategories.Count(x => x.Name == catName));
        }

        [Test]
        [Category("UnitTests")]
        public void AddingCategoryWithNestedClasses()
        {
            const string class1 = "RootCategory.Namespace.Class1";
            const string class2 = "RootCategory.Namespace.Class2";
            browserCatBuilder.AddCategory(class1);
            browserCatBuilder.AddCategory(class2);

            Assert.IsTrue(browserCatBuilder.ContainsCategory("RootCategory.Namespace"));
            Assert.AreEqual(1, browserCatBuilder.RootCategories.Count(x => x.Name == "RootCategory"));

            Assert.IsTrue(browserCatBuilder.ContainsClass("RootCategory.Namespace", "Class1"));
            Assert.IsTrue(browserCatBuilder.ContainsClass("RootCategory.Namespace", "Class2"));

            // Try to mix nested class and usual class.
            // It will be next structure:
            // + RootCategory
            //   + UsualClass
            //   + Namespace
            //     + Class1
            //     + Class2

            const string usualClass = "RootCategory.UsualClass";
            browserCatBuilder.AddCategory(usualClass);
            Assert.IsTrue(browserCatBuilder.ContainsClass("RootCategory", "UsualClass"));

            // Ensure, that 1st element is always set of usual classes.
            var cat = browserCatBuilder.RootCategories.FirstOrDefault(x => x.Name == "RootCategory");
            Assert.IsTrue(cat.Items[0] is BrowserInternalElementForClasses);
        }

        [Test]
        [Category("UnitTests")]
        public void CanAddCategory()
        {
            var root = addonCatBuilder.TryAddRootCategory("Peter");
            var leafCat = new BrowserInternalElement("Boyer", root);
            root.Items.Add(leafCat);

            Assert.Contains(leafCat, root.Items);
            Assert.Contains(root, addonCatBuilder.RootCategories);
        }

        [Test]
        [Category("UnitTests")]
        public void CanAddCategoryWithDelimiters()
        {
            addonCatBuilder.AddCategory("Peter.Boyer");
            Assert.IsTrue(addonCatBuilder.ContainsCategory("Peter.Boyer"));
        }

        [Test]
        [Category("UnitTests")]
        public void CanAddBrowserCategory()
        {
            browserCatBuilder.AddCategory("TopCategory.SubCategory");
            Assert.IsTrue(browserCatBuilder.ContainsCategory("TopCategory.Classes.SubCategory"));
        }

        #endregion

        #region Remove Categories

        [Test]
        [Category("UnitTests")]
        public void CanRemoveRootCategoryWithInternalElements()
        {
            var root = (BrowserRootElement)addonCatBuilder.TryAddRootCategory("Peter");
            var leafCat = new BrowserInternalElement("Boyer", root);
            root.Items.Add(leafCat);

            Assert.Contains(leafCat, root.Items);
            Assert.Contains(root, addonCatBuilder.RootCategories);

            addonCatBuilder.RemoveCategory("Peter");
            Assert.False(addonCatBuilder.RootCategories.Contains(root));
        }

        [Test]
        [Category("UnitTests")]
        public void CanRemoveCategoryWithDelimiters()
        {
            addonCatBuilder.AddCategory("Peter.Boyer");

            Assert.IsTrue(addonCatBuilder.ContainsCategory("Peter.Boyer"));

            addonCatBuilder.RemoveCategory("Peter.Boyer");
            Assert.IsNull(addonCatBuilder.GetCategoryByName("Peter.Boyer"));
        }

        [Test]
        [Category("UnitTests")]
        public void CanRunRemoveCategoryIfCategoryDoesntExist()
        {
            addonCatBuilder.AddCategory("Peter.Boyer");

            addonCatBuilder.RemoveCategory("Peter.Rabbit");
            Assert.IsNull(addonCatBuilder.GetCategoryByName("Peter.Rabbit"));
        }

        [Test]
        [Category("UnitTests")]
        public void CanRemoveEmptyCategoryDoesntExist()
        {
            addonCatBuilder.RemoveEmptyCategory("Some.Category.Which.Doesnt.Exist");
            Assert.IsNull(addonCatBuilder.GetCategoryByName("Some.Category.Which.Doesnt.Exist"));
        }

        [Test]
        [Category("UnitTests")]
        public void CanRemoveEmptyCategoryNotEmpty()
        {
            addonCatBuilder.AddCategory("TopCategory.SubCategory.SubSubCategory");

            addonCatBuilder.RemoveEmptyCategory("TopCategory.SubCategory");
            Assert.IsNotNull(addonCatBuilder.GetCategoryByName("TopCategory.SubCategory"));
        }

        [Test]
        [Category("UnitTests")]
        public void CanRemoveEmptyBrowserCategory()
        {
            browserCatBuilder.AddCategory("TopCategory.SubCategory.SomeClass");
            Assert.IsNotNull(
                browserCatBuilder.GetCategoryByName("TopCategory.SubCategory.Classes.SomeClass"));

            browserCatBuilder.RemoveEmptyCategory("TopCategory.SubCategory.Classes.SomeClass");
            Assert.IsNull(browserCatBuilder.
                GetCategoryByName("TopCategory.SubCategory.Classes.SomeClass"));

            browserCatBuilder.RemoveEmptyCategory("TopCategory.SubCategory.Classes");
            Assert.IsNull(browserCatBuilder.GetCategoryByName("TopCategory.SubCategory.Classes"));

            browserCatBuilder.RemoveEmptyCategory("TopCategory.SubCategory");
            Assert.IsNull(browserCatBuilder.GetCategoryByName("TopCategory.SubCategory"));

            browserCatBuilder.RemoveEmptyCategory("TopCategory");
            Assert.IsNull(browserCatBuilder.GetCategoryByName("TopCategory"));
        }

        #endregion

        #region Categories Search

        [Test]
        [Category("UnitTests")]
        public void CanSearchForPartOfTextAndGetResult()
        {
            const string catName = "Category.Child";
            addonCatBuilder.AddCategory(catName);
            Assert.IsTrue(addonCatBuilder.ContainsCategory(catName));
            Assert.AreEqual(1, addonCatBuilder.RootCategories.Count(x => x.Name == "Category"));
            var nestedCat = addonCatBuilder.GetCategoryByName("Category.Child");
            Assert.NotNull(nestedCat);
        }

        [Test]
        [Category("UnitTests")]
        public void CanAddMultiplyNestedCategory()
        {
            const string catName = "Category.Child.Thing.That";
            addonCatBuilder.AddCategory(catName);
            Assert.True(addonCatBuilder.ContainsCategory(catName));
        }

        [Test]
        [Category("UnitTests")]
        public void CanAddAndRemoveMultiplyNestedCategory()
        {
            const string catName = "Category.Child.Thing.That";
            addonCatBuilder.AddCategory(catName);
            Assert.True(addonCatBuilder.ContainsCategory(catName));
            addonCatBuilder.RemoveCategory(catName);
            Assert.False(addonCatBuilder.ContainsCategory(catName));
        }

        [Test]
        [Category("UnitTests")]
        public void CanRemoveRootAndRestOfChildrenOfNestedCategory()
        {
            const string catName = "Category.Child.Thing.That";
            addonCatBuilder.AddCategory(catName);
            Assert.True(addonCatBuilder.ContainsCategory(catName));
            addonCatBuilder.RemoveCategory("Category");
            Assert.False(addonCatBuilder.ContainsCategory(catName));
        }

        [Test]
        [Category("UnitTests")]
        public void CanAddMultiplyNestedCategoryMultipleTimes()
        {
            const string catName = "Category.Child.Thing.That";
            addonCatBuilder.AddCategory(catName);
            addonCatBuilder.AddCategory(catName);
            addonCatBuilder.AddCategory(catName);
            addonCatBuilder.AddCategory(catName);
            Assert.True(addonCatBuilder.ContainsCategory(catName));
        }

        #endregion
#endif
    }
}
