using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Dynamo;
using Dynamo.Controls;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Interfaces;
using Dynamo.LibraryViewExtensionWebView2.Handlers;
using Dynamo.LibraryViewExtensionWebView2;
using Dynamo.Search.SearchElements;
using Dynamo.Search;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.Interfaces;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace ViewExtensionLibraryTests
{
    public class DynamoLibraryItemsTests : DynamoModelTestBase
    {
        protected override string GetUserUserDataRootFolder()
        {
            return Path.GetTempPath();
        }

        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        [Category("UnitTests")]
        public void SearchNodeTest()
        {
            var nodeSearchModel = new NodeSearchModel();
            var path = @"C:\temp\xyz.dyf";
            var nodeName = "Code Block";
            var expectedQualifiedName = $"dyf://Core.Input.{nodeName}";

            for (int i = 0; i < 100; i++)
            {
                nodeSearchModel.Add(
                    new CustomNodeSearchElement(new Mock<ICustomNodeSource>().Object,
                    new CustomNodeInfo(Guid.NewGuid(), $"Node-{i}", $"Node-{i}-Category{i}", $"Node-{i}-Description", path))
                    );
            }

            nodeSearchModel.Add(
                    new CustomNodeSearchElement(new Mock<ICustomNodeSource>().Object,
                    new CustomNodeInfo(Guid.NewGuid(), nodeName, "Core.Input", "Node-Description", path))
                    );

            var pathmanager = new Mock<IPathManager>();
            var iconProvider = new IconResourceProvider(pathmanager.Object);

            SearchResultDataProvider searchResultDataProvider = new SearchResultDataProvider(nodeSearchModel, iconProvider);

            var extension = string.Empty;
            var searchResultStream = searchResultDataProvider.GetResource(nodeName, out extension);

            var searchResult = GetLoadedTypesFromJson(searchResultStream);
            List<LoadedTypeItem> nodesResult = searchResult.loadedTypes;

            Assert.AreEqual(nodesResult.Count, 1);
            Assert.AreEqual(expectedQualifiedName, nodesResult[0].fullyQualifiedName);
        }

        private LoadedTypeData<LoadedTypeItem> GetLoadedTypesFromJson(Stream stream)
        {
            using (var sr = new StreamReader(stream))
            {
                var serializer = new JsonSerializer();
                return (LoadedTypeData<LoadedTypeItem>)serializer.Deserialize(sr, typeof(LoadedTypeData<LoadedTypeItem>));
            }
        }

        /*
        [Test]
        [Category("UnitTests"), Category("Failure")]
        public void VerifyIconsForLibraryItems()
        {
            var iconProvider = new IconResourceProvider(GetModel().PathManager);
            var request = new Mock<IRequest>();
            var resource = "ViewExtensionLibraryTests.resources.libraryItems.json";
            using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
            {
                var data = GetLoadedTypesFromJson(s);
                var types = data.loadedTypes;

                var unresolved = new List<LoadedTypeItem>();
                foreach (var item in types)
                {
                    request.Setup(r => r.Url).Returns(item.iconUrl);
                    string extension = "txt";
                    using (var stream = iconProvider.GetResource(request.Object, out extension))
                    {
                        Assert.AreEqual("png", extension);
                        if (stream == null || stream.Length == 0)
                        {
                            unresolved.Add(item);
                        }
                    }
                }
                var sb = new StringBuilder();
                unresolved.ForEach(e => { var url = new IconUrl(new Uri(e.iconUrl)); sb.AppendFormat("Node: {0}, Image: {1}/{2}\n", e.fullyQualifiedName, url.Name, url.Path); });
                Assert.AreEqual(0, unresolved.Count, sb.ToString());
            }
        }

        [Test]
        [Category("UnitTests"), Category("Failure")]
        public void NodeItemDataProvider()
        {
            var provider = new NodeItemDataProvider(GetModel().SearchModel);
            var request = new Mock<IRequest>();
            string ext = "txt";
            using (var stream = provider.GetResource(request.Object, out ext))
            {
                Assert.AreEqual("json", ext);
                Assert.IsNotNull(stream);
                var data = GetLoadedTypesFromJson(stream);
                Assert.IsNotNull(data);
                Assert.Less(0, data.loadedTypes.Count);
                data.loadedTypes.ForEach(e =>
                {
                    Assert.IsNotNullOrEmpty(e.contextData);
                    Assert.IsNotNullOrEmpty(e.fullyQualifiedName);
                    Assert.IsNotNullOrEmpty(e.iconUrl);
                    Assert.IsNotNullOrEmpty(e.itemType);
                });
            }
        }

        [Test, Category("UnitTests"), Category("Failure")]
        public void LibraryViewCustomizationServiceLoaded()
        {
            var model = GetModel();
            var vm = DynamoViewModel.Start(new DynamoViewModel.StartConfiguration() { DynamoModel = model });
            var view = new DynamoView(vm);

            var customization = model.ExtensionManager.Service<ILibraryViewCustomization>();

            Assert.IsNotNull(customization);
            var eventanme = "SpecChanged";
            var controller = new Mock<IEventController>();
            customization.SpecificationUpdated += (o, e) => controller.Object.RaiseEvent(eventanme);

            var sectiontext = "Y";
            customization.AddSections(new[] { "X", "Y", "Z" }.Select(s => new LayoutSection(s)));

            var spec = customization.GetSpecification();
            Assert.AreEqual(3, spec.sections.Count);

            LibraryViewCustomizationTests.VerifyAddElements(customization, sectiontext, 3);

            LibraryViewCustomizationTests.VerifyAddIncludeInfo(customization, "X", 3);

            controller.Verify(c => c.RaiseEvent(eventanme), Times.Exactly(3)); //Only notified twice
        }

        private LoadedTypeData<LoadedTypeItem> GetLoadedTypesFromJson(Stream stream)
        {
            using (var sr = new StreamReader(stream))
            {
                var serializer = new JsonSerializer();
                return (LoadedTypeData< LoadedTypeItem>)serializer.Deserialize(sr, typeof(LoadedTypeData<LoadedTypeItem>));
            }
        }
        */
    }
}
