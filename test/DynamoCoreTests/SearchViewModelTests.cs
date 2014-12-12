using System;
using System.Linq;
using System.Threading;
using Dynamo.Search;
using Dynamo.UI;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    internal class SearchViewModelTests
    {
        private static SearchModel model;
        private static SearchViewModel viewModel;

        [SetUp]
        public void Init()
        {
            model = new SearchModel();
            viewModel = new SearchViewModel(null, model);
        }

        [Test]
        [Category("UnitTests")]
        [Category("Failure")]
        public void PopulateSearchTextWithSelectedResultReturnsExpectedResult()
        {
            var catName = "Animals";
            var descr = "";
            var path = "";

            model.Add(new CustomNodeInfo(Guid.NewGuid(), "xyz", catName, descr, path));
            model.Add(new CustomNodeInfo(Guid.NewGuid(), "abc", catName, descr, path));
            model.Add(new CustomNodeInfo(Guid.NewGuid(), "cat", catName, descr, path));
            model.Add(new CustomNodeInfo(Guid.NewGuid(), "dog", catName, descr, path));
            model.Add(new CustomNodeInfo(Guid.NewGuid(), "frog", catName, descr, path));
            model.Add(new CustomNodeInfo(Guid.NewGuid(), "Noodle", catName, descr, path));

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
            var result = SearchModel.ShortenCategoryName(categoryName);
            Assert.AreEqual(string.Empty, result);

            categoryName = null;
            result = SearchModel.ShortenCategoryName(categoryName);
            Assert.AreEqual(string.Empty, result);

            categoryName = "Category1";
            result = SearchModel.ShortenCategoryName(categoryName);
            Assert.AreEqual("Category1", result);

            categoryName = "Cat1 Cat" + Configurations.CategoryDelimiter + "Cat2 Cat" +
                                    Configurations.CategoryDelimiter + "Cat3";
            result = SearchModel.ShortenCategoryName(categoryName);
            Assert.AreEqual("Cat1 Cat " + Configurations.ShortenedCategoryDelimiter + " Cat2 Cat " +
                                      Configurations.ShortenedCategoryDelimiter + " Cat3", result);

            categoryName = "TenSymbol" + Configurations.CategoryDelimiter +
                           "TenSymbol" + Configurations.CategoryDelimiter +
                           "TenSymbol" + Configurations.CategoryDelimiter +
                           "TenSymbol" + Configurations.CategoryDelimiter +
                           "TenSymbol" + Configurations.CategoryDelimiter +
                           "MoreSymbols";
            result = SearchModel.ShortenCategoryName(categoryName);
            Assert.AreEqual("TenSymbol " + Configurations.ShortenedCategoryDelimiter +
                           " ... " + Configurations.ShortenedCategoryDelimiter +
                           " TenSymbol " + Configurations.ShortenedCategoryDelimiter +
                           " TenSymbol " + Configurations.ShortenedCategoryDelimiter +
                           " MoreSymbols", result);
        }
    }
}
