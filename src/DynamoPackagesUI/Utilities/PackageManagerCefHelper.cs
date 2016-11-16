using CefSharp;
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
    /// CEF calss to assist exploring packages, authors and logged in user packages.
    /// </summary>
    internal class PackageManagerCefHelper : CefHelper
    {

        public PackageManagerCefHelper(DynamoViewModel dynamoViewModel, PackageLoader model, PackageManagerViewModel pkgManagerViewModel) : base(dynamoViewModel, model, pkgManagerViewModel)
        {
        }

        
    }
}
