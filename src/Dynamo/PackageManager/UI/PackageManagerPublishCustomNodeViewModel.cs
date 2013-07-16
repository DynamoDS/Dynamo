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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Dynamo.PackageManager.UI;
using Dynamo.Utilities;
using Greg.Requests;
using Greg.Responses;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo.PackageManager
{
    /// <summary>
    /// The ViewModel for Package publishing </summary>
    public class PackageManagerPublishCustomNodeViewModel : NotificationObject
    {
        #region Properties


            /// <summary>
            /// This dialog is in one of two states.  Uploading or the user is filling out the dialog
            /// </summary>
            private bool _uploading = false;
            public bool Uploading
            {
                get { return _uploading; }
                set
                {
                    if (this._uploading != value)
                    {
                        this._uploading = value;
                        this.RaisePropertyChanged("Uploading");
                    }
                }
            }

            /// <summary>
            /// A handle for the package upload so the user can know the state of the upload.
            /// </summary>
            public PackageUploadHandle UploadHandle { get; set; }

            /// <summary>
            /// Client property 
            /// </summary>
            /// <value>
            /// The PackageManagerClient object for performing OAuth calls</value>
            public PackageManagerClient Client { get; internal set; }

            /// <summary>
            /// IsNewVersion property </summary>
            /// <value>
            /// Specifies whether we're negotiating uploading a new version </value>
            private bool _isNewVersion = false;
            public bool IsNewVersion
            {
                get { return _isNewVersion; }
                set
                {
                    if (this._isNewVersion != value)
                    {
                        this._isNewVersion = value;
                        this.RaisePropertyChanged("IsNewVersion");
                    }
                }
            }

            /// <summary>
            /// Dependencies property </summary>
            /// <value>
            /// The set of dependencies  </value>
            private PackageDependencyRootViewModel _dependencies = null;
            public List<PackageDependencyRootViewModel> Dependencies
            {
                get { 
                    _dependencies = _dependencies ?? new PackageDependencyRootViewModel(FunctionDefinition);

                    return new List<PackageDependencyRootViewModel>()
                        {
                            _dependencies
                        };
                }
            }

            /// <summary>
            /// AdditionalFiles property </summary>
            /// <value>
            /// Tells whether the publish UI is visible</value>
            private ObservableCollection<string> _additionalFiles = new ObservableCollection<string>();
            public ObservableCollection<string> AdditionalFiles
            {
                get { return _additionalFiles; }
                set
                {
                    if (this._additionalFiles != value)
                    {
                        this._additionalFiles = value;
                        this.RaisePropertyChanged("AdditionalFiles");
                    }
                }
            }

            /// <summary>
            /// Visible property </summary>
            /// <value>
            /// Tells whether the publish UI is visible</value>
            private Visibility _visible = Visibility.Visible;
            public Visibility Visible
            {
                get { return _visible; }
                set
                {
                    if (this._visible != value)
                    {
                        this._visible = value;
                        this.RaisePropertyChanged("Visible");
                    }
                }
            }

            /// <summary>
            /// Name property </summary>
            /// <value>
            /// The name of the node to be uploaded </value>
            private string _name = "";
            public string Name
            {
                get { return _name; }
                set
                {
                    if (this._name != value)
                    {
                        this._name = value;
                        this.RaisePropertyChanged("Name");
                        ((DelegateCommand<object>)this.SubmitCommand).RaiseCanExecuteChanged();
                    }
                }
            }

            /// <summary>
            /// Name property </summary>
            /// <value>
            /// The name of the node to be uploaded </value>
            private string _group = "";
            public string Group
            {
                get { return _group; }
                set
                {
                    if (this._group != value)
                    {
                        this._group = value;
                        this.RaisePropertyChanged("Group");
                        ((DelegateCommand<object>)this.SubmitCommand).RaiseCanExecuteChanged();
                    }
                }
            }

            /// <summary>
            /// Description property </summary>
            /// <value>
            /// The description to be uploaded </value>
            private string _Description = "";
            public string Description
            {
                get { return _Description; }
                set
                {
                    if (this._Description != value)
                    {
                        this._Description = value;
                        this.RaisePropertyChanged("Description");
                        ((DelegateCommand<object>)this.SubmitCommand).RaiseCanExecuteChanged();
                    }
                }
            }

            /// <summary>
            /// Keywords property </summary>
            /// <value>
            /// A string of space-delimited keywords</value>
            private string _Keywords = "";
            public string Keywords
            {
                get { return _Keywords; }
                set
                {
                    if (this._Keywords != value)
                    {
                        value = value.Replace(',', ' ').ToLower().Trim();
                        var options = RegexOptions.None;
                        var regex = new Regex(@"[ ]{2,}", options);
                        value = regex.Replace(value, @" ");

                        this._Keywords = value;
                        this.RaisePropertyChanged("Keywords");
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
            private string _MinorVersion = "";
            public string MinorVersion
            {
                get { return _MinorVersion; }
                set
                {
                    if (this._MinorVersion != value)
                    {
                        int val;
                        if (!Int32.TryParse(value, out val) || value == "") return;

                        this._MinorVersion = value;
                        this.RaisePropertyChanged("MinorVersion");
                        ((DelegateCommand<object>)this.SubmitCommand).RaiseCanExecuteChanged();
                    }
                }
            }

            /// <summary>
            /// BuildVersion property </summary>
            /// <value>
            /// The third element of the version</value>
            private string _BuildVersion = "";
            public string BuildVersion
            {
                get { return _BuildVersion; }
                set
                {
                    if (this._BuildVersion != value)
                    {
                        int val;
                        if (!Int32.TryParse(value, out val) || value == "") return;

                        this._BuildVersion = value;
                        this.RaisePropertyChanged("BuildVersion");
                        ((DelegateCommand<object>)this.SubmitCommand).RaiseCanExecuteChanged();
                    }
                }
            }

            /// <summary>
            /// MajorVersion property </summary>
            /// <value>
            /// The first element of the version</value>
            private string _MajorVersion = "";
            public string MajorVersion
            {
                get { return _MajorVersion; }
                set
                {
                    if (this._MajorVersion != value)
                    {
                        int val;
                        if (!Int32.TryParse(value, out val) || value == "") return;

                        this._MajorVersion = value;
                        this.RaisePropertyChanged("MajorVersion");
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
                    this.Name = FunctionDefinition.Workspace.Name;
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
                this.Name = value.name;
                this.Keywords = String.Join(" ", value.keywords);
                this._packageHeader = value;
            }}

        #endregion

        /// <summary>
        /// The class constructor. </summary>
        /// <param name="client"> Reference to to the PackageManagerClient object for the app </param>
        public PackageManagerPublishCustomNodeViewModel(PackageManagerClient client)
        {
            Client = client;

            this.SubmitCommand = new DelegateCommand<object>(this.Submit, this.CanSubmit);
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
        private void Submit(object arg)
        {

            var files = new List<string>();
            var deps = new List<PackageDependency>();
            var handle =  Client.PublishNewPackage(   
                                                    this.IsNewVersion,
                                                    this.Name,
                                                    this.FullVersion,
                                                    this.Description,
                                                    this.KeywordList,
                                                    "MIT",
                                                    this.Group,
                                                    files,
                                                    deps);
            this.Uploading = true;
            this.UploadHandle = handle;
            
        }

        private string _ErrorString = "";
        public string ErrorString
        {
            get { return _ErrorString; }
            set
            {
                if (this._ErrorString != value)
                {
                    this._ErrorString = value;
                    this.RaisePropertyChanged("ErrorString");
                }
            }
        }

        /// <summary>
        /// Delegate used to submit the element </summary>
        private bool CanSubmit(object arg)
        {

            if (Description.Length <= 10)
            {
                this.ErrorString = "Description must be longer than 10 characters.";
                return false;
            }

            if (this.Name.Length < 3)
            {
                this.ErrorString = "Name must be at least 3 characters.";
                return false;
            }

            if (this.MinorVersion.Length <= 0)
            {
                this.ErrorString = "You must provide a Minor version as a non-negative integer.";
                return false;
            }

            if (this.MajorVersion.Length <= 0)
            {
                this.ErrorString = "You must provide a Major version as a non-negative integer.";
                return false;
            }

            if (this.BuildVersion.Length <= 0)
            {
                this.ErrorString = "You must provide a Build version as a non-negative integer.";
                return false;
            }

            this.ErrorString = "";

            return true;
        }

    }

}
