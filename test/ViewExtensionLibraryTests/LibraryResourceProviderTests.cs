﻿using System;
using System.IO;
using System.Reflection;
using CefSharp;
using Dynamo;
using Dynamo.Extensions;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.LibraryUI;
using Dynamo.LibraryUI.Handlers;
using Dynamo.Search.SearchElements;
using Moq;
using NUnit.Framework;

namespace ViewExtensionLibraryTests
{
    public abstract class TestNodeSearchElement : NodeSearchElement
    {
        public TestNodeSearchElement(string category, string name, ElementTypes type, string path)
        {
            Name = name;
            FullCategoryName = category;
            ElementType = type;
            var keys = category.Split('.');
            foreach (var item in keys)
            {
                SearchKeywords.Add(item);
            }
            iconName = name;
            Assembly = path;
        }
    }

    public class LibraryResourceProviderTests
    {
        [Test]
        [Category("UnitTests")]
        public void EventControllerCallback()
        {
            var cmd = new Mock<ICommandExecutive>();
            var callback = new Mock<IJavascriptCallback>();
            callback.Setup(c => c.CanExecute).Returns(true);
            var controller = new EventController();

            controller.On("detailsViewContextDataChanged", callback.Object);
            controller.DetailsViewContextData = 5;

            callback.Verify(c => c.ExecuteAsync(5));
        }

        [Test]
        [Category("UnitTests")]
        public void ResourceHandlerFactoryRegistration()
        {
            var factory = new ResourceHandlerFactory();
            var moqprovider = new Mock<ResourceProviderBase>(true, "http") { CallBase = true };
            string extension = "txt";
            factory.RegisterProvider("/test/myprovider", moqprovider.Object);
            var req = new Mock<IRequest>();
            req.Setup(r => r.Url).Returns("http://domain/test/myprovider/xyz/abc");
            var handler = factory.GetResourceHandler(null, null, null, req.Object);

            moqprovider.Verify(p => p.GetResource(req.Object, out extension));
            Assert.IsNull(handler);
        }

        [Test]
        [Category("UnitTests")]
        public void ResourceHandlerFactoryProviderDonotExist()
        {
            var factory = new ResourceHandlerFactory();
            var moqprovider = new Mock<ResourceProviderBase>(true, "http") { CallBase = true };
            string extension = "txt";
            factory.RegisterProvider("/test/myprovider", moqprovider.Object);
            var req = new Mock<IRequest>();
            //Request url doesn't match the registered base url
            req.Setup(r => r.Url).Returns("http://domain/test/xyz/abc");
            var handler = factory.GetResourceHandler(null, null, null, req.Object);

            //Factory shouldn't be able to find a resource provider for given request
            //hence can't call GetResource on the given provider and returns null handler.
            moqprovider.Verify(p => p.GetResource(req.Object, out extension), Times.Never);
            Assert.IsNull(handler);
        }

        [Test]
        [Category("UnitTests")]
        public void ResourceHandlerFactoryReturnsValidHandler()
        {
            var factory = new ResourceHandlerFactory();
            var moqprovider = new Mock<ResourceProviderBase>(false, "http") { CallBase = true };
            string extension = "txt";
            moqprovider.Setup(p => p.GetResource(It.IsAny<IRequest>(), out extension)).Returns(new MemoryStream());

            factory.RegisterProvider("/test/myprovider", moqprovider.Object);
            var req = new Mock<IRequest>();
            req.Setup(r => r.Url).Returns("http://domain/test/myprovider/xyz/abc");
            var handler = factory.GetResourceHandler(null, null, null, req.Object);

            Assert.IsNotNull(handler);
            Assert.IsFalse(factory.HasHandlers);
        }

