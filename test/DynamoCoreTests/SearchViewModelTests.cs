using System;
using Dynamo.Search;
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
            var result = SearchViewModel.ShortenCategoryName("");
            Assert.AreEqual(string.Empty, result);

            result = SearchViewModel.ShortenCategoryName(null);
            Assert.AreEqual(string.Empty, result);
        }
    }
}
