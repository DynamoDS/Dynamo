using System;
using Dynamo.Search;
using Dynamo.Search.SearchElements;
using Dynamo.ViewModels;
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
    }
}
