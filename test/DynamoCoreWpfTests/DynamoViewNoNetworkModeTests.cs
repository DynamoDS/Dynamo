using Dynamo.Controls;
using Dynamo.Models;
using NUnit.Framework;

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
    }
}
