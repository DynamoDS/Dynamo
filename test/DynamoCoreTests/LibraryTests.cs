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
            libraryServices = LibraryServices.GetInstance();
            RegisterEvents();
        }

        [TearDown]
        public override void Cleanup()
        {
            UnRegisterEvents();
            libraryServices = null;
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

        [Test]
        [Category("UnitTests")]
        public void TestLoadNoNamespaceClass()
        {
            LibraryLoaded = false;

            string libraryPath = "FFITarget.dll";

            // All we need to do here is to ensure that the target has been loaded
            // at some point, so if it's already thre, don't try and reload it
            if (!libraryServices.Libraries.Any(x => x.EndsWith(libraryPath)))
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

        private static bool LibraryLoaded { get; set; }
        private LibraryServices libraryServices;
    }
}
