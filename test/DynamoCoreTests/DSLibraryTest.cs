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
    class DSLibraryTest : DSEvaluationViewModelUnitTest
    {
        private LibraryServices libraryServices = null;
        private ProtoCore.Core libraryServicesCore = null;

        [SetUp]
        public override void Init()
        {
            base.Init();

            var options = new ProtoCore.Options();
            options.RootModulePathName = string.Empty;
            libraryServicesCore = new ProtoCore.Core(options);
            libraryServicesCore.Executives.Add(ProtoCore.Language.kAssociative, 
                new ProtoAssociative.Executive(libraryServicesCore));
            libraryServicesCore.Executives.Add(ProtoCore.Language.kImperative, 
                new ProtoImperative.Executive(libraryServicesCore));

            libraryServices = new LibraryServices(libraryServicesCore);
        }

        [TearDown]
        public override void Cleanup()
        {
            libraryServicesCore.Cleanup();
            libraryServicesCore = null;
            libraryServices = null;

            base.Cleanup();
        }

        [Test]
        [Category("UnitTests")]
        public void TestPreLoadedLibrary()
        {
            var loadedLibs = libraryServices.Libraries;
            Assert.IsTrue(loadedLibs.Any());
        }

        [Test]
        [Category("UnitTests")]
        public void TestLoadDSFile()
        {
            bool libraryLoaded = false;

            libraryServices.LibraryLoaded += (sender, e) => libraryLoaded = true;
            libraryServices.LibraryLoadFailed += (sender, e) => Assert.Fail("Failed to load library: " + e.LibraryPath); 

            string libraryPath = Path.Combine(GetTestDirectory(), @"core\library\Dummy.ds");
            libraryServices.ImportLibrary(libraryPath, ViewModel.Model.Logger);
            Assert.IsTrue(libraryLoaded);

            var functions = libraryServices.GetFunctionGroups(libraryPath);
            Assert.IsNotNull(functions);
            Assert.IsTrue(functions.Any());
        }

        [Test]
        [Category("UnitTests")]
        public void TestLibraryAcrossSessions()
        {
            bool libraryLoaded = false;
            libraryServices.LibraryLoaded += (sender, e) => libraryLoaded = true;

            // library should be able to load
            string libraryPath = Path.Combine(GetTestDirectory(), @"core\library\Test.ds");
            libraryServices.ImportLibrary(libraryPath, ViewModel.Model.Logger);
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

        [Test]
        [Category("UnitTests")]
        public void TestLoadNoNamespaceClass()
        {
            bool libraryLoaded = false;

            libraryServices.LibraryLoaded += (sender, e) => libraryLoaded = true;
            libraryServices.LibraryLoadFailed += (sender, e) => Assert.Fail("Failed to load library: " + e.LibraryPath);

            string libraryPath = "FFITarget.dll";
            libraryServices.ImportLibrary(libraryPath, ViewModel.Model.Logger);
            Assert.IsTrue(libraryLoaded);

            // All we need to do here is to ensure that the target has been loaded
            // at some point, so if it's already thre, don't try and reload it
            if (!libraryServices.Libraries.Any(x => x.EndsWith(libraryPath)))
            {
                libraryServices.ImportLibrary(libraryPath, ViewModel.Model.Logger);
                Assert.IsTrue(libraryLoaded);
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
    }
}
