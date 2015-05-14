using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Dynamo.Models;
using Dynamo.Selection;
using Dynamo.Tests;
using Dynamo.ViewModels;
using DynamoShapeManager;
using NUnit.Framework;
using ProtoCore.Mirror;
using TestServices;

namespace Dynamo
{
    public class DynamoModelTestBase : UnitTestBase
    {
        protected DynamoModel CurrentDynamoModel { get; private set; }

        protected Preloader preloader;
        protected TestPathResolver pathResolver;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            StartDynamo();
        }

        public override void Cleanup()
        {
            try
            {
                preloader = null;
                DynamoSelection.Instance.ClearSelection();

                if (this.CurrentDynamoModel != null)
                {
                    this.CurrentDynamoModel.ShutDown(false);
                    this.CurrentDynamoModel = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }

            base.Cleanup();
        }

        protected virtual void StartDynamo()
        {
            var assemblyPath = Assembly.GetExecutingAssembly().Location;
            preloader = new Preloader(Path.GetDirectoryName(assemblyPath));
            preloader.Preload();

            TestPathResolver pathResolver = null;
            var preloadedLibraries = new List<string>();
            GetLibrariesToPreload(preloadedLibraries);

            if (preloadedLibraries.Any())
            {
                // Only when any library needs preloading will a path resolver be 
                // created, otherwise DynamoModel gets created without preloading 
                // any library.
                // 

                var pathResolverParams = new TestPathResolverParams()
                {
                    UserDataRootFolder = GetUserUserDataRootFolder(),
                    CommonDataRootFolder = GetCommonDataRootFolder()
                };

                pathResolver = new TestPathResolver(pathResolverParams);
                foreach (var preloadedLibrary in preloadedLibraries.Distinct())
                {
                    pathResolver.AddPreloadLibraryPath(preloadedLibrary);
                }
            }

            this.CurrentDynamoModel = DynamoModel.Start(
                new DynamoModel.DefaultStartConfiguration()
                {
                    PathResolver = pathResolver,
                    StartInTestMode = true,
                    GeometryFactoryPath = preloader.GeometryFactoryPath
                });
        }

        protected virtual void GetLibrariesToPreload(List<string> libraries)
        {
            // Nothing here by design. If you find yourself having to add 
            // anything here, something must be wrong. DynamoViewModelUnitTest
            // is designed to contain no test cases, so it does not need any 
            // preloaded library, all of which should only be specified in the
            // derived class.
        }

        protected virtual string GetUserUserDataRootFolder()
        {
            // Override in derived classed to provide a custom
            // UserAppDataRootFolder. Returning an empty string
            // here will cause the PathManager to use its default.
            return string.Empty;
        }

        protected virtual string GetCommonDataRootFolder()
        {
            // Override in derived classed to provide a custom
            // CommonAppDataRootFolder. Returning an empty string
            // here will cause the PathManager to use its default.
            return string.Empty;
        }

        protected T Open<T>(params string[] relativePathParts) where T : WorkspaceModel
        {
            var path = Path.Combine(relativePathParts);
            CurrentDynamoModel.ExecuteCommand(new DynamoModel.OpenFileCommand(path));
            return CurrentDynamoModel.CurrentWorkspace as T;
        }

        protected void BeginRun()
        {
            CurrentDynamoModel.ExecuteCommand(new DynamoModel.RunCancelCommand(false, false));
        }

        protected void EmptyScheduler()
        {
            while (CurrentDynamoModel.Scheduler.ProcessNextTask(false))
            {

            }
        }

        protected void AssertNoDummyNodes()
        {
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;

            var dummyNodes = nodes.OfType<DSCoreNodesUI.DummyNode>();
            string logs = string.Empty;
            foreach (var node in dummyNodes)
            {
                logs += string.Format("{0} is a {1} node\n", node.NickName, node.NodeNature);
            }

            double dummyNodesCount = dummyNodes.Count();
            if (dummyNodesCount >= 1)
            {
                Assert.Fail(logs + "Number of dummy nodes found in Sample: " + dummyNodesCount);
            }
        }

        protected IEnumerable<object> GetPreviewValues()
        {
            List<object> objects = new List<object>();
            foreach(var node in CurrentDynamoModel.CurrentWorkspace.Nodes)
            {
                objects.Add(GetPreviewValue(node.GUID));
            }
            return objects;
        }

        protected void AssertNullValues()
        {
            foreach (var node in CurrentDynamoModel.CurrentWorkspace.Nodes)
            {
                string varname = GetVarName(node.GUID);
                var mirror = GetRuntimeMirror(varname);
                Assert.IsNull(mirror);
            }
            
        }

        protected object GetPreviewValue(System.Guid guid)
        {
            string varname = GetVarName(guid);
            var mirror = GetRuntimeMirror(varname);
            Assert.IsNotNull(mirror);

            return mirror.GetData().Data;
        }

        protected string GetVarName(System.Guid guid)
        {
            var model = CurrentDynamoModel;
            var node = model.CurrentWorkspace.NodeFromWorkspace(guid);
            Assert.IsNotNull(node);

            int outportCount = node.OutPorts.Count;
            Assert.IsTrue(outportCount > 0);

            if(outportCount > 1) 
                return node.AstIdentifierBase; 
            else 
                return node.GetAstIdentifierForOutputIndex(0).Value;

        }

        protected string GetVarName(string guid)
        {
            var model = CurrentDynamoModel;
            var node = model.CurrentWorkspace.NodeFromWorkspace(guid);
            Assert.IsNotNull(node);

            int outportCount = node.OutPorts.Count;
            Assert.IsTrue(outportCount > 0);

            if (outportCount > 1) 
                return node.AstIdentifierBase; 
            else 
                return node.GetAstIdentifierForOutputIndex(0).Value;

        }

        /// <summary>
        ///     Used to reflect on runtime data such as values of a variable
        /// </summary>
        protected RuntimeMirror GetRuntimeMirror(string varName)
        {
            RuntimeMirror mirror = null;
            Assert.DoesNotThrow(() => mirror = CurrentDynamoModel.EngineController.GetMirror(varName));
            return mirror;
        }

        /// <summary>
        ///     Used to reflect on static data such as classes and class members
        /// </summary>
        protected ClassMirror GetClassMirror(string className)
        {
            ProtoCore.Core core = CurrentDynamoModel.EngineController.LiveRunnerCore;
            var classMirror = new ClassMirror(className, core);
            return classMirror;
        }

    }
}
