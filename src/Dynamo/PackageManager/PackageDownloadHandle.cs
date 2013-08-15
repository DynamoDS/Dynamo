﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Dynamo.Utilities;
using Greg.Requests;
using Greg.Responses;
using Microsoft.Practices.Prism.ViewModel;
using RestSharp;

namespace Dynamo.PackageManager
{
    public class PackageDownloadHandle : NotificationObject
    {
        public enum State
        {
            Uninitialized, Downloading, Downloaded, Installing, Installed, Error
        }

        private string _errorString = "";
        public string ErrorString { get { return _errorString; } set { _errorString = value; RaisePropertyChanged("ErrorString"); } }

        private State _downloadState = State.Uninitialized;

        public State DownloadState
        {
            get { return _downloadState; }
            set
            {
                _downloadState = value;
                RaisePropertyChanged("DownloadState");
            }
        }

        public Greg.Responses.PackageHeader Header { get; private set; }
        public string Name { get { return Header.name; } }

        private string _downloadPath;
        public string DownloadPath { get { return _downloadPath; } set { _downloadPath = value; RaisePropertyChanged("DownloadPath"); } }

        private string _versionName;
        public string VersionName { get { return _versionName; } set { _versionName = value; RaisePropertyChanged("VersionName"); } }

        private PackageDownloadHandle()
        {
            
        }

        public PackageDownloadHandle(Greg.Responses.PackageHeader header, string version)
        {
            this.Header = header;
            this.DownloadPath = "";
            this.VersionName = version;
        }

        public void Start()
        {
            dynSettings.Controller.PackageManagerClient.DownloadAndInstall(this);
        }

        public void Error(string errorString)
        {
            this.DownloadState = State.Error;
            this.ErrorString = errorString;
        }
        
        public void Done( string filePath )
        {
            this.DownloadState = State.Downloaded;
            this.DownloadPath = filePath;
        }

        private string BuildInstallDirectoryString()
        {
            // assembly_path/dynamo_packages/package_name

            Assembly dynamoAssembly = Assembly.GetExecutingAssembly();
            string location = Path.GetDirectoryName(dynamoAssembly.Location);
            return Path.Combine(location, "dynamo_packages", this.Name);

        }

        public bool Extract( out Package pkg )
        {

            this.DownloadState = State.Installing;

            // unzip, place files
            var unzipPath = Greg.Utility.FileUtilities.UnZip(DownloadPath);
            if (!Directory.Exists(unzipPath))
            {
                throw new Exception("The package was found to be empty and was not installed.");
            }
            
            var installedPath = BuildInstallDirectoryString();
            Directory.CreateDirectory(installedPath);

            // Now create all of the directories
            foreach (string dirPath in Directory.GetDirectories(unzipPath, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(unzipPath, installedPath));

            // Copy all the files
            foreach (string newPath in Directory.GetFiles(unzipPath, "*.*", SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(unzipPath, installedPath));

            // provide handle to installed package 
            pkg = new Package(installedPath, Header.name, VersionName);

            return true;
        }

        // cancel, install, redownload

    }

}
