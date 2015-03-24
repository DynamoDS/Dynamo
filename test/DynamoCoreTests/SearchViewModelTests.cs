using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Search;
using Dynamo.Search.SearchElements;
using Dynamo.UI;
using Dynamo.ViewModels;
using Dynamo.Wpf.ViewModels;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    internal class SearchViewModelTests
    {
        private static NodeSearchModel model;
        private static SearchViewModel viewModel;

        [SetUp]
        public void Init()
        {
            model = new NodeSearchModel();
            viewModel = new SearchViewModel(null, model);
        }

        [Test]
        [Category("UnitTests")]
        [Category("Failure")]
        public void PopulateSearchTextWithSelectedResultReturnsExpectedResult()
        {
            const string catName = "Animals";
            const string descr = "";
            const string path = "";

            model.Add(new CustomNodeSearchElement(null, new CustomNodeInfo(Guid.NewGuid(), "xyz", catName, descr, path)));
            model.Add(new CustomNodeSearchElement(null, new CustomNodeInfo(Guid.NewGuid(), "abc", catName, descr, path)));
            model.Add(new CustomNodeSearchElement(null, new CustomNodeInfo(Guid.NewGuid(), "cat", catName, descr, path)));
            model.Add(new CustomNodeSearchElement(null, new CustomNodeInfo(Guid.NewGuid(), "dog", catName, descr, path)));
            model.Add(new CustomNodeSearchElement(null, new CustomNodeInfo(Guid.NewGuid(), "frog", catName, descr, path)));
            model.Add(new CustomNodeSearchElement(null, new CustomNodeInfo(Guid.NewGuid(), "Noodle", catName, descr, path)));

            viewModel.SearchAndUpdateResults("xy");
            viewModel.PopulateSearchTextWithSelectedResult();
            Assert.AreEqual("xyz", viewModel.SearchText);

            viewModel.SearchAndUpdateResults("ood");
            viewModel.PopulateSearchTextWithSelectedResult();
            Assert.AreEqual("Noodle", viewModel.SearchText);

            viewModel.SearchAndUpdateResults("do");
            viewModel.PopulateSearchTextWithSelectedResult();
            Assert.AreEqual("dog", viewModel.SearchText);
        }

        [Test]
        [Category("UnitTests")]
        public void ShortenCategoryNameTests()
        {
            var categoryName = "";
            var result = Nodes.Utilities.ShortenCategoryName(categoryName);
            Assert.AreEqual(string.Empty, result);

            categoryName = null;
            result = Nodes.Utilities.ShortenCategoryName(categoryName);
            Assert.AreEqual(string.Empty, result);

            categoryName = "Category1";
            result = Nodes.Utilities.ShortenCategoryName(categoryName);
            Assert.AreEqual("Category1", result);

            categoryName = "Cat1 Cat" + Configurations.CategoryDelimiterString + "Cat2 Cat" +
                                    Configurations.CategoryDelimiterString + "Cat3";
            result = Nodes.Utilities.ShortenCategoryName(categoryName);
            Assert.AreEqual("Cat1 Cat" + Configurations.ShortenedCategoryDelimiter + "Cat2 Cat" +
                                      Configurations.ShortenedCategoryDelimiter + "Cat3", result);

            categoryName = "TenSymbol" + Configurations.CategoryDelimiterString +
                           "TenSymbol" + Configurations.CategoryDelimiterString +
                           "TenSymbol" + Configurations.CategoryDelimiterString +
                           "TenSymbol" + Configurations.CategoryDelimiterString +
                           "TenSymbol" + Configurations.CategoryDelimiterString +
                           "MoreSymbols";
            result = Nodes.Utilities.ShortenCategoryName(categoryName);
            Assert.AreEqual("TenSymbol" + Configurations.ShortenedCategoryDelimiter +
                           "..." + Configurations.ShortenedCategoryDelimiter +
                           "TenSymbol" + Configurations.ShortenedCategoryDelimiter +
                           "TenSymbol" + Configurations.ShortenedCategoryDelimiter +
                           "MoreSymbols", result);
        }

        #region InsertEntry tests

        [Test]
        [Category("UnitTests")]
        public void InsertEntry01EmptyTree()
        {
            var elementVM = CreateCustomNodeViewModel("Member", "TopCategory.SubCategory");

            Assert.IsFalse(viewModel.BrowserRootCategories.Any());

            viewModel.InsertEntry(elementVM, elementVM.Model.Categories);

            var category = viewModel.BrowserRootCategories.First(c => c.Name == "TopCategory").
                SubCategories.First(c => c.Name == "Classes" && c is ClassesNodeCategoryViewModel).
                SubCategories.First(c => c.Name == "SubCategory");

            Assert.IsTrue(category.Items.Contains(elementVM));
        }

        [Test]
        [Category("UnitTests")]
        public void InsertEntry02TheSameClass()
        {
            // Tree preparation
            var elementVM = CreateCustomNodeViewModel("Member1", "TopCategory.SubCategory");
            viewModel.InsertEntry(elementVM, elementVM.Model.Categories);

            // Second member addition to the same class. 
            elementVM = CreateCustomNodeViewModel("Member2", "TopCategory.SubCategory");
            viewModel.InsertEntry(elementVM, elementVM.Model.Categories);

            var category = viewModel.BrowserRootCategories.First(c => c.Name == "TopCategory").
                SubCategories.First(c => c.Name == "Classes" && c is ClassesNodeCategoryViewModel).
                SubCategories.First(c => c.Name == "SubCategory");

            Assert.IsNotNull(category.Items.FirstOrDefault(el => el.Name == "Member2"));
        }

        [Test]
        [Category("UnitTests")]
        public void InsertEntry03TheSameClassesContainer()
        {
            // Tree preparation
            var elementVM = CreateCustomNodeViewModel("Member1", "TopCategory.SubCategory1");
            viewModel.InsertEntry(elementVM, elementVM.Model.Categories);

            // Member addition to new class but the same classes container.
            elementVM = CreateCustomNodeViewModel("Member2", "TopCategory.SubCategory2");
            viewModel.InsertEntry(elementVM, elementVM.Model.Categories);

            var theClass = viewModel.BrowserRootCategories.First(c => c.Name == "TopCategory").
                SubCategories.First(c => c.Name == "Classes" && c is ClassesNodeCategoryViewModel);

            Assert.AreEqual(2, theClass.SubCategories.Count);
            Assert.IsNotNull(theClass.SubCategories.FirstOrDefault(c => c.Name == "SubCategory1"));

            var category = theClass.SubCategories.FirstOrDefault(c => c.Name == "SubCategory2");

            Assert.AreEqual(2, theClass.SubCategories.Count);
            Assert.IsNotNull(category);
            Assert.IsNotNull(category.Items.FirstOrDefault(c => c.Name == "Member2"));
        }

        [Test]
        [Category("UnitTests")]
        public void InsertEntry04NewPathIsLonger()
        {
            // Tree preparation
            var elementVM = CreateCustomNodeViewModel("Member1", "TopCategory.SubCategory1");
            viewModel.InsertEntry(elementVM, elementVM.Model.Categories);

            // Member addition to new path. Top category is the same. 
            elementVM = CreateCustomNodeViewModel("Member2", "TopCategory.SubCategory2.SubSubCat2");
            viewModel.InsertEntry(elementVM, elementVM.Model.Categories);

            var category = viewModel.BrowserRootCategories.First(c => c.Name == "TopCategory").
                SubCategories.First(c => c.Name == "SubCategory2").
                SubCategories.First(c => c.Name == "Classes" && c is ClassesNodeCategoryViewModel).
                SubCategories.First(c => c.Name == "SubSubCat2");

            Assert.IsNotNull(category.Items.FirstOrDefault(el => el.Name == "Member2"));
        }

        [Test]
        [Category("UnitTests")]
        public void InsertEntry05AddCategoryToClass()
        {
            // Tree preparation
            var elementVM = CreateCustomNodeViewModel("Member1", "TopCategory.SubCategory1");
            viewModel.InsertEntry(elementVM, elementVM.Model.Categories);

            // New member with new category addition to class. 
            elementVM = CreateCustomNodeViewModel("Member2", "TopCategory.SubCategory1.SubSubCat1");
            viewModel.InsertEntry(elementVM, elementVM.Model.Categories);

            var category = viewModel.BrowserRootCategories.First(c => c.Name == "TopCategory").
                SubCategories.First(c => c.Name == "SubCategory1");

            Assert.IsNotNull(category.Items.FirstOrDefault(el => el.Name == "Member1"));

            category = viewModel.BrowserRootCategories.First(c => c.Name == "TopCategory").
                SubCategories.First(c => c.Name == "SubCategory1").
                SubCategories.First(c => c.Name == "Classes" && c is ClassesNodeCategoryViewModel).
                SubCategories.First(c => c.Name == "SubSubCat1");

            Assert.IsNotNull(category.Items.FirstOrDefault(el => el.Name == "Member2"));
        }

        [Test]
        [Category("UnitTests")]
        public void InsertEntry06AddCategoryToClassDoNotRemoveClass()
        {
            // Tree preparation
            var elementVM = CreateCustomNodeViewModel("Member1", "TopCategory.SubCategory1");
            viewModel.InsertEntry(elementVM, elementVM.Model.Categories);

            elementVM = CreateCustomNodeViewModel("Member2", "TopCategory.SubCategory2");
            viewModel.InsertEntry(elementVM, elementVM.Model.Categories);

            // New member with new category addition to class.
            elementVM = CreateCustomNodeViewModel("Member3", "TopCategory.SubCategory1.SubSubCat1");
            viewModel.InsertEntry(elementVM, elementVM.Model.Categories);

            var category = viewModel.BrowserRootCategories.First(c => c.Name == "TopCategory").
                SubCategories.First(c => c.Name == "SubCategory1");

            Assert.IsNotNull(category.Items.FirstOrDefault(el => el.Name == "Member1"));

            category = viewModel.BrowserRootCategories.First(c => c.Name == "TopCategory").
                SubCategories.First(c => c.Name == "Classes" && c is ClassesNodeCategoryViewModel).
                SubCategories.First(c => c.Name == "SubCategory2");

            Assert.IsNotNull(category.Items.FirstOrDefault(el => el.Name == "Member2"));

            category = viewModel.BrowserRootCategories.First(c => c.Name == "TopCategory").
                SubCategories.First(c => c.Name == "SubCategory1").
                SubCategories.First(c => c.Name == "Classes" && c is ClassesNodeCategoryViewModel).
                SubCategories.First(c => c.Name == "SubSubCat1");

            Assert.IsNotNull(category.Items.FirstOrDefault(el => el.Name == "Member3"));
        }

        #endregion

        #region RemoveEntry tests

        [Test]
        [Category("UnitTests")]
        public void RemoveEntry01NotExistentEntryEmptyTree()
        {
            var element = CreateCustomNode("Member1", "TopCategory.SubCategory1");

            // No exception expected.
            viewModel.RemoveEntry(element);
        }

        [Test]
        [Category("UnitTests")]
        public void RemoveEntry02NotExistentEntryNonEmptyTree()
        {
            var element = CreateCustomNode("Member1", "TopCategory.SubCategory1");
            viewModel.InsertEntry(CreateCustomNodeViewModel(element), element.Categories);

            element = CreateCustomNode("Member2", "TopCategory.SubCategory1");
            viewModel.RemoveEntry(element);

            var category = viewModel.BrowserRootCategories.First(c => c.Name == "TopCategory").
                SubCategories.First(c => c.Name == "Classes" && c is ClassesNodeCategoryViewModel).
                SubCategories.First(c => c.Name == "SubCategory1");

            Assert.IsNotNull(category.Items.FirstOrDefault(it => it.Name == "Member1"));

            element = CreateCustomNode("Member2", "TopCategory");
            viewModel.RemoveEntry(element);

            category = viewModel.BrowserRootCategories.First(c => c.Name == "TopCategory").
                SubCategories.First(c => c.Name == "Classes" && c is ClassesNodeCategoryViewModel).
                SubCategories.First(c => c.Name == "SubCategory1");

            Assert.IsNotNull(category.Items.FirstOrDefault(it => it.Name == "Member1"));
        }

        [Test]
        [Category("UnitTests")]
        public void RemoveEntry03ClassInformationViewModelPresented()
        {
            var element = CreateCustomNode("Member1", "TopCategory.SubCategory1");
            viewModel.InsertEntry(CreateCustomNodeViewModel(element), element.Categories);

            var theClass = viewModel.BrowserRootCategories.First(c => c.Name == "TopCategory").
                SubCategories.First(c => c.Name == "Classes" && c is ClassesNodeCategoryViewModel);

            theClass.Items.Add(new ClassInformationViewModel());

            viewModel.RemoveEntry(element);

            Assert.IsFalse(viewModel.BrowserRootCategories.Any());
        }

        [Test]
        [Category("UnitTests")]
        public void RemoveEntry04TheSameClass()
        {
            // Tree preparation
            var element = CreateCustomNode("Member1", "TopCategory.SubCategory1");
            viewModel.InsertEntry(CreateCustomNodeViewModel(element), element.Categories);

            element = CreateCustomNode("Member2", "TopCategory.SubCategory1");
            viewModel.InsertEntry(CreateCustomNodeViewModel(element), element.Categories);

            viewModel.RemoveEntry(element);

            var category = viewModel.BrowserRootCategories.First(c => c.Name == "TopCategory").
                SubCategories.First(c => c.Name == "Classes" && c is ClassesNodeCategoryViewModel).
                SubCategories.First(c => c.Name == "SubCategory1");

            Assert.AreEqual(1, category.Items.Count);
            Assert.IsNotNull(category.Items.FirstOrDefault(c => c.Name == "Member1"));
        }

        [Test]
        [Category("UnitTests")]
        public void RemoveEntry05TheSameClassesContainer()
        {
            // Tree preparation
            var element = CreateCustomNode("Member1", "TopCategory.SubCategory1");
            viewModel.InsertEntry(CreateCustomNodeViewModel(element), element.Categories);

            element = CreateCustomNode("Member2", "TopCategory.SubCategory2");
            viewModel.InsertEntry(CreateCustomNodeViewModel(element), element.Categories);

            viewModel.RemoveEntry(element);

            var category = viewModel.BrowserRootCategories.First(c => c.Name == "TopCategory");

            Assert.IsNull(category.SubCategories.FirstOrDefault(c => c.Name == "SubCategory2"));
        }

        [Test]
        [Category("UnitTests")]
        public void RemoveEntry06OldPathIsLonger()
        {
            // Tree preparation
            var element = CreateCustomNode("Member1", "TopCategory.SubCategory1");
            viewModel.InsertEntry(CreateCustomNodeViewModel(element), element.Categories);

            element = CreateCustomNode("Member2", "TopCategory.SubCategory2.SubSubCat2");
            viewModel.InsertEntry(CreateCustomNodeViewModel(element), element.Categories);

            viewModel.RemoveEntry(element);

            var category = viewModel.BrowserRootCategories.First(c => c.Name == "TopCategory");

            Assert.IsNull(category.SubCategories.FirstOrDefault(c => c.Name == "SubCategory2"));
        }

        [Test]
        [Category("UnitTests")]
        public void RemoveEntry07RemoveCategoryFromClass()
        {
            // Tree preparation
            var element = CreateCustomNode("Member1", "TopCategory.SubCategory1");
            viewModel.InsertEntry(CreateCustomNodeViewModel(element), element.Categories);

            element = CreateCustomNode("Member2", "TopCategory.SubCategory1.SubSubCat1");
            viewModel.InsertEntry(CreateCustomNodeViewModel(element), element.Categories);

            viewModel.RemoveEntry(element);

            var category = viewModel.BrowserRootCategories.First(c => c.Name == "TopCategory").
                SubCategories.First(c => c.Name == "Classes" && c is ClassesNodeCategoryViewModel).
                SubCategories.First(c => c.Name == "SubCategory1");

            Assert.AreEqual(1, category.Items.Count);
            Assert.IsNotNull(category.Items.FirstOrDefault(c => c.Name == "Member1"));
        }

        [Test]
        [Category("UnitTests")]
        public void RemoveEntry08RemoveCategoryFromClassAddClassToContainer()
        {
            // Tree preparation            
            var element = CreateCustomNode("Member1", "TopCategory.SubCategory1");
            viewModel.InsertEntry(CreateCustomNodeViewModel(element), element.Categories);

            element = CreateCustomNode("Member2", "TopCategory.SubCategory2");
            viewModel.InsertEntry(CreateCustomNodeViewModel(element), element.Categories);

            element = CreateCustomNode("Member3", "TopCategory.SubCategory1.SubSubCat1");
            viewModel.InsertEntry(CreateCustomNodeViewModel(element), element.Categories);

            viewModel.RemoveEntry(element);

            var theClass = viewModel.BrowserRootCategories.First(c => c.Name == "TopCategory").
                SubCategories.First(c => c.Name == "Classes" && c is ClassesNodeCategoryViewModel);

            Assert.AreEqual(2, theClass.Items.Count);
            Assert.IsNotNull(theClass.SubCategories.FirstOrDefault(c => c.Name == "SubCategory1"));
            Assert.IsNotNull(theClass.SubCategories.FirstOrDefault(c => c.Name == "SubCategory2"));
        }

        [Test]
        [Category("UnitTests")]
        public void RemoveEntry09RemoveAllNodes()
        {
            var searchElements = PrepareTestTree();

            foreach (var element in searchElements)
                viewModel.RemoveEntry(element.Model);

            Assert.IsFalse(viewModel.BrowserRootCategories.Any());
        }

        #endregion

        #region Helpers

        private static NodeSearchElement CreateCustomNode(string name, string category,
            string description = "", string path = "")
        {
            var element = new CustomNodeSearchElement(null,
                new CustomNodeInfo(Guid.NewGuid(), name, category, description, path));

            return element;
        }

        private static NodeSearchElementViewModel CreateCustomNodeViewModel(string name, string category,
            string description = "", string path = "")
        {
            var element = CreateCustomNode(name, category, description, path);
            return new NodeSearchElementViewModel(element, null);
        }

        private static NodeSearchElementViewModel CreateCustomNodeViewModel(NodeSearchElement element)
        {
            return new NodeSearchElementViewModel(element, null);
        }

        /// <summary>
        /// Build complex tree.
        /// </summary>
        /// <returns>All members of built tree.</returns>
        private static IEnumerable<NodeSearchElementViewModel> PrepareTestTree()
        {
            // Next tree will be built.
            // Used blocks: ─, │, ┌, ┐, ┤, ├, ┴, ┬. 
            //
            //                                        Top
            //                          ┌──────────────┼────────────┐
            //                       Sub1_1         Sub1_2       Sub1_3
            //             ┌────────────┤              │            │
            //          Sub2_1         Classes      Sub2_3       Sub2_4
            //    ┌────────┤            │              │            ├────────────┐
            // Classes     Member2   Sub2_2         Classes         Member6   Classes
            //    │                     │              │                         │
            // Sub3_1                   Member3        │                      Sub3_4
            //    │                              ┌─────┴─────┐                   |
            //    Member1                     Sub3_2      Sub3_3                 Member7
            //                                   │           │
            //                                   Member4     Member5
            //
            var searchElements = new List<NodeSearchElementViewModel>();

            searchElements.Add(CreateCustomNodeViewModel("Member1", "Top.Sub1_1.Sub2_1.Sub3_1"));
            searchElements.Add(CreateCustomNodeViewModel("Member2", "Top.Sub1_1.Sub2_1"));
            searchElements.Add(CreateCustomNodeViewModel("Member3", "Top.Sub1_1.Sub2_2"));
            searchElements.Add(CreateCustomNodeViewModel("Member4", "Top.Sub1_2.Sub2_3.Sub3_2"));
            searchElements.Add(CreateCustomNodeViewModel("Member5", "Top.Sub1_2.Sub2_3.Sub3_3"));
            searchElements.Add(CreateCustomNodeViewModel("Member6", "Top.Sub1_3.Sub2_4"));
            searchElements.Add(CreateCustomNodeViewModel("Member7", "Top.Sub1_3.Sub2_4.Sub3_4"));

            return searchElements;
        }

        #endregion
    }
}
