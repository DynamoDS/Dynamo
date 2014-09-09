using Dynamo.DSEngine;
using Dynamo.Tests;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Dynamo.Tests
{
    class LibraryTests : DynamoViewModelUnitTest
    {
        [SetUp]
        public override void Init()
        {
            base.Init();
        }

        [Test]
        [Category("UnitTests")]
        public void TestLoadNoNamespaceClass()
        {
            LibraryServices libraryServices = LibraryServices.GetInstance();
            bool libraryLoaded = false;

            libraryServices.LibraryLoaded += (sender, e) => libraryLoaded = true;
            libraryServices.LibraryLoadFailed += (sender, e) => Assert.Fail("Failed to load library: " + e.LibraryPath);

            string libraryPath = "FFITarget.dll";
            libraryServices.ImportLibrary(libraryPath, ViewModel.Model.Logger);
            Assert.IsTrue(libraryLoaded);

            // Get function groups for global classes with no namespace
            var functions = libraryServices.GetFunctionGroups(libraryPath)
                                            .Where(x => x.Functions
                                            .Where(y => string.IsNullOrEmpty(y.Namespace)).Any());

            if (functions.Any())
            {
                var ctorfg = functions.Where(x => x.Functions.Where(y => y.Type == FunctionType.Constructor).Any());
                Assert.IsTrue(ctorfg.Any());
                foreach(var fg in ctorfg)
                {
                    foreach(var ctor in fg.Functions)
                        Assert.IsTrue(ctor.ClassName == ctor.UnqualifedClassName);   
                }
                
            }
        }
    }
}
