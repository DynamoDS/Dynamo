using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Interfaces;
using Dynamo.UpdateManager;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using NUnit.Framework;

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
    }
}