using System;
using System.Threading;
using System.Windows;
using Dynamo.Wpf.Properties;

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
            if (o == null) throw new ArgumentException(Resources.InvalidLoginUrl);

            // URL shouldn't be empty.
            // URL can be empty, if user's local date is incorrect.
            // This a known bug, described here: https://github.com/DynamoDS/Dynamo/pull/6112
            if (o.ToString().Length == 0)
            {
                MessageBox.Show(Resources.InvalidTimeZoneMessage,
                                Resources.InvalidLoginUrl,
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return false;
            }

            var url = new Uri(o.ToString());

            var navigateSuccess = false;

            // show the login
            context.Send(_ => {

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
