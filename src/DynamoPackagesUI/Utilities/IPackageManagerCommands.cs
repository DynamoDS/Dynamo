using CefSharp.Wpf;
using Dynamo.Models;
using Dynamo.PackageManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Dynamo.DynamoPackagesUI.Utilities
{
    internal interface IPackageManagerCommands
    {
        DynamoPackagesUIClient Client { get; set; }

        DynamoModel Model { get; set; }

        PackageLoader Loader { get; set; }

        //CEF Browser instance for rendering PM web UI
        ChromiumWebBrowser Browser { get; set; }

        string ProductName { get; }

        string SessionData { get; }

        Window ParentWindow { get; set; }

        //For Install Package command initalize this variable 
        dynamic DownloadRequest { get; set; }
        
        //For all other commands initialize this variable
        dynamic PackageRequest { get; set; }

        void InstallPackage(string downloadPath);

        string PackageOnExecuted(dynamic asset, dynamic version);

        bool Uninstall();

        void GoToRootDirectory();

        void UnmarkForUninstallation();

        string GetCustomPathForInstall();

    }
}
