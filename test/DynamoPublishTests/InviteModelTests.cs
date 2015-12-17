using Dynamo;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Publish;
using Dynamo.Publish.Models;
using Greg;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using RestSharp;

namespace DynamoPublishTests
{
    public class InviteModelTests : DynamoModelTestBase
    {
        private Mock<IAuthProvider> authenticationProvider;

        private InviteModel inviteModel;

        [SetUp]
        public void SetUp()
        {
            authenticationProvider = new Mock<IAuthProvider>();
            authenticationProvider.Setup(provider => provider.Username).Returns("DummyUserName");
        }

        [Test]
        public void SendRequestTest()
        {
            // Create mock of rest client.
            var restClient = new Mock<RestClient>();
            restClient.Setup(x => x.Execute(It.IsAny<IRestRequest>()))
                .Returns(new RestResponse()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = "{ target : \"foo@foo.com\"}"
                });

            // Create Invite model.
            inviteModel = new InviteModel(authenticationProvider.Object, restClient.Object);

            var response = inviteModel.Send();
            Assert.True(response);
        }
    }

}