        [Test]
        [Category("UnitTests")]
        public void ResourceHandlerFactoryRegistersStaticResourceHandler()
        {
            var factory = new ResourceHandlerFactory();
            var moqprovider = new Mock<ResourceProviderBase>(true, "http") { CallBase = true };
            string extension = "txt";
            moqprovider.Setup(p => p.GetResource(It.IsAny<IRequest>(), out extension)).Returns(new MemoryStream());

            factory.RegisterProvider("/test/myprovider", moqprovider.Object);
            var req = new Mock<IRequest>();
            req.Setup(r => r.Url).Returns("http://domain/test/myprovider/xyz");
            var handler = factory.GetResourceHandler(null, null, null, req.Object);

            Assert.IsNotNull(handler);
            Assert.IsTrue(factory.HasHandlers);
            Assert.AreEqual(1, factory.Handlers.Count);
            Assert.IsTrue(factory.Handlers.ContainsKey(req.Object.Url));
        }

        [Test]
        [Category("UnitTests")]
        public void CreateDllResourceProvider()
        {
            var factory = new ResourceHandlerFactory();
            var p1 = new DllResourceProvider("http://localhost/dist/v0.0.1",
                "ViewExtensionLibraryTests",
                Assembly.GetExecutingAssembly()
            );

            Assert.IsTrue(p1.IsStaticResource);
            Assert.AreEqual("http", p1.Scheme);

            string extension = "txt";
            var req = new Mock<IRequest>();
            req.Setup(r => r.Url).Returns("http://localhost/dist/v0.0.1/resources/Dynamo.svg");
            var stream = p1.GetResource(req.Object, out extension);

            Assert.IsNotNull(stream);
            Assert.AreEqual("svg", extension);
            var size = stream.Length;


            var p2 = new DllResourceProvider("http://localhost/dynamo",
                "ViewExtensionLibraryTests.resources.Dynamo.svg",
                Assembly.GetExecutingAssembly()
            );

            Assert.IsTrue(p1.IsStaticResource);
            Assert.AreEqual("http", p1.Scheme);

            req.Setup(r => r.Url).Returns("http://localhost/dynamo");
            stream = p2.GetResource(req.Object, out extension);

            Assert.IsNotNull(stream);
            Assert.AreEqual("svg", extension);
            Assert.AreEqual(size, stream.Length);
        }

        [Test]
        [Category("UnitTests")]
        public void ResourceHandlerFactoryGetDllResource()
        {
            var factory = new ResourceHandlerFactory();
            var p1 = new DllResourceProvider("http://localhost/dist/v0.0.1",
                "ViewExtensionLibraryTests",
                Assembly.GetExecutingAssembly()
            );

            var p2 = new DllResourceProvider("http://localhost/dynamo",
                "ViewExtensionLibraryTests.resources.Dynamo.svg",
                Assembly.GetExecutingAssembly()
            );

            factory.RegisterProvider("/dist/v0.0.1", p1);
            factory.RegisterProvider("/dynamo", p2);

            var req1 = new Mock<IRequest>();
            req1.Setup(r => r.Url).Returns("http://localhost/dist/v0.0.1/resources/Dynamo.svg");
            var req2 = new Mock<IRequest>();
            req2.Setup(r => r.Url).Returns("http://localhost/dynamo");

            var h1 = factory.GetResourceHandler(null, null, null, req1.Object);
            var h2 = factory.GetResourceHandler(null, null, null, req2.Object);

            Assert.IsNotNull(h1);
            Assert.IsNotNull(h2);
            Assert.IsTrue(factory.HasHandlers);
            Assert.AreEqual(2, factory.Handlers.Count);
        }

        [Test]
        [Category("UnitTests")]
        public void IconUrlRoundTrip()
        {
            var name = "icon";
            var path = @"C:\some where\resource.dll";
            var url = new IconUrl(name, path);
            Assert.AreEqual(name + ".Small", url.Name);
            Assert.AreEqual(path, url.Path);

            var newUrl = new IconUrl(new System.Uri(url.Url));
            Assert.AreEqual(url.Name, newUrl.Name);
            Assert.AreEqual(url.Path, newUrl.Path);

            //For custom node, if the customization dll doesn't exist at the 
            //given path it is turned to default.
            var customNode = new IconUrl(name, path, true);
            Assert.AreEqual(IconUrl.DefaultPath, customNode.Path);
            Assert.AreEqual(IconUrl.DefaultIcon, customNode.Name);
        }

