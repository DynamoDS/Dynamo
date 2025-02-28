using System;
using System.Collections.Generic;
using Dynamo.Events;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Scheduler;
using Greg;
using Greg.AuthProviders;
using NUnit.Framework;
using RestSharp;

namespace Dynamo.Tests.Configuration
{
    [TestFixture]
    class ExecutionSessionTests : DynamoModelTestBase
    {
        private TimeSpan lastExecutionDuration = new TimeSpan();
        private IEnumerable<string> packagePaths;
        private object authprovider;

        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            // Add multiple libraries to better simulate typical Dynamo application usage.
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("DSCPython.dll");
            libraries.Add("FunctionObject.ds");
            base.GetLibrariesToPreload(libraries);
        }
      
        protected override DynamoModel.IStartConfiguration CreateStartConfiguration(IPreferences settings)
        {
            return new DynamoModel.DefaultStartConfiguration()
            {
                PathResolver = pathResolver,
                StartInTestMode = true,
                GeometryFactoryPath = preloader.GeometryFactoryPath,
                Preferences = settings,
                ProcessMode = TaskProcessMode.Synchronous,
                AuthProvider = new MockTokenProvider()
            };
        }

        private class MockTokenProvider : IAuthProvider, IOAuth2AccessTokenProvider
        {
            public LoginState LoginState => LoginState.LoggedIn;

            public string Username => throw new NotImplementedException();

            public event Func<object, bool> RequestLogin;
            public event Action<LoginState> LoginStateChanged;

            public string GetAccessToken()
            {
                return "faketoken";
            }

            public bool Login()
            {
                throw new NotImplementedException();
            }

            public void Logout()
            {
                throw new NotImplementedException();
            }

            public void SignRequest(ref RestRequest m, RestClient client)
            {
                throw new NotImplementedException();
            }
        }

        [OneTimeSetUp]
        public void FixtureSetup()
        {
            ExecutionEvents.GraphPostExecution += ExecutionEvents_GraphPostExecution;
        }

        [Test]
        [Category("UnitTests")]
        public void TestExecutionSession()
        {
            RunModel(@"core\HomogeneousList\HomogeneousInputsValid.dyn");
            Assert.IsNotNull(lastExecutionDuration);
        }

        [Test]
        [Category("UnitTests")]
        public void TestExecutionSessionPackagePaths()
        {
            ExecutionEvents.GraphPreExecution += ExecutionEvents_GraphPreExecution;
            RunModel(@"core\HomogeneousList\HomogeneousInputsValid.dyn");
            Assert.IsNotEmpty(packagePaths, "packgePaths was empty");
            ExecutionEvents.GraphPreExecution -= ExecutionEvents_GraphPreExecution;
        }

        [Test]
        [Category("UnitTests")]
        public void TestExecutionSessionAuthProvider()
        {
            ExecutionEvents.GraphPreExecution += ExecutionEvents_GraphPreExecution;
            RunModel(@"core\HomogeneousList\HomogeneousInputsValid.dyn");
            Assert.IsInstanceOf<IOAuth2AccessTokenProvider>(authprovider, $"was not an instance of {nameof(IOAuth2AccessTokenProvider)}");
            ExecutionEvents.GraphPreExecution -= ExecutionEvents_GraphPreExecution;
        }

        private void ExecutionEvents_GraphPreExecution(Session.IExecutionSession session)
        {
            packagePaths = ExecutionEvents.ActiveSession.GetParameterValue(Session.ParameterKeys.PackagePaths) as IEnumerable<string>;
            authprovider = ExecutionEvents.ActiveSession.GetParameterValue(Session.ParameterKeys.AuthTokenProvider);

        }
        
        [OneTimeTearDown]
        public void TearDown()
        {
            ExecutionEvents.GraphPostExecution -= ExecutionEvents_GraphPostExecution;
        }

        private void ExecutionEvents_GraphPostExecution(Session.IExecutionSession session)
        {
            lastExecutionDuration = (TimeSpan)session.GetParameterValue(Session.ParameterKeys.LastExecutionDuration);
            Assert.IsNotNull(session.GetParameterKeys());

            var filepath = "ExecutionEvents.dyn";
            Assert.IsFalse(session.ResolveFilePath(ref filepath));
        }
    }
}
