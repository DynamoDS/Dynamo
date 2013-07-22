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
using System.Runtime.InteropServices;
using System.Threading;
using Dynamo.Utilities;
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
        private bool _browserVisible;

        /// <summary>
        ///     Visible property
        /// </summary>
        /// <value>
        ///     Tells whether the login UI is visible
        /// </value>
        private bool _visible;

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
        public bool BrowserVisible
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
        public bool Visible
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
            BrowserUri = new Uri("http://www.google.com");
            Visible = false;
            BrowserVisible = false;
        }

        /// <summary>
        ///     Shows the browser if hidden and navigates to Oxygen login, using PackageManagerClient's uri data.
        /// </summary>
        public void NavigateToLogin()
        {
            ThreadStart start = () => Client.Client.GetRequestTokenAsync((uri, token) =>
                {
                    BrowserUri = uri;
                }, Greg.Client.AuthorizationPageViewMode.Iframe);
            new Thread(start).Start();
        }

        /// <summary>
        ///     Do some fancy stuff with COM
        /// </summary>
        [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IOleServiceProvider
        {
            [PreserveSig]
            int QueryService([In] ref Guid guidService, [In] ref Guid riid, [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);
        }


        public void ShowLogin(object parameter)
        {
            Visible = true;
        }

        internal bool CanShowLogin(object parameter)
        {
            return true;
        }

        public void Login(object parameter)
        {

        }

        internal bool CanLogin(object parameter)
        {
            return true;
        }
    }
}