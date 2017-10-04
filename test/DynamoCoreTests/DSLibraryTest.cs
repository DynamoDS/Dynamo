using Dynamo.Engine;
using Dynamo.Exceptions;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Dynamo.Tests
{
    [Category("DSExecution")]
    class DSLibraryTest : DynamoModelTestBase
    {
        private LibraryServices libraryServices;

        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("Builtin.dll");
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

        public override void Setup()
        {
            base.Setup();
            libraryServices = CurrentDynamoModel.LibraryServices;
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
            bool libraryLoaded = false;

            libraryServices.LibraryLoaded += (sender, e) => libraryLoaded = true;
            libraryServices.LibraryLoadFailed += (sender, e) => Assert.Fail("Failed to load library: " + e.LibraryPath);

            string libraryPath = Path.Combine(TestDirectory, @"core\library\Dummy.ds");
            libraryServices.ImportLibrary(libraryPath);
            Assert.IsTrue(libraryLoaded);

            var functions = libraryServices.GetFunctionGroups(libraryPath);
            Assert.IsNotNull(functions);
            Assert.IsTrue(functions.Any());
        }

        [Test]
        [Category("UnitTests")]
        public void TestLoadDllFileFailure()
        {
            bool libraryLoaded = false;

            libraryServices.LibraryLoaded += (sender, e) => libraryLoaded = true;

            string libraryPath = Path.Combine(TestDirectory, @"core\library\Dummy.dll");
            try
            {
                libraryServices.ImportLibrary(libraryPath);

            }
            catch( System.Exception ex)
            {
                Assert.IsTrue(ex is LibraryLoadFailedException);
            }
            Assert.IsFalse(libraryLoaded);
        }

        [Test]
        [Category("UnitTests")]
        public void TestLibraryAcrossSessions()
        {
            bool libraryLoaded = false;
            libraryServices.LibraryLoaded += (sender, e) => libraryLoaded = true;

            // library should be able to load
            string libraryPath = Path.Combine(TestDirectory, @"core\library\Test.ds");
            libraryServices.ImportLibrary(libraryPath);
            Assert.IsTrue(libraryLoaded);

            // open dyn file which uses node in that library
            RunModel(@"core\library\t1.dyn");
            AssertPreviewValue("2cacc70a-23a8-4fe0-92d1-9b72ae3db10b", 1025);

            // open the other dyn file which uses node in that library, and
            // library should still be available
            RunModel(@"core\library\t2.dyn");
            AssertPreviewValue("880ea294-7a01-4a78-8602-54d73f4b681b", 43);
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
    }
}
