using Dynamo.Core;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Scheduler;
using DynamoServices;
using Greg;
using Greg.AuthProviders;
using Moq;

using NUnit.Framework;

namespace Dynamo.Tests
{
    class AuthenticationManagerTests
    {
        #region Logout

        [Test]
        public void Logout_CausesLogoutMethodToBeInvokedOnAuthProvider()
        {
            var logoutCalled = false;

            var pc_noAuthProvider = new AuthenticationManager(null);
            pc_noAuthProvider.Logout();

            Assert.IsFalse(logoutCalled);

            var ap = new Mock<IAuthProvider>();
            ap.Setup(x => x.Logout()).Callback(() => logoutCalled = true);

            var pc = new AuthenticationManager(ap.Object);

            pc.Logout();

            Assert.IsTrue(logoutCalled);
        }

        #endregion

        #region Login

        [Test]
        public void Login_CausesLoginMethodToBeInvokedOnAuthProvider()
        {
            var called = false;

            var pc_noAuthProvider = new AuthenticationManager(null);
            pc_noAuthProvider.Login();

            Assert.IsFalse(called);

            var ap = new Mock<IAuthProvider>();
            ap.Setup(x => x.Login()).Callback(() => called = true);

            var pc = new AuthenticationManager(ap.Object);

            pc.Login();

            Assert.IsTrue(called);
        }

        #endregion

        #region Toggle Login State

        [Test]
        public void ToggleLoginState()
        {
            bool loginCalled = false;

            var authProviderMock = new Mock<IAuthProvider>();
            authProviderMock.Setup(x => x.Login()).Callback(() => loginCalled = true);

            var authManager = new AuthenticationManager(authProviderMock.Object);

            authManager.ToggleLoginState(null);
            Assert.True(loginCalled);
        }

        #endregion

        #region Properties

        [Test]
        public void Property_Username()
        {
            var authProviderMock = new Mock<IAuthProvider>();
            var authManager = new AuthenticationManager(authProviderMock.Object);

            authProviderMock.Setup(x => x.Username).Returns("test");

            Assert.AreEqual(authManager.Username, "test");
        }

        [Test]
        public void Property_AuthProvider()
        {
            var authProviderMock = new Mock<IAuthProvider>();
            var authManager = new AuthenticationManager(authProviderMock.Object);

            Assert.AreEqual(authProviderMock.Object, authManager.AuthProvider);
        }

        [Test]
        public void Property_HasAuthProvider()
        {
            var authProviderMock = new Mock<IAuthProvider>();
            var authManager = new AuthenticationManager(authProviderMock.Object);

            Assert.IsTrue(authManager.HasAuthProvider);
        }

        [Test]
        public void Property_LoginState()
        {
            var authProviderMock = new Mock<IAuthProvider>();
            var authManager = new AuthenticationManager(authProviderMock.Object);

            Assert.IsNotNull(authManager.LoginState);
        }

        #endregion
    }

    [TestFixture]
    public class AuthServicesTests : DynamoModelTestBase
    {

        protected override DynamoModel.IStartConfiguration CreateStartConfiguration(IPreferences settings)
        {
            var authProviderMock = new Mock<IAuthProvider>();
            authProviderMock.Setup(x => x.LoginState).Returns(LoginState.LoggedIn);
            authProviderMock.As<IOAuth2AccessTokenProvider>().Setup(x => x.GetAccessToken()).Returns("faketoken");


            return new DynamoModel.DefaultStartConfiguration()
            {
                PathResolver = pathResolver,
                StartInTestMode = true,
                GeometryFactoryPath = preloader.GeometryFactoryPath,
                Preferences = settings,
                ProcessMode = TaskProcessMode.Synchronous,
                AuthProvider = authProviderMock.Object
            };
        }

        [Test]
        public void Test_OnRequestAuthProvider_FindsAuthProvider()
        {
#pragma warning disable AUTH_SERVICES
#pragma warning disable REQUEST_AUTHPROVIDER
            var result = AuthServices.AuthProvider;
#pragma warning restore REQUEST_AUTHPROVIDER
#pragma warning restore AUTH_SERVICES

            Assert.AreEqual((result as IOAuth2AccessTokenProvider).GetAccessToken(), "faketoken");
        }
    }
}
