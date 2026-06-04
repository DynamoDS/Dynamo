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
            var method = typeof(Dynamo.Controls.DynamoView).GetMethod("DisableExtensionWhenNoNetworkMode",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            Assert.IsNotNull(method);

            var shouldDisable = (bool)method.Invoke(View, new object[] { "d2599b24-88a6-47e6-be0c-25df5702f5a7", "Autodesk Assistant", "added" });
            Assert.IsTrue(shouldDisable);
        }

        [Test]
        public void McpViewExtensionIsDisabledWhenNoNetworkModeIsEnabled()
        {
            var method = typeof(Dynamo.Controls.DynamoView).GetMethod("DisableExtensionWhenNoNetworkMode",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            Assert.IsNotNull(method);

            var shouldDisable = (bool)method.Invoke(View, new object[] { "FAB268A8-6C83-4389-8AB8-F9AE92D6091E", "Dynamo MCP View Extension", "added" });
            Assert.IsTrue(shouldDisable);
        }
    }
}
