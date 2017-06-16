using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CefSharp;
using Dynamo;
using Dynamo.Extensions;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Interfaces;
using Dynamo.LibraryUI;
using Dynamo.LibraryUI.Handlers;
using Dynamo.Search;
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
        private const string EventX = "X";

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
            Mock<NodeSearchElement> moq = MockNodeSearchElement(fullname, creationName);
            var element = moq.Object;

            var provider = new NodeItemDataProvider(new NodeSearchModel());
            var item = provider.CreateLoadedTypeItem<LoadedTypeItem>(element);
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
            
            var provider = new NodeItemDataProvider(new NodeSearchModel());
            var item = provider.CreateLoadedTypeItem<LoadedTypeItem>(element);
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
            var expectedQualifiedName = "dyf://abc.xyz.somepackage.My Node";
            var moq = new Mock<ICustomNodeSource>();
            var element = new CustomNodeSearchElement(moq.Object, info);
            
            var provider = new NodeItemDataProvider(new NodeSearchModel());
            var item = provider.CreateLoadedTypeItem<LoadedTypeItem>(element);
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

            var provider = new NodeItemDataProvider(new NodeSearchModel());
            var item = provider.CreateLoadedTypeItem<LoadedTypeItem>(element);
            Assert.AreEqual(expectedQualifiedName, item.fullyQualifiedName);
            Assert.AreEqual(guid.ToString(), item.contextData);
            Assert.AreEqual(string.Empty, item.keywords);
            var url = new IconUrl(name, path, true);
            Assert.AreEqual(url.Url, item.iconUrl);
        }

        [Test]
        [Category("UnitTests")]
        public void LibraryDataUpdatedEventRaised()
        {
            const string libraryDataUpdated = "libraryDataUpdated";
            var timeout = 50; //50 milliseconds
            var resetevent = new AutoResetEvent(false);

            var model = new NodeSearchModel();
            var controller = new Mock<IEventController>();
            controller.Setup(c => c.RaiseEvent(It.IsAny<string>(), It.IsAny<object[]>())).Callback(() => resetevent.Set());

            var customization = new LibraryViewCustomization();

            var disposable = LibraryViewController.SetupSearchModelEventsObserver(model, controller.Object, customization, timeout);
            controller.Verify(c => c.RaiseEvent(libraryDataUpdated, It.IsAny<object[]>()), Times.Never);

            var d1 = MockNodeSearchElement("A", "B");
            var d2 = MockNodeSearchElement("C", "D");
            var d3 = MockNodeSearchElement("E", "F");
            model.Add(d1.Object);
            model.Add(d2.Object);
            model.Add(d3.Object);
            Assert.AreEqual(3, model.NumElements);

            Assert.IsTrue(resetevent.WaitOne(timeout*3));
            controller.Verify(c => c.RaiseEvent(libraryDataUpdated), Times.Once);

            var spec = customization.GetSpecification();
            var section = spec.sections.FirstOrDefault();
            Assert.AreEqual(1, spec.sections.Count);
            //There must be a section named "Add-ons" now.
            Assert.AreEqual("Add-ons", section.text);
            Assert.AreEqual(3, section.include.Count);
            Assert.AreEqual("A, C, E", string.Join(", ", section.include.Select(i => i.path)));

            //Dispose
            disposable.Dispose();
            d1 = MockNodeSearchElement("G", "B");
            d2 = MockNodeSearchElement("H", "D");
            d3 = MockNodeSearchElement("I", "F");
            model.Add(d1.Object);
            model.Add(d2.Object);
            model.Add(d3.Object);
            Assert.AreEqual(6, model.NumElements);
            controller.Verify(c => c.RaiseEvent(libraryDataUpdated, It.IsAny<object[]>()), Times.Once);
        }

        [Test]
        [Category("UnitTests")]
        public void SimpleEventObserver()
        {
            const string disposed = "Disposed";
            var controller = new Mock<IEventController>();
            using (var observer = new EventObserver<int, bool>(
                    x => controller.Object.RaiseEvent(EventX, x),
                    x => x % 2 == 0
                ))
            {

                observer.Disposed += () => controller.Object.RaiseEvent(disposed);
                var list = Enumerable.Range(1, 10).ToList();
                list.ForEach(x => observer.OnEvent(x)); //notify OnEvent
                controller.Verify(c => c.RaiseEvent(EventX, It.IsAny<bool>()), Times.Exactly(10));
                controller.Verify(c => c.RaiseEvent(EventX, true), Times.Exactly(5));
                controller.Verify(c => c.RaiseEvent(EventX, false), Times.Exactly(5));

                //Dispose is not yet called
                controller.Verify(c => c.RaiseEvent(disposed), Times.Never);
            }
            controller.Verify(c => c.RaiseEvent(disposed), Times.Once); //must be called once
        }

        [Test]
        [Category("UnitTests")]
        public void EvenNumberEventObserver()
        {
            const string EvenNumber = "Even Number";
            var controller = new Mock<IEventController>();
            int number = -1;
            var observer = new EventObserver<int, bool>(
                    x => {
                        if (x)
                            controller.Object.RaiseEvent(EvenNumber, number);
                    },
                    x => { number = x;  return number % 2 == 0; }
                );

            var list = Enumerable.Range(1, 10).ToList();
            list.ForEach(x => observer.OnEvent(x)); //notify OnEvent
            controller.Verify(c => c.RaiseEvent(It.IsAny<string>(), It.IsAny<object[]>()), Times.Exactly(5));
            controller.Verify(c => c.RaiseEvent(EvenNumber, 2), Times.Once);
            controller.Verify(c => c.RaiseEvent(EvenNumber, 4), Times.Once);
            controller.Verify(c => c.RaiseEvent(EvenNumber, 6), Times.Once);
            controller.Verify(c => c.RaiseEvent(EvenNumber, 8), Times.Once);
            controller.Verify(c => c.RaiseEvent(EvenNumber, 10), Times.Once);
        }

        [Test]
        [Category("UnitTests")]
        public void ThrottleAggregateEventObserver()
        {
            var timeout = 500;
            var resetevent = new AutoResetEvent(false);

            object[] objexts = { 0 };
            var controller = new Mock<IEventController>();
            controller.Setup(c => c.RaiseEvent(It.IsAny<string>(), It.IsAny<object[]>())).Callback<string, object[]>((s, x) => { objexts = x; resetevent.Set(); });

            var observer = new EventObserver<int, List<int>>(
                    x => controller.Object.RaiseEvent(EventX, x),
                    (x, y) => {
                        if (x == null) return new List<int>() { y};
                        x.Add(y);
                        return x;
                    }
                ).Throttle(TimeSpan.FromMilliseconds(timeout));

            var list = Enumerable.Range(1, 10).ToList();
            list.ForEach(x => observer.OnEvent(x)); //notify OnEvent

            resetevent.WaitOne(timeout * 3);
            controller.Verify(c => c.RaiseEvent(EventX, It.IsAny<object[]>()), Times.Once);
            Assert.IsTrue(list.SequenceEqual(objexts[0] as IEnumerable<int>));
        }

        [Test]
        [Category("UnitTests")]
        public void ThrottleIdentityEventObserver()
        {
            var timeout = 50;
            var resetevent = new AutoResetEvent(false);

            var controller = new Mock<IEventController>();
            controller.Setup(c => c.RaiseEvent(It.IsAny<string>(), It.IsAny<object[]>())).Callback(() => resetevent.Set());

            var observer = new EventObserver<int, int>(
                    x => controller.Object.RaiseEvent(EventX, x),
                    EventObserver<int, int>.Identity
                ).Throttle(TimeSpan.FromMilliseconds(timeout));

            var list = Enumerable.Range(1, 10).ToList();
            list.ForEach(x => observer.OnEvent(x)); //notify OnEvent

            resetevent.WaitOne(timeout*3);
            controller.Verify(c => c.RaiseEvent(EventX, It.IsAny<int>()), Times.Once);
            controller.Verify(c => c.RaiseEvent(EventX, list.Last()), Times.Once);
        }

        [Test]
        [Category("UnitTests")]
        public void ParallelEventObserver()
        {
            var resetevent = new AutoResetEvent(false);

            var controller = new Mock<IEventController>();
            var observer = new EventObserver<int, int>(
                    x => controller.Object.RaiseEvent(EventX, x),
                    (x, y) => x + y
                ).Throttle(TimeSpan.FromMilliseconds(10));

            var list = Enumerable.Range(1, 10);
            var result = Parallel.ForEach(list, x => observer.OnEvent(x));

            resetevent.WaitOne(250);
            Assert.IsTrue(result.IsCompleted);
            controller.Verify(c => c.RaiseEvent(EventX, It.IsAny<int>()), Times.Once);
            controller.Verify(c => c.RaiseEvent(EventX, 55), Times.Once);
        }

        [Test]
        [Category("UnitTests")]
        public void RefireThrottledEvents()
        {
            var resetevent = new AutoResetEvent(false);

            var controller = new Mock<IEventController>();
            var observer = new EventObserver<int, int>(
                    x => controller.Object.RaiseEvent(EventX, x),
                    (x, y) => x + y
                ).Throttle(TimeSpan.FromMilliseconds(10));

            var list = Enumerable.Range(1, 10);
            var result = Parallel.ForEach(list, x => observer.OnEvent(x));

            resetevent.WaitOne(250);
            Assert.IsTrue(result.IsCompleted);
            controller.Verify(c => c.RaiseEvent(EventX, It.IsAny<int>()), Times.Once);
            controller.Verify(c => c.RaiseEvent(EventX, list.Sum()), Times.Once);

            var list2 = Enumerable.Range(11, 10); //different range of values
            result = Parallel.ForEach(list2, x => observer.OnEvent(x));
            resetevent.WaitOne(250);
            Assert.IsTrue(result.IsCompleted);
            controller.Verify(c => c.RaiseEvent(EventX, It.IsAny<int>()), Times.Exactly(2));
            controller.Verify(c => c.RaiseEvent(EventX, list2.Sum()), Times.Once); //doesn't contain old values
        }

        [Test, Category("UnitTests")]
        public void AnonymousDisposable()
        {
            var controller = new Mock<IEventController>();
            var disposable = new AnonymousDisposable(() => controller.Object.RaiseEvent("Disposed"));
            disposable.Dispose();
            disposable.Dispose();
            disposable.Dispose();
            controller.Verify(c => c.RaiseEvent("Disposed"), Times.Once);
        }

        [Test, Category("UnitTests")]
        public void ConcurrentIconRequest()
        {
            var resetevent = new AutoResetEvent(false);
            var requests = (new[] { "A", "B", "C", "D", "E" })
                .Select(s => new IconUrl(s, s))
                .Select(icon => {
                    var req = new Mock<IRequest>();
                    req.Setup(r => r.Url).Returns(icon.Url);
                    return req;
                }).ToList();

            var pathmanager = new Mock<IPathManager>();
            var provider = new IconResourceProvider(pathmanager.Object);
            string ext;
            var result = Parallel.ForEach(requests, r => Assert.IsNotNull(provider.GetResource(r.Object, out ext)));

            resetevent.WaitOne(250);
            Assert.IsTrue(result.IsCompleted);
        }

        private static Mock<NodeSearchElement> MockNodeSearchElement(string fullname, string creationName)
        {
            var moq = new Mock<NodeSearchElement>() { CallBase = true };
            moq.Setup(e => e.FullName).Returns(fullname);
            moq.Setup(e => e.CreationName).Returns(creationName);
            return moq;
        }
    }
}
