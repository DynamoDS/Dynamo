using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Threading;
using Dynamo.Controls;
using Dynamo.PackageManager;
using Dynamo.Search;

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

            Client.Client.GetRequestTokenAsync((uri, token) => View.Dispatcher.Invoke((Action) (() =>
                {
                    this.AuthorizeUrl = this.View.webBrowser.Source = uri;
                })));
        }
        
        public void WebBrowserNavigatedEvent(object sender, NavigationEventArgs e)
        {
            View.webBrowser.Visibility = Visibility.Visible;

            if (View.webBrowser.Source.AbsoluteUri.IndexOf("Allow") > -1)
            {
                View.Visibility = Visibility.Hidden;
                Client.Client.GetAccessTokenAsync( (s) =>
                    {
                        Client.Client.IsAuthenticatedAsync((authed) => Console.WriteLine(authed)); // TODO: get rid of this stuff
                    });
            }
        }

    }
}
