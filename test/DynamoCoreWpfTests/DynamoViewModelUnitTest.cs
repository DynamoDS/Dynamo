using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Dynamo.Models;
using Dynamo.Scheduler;
using Dynamo.Selection;
using Dynamo.ViewModels;
using Dynamo.Wpf.ViewModels.Watch3D;
using DynamoShapeManager;
using NUnit.Framework;
using TestServices;

namespace Dynamo.Tests
{
    /// <summary>ix
    /// 
    ///     The DynamoViewModelUnitTests constructs the DynamoModel
    ///     and the DynamoViewModel, but does not construct the view.
    ///     You can use this class to create tests which ensure that the 
    ///     ViewModel and the Model are communicating properly.
    /// 
    ///     WARNING! You should think twice about using this class!  It's
    ///     often a better alternative to use DynamoModelTestBase or,
    ///     better yet, use an even lighter weight class.  
    ///
    /// </summary>
    public class DynamoViewModelUnitTest : DSEvaluationUnitTestBase
    {
        protected DynamoViewModel ViewModel;
        protected Preloader preloader;

        protected override DynamoModel GetModel()
        {
            return ViewModel.Model;
        }

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            StartDynamo();
        }

        public override void Cleanup()
        {
            try
            {
                preloader = null;
                DynamoSelection.Instance.ClearSelection();

                if (ViewModel == null)
                    return;

                var shutdownParams = new DynamoViewModel.ShutdownParams(
                    shutdownHost: false,
                    allowCancellation: false);

                ViewModel.PerformShutdownSequence(shutdownParams);
                ViewModel.RequestUserSaveWorkflow -= RequestUserSaveWorkflow;
                ViewModel = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }

            base.Cleanup();
        }

        private void RequestUserSaveWorkflow(object sender, WorkspaceSaveEventArgs e)
        {
            // Some test cases may create nodes or modify nodes, so when Dynamo
            // is shutting down, Dynamo will fire RequestUserSaveWorkflow event 
            // to save the change, if there is no a corresponding event handler, 
            // or the event handler fails to save the change, shut down process 
            // will be aborted and a lot of resource will not be released 
            // (details refer to DynamoViewModel.PerformShutdownSequence()).
            //
            // As this test fixture is UIless, DynamoView, which implements 
            // event handler for DynamoViewModel.RequestUserSaveWorkflow event, 
            // won't be created. To ensure resource be released properly, we 
            // implement event handler here and simply mark the save event's 
            // susccess status to true to notify Dynamo to continue the shut
            // down process.
            e.Success = true;
        }

        protected void StartDynamo()
        {
            var assemblyPath = Assembly.GetExecutingAssembly().Location;
            preloader = new Preloader(Path.GetDirectoryName(assemblyPath));
            preloader.Preload();

            TestPathResolver pathResolver = null;
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

            var model = DynamoModel.Start(
                new DynamoModel.DefaultStartConfiguration()
                {
                    PathResolver = pathResolver,
                    StartInTestMode = true,
                    GeometryFactoryPath = preloader.GeometryFactoryPath,
                    ProcessMode = TaskProcessMode.Synchronous
                });

            var watch3DViewParams = new Watch3DViewModelStartupParams(model);
            this.ViewModel = DynamoViewModel.Start(
                new DynamoViewModel.StartConfiguration()
                {
                    DynamoModel = model,
                    Watch3DViewModel = new DefaultWatch3DViewModel(null, watch3DViewParams)
                });

            this.ViewModel.RequestUserSaveWorkflow += RequestUserSaveWorkflow;
        }

        /// <summary>
        ///     Runs a basic unit tests that loads a file, runs it, and confirms that
        ///     nodes corresponding to given guids have OldValues that match the given
        ///     expected values.
        /// </summary>
        /// <param name="exampleFilePath">Path to DYN to run.</param>
        /// <param name="tests">
        ///     Key/Value pairs where the Key is a node Guid and the Value is the
        ///     expected OldValue for the node.
        /// </param>
        protected void RunExampleTest(
            string exampleFilePath, IEnumerable<KeyValuePair<Guid, object>> tests)
        {
            this.ViewModel.OpenCommand.Execute(exampleFilePath);
            this.ViewModel.HomeSpace.Run();

            foreach (var test in tests)
            {
                var runResult = this.ViewModel.Model.CurrentWorkspace.NodeFromWorkspace(test.Key).CachedValue.Data;
                Assert.AreEqual(test.Value, runResult);
            }
        }

        protected void OpenModel(string relativeFilePath)
        {
            string openPath = Path.Combine(TestDirectory, relativeFilePath);
            ViewModel.OpenCommand.Execute(openPath);
        }

        protected void OpenSampleModel(string relativeFilePath)
        {
            string openPath = Path.Combine(SampleDirectory, relativeFilePath);
            ViewModel.OpenCommand.Execute(openPath);
        }

        protected void RunModel(string relativeDynFilePath)
        {
            OpenModel(relativeDynFilePath);
            Assert.DoesNotThrow(() => ViewModel.HomeSpace.Run());
        }

        protected void RunCurrentModel() // Run currently loaded model.
        {
            Assert.DoesNotThrow(() => ViewModel.HomeSpace.Run());
        }
    }
}