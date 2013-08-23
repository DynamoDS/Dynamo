//Copyright © Autodesk, Inc. 2012. All rights reserved.
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Dynamo.Utilities;
using Greg;

namespace Dynamo.PackageManager
{
    /// <summary>
    /// Interaction logic for PackageManagerLoginView.xaml
    /// </summary>
    public partial class PackageManagerLoginView : UserControl
    {
        private PackageManagerLoginViewModel viewModel;

        public PackageManagerLoginView()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(PackageManagerLoginView_Loaded);
        }

        void PackageManagerLoginView_Loaded(object sender, RoutedEventArgs e)
        {
            viewModel = (PackageManagerLoginViewModel) DataContext;

            this.webBrowser.LoadCompleted += WebBrowserNavigatedEvent;
            this.LoginContainerStackPanel.IsVisibleChanged += delegate { if (this.LoginContainerStackPanel.Visibility == Visibility.Visible) viewModel.NavigateToLogin(); };
        }

        /// <summary>
        ///     The method called when the browser changes its url.
        /// </summary>
        /// <param name="sender">Originating object for the event </param>
        /// <param name="e">Parameters describing the navigation</param>
        public void WebBrowserNavigatedEvent(object sender, NavigationEventArgs e)
        {
            SetSilent(((WebBrowser)sender), true);
            if (e.Uri.AbsoluteUri.IndexOf("google", StringComparison.Ordinal) > -1)
                return;

            viewModel.BrowserVisible = true;

            if (e.Uri.AbsoluteUri.IndexOf("Allow", StringComparison.Ordinal) > -1)
            {
                viewModel.Visible = false;
                viewModel.Client.GetAccessToken();
            }
        }


        /// <summary>
        ///     SILENCE the errors with the web browser
        /// </summary>
        /// <param name="browser"> The browser to silence </param>
        /// <param name="silent">Parameters describing the navigation</param>
        public static void SetSilent(WebBrowser browser, bool silent)
        {
            if (browser == null)
                throw new ArgumentNullException("browser");

            // get an IWebBrowser2 from the document
            var sp = browser.Document as PackageManagerLoginViewModel.IOleServiceProvider;
            if (sp != null)
            {
                Guid IID_IWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
                Guid IID_IWebBrowser2 = new Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E");

                object webBrowser;
                sp.QueryService(ref IID_IWebBrowserApp, ref IID_IWebBrowser2, out webBrowser);
                if (webBrowser != null)
                {
                    webBrowser.GetType().InvokeMember("Silent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.PutDispProperty, null, webBrowser, new object[] { silent });
                }
            }
        }

        ///// <summary>
        /////     The method called when the browser changes its url.
        ///// </summary>
        ///// <param name="sender">Originating object for the event </param>
        ///// <param name="e">Parameters describing the navigation</param>
        //public void WebBrowserNavigatedEvent(object sender, NavigationEventArgs e)
        //{
        //    viewModel.WebBrowserNavigatedEvent(this.webBrowser, e);
        //}


    }
}
