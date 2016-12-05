using CefSharp.Wpf;
using Dynamo.DynamoPackagesUI.ViewModels;
using Dynamo.PackageManager;
using Dynamo.ViewModels;
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
    internal class CefHelper
    {
        internal readonly DynamoViewModel dynamoViewModel;
        internal PackageLoader Model { get; private set; }

        //CEF Browser instance in which PM UI will be displayed
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

        public CefHelper(DynamoViewModel dynamoViewModel, PackageLoader model, PackageManagerViewModel packageMgrViewModel)
        {
            this.dynamoViewModel = dynamoViewModel;
            this.Model = model;
            this.PackageMgrViewMdodel = packageMgrViewModel;
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
