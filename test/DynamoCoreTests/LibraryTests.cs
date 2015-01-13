using System;
using System.IO;
using System.Linq;
using System.Xml;
using Dynamo.DSEngine;
using Dynamo.Nodes;
using Dynamo.Search;
using Dynamo.Search.SearchElements;
using NUnit.Framework;
using DynCmd = Dynamo.Models.DynamoModel;

namespace Dynamo.Tests
{
    [TestFixture]
    class LibraryTests : DSEvaluationViewModelUnitTest
    {
        protected static bool LibraryLoaded { get; set; }

        [SetUp]
        public override void Init()
        {
            base.Init();
            RegisterEvents();
        }

        [TearDown]
        public override void Cleanup()
        {
            UnRegisterEvents();
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

        private CodeBlockNodeModel CreateCodeBlockNode()
        {
            var cbn = new CodeBlockNodeModel(ViewModel.Model.LibraryServices);

            var command = new DynCmd.CreateNodeCommand(cbn, 0, 0, true, false);
            ViewModel.ExecuteCommand(command);

            return cbn;
        }

        private void UpdateCodeBlockNodeContent(CodeBlockNodeModel cbn, string value)
        {
            var command = new DynCmd.UpdateModelValueCommand(cbn.GUID, "Code", value);
            ViewModel.ExecuteCommand(command);
        }

        #region Test cases
        [Test]
        [Category("UnitTests")]
        public void TestLoadNoNamespaceClass()
        {
            LibraryLoaded = false;

            string libraryPath = "FFITarget.dll";

            // All we need to do here is to ensure that the target has been loaded
            // at some point, so if it's already thre, don't try and reload it
            if (!libraryServices.IsLibraryLoaded(libraryPath))
            {
                libraryServices.ImportLibrary(libraryPath);
                Assert.IsTrue(LibraryLoaded);
            }

            // Get function groups for global classes with no namespace
            var functions = libraryServices.GetFunctionGroups(libraryPath)
                                            .Where(x => x.Functions
                                            .Where(y => string.IsNullOrEmpty(y.Namespace)).Any());

            if (functions.Any())
            {
                var ctorfg = functions.Where(x => x.Functions.Where(y => y.Type == FunctionType.Constructor).Any());
                Assert.IsTrue(ctorfg.Any());
                foreach (var fg in ctorfg)
                {
                    foreach (var ctor in fg.Functions)
                        Assert.IsTrue(ctor.ClassName == ctor.UnqualifedClassName);
                }

            }
        }

        [Test]
        [Category("UnitTests")]
        public void TestZeroTouchMigrationNoFileFound()
        {
            LibraryLoaded = false;

            string libraryPath = "FFITarget.dll";

            string tempPath = Path.GetTempPath();
            var uniqueDirectory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
            var tempDirectory = uniqueDirectory.FullName;
            string tempLibraryPath = Path.Combine(tempDirectory, libraryPath);

            File.Copy(libraryPath, tempLibraryPath);

            libraryServices.ImportLibrary(tempLibraryPath);

            Assert.IsTrue(LibraryLoaded);
        }

        [Test]
        [Category("UnitTests")]
        public void TestZeroTouchMigrationFileBadlyFormatted()
        {
            LibraryLoaded = false;

            string libraryPath = "FFITarget.dll";
            string badXMLPath = "FFITarget.Migrations.xml";

            string badXmlText = "<?xml version=\"1.0\"?>" + System.Environment.NewLine +
                "<migrations>" + System.Environment.NewLine +
                "<priorNameHint>" + System.Environment.NewLine +
                "Because I'm bad, I'm bad, Really, Really Bad." + System.Environment.NewLine +
                "</priorNameHint>" + System.Environment.NewLine +
                "</migrations>" + System.Environment.NewLine;

            string tempPath = Path.GetTempPath();
            var uniqueDirectory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
            var tempDirectory = uniqueDirectory.FullName;
            string tempLibraryPath = Path.Combine(tempDirectory, libraryPath);
            string tempBadXMLPath = Path.Combine(tempDirectory, badXMLPath);

            File.Copy(libraryPath, tempLibraryPath);

            System.IO.File.WriteAllText(tempBadXMLPath, badXmlText);

            // The proper behavior is for ImportLibrary to ignore the migrations file if it's badly formatted

            libraryServices.ImportLibrary(tempLibraryPath);

            Assert.IsTrue(LibraryLoaded);
        }

        [Test]
        [Category("UnitTests")]
        public void TestZeroTouchMigrationCannotFindOldMethodName()
        {
            LibraryLoaded = false;

            string libraryPath = "FFITarget.dll";
            string badXMLPath = "FFITarget.Migrations.xml";

            string badXmlText = "<?xml version=\"1.0\"?>" + System.Environment.NewLine +
                "<migrations>" + System.Environment.NewLine +
                "<priorNameHint>" + System.Environment.NewLine +
                "<oldName>I.Am.A.Method.That.Does.Not.Exist</oldName>" + System.Environment.NewLine +
                "<newName>FFITarget.Dummy.TwiceNewName</newName>" + System.Environment.NewLine +
                "</priorNameHint>" + System.Environment.NewLine +
                "</migrations>" + System.Environment.NewLine;

            string tempPath = Path.GetTempPath();
            var uniqueDirectory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
            var tempDirectory = uniqueDirectory.FullName;
            string tempLibraryPath = Path.Combine(tempDirectory, libraryPath);
            string tempBadXMLPath = Path.Combine(tempDirectory, badXMLPath);

            File.Copy(libraryPath, tempLibraryPath);

            System.IO.File.WriteAllText(tempBadXMLPath, badXmlText);

            // The proper behavior is for ImportLibrary to ignore the migrations file if it has errors

            libraryServices.ImportLibrary(tempLibraryPath);

            Assert.IsTrue(LibraryLoaded);
        }

        [Test]
        [Category("UnitTests")]
        public void MethodWithRefOutParams_NoLoad()
        {
            LibraryLoaded = false;

            string libraryPath = "FFITarget.dll";

            // All we need to do here is to ensure that the target has been loaded
            // at some point, so if it's already thre, don't try and reload it
            if (!libraryServices.IsLibraryLoaded(libraryPath))
            {
                libraryServices.ImportLibrary(libraryPath);
                Assert.IsTrue(LibraryLoaded);
            }

            // Get function groups for ClassFunctionality Class
            var functions = libraryServices.GetFunctionGroups(libraryPath)
                                            .SelectMany(x => x.Functions)
                                            .Where(y => y.ClassName.Contains("FFITarget.ClassWithRefParams"));

            Assert.IsTrue(functions.Select(x => x.FunctionName).Contains("ClassWithRefParams"));

            foreach (var function in functions)
            {
                string functionName = function.FunctionName;
                Assert.IsTrue(functionName != "MethodWithRefParameter" && functionName != "MethodWithOutParameter" && functionName != "MethodWithRefOutParameters");
            }
        }

        [Test]
        [Category("UnitTests")]
        public void LibraryLoaded_PrecompileCBN_ShowConflictWarnings()
        {
            var model = ViewModel.Model;

            // Create the initial code block node.
            var codeBlockNodeOne = CreateCodeBlockNode();
            UpdateCodeBlockNodeContent(codeBlockNodeOne, "Point.ByCoordinates();");

            // We should have one code block node by now.
            Assert.AreEqual(1, model.CurrentWorkspace.Nodes.Count());

            // Run 
            Assert.DoesNotThrow(() => ViewModel.HomeSpace.Run());

            string libraryPath = "FFITarget.dll";

            var libraryServices = ViewModel.Model.EngineController.LibraryServices;

            // All we need to do here is to ensure that the target has been loaded
            // at some point, so if it's already thre, don't try and reload it
            if (!libraryServices.IsLibraryLoaded(libraryPath))
            {
                libraryServices.ImportLibrary(libraryPath);
            }

            // Assert that once a library with classname conflicts is loaded the CBN
            // displays the warning
            Assert.IsTrue(codeBlockNodeOne.ToolTipText.Contains(string.Format(
                ProtoCore.StringConstants.kMultipleSymbolFoundFromName, "Point", "")));
        }

        [Test]
        [Category("UnitTests")]
        public void DumpLibraryToXmlZeroTouchTest()
        {
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

            var document = ViewModel.SearchViewModel.Model.ComposeXmlForLibrary();

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

                    node = document.SelectSingleNode(string.Format(
                        "//{0}[FullCategoryName='{1}' and Name='{2}']", 
                        typeof(ZeroTouchSearchElement).FullName,
                        function.Category, function.FunctionName));
                    Assert.IsNotNull(node);

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

        #endregion
    }
}
