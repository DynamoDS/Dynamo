using Dynamo.DynamoPackagesUI.Utilities;
using Dynamo.PackageManager;
using Dynamo.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.DynamoPackagesUI.ViewModels
{
    /// <summary>
    /// Package Manager View Model
    /// </summary>
    public class PackageManagerViewModel
    {
        public const string PACKAGE_MANAGER_URL = "http://dynamopackagemanager.com.s3-website-us-east-1.amazonaws.com";
        public string Address { get; set; }

        internal PackageManagerCefHelper CefHelper { get; set; }

        internal PublishCefHelper PublishCompCefHelper { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dynamoViewModel"></param>
        /// <param name="model"></param>
        /// <param name="address"></param>
        public PackageManagerViewModel(DynamoViewModel dynamoViewModel, PackageLoader model, string address)
        {
            CefHelper = new PackageManagerCefHelper(dynamoViewModel, model, this);
            PublishCompCefHelper = new PublishCefHelper(dynamoViewModel, model, this);

            var path = this.GetType().Assembly.Location;
            var config = ConfigurationManager.OpenExeConfiguration(path);
            this.Address = PACKAGE_MANAGER_URL + "/#/" + address;
        }

        public PackageManagerViewModel(string address)
        {
            //CefHelper = new PackageManagerCefHelper(dynamoViewModel, model, this);
            //PublishCompCefHelper = new PublishCefHelper(dynamoViewModel, model, this);

            var path = this.GetType().Assembly.Location;
            var config = ConfigurationManager.OpenExeConfiguration(path);
            this.Address = PACKAGE_MANAGER_URL + "/#/" + address;
        }
    }
}
