using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using Dynamo.DynamoSandbox;
using Dynamo.PackageManager;
using Greg;
using Greg.AuthProviders;

namespace Dynamo.Applications.Authentication
{
    internal class LoginService
    {
        private readonly SynchronizationContext _context;
        private readonly Window _parent;

        public LoginService(Window parent, SynchronizationContext context)
        {
            this._parent = parent;
            this._context = context;
        }

        public bool ShowLogin(object o)
        {
            var url = o as Uri;

            if (o == null) throw new Exception("Invalid URL for login page!");

            var navigateSuccess = false;

            // show the login
            _context.Send((_) => {

                var window = new BrowserWindow(url)
                {
                    Owner = _parent,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                window.Browser.Navigated += (sender, args) =>
                {
                    // if the user reaches this path, they've successfully logged in
                    if (args.Uri.LocalPath == "/OAuth/Allow")
                    {
                        navigateSuccess = true;
                        window.Close();
                    }
                };

                window.ShowDialog();

            }, null);

            return navigateSuccess;
        }
    }
}
