using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Reflection;
using Shapeways=ShapewaysClient.ShapewaysClient;
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

        internal void ShowShapewaysLogin(Shapeways client) 
        {
            context.Send((_) =>
            {

                var window = new BrowserWindow(new Uri(client.LoginUrl))
                {
                    Title = "Shapeways",
                    Owner = parent,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Height = 500,
                    Width = 1000
                };

                window.Browser.LoadCompleted += (sender, args) => 
                {
                    if (args.Uri.AbsolutePath == "/callbackDynamoShapeways") 
                    {
                        client.SetToken(args.Uri.PathAndQuery);
                        window.Close();
                    }
                };

                window.Browser.Loaded += (sender, args) =>
                {
                    HideScriptErrors(window.Browser, true);
                };

                window.ShowDialog();

            }, null);
        }

        /// <summary>
        /// Hides or allows displaying of script errors in WebBrowser control
        /// </summary>
        /// <param name="wb">WebBrowser object</param>
        /// <param name="Hide">Indicates whether scripts will be displayed or not</param>
        private void HideScriptErrors(WebBrowser wb, bool Hide)
        {
            FieldInfo fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fiComWebBrowser == null) return;
            object objComWebBrowser = fiComWebBrowser.GetValue(wb);
            if (objComWebBrowser == null) return;
            objComWebBrowser.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, objComWebBrowser, new object[] { Hide });
        }
    }
}
