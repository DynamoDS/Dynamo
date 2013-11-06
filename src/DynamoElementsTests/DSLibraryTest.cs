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
            LibraryServices libraryServices = new LibraryServices();
            List<string> loadedLibs = libraryServices.BuiltinLibraries;
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
            LibraryServices libraryServices = new LibraryServices();
            libraryServices.LibraryLoaded += (sender, e) => Assert.Fail("Failed to load library: " + e.LibraryPath);
            
            string libraryPath = Path.Combine(GetTestDirectory(), @"core\library\MultiReturnTest.dll");
            libraryServices.ImportLibrary(libraryPath);

            List<FunctionItem> functions = libraryServices[libraryPath];
            Assert.IsNotNull(functions);
            Assert.IsTrue(functions.Count > 0);
        }

        [Test]
        public void TestLoadDSFile()
        {
            LibraryServices libraryServices = new LibraryServices();
            libraryServices.LibraryLoadFailed += (sender, e) => Assert.Fail("Failed to load library: " + e.LibraryPath); 

            string libraryPath = Path.Combine(GetTestDirectory(), @"core\library\Dummy.ds");
            libraryServices.ImportLibrary(libraryPath);

            List<FunctionItem> functions = libraryServices[libraryPath];
            Assert.IsNotNull(functions);
            Assert.IsTrue(functions.Count > 0);
        }
    }
}
