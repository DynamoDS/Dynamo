using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dynamo.DSEngine;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [Category("DSExecution")]
    class DSLibraryTest : DSEvaluationUnitTest
    {
        [SetUp]
        public override void Init()
        {
            base.Init();
        }

        [TearDown]
        public override void Cleanup()
        {
            LibraryServices.GetInstance().Reset();
            base.Cleanup();
        }

        [Test]
        public void TestPreLoadedLibrary()
        {
            LibraryServices libraryServices = LibraryServices.GetInstance();
            var loadedLibs = libraryServices.Libraries;
            List<string> libs = libraryServices.Libraries.Select(
                lib => {return Path.GetFileName(lib);}).ToList();

            foreach (var lib in loadedLibs)
            {
               Assert.IsTrue(libs.Any(x => string.Compare(x, lib, true) == 0)); 
            }
        }

        [Test]
        public void TestLoadLibrary()
        {
            LibraryServices libraryServices = LibraryServices.GetInstance();
            bool libraryLoaded = false;

            libraryServices.LibraryLoaded += (sender, e) => libraryLoaded = true;
            libraryServices.LibraryLoadFailed += (sender, e) => Assert.Fail("Failed to load library: " + e.LibraryPath);
            
            string libraryPath = Path.Combine(GetTestDirectory(), @"core\library\MultiReturnTest.dll");
            libraryServices.ImportLibrary(libraryPath);
            Assert.IsTrue(libraryLoaded);

            var functions = libraryServices.GetFunctionGroups(libraryPath);
            Assert.IsNotNull(functions);
            Assert.IsTrue(functions.Any());
        }

        [Test]
        public void TestLoadDSFile()
        {
            LibraryServices libraryServices = LibraryServices.GetInstance();
            bool libraryLoaded = false;

            libraryServices.LibraryLoaded += (sender, e) => libraryLoaded = true;
            libraryServices.LibraryLoadFailed += (sender, e) => Assert.Fail("Failed to load library: " + e.LibraryPath); 

            string libraryPath = Path.Combine(GetTestDirectory(), @"core\library\Dummy.ds");
            libraryServices.ImportLibrary(libraryPath);
            Assert.IsTrue(libraryLoaded);

            var functions = libraryServices.GetFunctionGroups(libraryPath);
            Assert.IsNotNull(functions);
            Assert.IsTrue(functions.Any());
        }

        [Test]
        public void TestLibraryAcrossSessions()
        {
            LibraryServices libraryServices = LibraryServices.GetInstance();

            bool libraryLoaded = false;
            libraryServices.LibraryLoaded += (sender, e) => libraryLoaded = true;

            // library should be able to load
            string libraryPath = Path.Combine(GetTestDirectory(), @"core\library\Test.ds");
            libraryServices.ImportLibrary(libraryPath);
            Assert.IsTrue(libraryLoaded);

            // open dyn file which uses node in that library
            RunModel(@"core\library\t1.dyn");
            AssertValue("a", 1025);

            // open the other dyn file which uses node in that library, and
            // library should still be available
            RunModel(@"core\library\t2.dyn");
            AssertValue("a", 43);
        }
    }
}
