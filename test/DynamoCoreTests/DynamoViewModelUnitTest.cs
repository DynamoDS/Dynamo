using System;
using System.Collections.Generic;
using System.Linq;

using Dynamo.Models;
using Dynamo.Selection;
using Dynamo.ViewModels;
using DynamoShapeManager;
using NUnit.Framework;

using ProtoCore.Mirror;
using System.Reflection;
using System.IO;
using TestServices;

namespace Dynamo.Tests
{
    /// <summary>
    /// The DynamoViewModelUnitTests constructs the DynamoModel
    /// and the DynamoViewModel, but does not construct the view.
    /// You can use this class to create tests which ensure that the 
    /// ViewModel and the Model are communicating properly.
    /// </summary>
    public class DynamoViewModelUnitTest : UnitTestBase
    {
        protected DynamoViewModel ViewModel;
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

                if (ViewModel == null)
                    return;

                var shutdownParams = new DynamoViewModel.ShutdownParams(
                    shutdownHost: false,
                    allowCancellation: false);

                ViewModel.PerformShutdownSequence(shutdownParams);
                ViewModel.RequestUserSaveWorkflow -= RequestUserSaveWorkflow;
                ViewModel = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }

            base.Cleanup();

            GC.Collect();
        }

        private void RequestUserSaveWorkflow(object sender, WorkspaceSaveEventArgs e)
        {
            // Some test cases may create nodes or modify nodes, so when Dynamo
            // is shutting down, Dynamo will fire RequestUserSaveWorkflow event 
            // to save the change, if there is no a corresponding event handler, 
            // or the event handler fails to save the change, shut down process 
            // will be aborted and a lot of resource will not be released 
            // (details refer to DynamoViewModel.PerformShutdownSequence()).
            //
            // As this test fixture is UIless, DynamoView, which implements 
            // event handler for DynamoViewModel.RequestUserSaveWorkflow event, 
            // won't be created. To ensure resource be released properly, we 
            // implement event handler here and simply mark the save event's 
            // susccess status to true to notify Dynamo to continue the shut
            // down process.
            e.Success = true;
        }

        protected void VerifyModelExistence(Dictionary<string, bool> modelExistenceMap)
        {
            var nodes = ViewModel.Model.CurrentWorkspace.Nodes;
            foreach (var pair in modelExistenceMap)
            {
                Guid guid = Guid.Parse(pair.Key);
                var node = nodes.FirstOrDefault((x) => (x.GUID == guid));
                bool nodeExists = (null != node);
                Assert.AreEqual(nodeExists, pair.Value);
            }
        }

        protected void StartDynamo()
        {
            var assemblyPath = Assembly.GetExecutingAssembly().Location;
            preloader = new Preloader(Path.GetDirectoryName(assemblyPath));
            preloader.Preload();

            var model = DynamoModel.Start(
                new DynamoModel.DefaultStartConfiguration()
                {
                    PathResolver = pathResolver,
                    StartInTestMode = true,
                    GeometryFactoryPath = preloader.GeometryFactoryPath
                });

            this.ViewModel = DynamoViewModel.Start(
                new DynamoViewModel.StartConfiguration()
                {
                    DynamoModel = model
                });

            this.ViewModel.RequestUserSaveWorkflow += RequestUserSaveWorkflow;
        }

        /// <summary>
        ///     Runs a basic unit tests that loads a file, runs it, and confirms that
        ///     nodes corresponding to given guids have OldValues that match the given
        ///     expected values.
        /// </summary>
        /// <param name="exampleFilePath">Path to DYN to run.</param>
        /// <param name="tests">
        ///     Key/Value pairs where the Key is a node Guid and the Value is the
        ///     expected OldValue for the node.
        /// </param>
        protected void RunExampleTest(
            string exampleFilePath, IEnumerable<KeyValuePair<Guid, object>> tests)
        {
            this.ViewModel.OpenCommand.Execute(exampleFilePath);
            this.ViewModel.HomeSpace.Run();

            foreach (var test in tests)
            {
                var runResult = this.ViewModel.Model.CurrentWorkspace.NodeFromWorkspace(test.Key).CachedValue.Data;
                Assert.AreEqual(test.Value, runResult);
            }
        }

        protected void AssertNoDummyNodes()
        {
            var nodes = ViewModel.Model.CurrentWorkspace.Nodes;

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
            foreach(var node in ViewModel.Model.CurrentWorkspace.Nodes)
            {
                objects.Add(GetPreviewValue(node.GUID));
            }
            return objects;
        }

        protected void AssertNullValues()
        {
            foreach (var node in ViewModel.Model.CurrentWorkspace.Nodes)
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
            var model = ViewModel.Model;
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
            var model = ViewModel.Model;
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
        /// Used to reflect on runtime data such as values of a variable
        /// </summary>
        /// <param name="varName"></param>
        /// <returns></returns>
        protected RuntimeMirror GetRuntimeMirror(string varName)
        {
            RuntimeMirror mirror = null;
            Assert.DoesNotThrow(() => mirror = ViewModel.Model.EngineController.GetMirror(varName));
            return mirror;
        }

        /// <summary>
        /// Used to reflect on static data such as classes and class members
        /// </summary>
        /// <param name="varName"></param>
        /// <returns></returns>
        protected ClassMirror GetClassMirror(string className)
        {
            ProtoCore.Core core = ViewModel.Model.EngineController.LiveRunnerCore;
            var classMirror = new ClassMirror(className, core);
            return classMirror;
        }

    }
}