using System;
using System.Windows;
using System.Windows.Navigation;
using Dynamo.Controls;
using Dynamo.PackageManager;
using Dynamo.Utilities;
using RestSharp;

namespace Dynamo.Nodes.PackageManager
{
    public class PackageManagerLoginController
    {
        public PackageManagerClient Client { get; internal set; }
        public PackageManagerLoginUI View { get; internal set; }
        public dynBench Bench { get; internal set; }
        public Uri AuthorizeUrl { get; internal set;  }

        public PackageManagerLoginController(dynBench bench, PackageManagerClient client)
        {
            Client = client;
            View = new PackageManagerLoginUI(this);
            AuthorizeUrl = new Uri("http://www.autodesk.com");
            this.Bench = bench;
        }

        public void LoginButtonClick(object sender, RoutedEventArgs e)
        {
            NavigateToLogin();
        }

        public void NavigateToLogin()
        {
           Client.Client.GetRequestTokenAsync((uri, token) => View.Dispatcher.Invoke((Action) (() =>
               {
                   this.AuthorizeUrl = this.View.webBrowser.Source = uri;
               })), Greg.Client.AuthorizationPageViewMode.Desktop);
        }

        public void WebBrowserNavigatedEvent(object sender, NavigationEventArgs e)
        {
            View.webBrowser.Visibility = Visibility.Visible;

            if (View.webBrowser.Source.AbsoluteUri.IndexOf("Allow") > -1)
            {
                View.Visibility = Visibility.Hidden;
                Client.Client.GetAccessTokenAsync( (s) => Client.Client.IsAuthenticatedAsync((auth) => View.Dispatcher.Invoke((Action) (() =>
                    {
                        if (auth)
                        {
                            dynSettings.Bench.PackageManagerLoginState.Text = "Logged in";
                            dynSettings.Bench.PackageManagerLoginButton.IsEnabled = false;
                        }
                    }))));
            }
        }

    }
}
