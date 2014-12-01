using Dynamo.DSEngine;
using Dynamo.Nodes;
using Dynamo.Tests;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            var nodeGuid = Guid.NewGuid();
            var command = new DynCmd.CreateNodeCommand(
                nodeGuid, "Code Block", 0, 0, true, false);

            ViewModel.ExecuteCommand(command);
            var workspace = ViewModel.Model.CurrentWorkspace;
            var cbn = workspace.NodeFromWorkspace<CodeBlockNodeModel>(nodeGuid);

            Assert.IsNotNull(cbn);
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
                libraryServices.ImportLibrary(libraryPath, ViewModel.Model.Logger);
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

            libraryServices.ImportLibrary(tempLibraryPath, ViewModel.Model.Logger);

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

            libraryServices.ImportLibrary(tempLibraryPath, ViewModel.Model.Logger);

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

            libraryServices.ImportLibrary(tempLibraryPath, ViewModel.Model.Logger);

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
                libraryServices.ImportLibrary(libraryPath, ViewModel.Model.Logger);
                Assert.IsTrue(LibraryLoaded);
            }

            // Get function groups for ClassFunctionality Class
            var functions = libraryServices.GetFunctionGroups(libraryPath)
                                            .SelectMany(x => x.Functions)
                                            .Where(y => y.ClassName.Contains("FFITarget.ClassWithRefParams"));

            Assert.IsTrue(functions.Select(x => x.Name).Contains("ClassWithRefParams"));

            foreach (var function in functions)
            {
                string functionName = function.Name;
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
            Assert.AreEqual(1, model.Nodes.Count());

            // Run 
            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());
            
            string libraryPath = "FFITarget.dll";

            var libraryServices = ViewModel.Model.EngineController.LibraryServices;

            // All we need to do here is to ensure that the target has been loaded
            // at some point, so if it's already thre, don't try and reload it
            if (!libraryServices.IsLibraryLoaded(libraryPath))
            {
                libraryServices.ImportLibrary(libraryPath, ViewModel.Model.Logger);
            }

            // Assert that once a library with classname conflicts is loaded the CBN
            // displays the warning
            Assert.IsTrue(codeBlockNodeOne.ToolTipText.Contains(string.Format(
                ProtoCore.BuildData.WarningMessage.kMultipleSymbolFoundFromName, "Point", "")));
        }
        #endregion
    }
}
