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
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo.PackageManager
{
    /// <summary>
    ///     This is the core ViewModel for PackageManager login
    /// </summary>
    public class PackageManagerLoginViewModel : NotificationObject
    {
        #region Properties 

        /// <summary>
        ///     BrowserUri property
        /// </summary>
        /// <value>
        ///     The current location of the browser, observed by the UI
        /// </value>
        private Uri _browserUri;

        /// <summary>
        ///     BrowserVisible property
        /// </summary>
        /// <value>
        ///     Tells whether the browser is visible
        /// </value>
        private Visibility _browserVisible;

        /// <summary>
        ///     Visible property
        /// </summary>
        /// <value>
        ///     Tells whether the login UI is visible
        /// </value>
        private Visibility _visible;

        /// <summary>
        ///     Client property
        /// </summary>
        /// <value>
        ///     The PackageManagerClient object for performing OAuth calls
        /// </value>
        public PackageManagerClient Client { get; internal set; }

        /// <summary>
        ///     BrowserUri property
        /// </summary>
        /// <value>
        ///     The current uri for the browser
        /// </value>
        public Uri BrowserUri
        {
            get { return _browserUri; }
            set
            {
                if (_browserUri != value)
                {
                    _browserUri = value;
                    RaisePropertyChanged("BrowserUri");
                }
            }
        }

        /// <summary>
        ///     BrowserVisible property
        /// </summary>
        /// <value>
        ///     Specifies whether the browser is visible, observed by View
        /// </value>
        public Visibility BrowserVisible
        {
            get { return _browserVisible; }
            set
            {
                if (_browserVisible != value)
                {
                    _browserVisible = value;
                    RaisePropertyChanged("BrowserVisible");
                }
            }
        }

        /// <summary>
        ///     Visible property
        /// </summary>
        /// <value>
        ///     Specifies whether the View is visible, observed by View
        /// </value>
        public Visibility Visible
        {
            get { return _visible; }
            set
            {
                if (_visible != value)
                {
                    _visible = value;
                    RaisePropertyChanged("Visible");
                }
            }
        }

        #endregion

        /// <summary>
        ///     The class constructor.
        /// </summary>
        /// <param name="client"> Reference to to the PackageManagerClient object for the app </param>
        public PackageManagerLoginViewModel(PackageManagerClient client)
        {
            Client = client;
            BrowserUri = new Uri("http://www.autodesk.com");
            Visible = Visibility.Collapsed;
            BrowserVisible = Visibility.Collapsed;
        }

        /// <summary>
        ///     Shows the browser if hidden and navigates to Oxygen login, using PackageManagerClient's uri data.
        /// </summary>
        public void NavigateToLogin()
        {
            ThreadStart start = () => Client.Client.GetRequestTokenAsync((uri, token) =>
                {
                    BrowserVisible = Visibility.Visible;
                    BrowserUri = uri;
                }, Greg.Client.AuthorizationPageViewMode.Desktop);
            new Thread(start).Start();
        }

        /// <summary>
        ///     The method called when the browser changes its url.
        /// </summary>
        /// <param name="sender">Originating object for the event </param>
        /// <param name="e">Parameters describing the navigation</param>
        public void WebBrowserNavigatedEvent(object sender, NavigationEventArgs e)
        {
            if (e.Uri.AbsoluteUri.IndexOf("Allow", StringComparison.Ordinal) > -1)
            {
                Visible = Visibility.Hidden;
                Client.GetAccessToken();
            }
        }
    }

    /// <summary>
    ///     A helper utility class used to help bind the WebBrowser to the ViewModel
    /// </summary>
    public static class WebBrowserUtility
    {
        public static readonly DependencyProperty BindableSourceProperty =
            DependencyProperty.RegisterAttached("BindableSource", typeof (string), typeof (WebBrowserUtility),
                                                new UIPropertyMetadata(null, BindableSourcePropertyChanged));

        public static string GetBindableSource(DependencyObject obj)
        {
            return (string) obj.GetValue(BindableSourceProperty);
        }

        public static void SetBindableSource(DependencyObject obj, string value)
        {
            obj.SetValue(BindableSourceProperty, value);
        }

        public static void BindableSourcePropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var browser = o as WebBrowser;
            if (browser != null)
            {
                var uri = e.NewValue as string;
                browser.Source = uri != null ? new Uri(uri) : null;
            }
        }
    }
}