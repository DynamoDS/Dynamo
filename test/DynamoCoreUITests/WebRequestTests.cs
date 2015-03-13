using System.IO;

using SystemTestServices;

using DSCoreNodesUI;

using Dynamo.Models;
using Dynamo.Tests;

using NUnit.Framework;

namespace DynamoCoreUITests
{
    [TestFixture]
    public class WebRequestTests : SystemTestBase
    {
        [Test]
        public void WebRequest()
        {
            var openPath = Path.Combine(GetTestDirectory(ExecutingDirectory), @"core\web_request\WebRequest.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var ws = Model.CurrentWorkspace as HomeWorkspaceModel;

            AssertEvaluationCount(ws, 1);

            // Ensure that the response is valid
            var webRequestNode = ws.FirstNodeFromWorkspace<WebRequest>();
            Assert.NotNull(webRequestNode);

            var response = GetPreviewValue(webRequestNode.GUID.ToString()).ToString();
            Assert.True(response.Contains("Search the world's information"));

            // Switch to periodic mode
            ws.RunSettings.RunPeriod = 100;
            ws.RunSettings.RunType = RunType.Periodic;
            System.Threading.Thread.Sleep(1000);

            Assert.Greater(ws.EvaluationCount, 3);
        }
    }
}
