using Dynamo.Core;
using Greg.AuthProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamo.Publish.Models
{
    public class PublishModel
    {
        private AuthenticationManager manager;

        #region Initialization

        public PublishModel(AuthenticationManager manager)
        {
            manager = new AuthenticationManager(new OxygenProvider("https://accounts-staging.autodesk.com/"));
        }

        #endregion

        internal void Authenticate()
        {
            if (manager.HasAuthProvider)
            {
                manager.AuthProvider.Login();
            }
        }
    }
}
