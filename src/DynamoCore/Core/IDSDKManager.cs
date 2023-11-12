using System;
using System.Diagnostics;
using Autodesk.IDSDK;
using Greg;
using Greg.AuthProviders;
using RestSharp;

namespace Dynamo.Core
{
    /// <summary>
    /// The class to provide auth APIs for IDSDK related methods.
    /// </summary>
    public class IDSDKManager : IOAuth2AuthProvider, IOAuth2AccessTokenProvider
    {
        /// <summary>
        /// Used by the auth provider to request authentication.
        /// </summary>
        public event Func<object, bool> RequestLogin;
        /// <summary>
        /// Tracks any change in the login status.
        /// </summary>
        public event Action<LoginState> LoginStateChanged;

        /// <summary>
        /// Returns the login status of the current session.
        /// </summary>
        /// <returns>LoginState Enum value</returns>
        public LoginState LoginState
        {
            get
            {
                var result = IDSDK_IsLoggedIn();
                return result ? LoginState.LoggedIn : LoginState.LoggedOut;
            }
        }

        /// <summary>
        /// Returns the login status of the current session.
        /// </summary>
        /// <returns>Boolean Status Value</returns>
        public bool IsLoggedIn()
        {
            return LoginState == LoginState.LoggedIn;
        }

        /// <summary>
        /// Triggers login using Auth API, if the user is not already logged in. 
        /// </summary>
        /// <returns>True, if login was successfull, else False</returns>
        public bool Login()
        {
            OnLoginStateChanged(LoginState.LoggingIn);
            var result = IDSDK_Login();
            OnLoginStateChanged(result ? LoginState.LoggedIn : LoginState.LoggedOut);
            return result;
        }

        /// <summary>
        /// Logs out the user from the current session.
        /// </summary>
        public void Logout()
        {
            var res = IDSDK_Logout();
            OnLoginStateChanged(LoginState);
        }

        /// <summary>
        /// Gets the username of the logged in user.
        /// </summary>
        public string Username
        {
            get
            {
                var result = IDSDK_GetUserInfo();
                return result != null ? result.UserName : String.Empty;
            }
        }

        /// <summary>
        /// Used by the auth provider to sign request with the authorized token.
        /// </summary>
        public void SignRequest(ref RestRequest m, RestClient client)
        {
            if (LoginState == LoginState.LoggedOut && !Login())
            {
                throw new Exception("You must be logged in, to use the Package Manager.");
            }
            m.AddHeader("Authorization", $"Bearer {IDSDK_GetToken()}");
        }
        public string GetAccessToken()
        {
            return IDSDK_GetToken();
        }

        /// <summary>
        /// Checks if the user is logged in and adds the token to request header.
        /// </summary>
        internal void LoginRequest(ref RestRequest m, RestClient client)
        {
            if (LoginState == LoginState.LoggedIn)
            {
                m.AddHeader("Authorization", $"Bearer {IDSDK_GetToken()}");          
            }
            else {
                throw new Exception("You must be logged in, to use this service.");
            }
        }

        private void OnLoginStateChanged(LoginState state)
        {
            if (LoginStateChanged != null)
            {
                LoginStateChanged(state);
            }
        }

