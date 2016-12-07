using CefSharp.Wpf;
using Dynamo.DynamoPackagesUI.ViewModels;
using Dynamo.PackageManager;
using Dynamo.ViewModels;
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
        internal DynamoPackagesUIClient DPClient { get; set; }

        internal readonly DynamoViewModel dynamoViewModel;
        internal PackageLoader Model { get; private set; }

        //CEF Browser instance for rendering PM web UI
        public ChromiumWebBrowser CefBrowser { get; set; }

        public string SessionData
        {
            get
            {
                //return JsonConvert.SerializeObject(dynamoViewModel.Model.GetPackageManagerExtension().PackageManagerClient.GetSession());
                return JsonConvert.SerializeObject(new Dictionary<string, string>());
            }
        }

        public Window ParentWindow { get; set; }

        public PackageManagerViewModel PackageMgrViewMdodel { get; set; }

        public CefCommands(DynamoViewModel dynamoViewModel, PackageLoader model, PackageManagerViewModel packageMgrViewModel)
        {
            this.dynamoViewModel = dynamoViewModel;
            this.Model = model;
            this.PackageMgrViewMdodel = packageMgrViewModel;
            this.DPClient = new DynamoPackagesUIClient();
        }

        public string InstalledPackages
        {
            get { return JsonConvert.SerializeObject(Model.LocalPackages.ToList()); }
        }

        public bool Login()
        {
            return false;
            //return dynamoViewModel.Model.AuthenticationManager.AuthProvider.Login();
        }
    }
    
}
