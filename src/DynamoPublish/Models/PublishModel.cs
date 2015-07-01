using Dynamo.Core;
using Dynamo.Models;
using Dynamo.Wpf.Authentication;
using Greg.AuthProviders;
using Reach;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dynamo.Publish.Models
{
    public class PublishModel
    {
        private AuthenticationManager manager;
        public LoginService LoginService;

        public bool IsLoggedIn
        {
            get
            {
                return manager.LoginState == LoginState.LoggedIn;
            }
        }

        #region Initialization

        public PublishModel()
        {
            manager = new AuthenticationManager(new OxygenProvider("https://accounts-staging.autodesk.com/"));
        }

        #endregion

        internal void Authenticate()
        {
            if (manager.HasAuthProvider && LoginService != null)
            {
                manager.AuthProvider.RequestLogin += LoginService.ShowLogin;
                manager.AuthProvider.Login();
            }
        }

        internal void SendWorkspaces(IEnumerable<WorkspaceModel> workspaces)
        {
            string address = "http://10.39.165.198:3000/ws";
            var reachClient = new WorkspaceStorageClient(address, manager.Username);

            var result = reachClient.Send(workspaces.OfType<HomeWorkspaceModel>().First(), workspaces.OfType<CustomNodeWorkspaceModel>());
        }
        
    }
}
