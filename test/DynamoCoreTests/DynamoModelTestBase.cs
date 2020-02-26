using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.PackageManager;
using Dynamo.Scheduler;
using Dynamo.Search.SearchElements;
using Dynamo.Selection;
using Dynamo.Tests;
using Dynamo.Utilities;
using DynamoShapeManager;

using NUnit.Framework;
using TestServices;

namespace Dynamo
{
    public class DynamoModelTestBase : DSEvaluationUnitTestBase
    {
        protected DynamoModel CurrentDynamoModel { get; set; }

        protected Preloader preloader;
        protected TestPathResolver pathResolver;
        protected IPreferences dynamoSettings;

        protected override DynamoModel GetModel()
        {
            return CurrentDynamoModel;
        }

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            StartDynamo(dynamoSettings);
        }

        public override void Cleanup()
        {
            try
            {
                preloader = null;
                DynamoSelection.Instance.ClearSelection();

                if (CurrentDynamoModel != null)
                {
                    CurrentDynamoModel.ShutDown(false);
                    CurrentDynamoModel = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }

            base.Cleanup();
        }

        protected virtual void StartDynamo(IPreferences settings = null)
        {
            var assemblyPath = Assembly.GetExecutingAssembly().Location;
            preloader = new Preloader(Path.GetDirectoryName(assemblyPath));

            preloader.Preload();

            var preloadedLibraries = new List<string>();
            GetLibrariesToPreload(preloadedLibraries);

            if (preloadedLibraries.Any())
            {
                // Only when any library needs preloading will a path resolver be 
                // created, otherwise DynamoModel gets created without preloading 
                // any library.
                // 

                var pathResolverParams = new TestPathResolverParams()
                {
                    UserDataRootFolder = GetUserUserDataRootFolder(),
                    CommonDataRootFolder = GetCommonDataRootFolder()
                };

                pathResolver = new TestPathResolver(pathResolverParams);
                foreach (var preloadedLibrary in preloadedLibraries.Distinct())
                {
                    pathResolver.AddPreloadLibraryPath(preloadedLibrary);
                }
            }

            this.CurrentDynamoModel = DynamoModel.Start(CreateStartConfiguration(settings));
        }

        /// <summary>
        /// Derived test classes could override it to provide different 
        /// configuration.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        protected virtual DynamoModel.IStartConfiguration CreateStartConfiguration(IPreferences settings)
        {
            return new DynamoModel.DefaultStartConfiguration()
            {
                PathResolver = pathResolver,
                StartInTestMode = true,
                GeometryFactoryPath = preloader.GeometryFactoryPath,
                Preferences = settings,
                ProcessMode = TaskProcessMode.Synchronous
            };
        }

        protected T Open<T>(params string[] relativePathParts) where T : WorkspaceModel
        {
            var path = Path.Combine(relativePathParts);
            CurrentDynamoModel.ExecuteCommand(new DynamoModel.OpenFileCommand(path));
            return CurrentDynamoModel.CurrentWorkspace as T;
        }

        protected void BeginRun()
        {
            CurrentDynamoModel.ExecuteCommand(new DynamoModel.RunCancelCommand(false, false));
        }

        protected void EmptyScheduler()
        {
            while (CurrentDynamoModel.Scheduler.ProcessNextTask(false))
            {

            }
        }

        protected void OpenModel(string relativeFilePath)
        {
            string openPath = Path.Combine(TestDirectory, relativeFilePath);
            CurrentDynamoModel.ExecuteCommand(new DynamoModel.OpenFileCommand(openPath));
        }

        protected void OpenSampleModel(string relativeFilePath)
        {
            string openPath = Path.Combine(SampleDirectory, relativeFilePath);
            CurrentDynamoModel.ExecuteCommand(new DynamoModel.OpenFileCommand(openPath));
        }

        protected void RunModel(string relativeDynFilePath)
        {
            OpenModel(relativeDynFilePath);
            Assert.DoesNotThrow(BeginRun);
        }

        protected void RunCurrentModel() // Run currently loaded model.
        {
            Assert.DoesNotThrow(BeginRun);
        }

        protected WorkspaceModel GetWorkspaceById(string idStr)
        {
            Guid guidValue = Guid.Parse(idStr);
            var workspace = CurrentDynamoModel.Workspaces.FirstOrDefault(ws => ws.Guid == guidValue);
            return workspace ?? CurrentDynamoModel.CustomNodeManager.GetWorkspaceById(guidValue);
        }

        protected void SelectTabByGuid(Guid guid)
        {
            var workspaceToSwitch = GetWorkspaceById(guid.ToString());
            if (workspaceToSwitch != null && CurrentDynamoModel.CurrentWorkspace != workspaceToSwitch)
            {
                var index = CurrentDynamoModel.Workspaces.IndexOf(workspaceToSwitch);
                CurrentDynamoModel.ExecuteCommand(new DynamoModel.SwitchTabCommand(index));
            }
        }

        protected PackageLoader GetPackageLoader()
        {
            var extensions = CurrentDynamoModel.ExtensionManager.Extensions.OfType<PackageManagerExtension>();
            if (extensions.Any())
            {
                return extensions.First().PackageLoader;
            }

            return null;
        }

        protected void LoadPackage(string packageDirectory)
        {
            CurrentDynamoModel.PreferenceSettings.CustomPackageFolders.Add(packageDirectory);
            var loader = GetPackageLoader();
            var pkg = loader.ScanPackageDirectory(packageDirectory);
            loader.LoadPackages(new List<Package> { pkg });
        }

        protected NodeModel GetNodeInstance(string creationName)
        {
            var searchElementList = CurrentDynamoModel.SearchModel.SearchEntries.OfType<NodeSearchElement>();
            foreach (var element in searchElementList)
            {
                if (element.CreationName == creationName)
                {
                    return ((NodeSearchElement) element).CreateNode();
                }
            }
            return null;
        }
    }
}
