using System;
using System.Linq;
using System.Xml;
using Dynamo;
using Dynamo.Configuration;
using Dynamo.Core;
using Dynamo.Engine;
using Dynamo.Search;
using Dynamo.Search.SearchElements;
using Dynamo.ViewModels;
using NUnit.Framework;
using ProtoCore;
using TestServices;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    class LibraryTests : UnitTestBase
    {
        private LibraryServices libraryServices;
        private ProtoCore.Core libraryCore;

        protected static bool LibraryLoaded { get; set; }

        public override void Setup()
        {
            base.Setup();

            libraryCore = new ProtoCore.Core(new Options());
            libraryCore.Compilers.Add(Language.Associative, new ProtoAssociative.Compiler(libraryCore));
            libraryCore.Compilers.Add(Language.Imperative, new ProtoImperative.Compiler(libraryCore));
            libraryCore.ParsingMode = ParseMode.AllowNonAssignment;

            var pathResolver = new TestPathResolver();
            pathResolver.AddPreloadLibraryPath("DSCoreNodes.dll");

            var pathManager = new PathManager(new PathManagerParams
            {
                PathResolver = pathResolver
            });

            var settings = new PreferenceSettings();

            libraryServices = new LibraryServices(libraryCore, pathManager, settings);

            RegisterEvents();
        }

        public override void Cleanup()
        {
            UnRegisterEvents();
            libraryServices.Dispose();
            base.Cleanup();
        }

        private void RegisterEvents()
        {
            libraryServices.LibraryLoaded += OnLibraryLoaded;
            libraryServices.LibraryLoadFailed += OnLibraryLoadFailed;
        }

        private void UnRegisterEvents()
        {
            libraryServices.LibraryLoaded -= OnLibraryLoaded;
            libraryServices.LibraryLoadFailed -= OnLibraryLoadFailed;
        }

        public static void OnLibraryLoaded(object sender, EventArgs e)
        {
            LibraryLoaded = true;
        }

        public static void OnLibraryLoadFailed(object sender, EventArgs e)
        {
            LibraryServices.LibraryLoadFailedEventArgs a = e as LibraryServices.LibraryLoadFailedEventArgs;
            if (null != a)
                Assert.Fail("Failed to load library: " + a.LibraryPath);
            else
                Assert.Fail("Failed to load library");
        }

        [Test]
        [Category("UnitTests")]
        public void DumpLibraryToXmlZeroTouchTest()
        {
            var searchViewModel = new SearchViewModel(new NodeSearchModel());

            LibraryLoaded = false;

            string libraryPath = "DSOffice.dll";

            // All we need to do here is to ensure that the target has been loaded
            // at some point, so if it's already here, don't try and reload it
            if (!libraryServices.IsLibraryLoaded(libraryPath))
            {
                libraryServices.ImportLibrary(libraryPath);
                Assert.IsTrue(LibraryLoaded);
            }

            var fgToCompare = libraryServices.GetFunctionGroups(libraryPath);
            foreach (var funcGroup in fgToCompare)
            {
                foreach (var functionDescriptor in funcGroup.Functions)
                {
                    if (functionDescriptor.IsVisibleInLibrary && !functionDescriptor.DisplayName.Contains("GetType"))
                    {
                        searchViewModel.Model.Add(new ZeroTouchSearchElement(functionDescriptor));
                    }
                }
            }

            var document = searchViewModel.Model.ComposeXmlForLibrary(ExecutingDirectory);

            Assert.AreEqual("LibraryTree", document.DocumentElement.Name);

            XmlNode node, subNode;
            foreach (var functionGroup in fgToCompare)
            {
                foreach (var function in functionGroup.Functions)
                {
                    // Obsolete, not visible and "GetType" functions are not presented in UI tree.
                    // So they are should not be presented in dump.
                    if (function.IsObsolete || !function.IsVisibleInLibrary || function.FunctionName.Contains("GetType"))
                        continue;

                    var category = function.Category;
                    var group = SearchElementGroup.Action;
                    category = searchViewModel.Model.ProcessNodeCategory(category, ref group);

                    node = document.SelectSingleNode(string.Format(
                        "//{0}[FullCategoryName='{1}' and Name='{2}']",
                        typeof(ZeroTouchSearchElement).FullName, category, function.FunctionName));
                    Assert.IsNotNull(node);

                    subNode = node.SelectSingleNode("Group");
                    Assert.IsNotNull(subNode.FirstChild);
                    Assert.AreEqual(group.ToString(), subNode.FirstChild.Value);

                    // 'FullCategoryName' is already checked.
                    // 'Name' is already checked.

                    subNode = node.SelectSingleNode("Description");
                    Assert.IsNotNull(subNode.FirstChild);

                    // No check Description on text equality because for some reason 
                    // function.Descriptions are different in real executing Dynamo and
                    // Dynamo started from tests.
                    //
                    // For example: 
                    // tests  function.Description: Excel.ReadFromFile (file: FileInfo, sheetName: string): var[][]
                    // normal function.Description: Excel.ReadFromFile (file: var, sheetName: string): var[][]
                }
            }
        }

        [Test]
        public void SearchHiddenInterfaceNodeTest()
        {
            var searchViewModel = new SearchViewModel(new NodeSearchModel());

            LibraryLoaded = false;

            string libraryPath = "FFITarget.dll";

            // All we need to do here is to ensure that the target has been loaded
            // at some point, so if it's already here, don't try and reload it
            if (!libraryServices.IsLibraryLoaded(libraryPath))
            {
                libraryServices.ImportLibrary(libraryPath);
                Assert.IsTrue(LibraryLoaded);
            }

            var fgToCompare = libraryServices.GetFunctionGroups(libraryPath);
            foreach (var funcGroup in fgToCompare)
            {
                foreach (var functionDescriptor in funcGroup.Functions)
                {
                    if (functionDescriptor.IsVisibleInLibrary && !functionDescriptor.DisplayName.Contains("GetType"))
                    {
                        searchViewModel.Model.Add(new ZeroTouchSearchElement(functionDescriptor));
                    }
                }
            }

            var searchString = "InterfaceA";
            var nodes = searchViewModel.Search(searchString);
            var foundNodes = nodes.Where(n => n.Class.Equals(searchString));
            Assert.IsFalse(foundNodes.Any());

            searchString = "DerivedFromInterfaceA";
            nodes = searchViewModel.Search(searchString);
            foundNodes = nodes.Where(n => n.Class.Equals(searchString));
            Assert.AreEqual(2, foundNodes.Count());

            searchString = "TraceableId";
            nodes = searchViewModel.Search(searchString);
            foundNodes = nodes.Where(n => n.Class.Equals(searchString));
            Assert.IsFalse(foundNodes.Any());

            searchString = "ISerializable";
            nodes = searchViewModel.Search(searchString);
            foundNodes = nodes.Where(n => n.Class.Equals(searchString));
            Assert.IsFalse(foundNodes.Any());
        }

        [Test]
        public void SearchHiddenEnumTest()
        {
            var searchViewModel = new SearchViewModel(new NodeSearchModel());

            LibraryLoaded = false;

            string libraryPath = "FFITarget.dll";

            // All we need to do here is to ensure that the target has been loaded
            // at some point, so if it's already here, don't try and reload it
            if (!libraryServices.IsLibraryLoaded(libraryPath))
            {
                libraryServices.ImportLibrary(libraryPath);
                Assert.IsTrue(LibraryLoaded);
            }

            var fgToCompare = libraryServices.GetFunctionGroups(libraryPath);
            foreach (var funcGroup in fgToCompare)
            {
                foreach (var functionDescriptor in funcGroup.Functions)
                {
                    if (functionDescriptor.IsVisibleInLibrary && !functionDescriptor.DisplayName.Contains("GetType"))
                    {
                        searchViewModel.Model.Add(new ZeroTouchSearchElement(functionDescriptor));
                    }
                }
            }

            var searchString = "Days";
            var nodes = searchViewModel.Search(searchString);
            var foundNodes = nodes.Where(n => n.Class.Equals(searchString));
            Assert.IsFalse(foundNodes.Any());

            searchString = "Sunday";
            nodes = searchViewModel.Search(searchString);
            foundNodes = nodes.Where(n => n.Class.Equals(searchString));
            Assert.IsFalse(foundNodes.Any());

            searchString = "Tuesday";
            nodes = searchViewModel.Search(searchString);
            foundNodes = nodes.Where(n => n.Class.Equals(searchString));
            Assert.IsFalse(foundNodes.Any());
        }
    }
}
