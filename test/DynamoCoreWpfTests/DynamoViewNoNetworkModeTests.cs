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

            var shouldDisable = (bool)method.Invoke(View, new object[] { "Autodesk Assistant", "added" });
            Assert.IsTrue(shouldDisable);
        }
    }
}
