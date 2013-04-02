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
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Dynamo.Utilities;
using Greg.Responses;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo.PackageManager
{
    /// <summary>
    /// The ViewModel for Package publishing </summary>
    public class PackageManagerPublishViewModel : NotificationObject
    {
        #region Properties

            // <summary>
            /// Client property </summary>
            /// <value>
            /// The PackageManagerClient object for performing OAuth calls</value>
            public PackageManagerClient Client { get; internal set; }

            /// <summary>
            /// IsNewVersion property </summary>
            /// <value>
            /// Specifies whether we're negotiating uploading a new version </value>
            private bool _isNewVersion;
            public bool IsNewVersion
            {
                get { return _isNewVersion; }
                set
                {
                    if (this._isNewVersion != value)
                    {
                        this._isNewVersion = value;
                        this.RaisePropertyChanged(() => this.IsNewVersion);
                    }
                }
            }

            /// <summary>
            /// Visible property </summary>
            /// <value>
            /// Tells whether the publish UI is visible</value>
            private Visibility _visible;
            public Visibility Visible
            {
                get { return _visible; }
                set
                {
                    if (this._visible != value)
                    {
                        this._visible = value;
                        this.RaisePropertyChanged(() => this.Visible);
                    }
                }
            }

            /// <summary>
            /// Name property </summary>
            /// <value>
            /// The name of the node to be uploaded </value>
            public string Name { get { return (FunctionDefinition != null) ? FunctionDefinition.Workspace.Name : ""; }}

            /// <summary>
            /// Description property </summary>
            /// <value>
            /// The description to be uploaded </value>
            private string _Description;
            public string Description
            {
                get { return _Description; }
                set
                {
                    if (this._Description != value)
                    {
                        this._Description = value;
                        this.RaisePropertyChanged(() => this.Description);
                        ((DelegateCommand<object>)this.SubmitCommand).RaiseCanExecuteChanged();
                    }
                }
            }

            /// <summary>
            /// Keywords property </summary>
            /// <value>
            /// A string of space-delimited keywords</value>
            private string _Keywords;
            public string Keywords
            {
                get { return _Keywords; }
                set
                {
                    if (this._Keywords != value)
                    {
                        this._Keywords = value;
                        this.RaisePropertyChanged(() => this.Keywords);
                        KeywordList = value.Split(' ').Where(x => x.Length > 0).ToList();
                        ((DelegateCommand<object>)this.SubmitCommand).RaiseCanExecuteChanged();
                    }
                }
            }

            /// <summary>
            /// KeywordList property </summary>
            /// <value>
            /// A list of keywords, usually produced by parsing Keywords</value>
            public List<string> KeywordList { get; set; }

            /// <summary>
            /// FullVersion property </summary>
            /// <value>
            /// The major, minor, and build version joined into one string</value>
            public string FullVersion { get { return this.MajorVersion + "." + this.MinorVersion + "." + this.BuildVersion;  } }

            /// <summary>
            /// MinorVersion property </summary>
            /// <value>
            /// The second element of the version</value>
            private string _MinorVersion;
            public string MinorVersion
            {
                get { return _MinorVersion; }
                set
                {
                    if (this._MinorVersion != value)
                    {
                        this._MinorVersion = value;
                        this.RaisePropertyChanged(() => this.MinorVersion);
                        ((DelegateCommand<object>)this.SubmitCommand).RaiseCanExecuteChanged();
                    }
                }
            }

            /// <summary>
            /// BuildVersion property </summary>
            /// <value>
            /// The third element of the version</value>
            private string _BuildVersion;
            public string BuildVersion
            {
                get { return _BuildVersion; }
                set
                {
                    if (this._BuildVersion != value)
                    {
                        this._BuildVersion = value;
                        this.RaisePropertyChanged(() => this.BuildVersion);
                        ((DelegateCommand<object>)this.SubmitCommand).RaiseCanExecuteChanged();
                    }
                }
            }

            /// <summary>
            /// MajorVersion property </summary>
            /// <value>
            /// The first element of the version</value>
            private string _MajorVersion;
            public string MajorVersion
            {
                get { return _MajorVersion; }
                set
                {
                    if (this._MajorVersion != value)
                    {
                        this._MajorVersion = value;
                        this.RaisePropertyChanged(() => this.MajorVersion);
                        ((DelegateCommand<object>)this.SubmitCommand).RaiseCanExecuteChanged();
                    }
                }
            }

            /// <summary>
            /// FunctionDefinition property </summary>
            /// <value>
            /// The FuncDefinition for the current package to be uploaded</value>
            private FunctionDefinition _FunctionDefinition;
            public FunctionDefinition FunctionDefinition
            {
                get { return _FunctionDefinition; }
                set
                {
                    _FunctionDefinition = value;
                    this.RaisePropertyChanged(() => this.Name );
                    this.Visible = Visibility.Visible;
                    this.RaisePropertyChanged(() => this.Visible);
                }
            }

            /// <summary>
            /// PackageHeader property </summary>
            /// <value>
            /// The PackageHeader if we're uploading a new verson, setting this updates
            /// almost all of the fields in this object</value>
            private PackageHeader _packageHeader;
            public PackageHeader PackageHeader { get { return _packageHeader; }
            set
            {
                this.IsNewVersion = true;
                this.Description = value.description;
                string[] versionSplit = value.versions[value.versions.Count - 1].version.Split('.');
                this.MajorVersion = versionSplit[0];
                this.MinorVersion = versionSplit[1];
                this.BuildVersion = versionSplit[2];
                this.Keywords = String.Join(" ", value.keywords);
                this._packageHeader = value;
            }}

        #endregion

        /// <summary>
        /// The class constructor. </summary>
        /// <param name="client"> Reference to to the PackageManagerClient object for the app </param>
        public PackageManagerPublishViewModel(PackageManagerClient client)
        {
            Client = client;
            this.SubmitCommand = new DelegateCommand<object>(this.OnSubmit, this.CanSubmit);
            this.Clear();
            this.Visible = Visibility.Collapsed;
        }

        /// <summary>
        /// Clear all of the properties displayed to the user</summary>
        public void Clear()
        {
            this.IsNewVersion = false;
            this.Keywords = "";
            this.KeywordList = new List<string>();
            this.Description = "";
            this.MinorVersion = "";
            this.MajorVersion = "";
            this.BuildVersion = "";
        }

        /// <summary>
        /// SubmitCommand property </summary>
        /// <value>
        /// A command which, when executed, submits the current package</value>
        public ICommand SubmitCommand { get; private set; }

        /// <summary>
        /// Delegate used to submit the element</summary>
        private void OnSubmit(object arg)
        {
            if (!this.IsNewVersion)
            {
                var pkg = Client.GetPackageUpload(this.FunctionDefinition,
                                                                        this.FullVersion,
                                                                        this.Description, this.KeywordList, "MIT", "global");
                if (pkg != null)
                {
                    Client.Publish(pkg, this.FunctionDefinition);
                    dynSettings.Controller.PackageManagerClient.ShowPackageControlInformation();
                    this.Visible = Visibility.Collapsed;
                }
            }
            else // new version
            {
                var pkgVersion = Client.GetPackageVersionUpload(this.FunctionDefinition,
                                                                this.PackageHeader,
                                                                this.FullVersion,
                                                                this.Description, this.KeywordList, "MIT", "global");
                if (pkgVersion != null)
                {
                    Client.Publish(pkgVersion);
                    dynSettings.Controller.PackageManagerClient.ShowPackageControlInformation();
                    this.Visible = Visibility.Collapsed;
                }
            }
        }

        /// <summary>
        /// Delegate used to submit the element </summary>
        private bool CanSubmit(object arg)
        {
            return (this.Client.IsLoggedIn && this.Description.Length > 3 && this.Name.Length > 0 && this.KeywordList.Count > 0 && 
                    this.MinorVersion.Length > 0 && this.MajorVersion.Length > 0 && this.BuildVersion.Length > 0);
        }

    }

}