        [Test]
        [Category("UnitTests")]
        public void NodeSearchElementLoadedType()
        {
            var fullname = "abc.xyz.something";
            var creationName = "create abc xyz";
            var moq = new Mock<NodeSearchElement>() { CallBase = true };
            var element = moq.Object;
            moq.Setup(e => e.FullName).Returns(fullname);
            moq.Setup(e => e.CreationName).Returns(creationName);
            
            var provider = new NodeItemDataProvider(null);
            var item = provider.CreateLoadedTypeItem(element);
            Assert.AreEqual(fullname, item.fullyQualifiedName);
            Assert.AreEqual(creationName, item.contextData);
        }

        [Test]
        [Category("UnitTests")]
        public void PackagedNodeSearchElementLoadedType()
        {
            var category = "abc.xyz.somepackage";
            var name = "My Node";
            var creationName = "create abc xyz";
            var expectedQualifiedName = "pkg://abc.xyz.somepackage.My Node";
            var path = @"C:\temp\packages\test.dll";
            var moq = new Mock<TestNodeSearchElement>(category, name, ElementTypes.Packaged, path) { CallBase = true };
            var element = moq.Object;
            moq.Setup(e => e.CreationName).Returns(creationName);
            
            var provider = new NodeItemDataProvider(null);
            var item = provider.CreateLoadedTypeItem(element);
            Assert.AreEqual(expectedQualifiedName, item.fullyQualifiedName);
            Assert.AreEqual(creationName, item.contextData);
            Assert.AreEqual("abc, xyz, somepackage", item.keywords);

            var url = new IconUrl(new Uri(item.iconUrl));
            Assert.AreEqual("My%20Node.Small", url.Name);
            Assert.AreEqual(path, url.Path);
        }

        [Test]
        [Category("UnitTests")]
        public void CustomNodeSearchElementLoadedType()
        {
            var category = "abc.xyz.somepackage";
            var name = "My Node";
            var path = @"C:\temp\xyz.dyf";
            var guid = Guid.NewGuid();
            var info = new CustomNodeInfo(guid, name, category, "some description", path);
            var expectedQualifiedName = "abc.xyz.somepackage.My Node";
            var moq = new Mock<ICustomNodeSource>();
            var element = new CustomNodeSearchElement(moq.Object, info);
            
            var provider = new NodeItemDataProvider(null);
            var item = provider.CreateLoadedTypeItem(element);
            Assert.AreEqual(expectedQualifiedName, item.fullyQualifiedName);
            Assert.AreEqual(guid.ToString(), item.contextData);
            Assert.AreEqual(string.Empty, item.keywords);

            var url = new IconUrl(new Uri(item.iconUrl));
            Assert.AreEqual(IconUrl.DefaultIcon, url.Name);
            Assert.AreEqual(IconUrl.DefaultPath, url.Path);
        }

        [Test]
        [Category("UnitTests")]
        public void PackagedCustomNodeSearchElementLoadedType()
        {
            var category = "abc.xyz.somepackage";
            var name = "My Node";
            var path = @"C:\temp\xyz.dyf";
            var guid = Guid.NewGuid();
            var info = new CustomNodeInfo(guid, name, category, "some description", path) { IsPackageMember = true };
            var expectedQualifiedName = "pkg://abc.xyz.somepackage.My Node";
            var moq = new Mock<ICustomNodeSource>();
            var element = new CustomNodeSearchElement(moq.Object, info);

            var provider = new NodeItemDataProvider(null);
            var item = provider.CreateLoadedTypeItem(element);
            Assert.AreEqual(expectedQualifiedName, item.fullyQualifiedName);
            Assert.AreEqual(guid.ToString(), item.contextData);
            Assert.AreEqual(string.Empty, item.keywords);
            var url = new IconUrl(name, path, true);
            Assert.AreEqual(url.Url, item.iconUrl);
        }
    }
}
