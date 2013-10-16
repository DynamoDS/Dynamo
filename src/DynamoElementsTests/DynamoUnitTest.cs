using System;
using System.IO;
using System.Reflection;
using Dynamo.FSchemeInterop;
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