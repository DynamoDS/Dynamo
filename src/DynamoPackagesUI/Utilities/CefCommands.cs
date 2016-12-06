using CefSharp.Wpf;
using Dynamo.DynamoPackagesUI.ViewModels;
using Dynamo.Models;
using Dynamo.PackageManager;
using Dynamo.ViewModels;
using Dynamo.Wpf.Interfaces;
using DynamoPackagesUI.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Dynamo.DynamoPackagesUI.Utilities
{
    /// <summary>
    /// Base class for CEF which assits in all Package Manager functionalities
    /// </summary>
    internal class CefCommands
    {
        internal DynamoPackagesUIClient Client { get; set; }

        //internal readonly DynamoViewModel dynamoViewModel;
        internal IBrandingResourceProvider ResourceProvider { get; set; }

        internal DynamoModel Model { get; set; }

        internal PackageLoader Loader { get; private set; }

        //CEF Browser instance for rendering PM web UI
        public ChromiumWebBrowser Browser { get; set; }

        public string SessionData
        {
            get
            {
                //return JsonConvert.SerializeObject(dynamoViewModel.PackageLoader.GetPackageManagerExtension().PackageManagerClient.GetSession());
                return JsonConvert.SerializeObject(new Dictionary<string, string>());
            }
        }

        public Window ParentWindow { get; set; }


        public CefCommands(IBrandingResourceProvider resourceProvider, PackageLoader loader, DynamoModel model)
        {
            this.ResourceProvider = resourceProvider;
            this.Loader = loader;
            this.Model = model;

            this.Client = new DynamoPackagesUIClient();
        }

        public string InstalledPackages
        {
            get { return JsonConvert.SerializeObject(Loader.LocalPackages.ToList()); }
        }

        public bool Login()
        {
            return false;
            //return dynamoViewModel.Loader.AuthenticationManager.AuthProvider.Login();
        }
    }
    
}
