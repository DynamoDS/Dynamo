using Dynamo.Core;

using Greg;

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

            authManager.ToggleLoginState(authManager);
            Assert.True(loginCalled);
        }

        [Test]
        public void CanToggleLoginState()
        {
            var authProviderMock = new Mock<IAuthProvider>();
            var authManager = new AuthenticationManager(authProviderMock.Object);

            Assert.IsTrue(authManager.CanToggleLoginState(authManager));
        }

        #endregion
    }
}
