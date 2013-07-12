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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Greg;

namespace Dynamo.PackageManager
{
    /// <summary>
    /// Interaction logic for PackageManagerLoginView.xaml
    /// </summary>
    public partial class PackageManagerLoginView : UserControl
    {
        private PackageManagerLoginViewModel viewModel;

        public PackageManagerLoginView( PackageManagerLoginViewModel viewModel )
        {
            InitializeComponent();
            this.DataContext = this.viewModel = viewModel;
            this.webBrowser.LoadCompleted += viewModel.WebBrowserNavigatedEvent;
            this.LoginContainerStackPanel.IsVisibleChanged += delegate { if (this.LoginContainerStackPanel.Visibility == Visibility.Visible) viewModel.NavigateToLogin(); };
        }

        /// <summary>
        ///     The method called when the browser changes its url.
        /// </summary>
        /// <param name="sender">Originating object for the event </param>
        /// <param name="e">Parameters describing the navigation</param>
        public void WebBrowserNavigatedEvent(object sender, NavigationEventArgs e)
        {
            viewModel.WebBrowserNavigatedEvent(this.webBrowser, e);
        }


    }
}
