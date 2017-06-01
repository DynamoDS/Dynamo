using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using CefSharp;
using Dynamo;
using Dynamo.LibraryUI.Handlers;
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
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        [Category("UnitTests")]
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
        [Category("UnitTests")]
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

        private LoadedTypeData<LoadedTypeItem> GetLoadedTypesFromJson(Stream stream)
        {
            using (var sr = new StreamReader(stream))
            {
                var serializer = new JsonSerializer();
                return (LoadedTypeData< LoadedTypeItem>)serializer.Deserialize(sr, typeof(LoadedTypeData<LoadedTypeItem>));
            }
        }
    }
}
