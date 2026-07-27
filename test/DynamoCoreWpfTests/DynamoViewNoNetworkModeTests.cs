using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.Models;
using Dynamo.ViewModels;
using NUnit.Framework;
using TestServices;

namespace DynamoCoreWpfTests
{
    public class DynamoViewNoNetworkModeTests : DynamoTestUIBase
    {
        protected override DynamoModel.IStartConfiguration CreateStartConfiguration(Dynamo.Interfaces.IPathResolver pathResolver)
        {
            return new DynamoModel.DefaultStartConfiguration()
            {
                PathResolver = pathResolver,
                StartInTestMode = true,
                GeometryFactoryPath = preloader.GeometryFactoryPath,
                ProcessMode = Dynamo.Scheduler.TaskProcessMode.Synchronous,
                NoNetworkMode = true
            };
        }

        [Test]
        public void AutodeskAssistantExtensionIsDisabledWhenNoNetworkModeIsEnabled()
        {
            var shouldDisable = View.DisableExtensionWhenNoNetworkMode(
                DynamoView.AutodeskAssistantExtensionId,
                "Autodesk Assistant",
                "added");

            Assert.IsTrue(shouldDisable);
        }

        [Test]
        public void McpViewExtensionIsDisabledWhenNoNetworkModeIsEnabled()
        {
            var shouldDisable = View.DisableExtensionWhenNoNetworkMode(
                DynamoView.McpViewExtensionId,
                "Dynamo MCP View Extension",
                "added");

            Assert.IsTrue(shouldDisable);
        }

        [Test]
        public void AutodeskAssistantExtensionIsNotDisabledWhenNoNetworkModeIsFalse()
        {
            var pathResolver = new TestPathResolver();
            DynamoModel modelWithoutNoNetworkMode = null;
            DynamoViewModel viewModelWithoutNoNetworkMode = null;
            DynamoView viewWithoutNoNetworkMode = null;

            try
            {
                modelWithoutNoNetworkMode = DynamoModel.Start(new DynamoModel.DefaultStartConfiguration()
                {
                    PathResolver = pathResolver,
                    StartInTestMode = true,
                    GeometryFactoryPath = preloader.GeometryFactoryPath,
                    ProcessMode = Dynamo.Scheduler.TaskProcessMode.Synchronous,
                    NoNetworkMode = false
                });

                viewModelWithoutNoNetworkMode = DynamoViewModel.Start(new DynamoViewModel.StartConfiguration()
                {
                    DynamoModel = modelWithoutNoNetworkMode
                });

                viewWithoutNoNetworkMode = new DynamoView(viewModelWithoutNoNetworkMode);

                var shouldDisable = viewWithoutNoNetworkMode.DisableExtensionWhenNoNetworkMode(
                    DynamoView.AutodeskAssistantExtensionId,
                    "Autodesk Assistant",
                    "added");

                Assert.IsFalse(shouldDisable);
            }
            finally
            {
                if (viewWithoutNoNetworkMode != null && viewWithoutNoNetworkMode.IsLoaded)
                {
                    viewWithoutNoNetworkMode.Close();
                }

                if (viewModelWithoutNoNetworkMode != null)
                {
                    var shutdownParams = new DynamoViewModel.ShutdownParams(shutdownHost: false, allowCancellation: false);
                    viewModelWithoutNoNetworkMode.PerformShutdownSequence(shutdownParams);
                }
            }
        }

        [Test]
        public void UnrecognizedExtensionIsNotDisabledWhenNoNetworkModeIsEnabled()
        {
            var shouldDisable = View.DisableExtensionWhenNoNetworkMode(
                "11111111-1111-1111-1111-111111111111",
                "Some Other Extension",
                "added");

            Assert.IsFalse(shouldDisable);
        }

        [Test]
        public void AutodeskAssistantExtensionIsDisabledWhenIDSDKIsNotInitialized()
        {
            var pathResolver = new TestPathResolver();
            DynamoModel modelWithUninitializedIDSDK = null;
            DynamoViewModel viewModelWithUninitializedIDSDK = null;
            DynamoView viewWithUninitializedIDSDK = null;

            try
            {
                // IDSDKManager.IsIDSDKInitialized returns false when the native IDSDK library
                // is not installed (e.g. test environments, VMs without Autodesk Identity).
                modelWithUninitializedIDSDK = DynamoModel.Start(new DynamoModel.DefaultStartConfiguration()
                {
                    PathResolver = pathResolver,
                    StartInTestMode = true,
                    GeometryFactoryPath = preloader.GeometryFactoryPath,
                    ProcessMode = Dynamo.Scheduler.TaskProcessMode.Synchronous,
                    NoNetworkMode = false,
                    AuthProvider = new IDSDKManager()
                });

                viewModelWithUninitializedIDSDK = DynamoViewModel.Start(new DynamoViewModel.StartConfiguration()
                {
                    DynamoModel = modelWithUninitializedIDSDK
                });

                viewWithUninitializedIDSDK = new DynamoView(viewModelWithUninitializedIDSDK);

                var shouldDisable = viewWithUninitializedIDSDK.DisableExtensionWhenNoNetworkMode(
                    DynamoView.AutodeskAssistantExtensionId,
                    "Autodesk Assistant",
                    "added");

                Assert.IsTrue(shouldDisable);
            }
            finally
            {
                if (viewWithUninitializedIDSDK != null && viewWithUninitializedIDSDK.IsLoaded)
                {
                    viewWithUninitializedIDSDK.Close();
                }

                if (viewModelWithUninitializedIDSDK != null)
                {
                    var shutdownParams = new DynamoViewModel.ShutdownParams(shutdownHost: false, allowCancellation: false);
                    viewModelWithUninitializedIDSDK.PerformShutdownSequence(shutdownParams);
                }
            }
        }

        [Test]
        public void McpViewExtensionIsDisabledWhenIDSDKIsNotInitialized()
        {
            var pathResolver = new TestPathResolver();
            DynamoModel modelWithUninitializedIDSDK = null;
            DynamoViewModel viewModelWithUninitializedIDSDK = null;
            DynamoView viewWithUninitializedIDSDK = null;

            try
            {
                modelWithUninitializedIDSDK = DynamoModel.Start(new DynamoModel.DefaultStartConfiguration()
                {
                    PathResolver = pathResolver,
                    StartInTestMode = true,
                    GeometryFactoryPath = preloader.GeometryFactoryPath,
                    ProcessMode = Dynamo.Scheduler.TaskProcessMode.Synchronous,
                    NoNetworkMode = false,
                    AuthProvider = new IDSDKManager()
                });

                viewModelWithUninitializedIDSDK = DynamoViewModel.Start(new DynamoViewModel.StartConfiguration()
                {
                    DynamoModel = modelWithUninitializedIDSDK
                });

                viewWithUninitializedIDSDK = new DynamoView(viewModelWithUninitializedIDSDK);

                var shouldDisable = viewWithUninitializedIDSDK.DisableExtensionWhenNoNetworkMode(
                    DynamoView.McpViewExtensionId,
                    "Dynamo MCP View Extension",
                    "added");

                Assert.IsTrue(shouldDisable);
            }
            finally
            {
                if (viewWithUninitializedIDSDK != null && viewWithUninitializedIDSDK.IsLoaded)
                {
                    viewWithUninitializedIDSDK.Close();
                }

                if (viewModelWithUninitializedIDSDK != null)
                {
                    var shutdownParams = new DynamoViewModel.ShutdownParams(shutdownHost: false, allowCancellation: false);
                    viewModelWithUninitializedIDSDK.PerformShutdownSequence(shutdownParams);
                }
            }
        }
    }
}
