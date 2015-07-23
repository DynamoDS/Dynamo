using Dynamo;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Publish;
using Dynamo.Publish.Models;
using Greg;
using Moq;
using NUnit.Framework;
using Reach.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            client.Setup(c =>
                // If it's sent any workspace or any custom nodes, result always will be successful.
                c.Send(It.IsAny<HomeWorkspaceModel>(), It.IsAny<IEnumerable<CustomNodeWorkspaceModel>>())).
                Returns(Resource.WorkspacesSendSucceededServerResponse);

            // Create publish model.
            publishModel = new PublishModel(authenticationProvider.Object, 
                CurrentDynamoModel.CustomNodeManager, client.Object);            
        }

        [Test]
        public void SendWorkspaceTest()
        {
            string openPath = Path.Combine(TestDirectory, @"Libraries\DynamoPublishTests\PublishWorkspaceTest.dyn");
            OpenModel(openPath);

            publishModel.Send(CurrentDynamoModel.Workspaces);

            Assert.IsNotNull(publishModel.HomeWorkspace);
            Assert.AreEqual(1, publishModel.CustomNodeWorkspaces.Count);
        }

        [Test]
        public void PublishModelStateTest()
        {
            string openPath = Path.Combine(TestDirectory, @"Libraries\DynamoPublishTests\PublishWorkspaceTest.dyn");
            OpenModel(openPath);

            publishModel.SendAsynchronously(CurrentDynamoModel.Workspaces);
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
