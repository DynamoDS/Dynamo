using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Dynamo.Configuration;
using Dynamo.Core;
using Dynamo.Interfaces;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.PackageManager;
using Dynamo.PackageManager.UI;
using Dynamo.Scheduler;
using Dynamo.Wpf.Extensions;
using DynamoCoreWpfTests.Utility;
using DynamoServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Moq;
using NUnit.Framework;

namespace DynamoCoreWpfTests.PackageManager
{
    [TestFixture]
    class PackageManagerViewExtensionTests : DynamoTestUIBase
    {
        private string PackagesDirectory { get { return Path.Combine(GetTestDirectory(ExecutingDirectory), "pkgs"); } }
        internal string BuiltinPackagesTestDir { get { return Path.Combine(GetTestDirectory(ExecutingDirectory), "builtinpackages testdir", "Packages"); } }


        #region extensionGeneration
        private string extensionPath;
        private string manifestPath;
        private string extensionManifest =
            @"<ViewExtensionDefinition>
          <AssemblyPath>..\bin\TestViewExtension.dll</AssemblyPath>
          <TypeName>TestViewExtension</TypeName>
        </ViewExtensionDefinition>";
        //this source is compiled at runtime when this test is run, and the resulting extension is loaded then.
        //this extension has a few public properties we can check to make sure dynamo has loaded it correctly.
        private string testViewExtensionSource =
       @"using System;
        using Dynamo.Wpf.Extensions;
        public class TestViewExtension: IViewExtension {
         public int loadedCount = 0;
         public int startupCount = 0;
         public void Dispose() {}

         public void Startup(ViewStartupParams p) {
          startupCount = startupCount + 1;
         }

         public void Loaded(ViewLoadedParams p) {
          loadedCount = loadedCount + 1;
         }

         public void Shutdown() {}

         public string UniqueId {
          get {
           return Guid.NewGuid().ToString();
          }
         }

         public string Name {
          get {
           return ""Test View Extension"";
          }
         }

        }";
        #endregion;

        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

        protected override DynamoModel.IStartConfiguration CreateStartConfiguration(IPathResolver pathResolver)
        {
            return new DynamoModel.DefaultStartConfiguration()
            {
                PathResolver = pathResolver,
                StartInTestMode = true,
                GeometryFactoryPath = preloader.GeometryFactoryPath,
                ProcessMode = TaskProcessMode.Synchronous,
                Preferences = new PreferenceSettings()
                {
                    CustomPackageFolders = new List<string>() { Path.Combine(PackagesDirectory, "subPackageDirectory") }
                }
            };
        }

        [OneTimeSetUp]
        /// <summary>
        /// This method compiles an extension at test time and injects it and an extension manifest into the testing package folder
        /// so that the extension gets loaded when the test instance of Dynamo starts.
        /// </summary>
        public virtual void GenerateNewExtension()
        {
            var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            var compilation = CSharpCompilation.Create("TestViewExtension", new[] { CSharpSyntaxTree.ParseText(testViewExtensionSource) }, GetGlobalReferences(), options);


            var results = compilation.Emit("TestViewExtension.dll");
            if (results.Diagnostics.Count() > 0)
            {
                Console.WriteLine("Compile ERROR");
                foreach (var error in results.Diagnostics)
                {
                    Console.WriteLine(error.GetMessage());
                }
            }
            else
            {
                Console.WriteLine("Compile OK");

                //move the new assembly into the package directory/bin folder.
                extensionPath = Path.Combine(PackagesDirectory, "subPackageDirectory", "runtimeGeneratedExtension", "bin", "TestViewExtension.dll");
                File.Copy("TestViewExtension.dll", extensionPath, true);

                //copy the manifest as well.
                manifestPath = Path.Combine(PackagesDirectory, "subPackageDirectory", "runtimeGeneratedExtension",
                    "extra", "TestViewExtension_ViewExtensionDefinition.xml");
                File.WriteAllText(manifestPath, extensionManifest);
            }
        }

