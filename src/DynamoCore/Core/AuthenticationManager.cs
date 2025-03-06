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
        private static LoginState loginStateInitial;

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
        public LoginState LoginState
        {
            get { return HasAuthProvider ? authProvider.LoginState : LoginState.LoggedOut; }
        }

        /// <summary>
        /// This Property will return the initial LoginState assigned in the constructor in order to be used in the Initialization flow. Others features require direct calls to the LoginState due to are on demand.
        /// </summary>
        public LoginState LoginStateInitial
        {
            get { return loginStateInitial; }
        }

        internal bool IsLoggedIn()
        {            
            return HasAuthProvider && authProvider.LoginState == LoginState.LoggedIn ? true : false;
        }

        /// <summary>
        /// This function will return the value by checking the LoginStateSingle property (which load its value only once time) used only in the initialization flow
        /// </summary>
        /// <returns></returns>
        internal bool IsLoggedInInitial()
        {
            return HasAuthProvider && LoginStateInitial == LoginState.LoggedIn ? true : false;
        }

        /// <summary>
        ///     The username of the current user, if logged in.  Otherwise null
        /// </summary>
        public string Username
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
                loginStateInitial = LoginState;
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
                loginStateInitial = status;
                LoginStateChanged(status);
            }
        }

        /// <summary>
        /// Returns whether the IDSDK is initialized or not for Dynamo Sandbox, in host environment defaults to true.
        /// </summary>
        internal bool IsIDSDKInitialized()
        {
            if (authProvider is IDSDKManager idsdkProvider)
            {
                if (!idsdkProvider.IsIDSDKInitialized)
                {
                    return false;
                }
            }
            return true;
        }

    }
}
