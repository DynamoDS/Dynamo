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

            var ws = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;
            var webRequestNode = ws.FirstNodeFromWorkspace<WebRequest>();
            Assert.NotNull(webRequestNode);

            var response = GetPreviewValue(webRequestNode.GUID.ToString()).ToString();
            Assert.True(response.Contains("Search the world's information"));
        }
    }
}
