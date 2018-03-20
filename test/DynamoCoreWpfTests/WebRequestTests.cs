using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;

using SystemTestServices;
using Dynamo.Graph;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.Tests;

using NUnit.Framework;

using WebRequest = CoreNodeModels.WebRequest;
using Dynamo.Graph.Nodes.ZeroTouch;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    public class PeriodicUpdateTest : SystemTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("VMDataBridge.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        public void WebRequest()
        {
            if (!CheckForInternetConnection())
            {
                Assert.Inconclusive("Not connected to the internet.");
            }

            var openPath = Path.Combine(GetTestDirectory(ExecutingDirectory), @"core\web_request\WebRequest.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var ws = Model.CurrentWorkspace as HomeWorkspaceModel;

            AssertEvaluationCount(ws, 1);

            // Ensure that the response is valid
            var webRequestNode = ws.FirstNodeFromWorkspace<WebRequest>();
            Assert.NotNull(webRequestNode);

            var response = GetPreviewValue(webRequestNode.GUID.ToString()).ToString();
            Assert.True(response.Contains("Search the world's information"));

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            // Switch to periodic mode
            ws.RunSettings.RunPeriod = 100;
            ws.RunSettings.RunType = RunType.Periodic;

            var startCount = ws.EvaluationCount;
            while (ws.EvaluationCount - startCount < 5)
            {
                if (stopWatch.Elapsed.Seconds > 20)
                {
                    Assert.Fail("There were not enough web requests made to pass. Either the run periodic functionality does not work for this node, or the internet connection is slow.");
                }
            }

            Assert.Pass("There were {0} evaluations.", ws.EvaluationCount);
        }

        [Test]
        public void DateTimeNow()
        {
            var openPath = Path.Combine(GetTestDirectory(ExecutingDirectory), @"core\periodic_update\datetimenow.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var ws = Model.CurrentWorkspace as HomeWorkspaceModel;
            var dateTimeNow = ws.FirstNodeFromWorkspace<DSFunction>();
            var guid = dateTimeNow.GUID.ToString();

            Assert.NotNull(dateTimeNow);

            var time1 = GetPreviewValue(guid).ToString();

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            // Switch to periodic mode
            ws.RunSettings.RunPeriod = 100;
            ws.RunSettings.RunType = RunType.Periodic;

            while (stopWatch.Elapsed.Seconds <= 1) { }

            stopWatch.Stop();
            var time2 = GetPreviewValue(guid).ToString();

            Assert.AreNotEqual(time1, time2);
        }


        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (var stream = client.OpenRead("http://www.google.com"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
