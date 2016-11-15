using Dynamo.PackageManager;
using Dynamo.PackageManager.Interfaces;
using Dynamo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.PackageManager.Utilities
{
    internal class PublishCefHelper : CefHelper
    {
        //private readonly DynamoViewModel dynamoViewModel;
        private MutatingFileCompressor fileCompressor;
        private IFileInfo fileToUpload;

        private dynamic _versionCustomData;

        private PackageManagerViewModel packageMgrViewModel { get; set; }

        public PublishCefHelper(DynamoViewModel dynamoViewModel, PackageLoader model, PackageManagerViewModel pkgManagerViewModel) : base(dynamoViewModel, model, pkgManagerViewModel)
        {
            fileCompressor = new MutatingFileCompressor();
            //customNodeDefinitions = new List<CustomNodeDefinition>();
            //Dependencies = new List<PackageDependency>();
            //Assemblies = new List<PackageAssembly>();
            //PackageAssemblyNodes = new List<TypeLoadData>();
            //FilesToUpload = new List<string>();
        }
    }

}
