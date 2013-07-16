using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Dynamo.Utilities;
using Greg.Responses;
using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo.PackageManager
{
    public class PackageUploadHandle : NotificationObject
    {
        public enum State
        {
            Uninitialized, Uploading, Uploaded, Error
        }

        private string _errorString = "";
        public string ErrorString { get { return _errorString; } set { _errorString = value; RaisePropertyChanged("ErrorString"); } }

        private State _uploadState = State.Uninitialized;

        public State UploadState
        {
            get { return _uploadState; }
            set
            {
                _uploadState = value;
                RaisePropertyChanged("UploadState");
            }
        }

        public PackageHeader Header { get; private set; }
        public string Name { get { return Header.name; } }

        private string _versionName;
        public string VersionName { get { return _versionName; } set { _versionName = value; RaisePropertyChanged("VersionName"); } }

        public PackageUploadHandle(PackageHeader header, string version)
        {
            this.Header = header;
        }

        public void Start()
        {
            //dynSettings.Controller.PackageManagerClient.DownloadAndInstall(this);
        }

        public void Error(string errorString)
        {
            this.UploadState = State.Error;
            this.ErrorString = errorString;
        }

        public void Done(string filePath)
        {
            this.UploadState = State.Uploaded;
        }

    }



}
