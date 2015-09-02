using System;
using System.Threading;
using System.Windows;

namespace Dynamo.Wpf.Authentication
{
    public class LoginService
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
            var url = new Uri(o.ToString());

            if (o == null) throw new ArgumentException(Dynamo.Wpf.Properties.Resources.InvalidLoginUrl);

            var navigateSuccess = false;

            // show the login
            context.Send((_) => {

                var window = new BrowserWindow(url)
                {
                    Title = Dynamo.Wpf.Properties.Resources.AutodeskSignIn,
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
