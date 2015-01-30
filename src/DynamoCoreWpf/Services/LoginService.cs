using System;
using System.Threading;
using System.Windows;
using Dynamo.Properties;

namespace Dynamo.Applications.Authentication
{
    internal class LoginService
    {
        private readonly SynchronizationContext context;
        private readonly Window parent;

        public LoginService(Window parent, SynchronizationContext context)
        {
            this.parent = parent;
            this.context = context;
        }

        public bool ShowLogin(object o)
        {
            var url = o as Uri;

            if (o == null) throw new ArgumentException("Invalid URL for login page!");

            var navigateSuccess = false;

            // show the login
            context.Send((_) => {

                var window = new BrowserWindow(url)
                {
                    Title = Resources.AutodeskSignIn,
                    Owner = parent,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                window.Browser.Navigated += (sender, args) =>
                {
                    // if the user reaches this path, they've successfully logged in
                    // note that this is necessary, but not sufficient for full authentication
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
