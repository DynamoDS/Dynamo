using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Dynamo.DynamoSandbox;
using Dynamo.PackageManager;
using Greg;
using Greg.AuthProviders;

namespace Dynamo.Applications.Authentication
{
    internal class LoginService
    {
        private SynchronizationContext context;

        public LoginService(SynchronizationContext context)
        {
            this.context = context;
        }

        public bool ShowLogin(object o)
        {
            var url = o as Uri;

            if (o == null) throw new Exception("Invalid URL for login page!");

            var success = false;

            context.Send((_) => {
                var window = new BrowserWindow(url);

                window.Browser.Navigated += (sender, args) =>
                {
                    if (args.Uri.LocalPath == "/OAuth/Allow")
                    {
                        success = true;
                        window.Close();
                    }
                };

                window.ShowDialog();
            }, null);

            return success;
        }
    }
}
