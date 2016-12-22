using Dynamo.DynamoPackagesUI.Utilities;
using Dynamo.Models;
using Dynamo.PackageManager;
using Dynamo.ViewModels;
using Dynamo.Wpf.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.DynamoPackagesUI.ViewModels
{
    /// <summary>
    /// Package Manager View Loader
    /// </summary>
    public class PackageManagerViewModel
    {
        public const string PACKAGE_MANAGER_URL = "http://dynamopackagemanager.com.s3-website-us-east-1.amazonaws.com";
        
        public string Address { get; set; }

        internal IPackageManagerCommands PkgMgrCommands { get; set; }

        


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dynamoViewModel"></param>
        /// <param name="model"></param>
        /// <param name="address"></param>
        public PackageManagerViewModel(PackageLoader loader, DynamoModel model, string address)
        {
            PkgMgrCommands = new PackageManagerCommands(loader, model);

            var path = this.GetType().Assembly.Location;
            var config = ConfigurationManager.OpenExeConfiguration(path);
            this.Address = PACKAGE_MANAGER_URL + "/#/" + address;
        }

    }
}
