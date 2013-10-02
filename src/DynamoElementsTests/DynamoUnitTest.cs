using System;
using System.IO;
using System.Reflection;
using Dynamo.FSchemeInterop;
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
                DynamoLogger.Instance.FinishLogging();
                Controller.ShutDown();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }

            base.Cleanup();
        }

        private void StartDynamo()
        {
            try
            {
                DynamoLogger.Instance.StartLogging();

                //create a new instance of the ViewModel
                Controller = new DynamoController(new ExecutionEnvironment(), typeof(DynamoViewModel), Context.NONE)
                {
                    Testing = true
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}