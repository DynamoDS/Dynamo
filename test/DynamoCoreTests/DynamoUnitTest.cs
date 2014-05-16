﻿using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Interfaces;
using Dynamo.UpdateManager;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using NUnit.Framework;

using ProtoCore.Mirror;

namespace Dynamo.Tests
{
    public class DynamoUnitTest : UnitTestBase
    {
        protected DynamoController Controller;

        [SetUp]
        public override void Init()
        {
            base.Init();
            StartDynamo();
        }

        [TearDown]
        public override void Cleanup()
        {
            try
            {
                Controller.ShutDown(false, null);
                this.Controller = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }

            base.Cleanup();
            System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeShutdown();
            GC.Collect();
        }

        protected void VerifyModelExistence(Dictionary<string, bool> modelExistenceMap)
        {
            var nodes = Controller.DynamoModel.CurrentWorkspace.Nodes;
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
            var logger = new DynamoLogger();
            dynSettings.DynamoLogger = logger;

            var updateManager = new UpdateManager.UpdateManager(logger);

            ////create a new instance of the ViewModel
            Controller = new DynamoController(Context.NONE, updateManager,
                new DefaultWatchHandler(), new PreferenceSettings());
            DynamoController.IsTestMode = true;
            Controller.DynamoViewModel = new DynamoViewModel(Controller, null);
            Controller.VisualizationManager = new VisualizationManager();   
        }

        /// <summary>
        /// Enables starting Dynamo with a mock IUpdateManager
        /// </summary>
        /// <param name="updateManager"></param>
        /// <param name="watchHandler"></param>
        /// <param name="preferences"></param>
        /// <param name="visualizationManager"></param>
        protected void StartDynamo(IUpdateManager updateManager, IWatchHandler watchHandler, IPreferences preferences, IVisualizationManager visualizationManager)
        {
            //create a new instance of the ViewModel
            Controller = new DynamoController(Context.NONE, updateManager, watchHandler, preferences);
            Controller.DynamoViewModel = new DynamoViewModel(Controller, null);
            DynamoController.IsTestMode = true;
            Controller.VisualizationManager = new VisualizationManager();
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
            var model = dynSettings.Controller.DynamoModel;
            model.Open(exampleFilePath);

            dynSettings.Controller.RunExpression(null);

            foreach (var test in tests)
            {
                var runResult = model.CurrentWorkspace.NodeFromWorkspace(test.Key).CachedValue.Data;
                Assert.AreEqual(test.Value, runResult);
            }
        }

        protected void AssertNoDummyNodes()
        {
            var nodes = Controller.DynamoModel.Nodes;

            double dummyNodesCount = nodes.OfType<DSCoreNodesUI.DummyNode>().Count();
            if (dummyNodesCount >= 1)
            {
                Assert.Fail("Number of dummy nodes found in Sample: " + dummyNodesCount);
            }
        }

        protected void GetPreviewValues()
        {
            Controller.DynamoModel.Nodes.ForEach(node => GetPreviewValue(node.GUID));
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
            var model = Controller.DynamoModel;
            var node = model.CurrentWorkspace.NodeFromWorkspace(guid);
            Assert.IsNotNull(node);
            return node.AstIdentifierBase;
        }

        protected string GetVarName(string guid)
        {
            var model = Controller.DynamoModel;
            var node = model.CurrentWorkspace.NodeFromWorkspace(guid);
            Assert.IsNotNull(node);
            return node.AstIdentifierBase;
        }

        protected RuntimeMirror GetRuntimeMirror(string varName)
        {
            RuntimeMirror mirror = null;
            Assert.DoesNotThrow(() => mirror = Controller.EngineController.GetMirror(varName));
            return mirror;
        }

    }
}