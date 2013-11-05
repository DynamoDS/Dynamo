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
    class DSLibraryTest : DynamoUnitTest
    {
        [Test]
        public void TestPreLoadedLibrary()
        {
            List<string> loadedLibs = DSLibraryServices.Instance.PreLoadedLibraries;
            List<string> libs = DSLibraryServices.Instance.Libraries.Select(
                lib => {return Path.GetFileName(lib);}).ToList();

            foreach (var lib in loadedLibs)
            {
               Assert.IsTrue(libs.Any(x => string.Compare(x, lib, true) == 0)); 
            }
        }

        [Test]
        public void TestLoadLibrary()
        {
            EventHandler<Dynamo.DSEngine.DSLibraryServices.LibraryLoadFailedEventArgs> libraryLoadFailed =
                (sender, e) => Assert.Fail("Failed to load library: " + e.LibraryPath);

            DSLibraryServices.Instance.LibraryLoadFailed += libraryLoadFailed;
            string libraryPath = Path.Combine(GetTestDirectory(), @"core\library\MultiReturnTest.dll");
            DSLibraryServices.Instance.ImportLibrary(libraryPath);

            List<DSFunctionItem> functions = DSLibraryServices.Instance[libraryPath];
            Assert.IsNotNull(functions);
            Assert.IsTrue(functions.Count > 0);

            DSLibraryServices.Instance.LibraryLoadFailed -= libraryLoadFailed;
        }

        [Test]
        public void TestLoadDSFile()
        {
            EventHandler<DSLibraryServices.LibraryLoadFailedEventArgs> libraryLoadFailed =
                (sender, e) => Assert.Fail("Failed to load library: " + e.LibraryPath);

            DSLibraryServices.Instance.LibraryLoadFailed += libraryLoadFailed;
            string libraryPath = Path.Combine(GetTestDirectory(), @"core\library\Dummy.ds");
            DSLibraryServices.Instance.ImportLibrary(libraryPath);

            List<DSFunctionItem> functions = DSLibraryServices.Instance[libraryPath];
            Assert.IsNotNull(functions);
            Assert.IsTrue(functions.Count > 0);

            DSLibraryServices.Instance.LibraryLoadFailed -= libraryLoadFailed;
        }
    }
}
