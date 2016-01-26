using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Greg;
using Greg.AuthProviders;

namespace Dynamo.Core
{
    public class AuthenticationManager
    {
        private readonly IAuthProvider authProvider;

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

        /// <summary>
        /// Check if able to toggle login state
        /// </summary>
        internal bool CanToggleLoginState(object o)
        {
            return this.LoginState == LoginState.LoggedOut || this.LoginState == LoginState.LoggedIn;
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
