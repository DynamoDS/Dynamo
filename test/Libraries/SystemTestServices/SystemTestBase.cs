using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

using Dynamo;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Tests;
using Dynamo.Utilities;
using Dynamo.ViewModels;

using DynamoUtilities;

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
        protected string workingDirectory;

        #region protected properties

        protected DynamoViewModel ViewModel { get; set; }

        protected DynamoView View { get; set; }

        protected DynamoModel Model { get; set; }

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
            AssemblyResolver.Setup();

            SetupCore();

            if (string.IsNullOrEmpty(workingDirectory))
            {
                workingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }

            DynamoPathManager.PreloadAsmLibraries(DynamoPathManager.Instance);

            CreateTemporaryFolder();

            // Setup Temp PreferenceSetting Location for testing
            PreferenceSettings.DynamoTestPath = Path.Combine(TempFolder, "UserPreferenceTest.xml");

            StartDynamo();
        }

        public virtual void SetupCore()
        {

        }

        public virtual void StartDynamo()
        {
            Model = DynamoModel.Start(
                new DynamoModel.StartConfiguration()
                {
                    StartInTestMode = true,
                    DynamoCorePath = DynamoPathManager.Instance.MainExecPath
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

        [TearDown]
        public virtual void TearDown()
        {
            //Ensure that we leave the workspace marked as
            //not having changes.
            ViewModel.HomeSpace.HasUnsavedChanges = false;

            if (View.IsLoaded)
                View.Close();

            if (ViewModel != null)
            {
                var shutdownParams = new DynamoViewModel.ShutdownParams(false, false);

                ViewModel.PerformShutdownSequence(shutdownParams);
                ViewModel = null;
            }

            View = null;
            Model = null;

            GC.Collect();

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
        public void OpenDynamoDefinition(string relativeFilePath)
        {
            string samplePath = Path.Combine(workingDirectory, relativeFilePath);
            string testPath = Path.GetFullPath(samplePath);

            Assert.IsTrue(File.Exists(testPath), string.Format("Could not find file: {0} for testing.", testPath));

            ViewModel.OpenCommand.Execute(testPath);
        }

        #endregion

        #region Utility functions

        public static string GetTestDirectory(string executingDirectory)
        {
            var directory = new DirectoryInfo(executingDirectory);
            return Path.Combine(directory.Parent.Parent.Parent.FullName, "test");
        }

        protected void CreateTemporaryFolder()
        {
            string tempPath = Path.GetTempPath();
            TempFolder = Path.Combine(tempPath, "dynamoTmp\\" + Guid.NewGuid().ToString("N"));

            if (!Directory.Exists(TempFolder))
                Directory.CreateDirectory(TempFolder);
        }

        public void RunCurrentModel()
        {
            Assert.DoesNotThrow(() => Model.Workspaces.OfType<HomeWorkspaceModel>().First().Run());
        }

        public void AssertNoDummyNodes()
        {
            var nodes = ViewModel.Model.CurrentWorkspace.Nodes;

            double dummyNodesCount = nodes.OfType<DSCoreNodesUI.DummyNode>().Count();
            if (dummyNodesCount >= 1)
            {
                Assert.Fail("Number of dummy nodes found in Sample: " + dummyNodesCount);
            }
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

        public void AssertClassName(string guid, string className)
        {
            string varname = GetVarName(guid);
            var mirror = GetRuntimeMirror(varname);
            Assert.IsNotNull(mirror);
            var classInfo = mirror.GetData().Class;
            Assert.AreEqual(classInfo.ClassName, className);
        }

        protected static bool IsFuzzyEqual(double d0, double d1, double tol)
        {
            return System.Math.Abs(d0 - d1) < tol;
        }

        private string GetVarName(string guid)
        {
            var model = ViewModel.Model;
            var node = model.CurrentWorkspace.NodeFromWorkspace(guid);
            Assert.IsNotNull(node);
            return node.AstIdentifierBase;
        }

        private RuntimeMirror GetRuntimeMirror(string varName)
        {
            RuntimeMirror mirror = null;
            Assert.DoesNotThrow(() => mirror = ViewModel.Model.EngineController.GetMirror(varName));
            return mirror;
        }

        protected bool IsNodeInErrorOrWarningState(string guid)
        {
            var model = ViewModel.Model;
            var node = model.CurrentWorkspace.NodeFromWorkspace(guid);
            Assert.IsNotNull(node);
            return node.State == Dynamo.Models.ElementState.Error ||
                    node.State == Dynamo.Models.ElementState.Warning;
        }

        #endregion
    }
}
