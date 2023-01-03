using System;
using System.IO;
using System.Windows.Threading;
using Dynamo.Models;
using Dynamo.PackageManager;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.Utilities;
using DynamoCoreWpfTests;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    public class CrashReportingTests : DynamoModelTestBase
    {
        // This is the stack trace produced by the known crash produced when 
        // opening the Core.Math sample, selecting all nodes and doing NodeToCode.
        private string StackTrace = @"
            Object reference not set to an instance of an object.
            at Dynamo.Graph.Nodes.CodeBlockNodeModel.GetTypeHintForOutput(Int32 index)
            at Dynamo.Engine.NodeToCode.NodeToCodeCompiler.GetInputOutputMap(IEnumerable`1 nodes, Dictionary`2& inputMap, Dictionary`2& outputMap, Dictionary`2& renamingMap, Dictionary`2& typeHintMap)
            at Dynamo.Engine.NodeToCode.NodeToCodeCompiler.NodeToCode(Core core, IEnumerable`1 workspaceNodes, IEnumerable`1 nodes, INamingProvider namingProvider)
            at Dynamo.Graph.Workspaces.NodesToCodeExtensions.ConvertNodesToCodeInternal(WorkspaceModel workspace, EngineController engineController, INamingProvider namingProvider)
            at Dynamo.Models.DynamoModel.ConvertNodesToCodeImpl(ConvertNodesToCodeCommand command)
            at Dynamo.Models.DynamoModel.ExecuteCommand(RecordableCommand command)
            at MS.Internal.Commands.CommandHelpers.CriticalExecuteCommandSource(ICommandSource commandSource, Boolean userInitiated)
            at System.Windows.Controls.MenuItem.InvokeClickAfterRender(Object arg)
            at System.Windows.Threading.ExceptionWrapper.InternalRealCall(Delegate callback, Object args, Int32 numArgs)
            at System.Windows.Threading.ExceptionWrapper.TryCatchWhen(Object source, Delegate callback, Object args, Int32 numArgs, Delegate catchHandler)
            at System.Windows.Threading.DispatcherOperation.InvokeImpl()
            at System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state, Boolean preserveSyncCtx)
            at System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback callback, Object state, Boolean preserveSyncCtx)
            at System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback callback, Object state)
            at MS.Internal.CulturePreservingExecutionContext.Run(CulturePreservingExecutionContext executionContext, ContextCallback callback, Object state)
            at System.Windows.Threading.DispatcherOperation.Invoke()
            at System.Windows.Threading.Dispatcher.ProcessQueue()
            at System.Windows.Threading.Dispatcher.WndProcHook(IntPtr hwnd, Int32 msg, IntPtr wParam, IntPtr lParam, Boolean& handled)
            at MS.Win32.HwndWrapper.WndProc(IntPtr hwnd, Int32 msg, IntPtr wParam, IntPtr lParam, Boolean& handled)
            at MS.Win32.HwndSubclass.DispatcherCallbackOperation(Object o)
            at System.Windows.Threading.ExceptionWrapper.InternalRealCall(Delegate callback, Object args, Int32 numArgs)
            at System.Windows.Threading.ExceptionWrapper.TryCatchWhen(Object source, Delegate callback, Object args, Int32 numArgs, Delegate catchHandler)
            at System.Windows.Threading.Dispatcher.LegacyInvokeImpl(DispatcherPriority priority, TimeSpan timeout, Delegate method, Object args, Int32 numArgs)
            at MS.Win32.HwndSubclass.SubclassWndProc(IntPtr hwnd, Int32 msg, IntPtr wParam, IntPtr lParam)
            at MS.Win32.UnsafeNativeMethods.DispatchMessage(MSG& msg)
            at System.Windows.Threading.Dispatcher.PushFrameImpl(DispatcherFrame frame)
            at System.Windows.Application.RunDispatcher(Object ignore)
            at System.Windows.Application.RunInternal(Window window)
            at DynamoSandbox.DynamoCoreSetup.RunApplication(Application app)";

        // This are example packages names used to simulate loaded packages information
        private string Packages = @"
            - Package Example A
            - Package Example B";

        [Test]
        [Ignore("Test ignored because the web browser is not closed after the test execution")]
        public void CanReportBugWithNoContent()
        {
            // Create a crash report to submit
            var crashReport = Wpf.Utilities.CrashUtilities.BuildMarkdownContent(null, null);
            Assert.IsNotNullOrEmpty(crashReport);

            // Mock url for request
            string url = Wpf.Utilities.CrashUtilities.GithubNewIssueUrlFromCrashContent(crashReport);
            Assert.IsNotNullOrEmpty(url);

            // Report a bug with no details
            Assert.DoesNotThrow(() => DynamoViewModel.ReportABug());
        }

        [Test]
        [Ignore("Test ignored because the web browser is not closed after the test execution")]
        public void CanReportBugWithContent()
        {
            // Mock Dynamo version
            var dynamoVersion = "2.1.0";

            // Create a crash report to submit
            var crashReport = Wpf.Utilities.CrashUtilities.BuildMarkdownContent(dynamoVersion, Packages);
            Assert.IsNotNullOrEmpty(crashReport);

            // Report a bug with a stack trace
            Assert.DoesNotThrow(() => DynamoViewModel.ReportABug(crashReport));
        }

        [Test]
        public void StackTraceIncludedInReport()
        {
            // Mock Dynamo version
            var dynamoVersion = AssemblyHelper.GetDynamoVersion().ToString();

            // Create a crash report to submit
            var crashReport = Wpf.Utilities.CrashUtilities.BuildMarkdownContent(dynamoVersion, Packages);
            Assert.IsNotNullOrEmpty(crashReport);

            // Mock url for request
            string url = Wpf.Utilities.CrashUtilities.GithubNewIssueUrlFromCrashContent(crashReport);
            Assert.IsNotNullOrEmpty(url);

            // Get body content from request
            var query = "body=";
            var startIndex = url.IndexOf(query) + query.Length;
            var body = url.Substring(startIndex);
            var decoded = Uri.UnescapeDataString(body);

            // Verify request contains the dynamoVersion
            Assert.True(decoded.Contains(dynamoVersion));
            // Verify request contains the packages information
            Assert.True(decoded.Contains(Packages));

            // TODO - Can be re-added when stack traces are uploaded automatically (currently manual)
            // Verify request contains the stack trace
            // Assert.True(decoded.Contains(StackTrace));
        }

        [Test]
        public void NullPackageLoader()
        {
            // Mock Dynamo version
            var dynamoVersion = AssemblyHelper.GetDynamoVersion().ToString();

            //Get packages data from null package loader
            var packagesData = Wpf.Utilities.CrashUtilities.PackagesToMakrdown(null);

            // Create a crash report to submit
            var crashReport = Wpf.Utilities.CrashUtilities.BuildMarkdownContent(dynamoVersion, packagesData);
            Assert.IsNotNullOrEmpty(crashReport);

            // Mock url for request
            string url = Wpf.Utilities.CrashUtilities.GithubNewIssueUrlFromCrashContent(crashReport);
            Assert.IsNotNullOrEmpty(url);

            // Get body content from request
            var query = "body=";
            var startIndex = url.IndexOf(query) + query.Length;
            var body = url.Substring(startIndex);
            var decoded = Uri.UnescapeDataString(body);

            var expectedString = "## What packages or external references (if any) were used?" + Environment.NewLine + "(Fill in here)";

            // Verify request contains the packages information
            Assert.True(decoded.Contains(expectedString));
        }

        [Test]
        public void NoLoadedPackages()
        {
            // Mock Dynamo version
            var dynamoVersion = AssemblyHelper.GetDynamoVersion().ToString();

            //Gets package loader
            var packageLoader = CurrentDynamoModel.GetPackageManagerExtension()?.PackageLoader;
            Assert.IsNotNull(packageLoader);
            Assert.IsEmpty(packageLoader.LocalPackages);

            //Get packages data from null package loader
            var packagesData = Wpf.Utilities.CrashUtilities.PackagesToMakrdown(packageLoader);

            // Create a crash report to submit
            var crashReport = Wpf.Utilities.CrashUtilities.BuildMarkdownContent(dynamoVersion, packagesData);
            Assert.IsNotNullOrEmpty(crashReport);

            // Mock url for request
            string url = Wpf.Utilities.CrashUtilities.GithubNewIssueUrlFromCrashContent(crashReport);
            Assert.IsNotNullOrEmpty(url);

            // Get body content from request
            var query = "body=";
            var startIndex = url.IndexOf(query) + query.Length;
            var body = url.Substring(startIndex);
            var decoded = Uri.UnescapeDataString(body);

            var expectedString = "No loaded packages were found.";

            // Verify request contains the packages information
            Assert.True(decoded.Contains(expectedString));
        }

        [Test]
        public void TestCERTool()
        {
            try
            {
                throw new Exception("test");
            }
            catch
            {
                var dumpLocation = CrashReportTool.CreateMiniDumpFile();
                Assert.IsFalse(string.IsNullOrEmpty(dumpLocation));
                Assert.IsTrue(File.Exists(dumpLocation));
                File.Delete(dumpLocation);//cleanup
            }
        }
        [Test]
        public void TestAppNameSentToCER()
        {
            CurrentDynamoModel.HostName = null;
            var name = CrashReportTool.GetHostAppName(CurrentDynamoModel);
            //if both hostname and hostinfo.hostname are null, then use proc name.
            Assert.True(name.Contains("testhost") ||  name.Contains("nunit-agent"));
            CurrentDynamoModel.HostName = "dynamotestmock";
            name = CrashReportTool.GetHostAppName(CurrentDynamoModel);
            //use hostname over proc name
            Assert.AreEqual(CurrentDynamoModel.HostName, name);
            CurrentDynamoModel.HostAnalyticsInfo = new  HostAnalyticsInfo(){HostName = "123"};
            name = CrashReportTool.GetHostAppName(CurrentDynamoModel);
            //prefer hostinfo.hostname over others.
            Assert.AreEqual(CurrentDynamoModel.HostAnalyticsInfo.HostName, name);
        }
    }
}
