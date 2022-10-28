using System;
using Autodesk.IDSDK;

namespace Dynamo.Core
{
    public class IDSDKManager
    {
        private const string IDSDK_CLIENT_ID = "IAtF1TBSlCeGqWAXKsBkcZBBwomALZsq";
        /// <summary>
        /// Triggers login using Auth API, if the user is not already logged in. 
        /// </summary>
        /// <returns></returns>
        public bool Login()
        {
            if (IsLoggedIn())
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

        /// <summary>
        /// Returns the login status of the current user.
        /// </summary>
        /// <returns>True, if logged in, else False</returns>
        public bool IsLoggedIn()
        {
            if (Initialize())
            {
                bool ret = Client.IsLoggedIn();
                return ret;
            }
            return false;
        }
        /// <summary>
        /// Logs out the user from the current session.
        /// </summary>
        public bool Logout()
        {
            if (IsLoggedIn())
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

        /// <summary>
        /// Returns the OAuth2 token for the current session.
        /// </summary>
        public string GetToken()
        {
            String strToken = String.Empty;

            idsdk_status_code bRet = Client.GetToken(out strToken);
            return strToken;
        }

        private bool SetProductConfigs(String productLineCode, idsdk_server server, String oauthKey)
        {
            idsdk_status_code bRet = Client.SetProductConfig(oauthKey, "", productLineCode, "2024", "1.2.3.4", server);
            return Client.IsSuccess(bRet);
        }

        private bool Initialize()
        {
            idsdk_status_code bRet = Client.Init();

            if (Client.IsSuccess(bRet))
            {
                if (Client.IsInitialized())
                {
                    bool ret = SetProductConfigs("Dynamo", idsdk_server.IDSDK_PRODUCTION_SERVER, IDSDK_CLIENT_ID);
                    return ret;
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
    }
}
