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

    public class PackageManagerPublishViewModel : NotificationObject
    {
        #region Properties

            public PackageManagerClient Client { get; internal set; }

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

            public string Name { get { return (FunctionDefinition != null) ? FunctionDefinition.Workspace.Name : ""; }}
            
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

            public List<string> KeywordList { get; set; }

            public string FullVersion { get { return this.MajorVersion + "." + this.MinorVersion + "." + this.BuildVersion;  } }

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

            // required to upload a new version
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

        public PackageManagerPublishViewModel(PackageManagerClient client)
        {
            Client = client;
            this.SubmitCommand = new DelegateCommand<object>(this.OnSubmit, this.CanSubmit);
            this.Clear();
            this.Visible = Visibility.Collapsed;
        }

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
        
        public ICommand SubmitCommand { get; private set; }

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
                    Client.Publish(pkgVersion, this.FunctionDefinition);
                    dynSettings.Controller.PackageManagerClient.ShowPackageControlInformation();
                    this.Visible = Visibility.Collapsed;
                }
            }
        }

        private bool CanSubmit(object arg)
        {
            return (this.Client.IsLoggedIn && this.Description.Length > 3 && this.Name.Length > 0 && this.KeywordList.Count > 0 && 
                    this.MinorVersion.Length > 0 && this.MajorVersion.Length > 0 && this.BuildVersion.Length > 0);
        }

    }

}
