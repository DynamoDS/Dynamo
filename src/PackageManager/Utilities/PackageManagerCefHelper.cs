using CefSharp;
using Dynamo.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static Dynamo.PackageManager.PackageDownloadHandle;

namespace Dynamo.PackageManager.Utilities
{
    internal class PackageManagerCefHelper : CefHelper
    {

        public PackageManagerCefHelper(DynamoViewModel dynamoViewModel, PackageLoader model, PackageManagerViewModel pkgManagerViewModel) : base(dynamoViewModel, model, pkgManagerViewModel)
        {
        }

        
    }
}
