using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dynamo.Configuration;
using Dynamo.Engine;
using Dynamo.Exceptions;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [Category("DSExecution")]
    class DSLibraryTest : DynamoModelTestBase
    {
        private LibraryServices libraryServices;

        protected static bool LibraryLoaded { get; set; }

        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

        public override void Setup()
        {
            base.Setup();
            libraryServices = CurrentDynamoModel.LibraryServices;
            RegisterEvents();
        }

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

        [Test]
        [Category("UnitTests")]
        public void TestPreLoadedLibrary()
        {
            var loadedLibs = libraryServices.ImportedLibraries;
            Assert.IsTrue(loadedLibs.Any());
        }

        [Test]
        [Category("UnitTests")]
        public void TestLoadDSFile()
        {
            LibraryLoaded = false;

            string libraryPath = Path.Combine(TestDirectory, @"core\library\Dummy.ds");
            libraryServices.ImportLibrary(libraryPath);
            Assert.IsTrue(LibraryLoaded);

            var functions = libraryServices.GetFunctionGroups(libraryPath);
            Assert.IsNotNull(functions);
            Assert.IsTrue(functions.Any());
        }

        [Test]
        [Category("UnitTests")]
        public void TestLoadDllFileFailure()
        {
            LibraryLoaded = false;

            string libraryPath = Path.Combine(TestDirectory, @"core\library\Dummy.dll");
            try
            {
                libraryServices.ImportLibrary(libraryPath);
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex is LibraryLoadFailedException);
            }
            Assert.IsFalse(LibraryLoaded);
        }

        [Test]
        [Category("UnitTests")]
        public void TestLoadDllFileSuccess()
        {
            LibraryLoaded = false;

            string libraryPath = Path.Combine(TestDirectory, @"FFITarget.dll");
            try
            {
                libraryServices.ImportLibrary(libraryPath);
            }
            catch(Exception ex)
            {
                Assert.IsTrue(ex is LibraryLoadFailedException);
            }
            Assert.IsTrue(LibraryLoaded);
        }

        [Test]
        [Category("UnitTests")]
        public void TestLibraryAcrossSessions()
        {
            LibraryLoaded = false;

            // library should be able to load
            string libraryPath = Path.Combine(TestDirectory, @"core\library\Test.ds");
            libraryServices.ImportLibrary(libraryPath);
            Assert.IsTrue(LibraryLoaded);

            // open dyn file which uses node in that library
            RunModel(@"core\library\t1.dyn");
            AssertPreviewValue("2cacc70a-23a8-4fe0-92d1-9b72ae3db10b", 1025);

            // open the other dyn file which uses node in that library, and
            // library should still be available
            RunModel(@"core\library\t2.dyn");
            AssertPreviewValue("880ea294-7a01-4a78-8602-54d73f4b681b", 43);
        }

        [Test]
        [Category("UnitTests")]
        public void TestDllLibraryAcrossSessions()
        {
            LibraryLoaded = false;

            // Library should be able to load
            string libraryPath = Path.Combine(TestDirectory, @"FFITarget.dll");
            libraryServices.ImportLibrary(libraryPath);
            Assert.IsTrue(LibraryLoaded);

            // Open dyn file which uses node in that library
            RunModel(@"core\library\t1dll.dyn");
            AssertNoDummyNodes();

            // Open the other dyn file which uses node in that library, and
            // library should still be available
            RunModel(@"core\library\t2dll.dyn");
            AssertNoDummyNodes();
        }

        [Test]
        [Category("UnitTests")]
        public void TestDllLibraryLoadAtStartup()
        {
            // Get the default custom package folders
            List<string> initialCustomPackageFolders = CurrentDynamoModel.PreferenceSettings.CustomPackageFolders;

            // Shutdown the current Dynamo model
            CurrentDynamoModel.ShutDown(false);
            CurrentDynamoModel = null;

            // Create new default preferences with the required custom package folders
            PreferenceSettings preferenceSettings = new PreferenceSettings();
            preferenceSettings.CustomPackageFolders = initialCustomPackageFolders;
            string libraryPath = Path.Combine(TestDirectory, @"FFITarget.dll");
            preferenceSettings.CustomPackageFolders.Add(libraryPath);

            // Resart Dynamo using the new preferences
            StartDynamo(preferenceSettings);

            // Open the dyn file which uses node in that library which should be available
            RunModel(@"core\library\t1dll.dyn");
            AssertNoDummyNodes();
        }

        [Test]
        public void TestOverloadedMethodsWithDifferentPrimitiveType()
        {
            RunModel(@"core\library\PrimitiveType.dyn");

            AssertPreviewValue("fda58ebb-d3d8-46d1-9851-49a2b3235128", 1);
            AssertPreviewValue("ef093169-6a45-4346-b361-1905c7b3a79c", 2);
        }

        [Test]
        public void TestOverloadedMethodsWithDifferentIEnumerableType()
        {
            RunModel(@"core\library\IEnumerableOfDifferentObjectType.dyn");
            AssertPreviewValue("0c9c34fa-236c-43c0-a5b1-44139c83cbb6", 3);
            AssertPreviewValue("3cb6f401-2811-4b66-9d46-83f2deb1dacb", 4);
        }

        [Test]
        [Category("UnitTests")]
        public void TestAddStandardLibraryPath()
        {
            // Get the default custom package folders
            List<string> customPackageFolders = CurrentDynamoModel.PreferenceSettings.CustomPackageFolders;

            // Test that the default number of folders is correct
            Assert.IsTrue(customPackageFolders.Count == 1);

            // Test that the path is added as expected
            CurrentDynamoModel.AddPackagePath(TestDirectory, "");
            Assert.IsTrue(customPackageFolders.Count == 2);

            // Test that the path is not duplicated
            CurrentDynamoModel.AddPackagePath(TestDirectory, "");
            Assert.IsTrue(customPackageFolders.Count == 2);
        }

        [Test]
        [Category("UnitTests")]
        public void TestAddDllLibraryPath()
        {
            // Get the default custom package folders
            List<string> customPackageFolders = CurrentDynamoModel.PreferenceSettings.CustomPackageFolders;

            // Test that the default number of folders is correct
            Assert.IsTrue(customPackageFolders.Count == 1);

            string filename = @"DLL.dll";
            string packagePath = Path.Combine(TestDirectory, @"pkgs\Custom Rounding\extra");
            string libraryPath = Path.Combine(packagePath, filename);

            // Test that the full file path is added as expected
            CurrentDynamoModel.AddPackagePath(packagePath, filename);
            Assert.IsTrue(customPackageFolders.Count == 2);
            int count = customPackageFolders.Where(s => s == libraryPath).Count();
            Assert.IsTrue(count == 1);

            // Test that the full file path is not duplicated
            CurrentDynamoModel.AddPackagePath(packagePath, filename);
            Assert.IsTrue(customPackageFolders.Count == 2);
            count = customPackageFolders.Where(s => s == libraryPath).Count();
            Assert.IsTrue(count == 1);
        }
    }
}
