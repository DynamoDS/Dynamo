using System;

using Greg;
using Greg.AuthProviders;

namespace Dynamo.Core
{
    /// <summary>
    ///     This is a wrapper for <see cref="IAuthProvider"/> functionality.
    ///     It is used for oxygen authentication.
    /// </summary>
    public class AuthenticationManager
    {
        private readonly IAuthProvider authProvider;

        /// <summary>
        ///     Occurs when login state is changed
        /// </summary>
        public event Action<LoginState> LoginStateChanged;

        /// <summary>
        ///     Determines if the this.client has login capabilities
        /// </summary>
        public bool HasAuthProvider
        {
            get { return authProvider != null; }
        }

        /// <summary>
        ///     Specifies whether the user is logged in or not.
        /// </summary>
        internal LoginState LoginState
        {
            get { return HasAuthProvider ? authProvider.LoginState : LoginState.LoggedOut; }
        }

        /// <summary>
        ///     The username of the current user, if logged in.  Otherwise null
        /// </summary>
        internal string Username
        {
            get { return HasAuthProvider ? authProvider.Username : ""; }
        }

        /// <summary>
        /// Current IAuthProvider
        /// </summary>
        public IAuthProvider AuthProvider
        {
            get { return authProvider; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationManager"/> class.
        /// </summary>
        /// <param name="authProvider">IAuthProvider functionality</param>
        public AuthenticationManager(IAuthProvider authProvider)
        {
            this.authProvider = authProvider;

            // The lack of AuthProvider indicates that the user cannot login for this
            // session.  Hence, we do not subscribe to this event.
            if (this.authProvider != null)
            {
                this.authProvider.LoginStateChanged += OnLoginStateChanged;
            }
        }


        /// <summary>
        /// Toggle current login state
        /// </summary>
        internal void ToggleLoginState(object o)
        {
            if (LoginState == LoginState.LoggedIn)
            {
                Logout();
            }
            else
            {
                Login();
            }
        }

        internal void Login()
        {
            if (!HasAuthProvider) return;
            this.authProvider.Login();
        }

        internal void Logout()
        {
            if (!HasAuthProvider) return;
            this.authProvider.Logout();
        }

        private void OnLoginStateChanged(LoginState status)
        {
            if (LoginStateChanged != null)
            {
                LoginStateChanged(status);
            }
        }

    }
}
