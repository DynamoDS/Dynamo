using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.FSchemeInterop;
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
                Controller.ShutDown(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }

            base.Cleanup();
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
            //create a new instance of the ViewModel
            Controller = new DynamoController(new ExecutionEnvironment(), typeof (DynamoViewModel), Context.NONE, new UpdateManager.UpdateManager())
            {
                Testing = true
            };
        }

        /// <summary>
        /// Enables starting Dynamo with a mock IUpdateManager
        /// </summary>
        /// <param name="updateManager"></param>
        protected void StartDynamo(IUpdateManager updateManager)
        {
            //create a new instance of the ViewModel
            Controller = new DynamoController(new ExecutionEnvironment(), typeof(DynamoViewModel), Context.NONE, updateManager)
            {
                Testing = true
            };
        }
    }
}