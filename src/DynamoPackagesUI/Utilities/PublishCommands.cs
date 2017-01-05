using Dynamo.DynamoPackagesUI.ViewModels;
using Dynamo.Models;
using Dynamo.PackageManager;
using Dynamo.PackageManager.Interfaces;
using Dynamo.ViewModels;
using Dynamo.Wpf.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.DynamoPackagesUI.Utilities
{
    /// <summary>
    /// CEF Class to assist Publishing the Dynamo Packages
    /// </summary>
    internal class PublishCommands
    {
        //private readonly DynamoViewModel dynamoViewModel;
        private MutatingFileCompressor fileCompressor;
        private IFileInfo fileToUpload;

        private dynamic _versionCustomData;

        private PackageManagerViewModel packageMgrViewModel { get; set; }

        public PublishCommands(PackageLoader loader, DynamoModel model) 
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
