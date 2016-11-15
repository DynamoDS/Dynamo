using Dynamo.PackageManager.Utilities;
using Dynamo.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.PackageManager
{
    public class PackageManagerViewModel
    {
        public string Address { get; set; }

        internal PackageManagerCefHelper CefHelper { get; set; }

        internal PublishCefHelper PublishCompCefHelper { get; set; }

        public PackageManagerViewModel(DynamoViewModel dynamoViewModel, PackageLoader model, string address)
        {
            CefHelper = new PackageManagerCefHelper(dynamoViewModel, model, this);
            PublishCompCefHelper = new PublishCefHelper(dynamoViewModel, model, this);

            var path = this.GetType().Assembly.Location;
            var config = ConfigurationManager.OpenExeConfiguration(path);
            this.Address = config.AppSettings.Settings["packageManagerWebAddress"].Value + "/#/" + address;
        }
        public PackageManagerViewModel(string address)
        {
            //CefHelper = new PackageManagerCefHelper(dynamoViewModel, model, this);
            //PublishCompCefHelper = new PublishCefHelper(dynamoViewModel, model, this);

            var path = this.GetType().Assembly.Location;
            var config = ConfigurationManager.OpenExeConfiguration(path);
            this.Address = config.AppSettings.Settings["packageManagerWebAddress"].Value + "/#/" + address;
        }
    }
}