        #region IDSDK methods
        private class IDSDK_User_Info {
            internal string UserId { get; set; }
            internal string UserFirstName { get; set; }
            internal string UserLastName { get; set; }
            internal string UserEmail { get; set; }
            internal string AnalyticsId { get; set; }
            internal string UserName { get; set; }
            internal IDSDK_User_Info(string userId, string userFirstName, string userLastName, string userEmail, string analyticsId, string userName) {
                UserId = userId;
                UserFirstName = userFirstName;
                UserLastName = userLastName;
                UserEmail = userEmail;
                AnalyticsId = analyticsId;
                UserName = userName;
            }
        }
        private bool IDSDK_Login()
        {
            if (IDSDK_IsLoggedIn())
            {
                return true;
            }
            else
            {
                if (Initialize())
                {
                    idsdk_status_code statusCode = Client.Login();
                    if (Client.IsSuccess(statusCode))
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        private bool IDSDK_IsLoggedIn()
        {
            if (Initialize())
            {
                bool ret = Client.IsLoggedIn();
                return ret;
            }
            return false;
        }
        private bool IDSDK_Logout()
        {
            if (IDSDK_IsLoggedIn())
            {
                idsdk_status_code statusCode = Client.Logout(idsdk_logout_flags.IDSDK_LOGOUT_MODE_SILENT);
                if (Client.IsSuccess(statusCode))
                {
                    Deinitialize();
                    return true;
                }
            }
            return false;
        }
        private IDSDK_User_Info IDSDK_GetUserInfo()
        {
            if (Client.IsInitialized() && Client.IsLoggedIn())
            {
                idsdk_status_code statusCode = Client.GetUserInfo(out bool loginState, out string userId, out string userFirstName, out string userLastName,
                    out string userEmail, out string analyticsId, out string userName, out uint loginExpireDay);

                IDSDK_User_Info strUserInfo = null;
                if (Client.IsSuccess(statusCode))
                {
                    strUserInfo = new IDSDK_User_Info(userId, userFirstName, userLastName, userEmail, analyticsId, userName);
                }
                return strUserInfo;
            }
            return null;
        }
        #endregion

        #region IDSDK Utilities
        private bool SetProductConfigs(string productLineCode, idsdk_server server, string oauthKey)
        {
            idsdk_status_code bRet = Client.SetProductConfig(oauthKey, "", productLineCode, DateTime.Now.Year.ToString(), "1.2.3.4", server);
            return Client.IsSuccess(bRet);
        }

        /// <summary>
        /// Returns the OAuth2 token for the current session, or an empty string if token is not available.
        /// </summary>
        private string IDSDK_GetToken()
        {
            idsdk_status_code ret = Client.GetToken(out string strToken);
            if (Client.IsSuccess(ret))
            {
                return strToken;
            }
            return String.Empty;
        }

        private bool Initialize()
        {
            if (Client.IsInitialized()) return true;
            idsdk_status_code bRet = Client.Init();

            if (Client.IsSuccess(bRet))
            {
                if (Client.IsInitialized())
                {
                    try
                    {
                        IntPtr hWnd = Process.GetCurrentProcess().MainWindowHandle;
                        if (hWnd != null)
                        {
                            Client.SetHost(hWnd);
                        }

                        bool ret = GetClientIDAndServer(out idsdk_server server, out string client_id);
                        if (ret) 
                        {
                            ret = SetProductConfigs("Dynamo", server, client_id);
                            Client.SetServer(server);
                            return ret;
                        }
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }
            return false;
        }
        private bool Deinitialize()
        {
            idsdk_status_code bRet = Client.DeInit();

            if (Client.IsSuccess(bRet))
            {
                return true;
            }
            return false;
        }
        private bool GetClientIDAndServer(out idsdk_server server, out string client_id)
        {
            server = idsdk_server.IDSDK_PRODUCTION_SERVER;
                
            client_id = DynamoUtilities.PathHelper.getServiceConfigValues(this, "IDSDK_CLIENT_ID");

            string env = DynamoUtilities.PathHelper.getServiceConfigValues(this, "IDSDK_ENVIRONMENT");
            if (!string.IsNullOrEmpty(env))
            {
                if (env.Trim().ToLower() == "stg")
                {
                    server = idsdk_server.IDSDK_STAGING_SERVER;
                }
                else if (env.Trim().ToLower() == "dev")
                {
                    server = idsdk_server.IDSDK_DEVELOPMENT_SERVER;
                }
            }
            return !string.IsNullOrEmpty(client_id);
        }
        #endregion
    }
}
