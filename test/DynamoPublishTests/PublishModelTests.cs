using Dynamo;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Publish;
using Dynamo.Publish.Models;
using Dynamo.Publish.Properties;
using Greg;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reach.Upload;
using System.Net;
using System.Threading;
using Dynamo.Graph;
using Dynamo.Graph.Workspaces;
using Reach.Messages.Data;

namespace DynamoPublishTests
{
    public class PublishModelTests : DynamoModelTestBase
    {
        Mock<IAuthProvider> authenticationProvider;

        PublishModel publishModel;

        [SetUp]
        public void SetUp()
        {
            authenticationProvider = new Mock<IAuthProvider>();
            authenticationProvider.Setup(provider => provider.Username).Returns("DummyUserName");

            // Create mock of reach client.
            var client = new Mock<IWorkspaceStorageClient>();
            var success = new Mock<IReachHttpResponse>();

            success.Setup(resp => resp.Content).Returns(Resources.WorkspacesSendSucceededServerResponse);
            success.Setup(resp => resp.StatusCode).Returns(HttpStatusCode.OK);

            var successMock = success.Object;

            client.Setup(c =>
                // If it's sent any workspace or any custom nodes, result always will be successful.
                c.Send(It.IsAny<HomeWorkspaceModel>(), It.IsAny<IEnumerable<CustomNodeWorkspaceModel>>(), null)).Returns(Task.FromResult(successMock));

            // Create publish model.
            publishModel = new PublishModel(authenticationProvider.Object, CurrentDynamoModel.CustomNodeManager, client.Object);
        }

        private void AssertNumberOfDeps(int numDeps)
        {
            var hws = CurrentDynamoModel.CurrentWorkspace as HomeWorkspaceModel;

            Assert.NotNull(hws, "The current workspace is not a " + typeof(HomeWorkspaceModel).FullName);

            var deps = PublishModel.WorkspaceDependencies.Collect(hws, CurrentDynamoModel.CustomNodeManager);

            Assert.IsNotNull(deps.HomeWorkspace);
            Assert.AreEqual(numDeps, deps.CustomNodeWorkspaces.Count());
        }

        [Test]
        public void WorkspaceDependencies_Collect_ShouldWorkCorrectlyForWorkspaceWithNoDeps()
        {
            string openPath = Path.Combine(TestDirectory, @"Libraries\DynamoPublishTests\PublishWorkspaceTestNoDeps.dyn");
            OpenModel(openPath);

            AssertNumberOfDeps(0);
        }

        [Test]
        public void WorkspaceDependencies_Collect_ShouldWorkCorrectlyForWorkspaceWithOneDep()
        {
            string openPath = Path.Combine(TestDirectory, @"Libraries\DynamoPublishTests\PublishWorkspaceTestOneDep.dyn");
            OpenModel(openPath);

            AssertNumberOfDeps(1);
        }

        [Test]
        public void WorkspaceDependencies_Collect_ShouldNotCrashForWorkspaceWithProxyNode()
        {
            var openPath = Path.Combine(TestDirectory, @"Libraries\DynamoPublishTests\CustCrash.dyn");
            OpenModel(openPath);

            var hws = CurrentDynamoModel.CurrentWorkspace as HomeWorkspaceModel;
            publishModel.SendAsync(hws);

            // wait for sending completion
            PublishModel.UploadState state;
            var timesOfSleep = 0;
            do
            {
                Thread.Sleep(500);
                timesOfSleep++;
                state = publishModel.State;
            } while ((state == PublishModel.UploadState.Uninitialized
                || state == PublishModel.UploadState.Uploading)
                // do not wait indefinetely
                && timesOfSleep < 10);

            Assert.AreEqual(PublishModel.UploadState.Failed, publishModel.State);
            Assert.IsNotNull(publishModel.NotFoundCustomNodeName);
        }

        [Test]
        public void WorkspaceDependencies_Collect_ShouldWorkCorrectlyForWorkspaceWithNestedDeps()
        {
            string openPath = Path.Combine(TestDirectory, @"Libraries\DynamoPublishTests\PublishWorkspaceTestTwoDeps.dyn");
            OpenModel(openPath);

            AssertNumberOfDeps(2);
        }

        [Test]
        public void PublishModelStateTest()
        {
            string openPath = Path.Combine(TestDirectory, @"Libraries\DynamoPublishTests\PublishWorkspaceTestTwoDeps.dyn");
            OpenModel(openPath);

            var hws = CurrentDynamoModel.CurrentWorkspace as HomeWorkspaceModel;
            Assert.NotNull(hws, "The current workspace is not a " + typeof(HomeWorkspaceModel).FullName);

            publishModel.SendAsync(hws);
            Assert.AreEqual(PublishModel.UploadState.Uploading, publishModel.State);

            var startTime = DateTime.Now;
            var halfMinute = new TimeSpan(0, 0, 30);
            // Wait until workspace is sent.
            while (publishModel.State == PublishModel.UploadState.Uploading)
            {
                var now = DateTime.Now;
                if (now - startTime > halfMinute)
                    Assert.Fail("Couldn't send workspace to customizer in time.");
            }

            Assert.AreEqual(PublishModel.UploadState.Succeeded, publishModel.State);
        }
    }
}