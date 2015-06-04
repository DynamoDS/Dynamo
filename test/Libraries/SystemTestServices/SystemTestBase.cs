using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

using Dynamo;
using Dynamo.Controls;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Tests;
using Dynamo.UpdateManager;
using Dynamo.ViewModels;
using Dynamo.Wpf.ViewModels.Core;

using DynamoShapeManager;

using NUnit.Framework;

using ProtoCore.Mirror;

using TestServices;

namespace SystemTestServices
{
    /// <summary>
    /// SystemTestBase is the base class for all 
    /// Dynamo system tests.
    /// </summary>
    public abstract class SystemTestBase
    {
        protected IPathResolver pathResolver;
        protected string workingDirectory;
        private Preloader preloader;
        private AssemblyResolver assemblyResolver;

        #region protected properties

        protected DynamoViewModel ViewModel { get; set; }

        protected DynamoView View { get; set; }

        protected DynamoModel Model { get; set; }

        protected IUpdateManager UpdateManager { get; set; }

        protected string ExecutingDirectory
        {
            get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); }
        }

        protected string TempFolder { get; private set; }

        #endregion

        #region public methods

        [SetUp]
        public virtual void Setup()
        {
            var testConfig = GetTestSessionConfiguration();

            if (assemblyResolver == null)
            {
                assemblyResolver = new AssemblyResolver();
                assemblyResolver.Setup(testConfig.DynamoCorePath);
            }

            SetupCore();

            if (string.IsNullOrEmpty(workingDirectory))
            {
                workingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }

            CreateTemporaryFolder();

            // Setup Temp PreferenceSetting Location for testing
            PreferenceSettings.DynamoTestPath = Path.Combine(TempFolder, "UserPreferenceTest.xml");

            StartDynamo(testConfig);
        }

        /// <summary>
        /// Override this method in derived class to return a 
        /// custom configuration.
        /// </summary>
        /// <returns></returns>
        protected virtual TestSessionConfiguration GetTestSessionConfiguration()
        {
            return new TestSessionConfiguration();
        }

        [TearDown]
        public virtual void TearDown()
        {
            //Ensure that we leave the workspace marked as
            //not having changes.
            ViewModel.HomeSpace.HasUnsavedChanges = false;

            if (null != View && View.IsLoaded)
                View.Close();

            if (ViewModel != null)
            {
                var shutdownParams = new DynamoViewModel.ShutdownParams(false, false);

                ViewModel.PerformShutdownSequence(shutdownParams);
                ViewModel = null;
            }

            View = null;
            Model = null;
            preloader = null;
            pathResolver = null;

            if (assemblyResolver != null)
            {
                assemblyResolver.TearDown();
                assemblyResolver = null;
            }

            try
            {
                var directory = new DirectoryInfo(TempFolder);
                directory.Delete(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        [TestFixtureTearDown]
        public virtual void FinalTearDown()
        {
            // Fix for COM exception on close
            // See: http://stackoverflow.com/questions/6232867/com-exceptions-on-exit-with-wpf 
            //Dispatcher.CurrentDispatcher.InvokeShutdown();
        }

        #endregion

        #region protected methods

        /// <summary>
        /// SetupCore allows inheritors to provide custom setup logic.
        /// </summary>
        protected virtual void SetupCore(){}

        protected virtual void GetLibrariesToPreload(List<string> libraries)
        {
            // Nothing here by design. If you find yourself having to add 
            // anything here, something must be wrong. DynamoViewModelUnitTest
            // is designed to contain no test cases, so it does not need any 
            // preloaded library, all of which should only be specified in the
            // derived class.
        }

        protected virtual void StartDynamo(TestSessionConfiguration testConfig)
        {
            preloader = new Preloader(testConfig.DynamoCorePath, testConfig.RequestedLibraryVersion);
            preloader.Preload();

            var preloadedLibraries = new List<string>();
            GetLibrariesToPreload(preloadedLibraries);

            if (preloadedLibraries.Any())
            {
                if (pathResolver == null)
                    pathResolver = new TestPathResolver();

                var pr = pathResolver as TestPathResolver;
                foreach (var preloadedLibrary in preloadedLibraries.Distinct())
                {
                    pr.AddPreloadLibraryPath(preloadedLibrary);
                }
            }

            Model = DynamoModel.Start(
                new DynamoModel.DefaultStartConfiguration()
                {
                    StartInTestMode = true,
                    PathResolver = pathResolver,
                    GeometryFactoryPath = preloader.GeometryFactoryPath,
                    UpdateManager = this.UpdateManager
                });

            ViewModel = DynamoViewModel.Start(
                new DynamoViewModel.StartConfiguration()
                {
                    DynamoModel = Model
                });

            //create the view
            View = new DynamoView(ViewModel);
            View.Show();

            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
        }

        /// <summary>
        /// Open and run a Dynamo definition given a relative
        /// path from the working directory.
        /// </summary>
        /// <param name="subPath"></param>
        protected void OpenAndRunDynamoDefinition(string subPath)
        {
            OpenDynamoDefinition(subPath);
            Assert.DoesNotThrow(() => ((HomeWorkspaceModel)Model.CurrentWorkspace).Run());
        }

        /// <summary>
        /// Open a Dynamo definition given a relative
        /// path from the working directory
        /// </summary>
        /// <param name="relativeFilePath"></param>
        protected void OpenDynamoDefinition(string relativeFilePath)
        {
            string samplePath = Path.Combine(workingDirectory, relativeFilePath);
            string testPath = Path.GetFullPath(samplePath);

            Assert.IsTrue(File.Exists(testPath), string.Format("Could not find file: {0} for testing.", testPath));

            ViewModel.OpenCommand.Execute(testPath);
        }

        protected bool IsNodeInErrorOrWarningState(string guid)
        {
            var model = ViewModel.Model;
            var node = model.CurrentWorkspace.NodeFromWorkspace(guid);
            Assert.IsNotNull(node);
            return node.State == Dynamo.Models.ElementState.Error ||
                    node.State == Dynamo.Models.ElementState.Warning;
        }

        protected void AssertNoDummyNodes()
        {
            var nodes = ViewModel.Model.CurrentWorkspace.Nodes;

            double dummyNodesCount = nodes.OfType<DSCoreNodesUI.DummyNode>().Count();
            if (dummyNodesCount >= 1)
            {
                Assert.Fail("Number of dummy nodes found in Sample: " + dummyNodesCount);
            }
        }

        protected void CreateTemporaryFolder()
        {
            string tempPath = Path.GetTempPath();
            TempFolder = Path.Combine(tempPath, "dynamoTmp\\" + Guid.NewGuid().ToString("N"));

            if (!Directory.Exists(TempFolder))
                Directory.CreateDirectory(TempFolder);
        }

        protected static bool IsFuzzyEqual(double d0, double d1, double tol)
        {
            return System.Math.Abs(d0 - d1) < tol;
        }

        protected void AssertEvaluationCount(HomeWorkspaceModel homeWorkspace, int expected)
        {
            Assert.AreEqual(homeWorkspace.EvaluationCount, expected);
        }

        #endregion

        #region public methods

        public static string GetTestDirectory(string executingDirectory)
        {
            var directory = new DirectoryInfo(executingDirectory);
            return Path.Combine(directory.Parent.Parent.Parent.FullName, "test");
        }

        public void RunCurrentModel()
        {
            Assert.DoesNotThrow(() => Model.Workspaces.OfType<HomeWorkspaceModel>().First().Run());
        }

        public void AssertPreviewCount(string guid, int count)
        {
            string varname = GetVarName(guid);
            var mirror = GetRuntimeMirror(varname);
            Assert.IsNotNull(mirror);

            var data = mirror.GetData();
            Assert.IsTrue(data.IsCollection);
            Assert.AreEqual(count, data.GetElements().Count);
        }

        public NodeModel GetNode<T>(string guid) where T : NodeModel
        {
            var allNodes = ViewModel.Model.CurrentWorkspace.Nodes;
            var nodes = allNodes.Where(x => string.CompareOrdinal(x.GUID.ToString(), guid) == 0);
            if (nodes.Count() < 1)
                return null;
            else if (nodes.Count() > 1)
                throw new Exception("There are more than one nodes with the same GUID!");
            return nodes.ElementAt(0) as T;
        }

        public object GetPreviewValue(string guid)
        {
            string varname = GetVarName(guid);
            var mirror = GetRuntimeMirror(varname);
            Assert.IsNotNull(mirror);

            return mirror.GetData().Data;
        }

        /// <summary>
        /// Get a collection from a node's mirror data.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>A list of objects if the data is a collection, else null.</returns>
        public List<object> GetPreviewCollection(string guid)
        {
            string varname = GetVarName(guid);
            var mirror = GetRuntimeMirror(varname);
            Assert.IsNotNull(mirror);
            var data = mirror.GetData();
            if (data == null)
            {
                Assert.Fail("The mirror has no data.");
            }

            var dataColl = mirror.GetData().GetElements();
            if (dataColl == null)
            {
                return null;
            }

            var elements = dataColl.Select(x => x.Data).ToList();

            return elements;
        }

        public object GetPreviewValueAtIndex(string guid, int index)
        {
            string varname = GetVarName(guid);
            var mirror = GetRuntimeMirror(varname);
            Assert.IsNotNull(mirror);
            var data = mirror.GetData();
            if (data == null) return null;
            if (!data.IsCollection) return null;
            var elements = data.GetElements();
            return elements[index].Data;
        }

        public List<object> GetFlattenedPreviewValues(string guid)
        {
            string varname = GetVarName(guid);
            var mirror = GetRuntimeMirror(varname);
            Assert.IsNotNull(mirror);
            var data = mirror.GetData();
            if (data == null) return null;
            if (!data.IsCollection)
            {
                return data.Data == null ? new List<object>() : new List<object>() { data.Data };
            }
            var elements = data.GetElements();

            var objects = GetSublistItems(elements);

            return objects;
        }

        public void AssertClassName(string guid, string className)
        {
            string varname = GetVarName(guid);
            var mirror = GetRuntimeMirror(varname);
            Assert.IsNotNull(mirror);
            var classInfo = mirror.GetData().Class;
            Assert.AreEqual(classInfo.ClassName, className);
        }

        #endregion

        private static List<object> GetSublistItems(IEnumerable<MirrorData> datas)
        {
            var objects = new List<object>();
            foreach (var data in datas)
            {
                if (!data.IsCollection)
                {
                    objects.Add(data.Data);
                }
                else
                {
                    objects.AddRange(GetSublistItems(data.GetElements()));
                }
            }
            return objects;
        }

        protected string GetVarName(string guid)
        {
            var model = ViewModel.Model;
            var node = model.CurrentWorkspace.NodeFromWorkspace(guid);
            Assert.IsNotNull(node);
            return node.AstIdentifierBase;
        }

        protected RuntimeMirror GetRuntimeMirror(string varName)
        {
            RuntimeMirror mirror = null;
            Assert.DoesNotThrow(() => mirror = ViewModel.Model.EngineController.GetMirror(varName));
            return mirror;
        }

    }
}
