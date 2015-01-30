using System;
using System.Collections.Generic;
using System.Linq;

using Dynamo.Models;
using Dynamo.Selection;
using Dynamo.ViewModels;

using NUnit.Framework;

using ProtoCore.Mirror;
using DynamoUtilities;
using System.Reflection;
using System.IO;

namespace Dynamo.Tests
{
    public class DynamoViewModelUnitTest : UnitTestBase
    {
        protected DynamoViewModel ViewModel;
        protected DynamoModel Model;

        public override void Init()
        {
            base.Init();
            StartDynamo();
        }

        public override void Cleanup()
        {
            try
            {
                DynamoSelection.Instance.ClearSelection();

                var shutdownParams = new DynamoViewModel.ShutdownParams(
                    shutdownHost: false, allowCancellation: false);

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
            DynamoPathManager.Instance.InitializeCore(
               Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            DynamoPathManager.PreloadAsmLibraries(DynamoPathManager.Instance);
            
            this.Model = DynamoModel.Start(
                new DynamoModel.StartConfiguration()
                {
                    StartInTestMode = true
                });

            this.ViewModel = DynamoViewModel.Start(
                new DynamoViewModel.StartConfiguration()
                {
                    DynamoModel = this.Model
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
            this.ViewModel.Model.RunExpression();

            foreach (var test in tests)
            {
                var runResult = this.ViewModel.Model.CurrentWorkspace.NodeFromWorkspace(test.Key).CachedValue.Data;
                Assert.AreEqual(test.Value, runResult);
            }
        }

        protected void AssertNoDummyNodes()
        {
            var nodes = ViewModel.Model.Nodes;

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
            foreach(var node in ViewModel.Model.Nodes)
            {
                objects.Add(GetPreviewValue(node.GUID));
            }
            return objects;
        }

        protected void AssertNullValues()
        {
            foreach (var node in ViewModel.Model.Nodes)
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

        protected RuntimeMirror GetRuntimeMirror(string varName)
        {
            RuntimeMirror mirror = null;
            Assert.DoesNotThrow(() => mirror = ViewModel.Model.EngineController.GetMirror(varName));
            return mirror;
        }

        protected string GetCommandPathByFileName(string fileName, string fileExtension = "txt")
        {
            var partPathName = string.Format("core\\commands\\{0}.{1}", fileName, fileExtension);
            return Path.Combine(GetTestDirectory(), partPathName);
        }
    }
}