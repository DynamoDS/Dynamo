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
        public void AssistantAndMcpTabsAreDisabledWhenIDSDKIsNotInitialized()
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

                // Simulate the extension having loaded its tab into the sidebar.
                // Adding to SideBarTabItems triggers CollectionChanged → DisableExtensionTabsWhenIDSDKNotInitialized automatically.
                var assistantTab = new System.Windows.Controls.TabItem { Uid = DynamoView.AutodeskAssistantExtensionId };
                var mcpTab = new System.Windows.Controls.TabItem { Uid = DynamoView.McpViewExtensionId };
                viewModelWithUninitializedIDSDK.SideBarTabItems.Add(assistantTab);
                viewModelWithUninitializedIDSDK.SideBarTabItems.Add(mcpTab);

                Assert.IsFalse(assistantTab.IsEnabled);
                Assert.IsFalse(mcpTab.IsEnabled);
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
        public void AssistantTabRemainsDisabledWhenReAddedToSidebarAfterWorkspaceOpen()
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

                // Initial add — extension loads its tab during startup.
                var assistantTab = new System.Windows.Controls.TabItem { Uid = DynamoView.AutodeskAssistantExtensionId };
                viewModelWithUninitializedIDSDK.SideBarTabItems.Add(assistantTab);
                Assert.IsFalse(assistantTab.IsEnabled, "Tab should be disabled on initial add");

                // Simulate the extension removing and re-adding its tab (e.g. on workspace open).
                viewModelWithUninitializedIDSDK.SideBarTabItems.Remove(assistantTab);
                var reAddedTab = new System.Windows.Controls.TabItem { Uid = DynamoView.AutodeskAssistantExtensionId };
                viewModelWithUninitializedIDSDK.SideBarTabItems.Add(reAddedTab);

                Assert.IsFalse(reAddedTab.IsEnabled, "Re-added tab should still be disabled when IDSDK is not initialized");
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
        public void AssistantAndMcpTabsAreNotDisabledWhenIDSDKIsInitialized()
        {
            var pathResolver = new TestPathResolver();
            DynamoModel modelWithNullAuthProvider = null;
            DynamoViewModel viewModelWithNullAuthProvider = null;
            DynamoView viewWithNullAuthProvider = null;

            try
            {
                // When AuthProvider is null (host environment or no IDSDK configured),
                // IsIDSDKInitialized() returns true — tabs should remain enabled.
                modelWithNullAuthProvider = DynamoModel.Start(new DynamoModel.DefaultStartConfiguration()
                {
                    PathResolver = pathResolver,
                    StartInTestMode = true,
                    GeometryFactoryPath = preloader.GeometryFactoryPath,
                    ProcessMode = Dynamo.Scheduler.TaskProcessMode.Synchronous,
                    NoNetworkMode = false
                });

                viewModelWithNullAuthProvider = DynamoViewModel.Start(new DynamoViewModel.StartConfiguration()
                {
                    DynamoModel = modelWithNullAuthProvider
                });

                viewWithNullAuthProvider = new DynamoView(viewModelWithNullAuthProvider);

                var assistantTab = new System.Windows.Controls.TabItem { Uid = DynamoView.AutodeskAssistantExtensionId };
                var mcpTab = new System.Windows.Controls.TabItem { Uid = DynamoView.McpViewExtensionId };
                viewModelWithNullAuthProvider.SideBarTabItems.Add(assistantTab);
                viewModelWithNullAuthProvider.SideBarTabItems.Add(mcpTab);

                viewWithNullAuthProvider.DisableExtensionTabsWhenIDSDKNotInitialized();

                Assert.IsTrue(assistantTab.IsEnabled);
                Assert.IsTrue(mcpTab.IsEnabled);
            }
            finally
            {
                if (viewWithNullAuthProvider != null && viewWithNullAuthProvider.IsLoaded)
                {
                    viewWithNullAuthProvider.Close();
                }

                if (viewModelWithNullAuthProvider != null)
                {
                    var shutdownParams = new DynamoViewModel.ShutdownParams(shutdownHost: false, allowCancellation: false);
                    viewModelWithNullAuthProvider.PerformShutdownSequence(shutdownParams);
                }
            }
        }
    }
}