        private static PortableExecutableReference[] GetGlobalReferences()
        {
            var assemblies = new[]
            {
        typeof(object).Assembly,
        typeof(Console).Assembly
    };
            var returnList = assemblies
                .Select(a => MetadataReference.CreateFromFile(a.Location))
                .ToList();
            //The location of the .NET assemblies
            var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
            returnList.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")));

            returnList.Add(MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DynamoCore.dll")));
            returnList.Add(MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DynamoCoreWpf.dll")));

            return returnList.ToArray();
        }

        [Test]
        public void PackageManagerLoadsRuntimeGeneratedExtension()
        {
            Assert.IsTrue(View.viewExtensionManager.ViewExtensions.Select(x => x.Name).Contains("Test View Extension"));
        }

        [Test]
        public void PackageManagerViewExtensionHasCorrectNumberOfRequestedExtensions()
        {
            var pkgViewExtension = View.viewExtensionManager.ViewExtensions.OfType<PackageManagerViewExtension>().FirstOrDefault();
            Assert.AreEqual(pkgViewExtension.RequestedExtensions.Count(), 1);
        }

        [Test]
        public void LateLoadedViewExtensionsHaveMethodsCalled()
        {
            var pkgviewExtension = View.viewExtensionManager.ViewExtensions.OfType<PackageManagerViewExtension>().FirstOrDefault();
            var pkgDir = Path.Combine(PackagesDirectory, "SampleViewExtension");

            var viewExtensionLoadStart = false;
            var viewExtensionLoaded = false;
            var viewExtensionAdd = false;

            var startCount = 0;
            var loadedCount = 0;
            var viewLoaded = false;

            pkgviewExtension.RequestLoadExtension += (extensionPath) =>
            {
                viewExtensionLoadStart = true;

                var mockExtension = new Mock<IViewExtension>();
                mockExtension.Setup(ext => ext.Startup(It.IsAny<ViewStartupParams>())).Callback(() =>
                {
                    startCount = startCount + 1;
                    Assert.Fail();
                });

                mockExtension.Setup(ext => ext.Loaded(It.IsAny<ViewLoadedParams>()))
               .Callback(() =>
               {
                   viewExtensionLoaded = true;
                   loadedCount = loadedCount + 1;
               });
                return mockExtension.Object;
            };
            pkgviewExtension.RequestAddExtension += (extension) =>
            {
                viewExtensionAdd = true;
            };

            //must wait until the view is loaded to accurately test late loading of a viewExtension package.
            View.Loaded += (sender, args) =>
            {
                viewLoaded = true;
                var loader = Model.GetPackageManagerExtension().PackageLoader;
                var pkg = loader.ScanPackageDirectory(pkgDir);
                loader.LoadPackages(new List<Package> { pkg });
                Assert.AreEqual(0, loader.RequestedExtensions.Count());
                Assert.AreEqual(2, View.viewExtensionManager.ViewExtensions.OfType<PackageManagerViewExtension>().FirstOrDefault().RequestedExtensions.Count());
                Assert.IsTrue(viewExtensionLoadStart);
                Assert.IsTrue(viewExtensionAdd);
                Assert.IsTrue(viewExtensionLoaded);
                Assert.AreEqual(1, loadedCount);
                Assert.AreEqual(0, startCount);
            };

            ViewExtensionTests.RaiseLoadedEvent(View);
            Assert.IsTrue(viewLoaded, "view was never loaded, invalid test");

        }

        [Test]
        public void StartUpAndLoadedAreCalledOnceOnViewExtensionsInPackges()
        {
            //this extension is compiled at testTime and injected into test package folder.
            //source is above.
            dynamic testExtension = View.viewExtensionManager.ViewExtensions.Where(x => x.Name == "Test View Extension").FirstOrDefault();
            Assert.AreEqual(1, testExtension.startupCount);
            Assert.AreEqual(1, testExtension.loadedCount);
        }

        [Test]
        public void PackageManagerViewExtesion_TriesToLoadLayoutSpecForBuiltInPackages()
        {
            //check that the packageManagerViewExtension requested the test layout spec to be applied.
            var packageManagerViewExtension = View.viewExtensionManager.ViewExtensions.OfType<PackageManagerViewExtension>().FirstOrDefault();
            var packageManager = ViewModel.Model.ExtensionManager.Extensions.OfType<PackageManagerExtension>().FirstOrDefault();
            Assert.IsNotNull(packageManagerViewExtension);
            Assert.IsNotNull(packageManager);
            //set builtin packages dir to test directory.
            PathManager.BuiltinPackagesDirectory = BuiltinPackagesTestDir;

            //load a bltin package manually. This package contains a layout spec.
            var builtInPackageLocation = Path.Combine(BuiltinPackagesTestDir, "SignedPackage2");
            var bltinPkg = packageManager.PackageLoader.ScanPackageDirectory(builtInPackageLocation);
            packageManager.PackageLoader.LoadPackages(new List<Package> { bltinPkg });


            //trigger packmanViewExt to load layoutspecs. Usually this would just happen at startup, but we're loading the bltin package late.
            packageManagerViewExtension.Loaded(new ViewLoadedParams(View, ViewModel));
            Assert.AreEqual(1, packageManagerViewExtension.RequestedLayoutSpecPaths.Count());
            Assert.AreEqual(Path.Combine(BuiltinPackagesTestDir, "SignedPackage2", "extra", "layoutspecs.json"), packageManagerViewExtension.RequestedLayoutSpecPaths.FirstOrDefault());
        }

        [Test]
        public void PackageManagerViewExtesion_SendsNotificationForPackagesThatTargetDifferentHost_AtExtensionLoad()
        {
            var count = 0;
            //check that the packageManagerViewExtension logged a notification for a package that targets revit.
            var packageManagerViewExtension = View.viewExtensionManager.ViewExtensions.OfType<PackageManagerViewExtension>().FirstOrDefault();
            var packageManager = ViewModel.Model.ExtensionManager.Extensions.OfType<PackageManagerExtension>().FirstOrDefault();

            Assert.IsNotNull(packageManagerViewExtension);
            Assert.IsNotNull(packageManager);

            //load a package manually. This package targets Revit.
            var packageForAnotherHost = new Package("nowhere", "nothing", "1.2.3", "MIT");
            packageForAnotherHost.HostDependencies = new List<string>() { "Revit" };
            packageManager.PackageLoader.LoadPackages(new List<Package> { packageForAnotherHost });
            //attach handler, after forcing packge to load - analgous to mocking startup sequence.
            (packageManagerViewExtension as INotificationSource).NotificationLogged += PackageManagerViewExtensionTests_NotificationLogged;

            //force Loaded, which should run the check.
            packageManagerViewExtension.Loaded(new ViewLoadedParams(View, ViewModel));

            //check that notification is raised.
            Assert.AreEqual(1, count);

            (packageManagerViewExtension as INotificationSource).NotificationLogged -= PackageManagerViewExtensionTests_NotificationLogged;


            void PackageManagerViewExtensionTests_NotificationLogged(NotificationMessage obj)
            {
                count = count + 1;
            }
        }

        [Test]
        public void PackageManagerViewExtesion_SendsNotificationForPackagesThatTargetDifferentHost_AtLatePackageLoad()
        {
            var count = 0;
            //check that the packageManagerViewExtension logged a notification for a package that targets revit.
            var packageManagerViewExtension = View.viewExtensionManager.ViewExtensions.OfType<PackageManagerViewExtension>().FirstOrDefault();
            var packageManager = ViewModel.Model.ExtensionManager.Extensions.OfType<PackageManagerExtension>().FirstOrDefault();
            (packageManagerViewExtension as INotificationSource).NotificationLogged += PackageManagerViewExtensionTests_NotificationLogged;

            Assert.IsNotNull(packageManagerViewExtension);
            Assert.IsNotNull(packageManager);

            //load a package manually. This package targets Revit.
            var packageForAnotherHost = new Package("nowhere", "nothing", "1.2.3", "MIT");
            packageForAnotherHost.HostDependencies = new List<string>() { "Revit" };
            packageManager.PackageLoader.LoadPackages(new List<Package> { packageForAnotherHost });

            //check that notification is raised.
            Assert.AreEqual(1, count);

            (packageManagerViewExtension as INotificationSource).NotificationLogged -= PackageManagerViewExtensionTests_NotificationLogged;


            void PackageManagerViewExtensionTests_NotificationLogged(NotificationMessage obj)
            {
                count = count + 1;
            }
        }

        [Test]
        public void TestCrashInPackage()
        {
            var pkgDir = Path.Combine(PackagesDirectory, "SampleViewExtension_Crash");

            var currentDynamoModel = ViewModel.Model;

            var loader = currentDynamoModel.GetPackageManagerExtension().PackageLoader;

            var pkg = loader.ScanPackageDirectory(pkgDir);
            Assert.IsNotNull(pkg);

            int count = 0;
            void DynamoConsoleLogger_LogErrorToDynamoConsole(string obj)
            {
                if (obj.Contains("Unhandled exception coming from package"))
                {
                    count++;
                }
            }

            Assert.IsFalse(DynamoModel.IsCrashing);

            DynamoConsoleLogger.LogErrorToDynamoConsole += DynamoConsoleLogger_LogErrorToDynamoConsole;

            NotImplementedException expectedEx = null;
            try
            {
                loader.LoadPackages(new List<Package>() { pkg });
                DispatcherUtil.DoEventsLoop(() => count > 0);
            }
            catch (NotImplementedException ex)
            {
                expectedEx = ex;
            }

            var loadedPkg = currentDynamoModel.GetPackageManagerExtension()?.PackageLoader?.LocalPackages?.FirstOrDefault(p =>
            {
                return p.RootDirectory.EndsWith("SampleViewExtension_Crash", StringComparison.OrdinalIgnoreCase);
            });

            Assert.AreEqual(PackageLoadState.StateTypes.Loaded, loadedPkg.LoadState.State);

            Assert.IsNotNull(expectedEx);
            Assert.IsNotNull(expectedEx.TargetSite?.Module?.Assembly);
            Assert.IsTrue(expectedEx.TargetSite?.Module?.Assembly.Location.StartsWith(pkg.RootDirectory, StringComparison.OrdinalIgnoreCase));
            Assert.IsFalse(DynamoModel.IsCrashing);
            Assert.AreEqual(1, count);

            DynamoConsoleLogger.LogErrorToDynamoConsole -= DynamoConsoleLogger_LogErrorToDynamoConsole;
        }

        [OneTimeTearDown]
        /// <summary>
        /// This method cleans up the manifest so the generated extension does not load
        /// during other tests.
        /// </summary>
        public void RemoveExtension()
        {
            //TODO it would be good to cleanup the dll as well but we can't as it is currently loaded.
            File.Delete(manifestPath);
        }
    }
}
