using Dynamo.ViewModels;
using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Dynamo.Tests
{
    [TestFixture]
    public class CrashReportingTests
    {
        /// <summary>
        /// Browser tab open on the GitHub new issue page should contain these words in the title
        /// </summary>
        List<string> TargetWords = new List<string> { "NEW ISSUE", "DYNAMODS" };
        /// <summary>
        /// If user is not logged in to GitHub, browser tab trying to open the GitHub new issue page should contain these words in the title
        /// </summary>
        List<string> TargetWordsLoggedOutFallback = new List<string> { "SIGN IN", "GITHUB" };

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

        [Test]
        public void CanReportBugWithNoContent()
        {
            // report a bug with no details
            Assert.DoesNotThrow(() => DynamoViewModel.ReportABug());

            // give the system time to launch a browser & open the page
            Thread.Sleep(4000);

            // check browser is open on correct page
            Assert.True(BrowserIsOpenOnPageWithTitleMatching(TargetWords));
        }

        [Test]
        public void CanReportBugWithContent()
        {
            // report a bug with details
            var details = "Exception thrown somewhere";
            Assert.IsNotNull(details);
            Assert.DoesNotThrow(() => DynamoViewModel.ReportABug(details));

            // give the system time to launch a browser & open the page
            Thread.Sleep(4000);

            // check browser is open on correct page
            Assert.True(BrowserIsOpenOnPageWithTitleMatching(TargetWords));
        }

        [Test]
        public void CanReportBugWithLongContent()
        {
            // report a bug with very long content
            Assert.IsNotNull(StackTrace);
            Assert.DoesNotThrow(() => DynamoViewModel.ReportABug(StackTrace));

            // give the system time to launch a browser & open the page
            Thread.Sleep(4000);

            // check browser is open on correct page
            Assert.True(BrowserIsOpenOnPageWithTitleMatching(TargetWords));
        }


        /// <summary>
        /// Checks there is a process open whose main window title matches the supplied words.
        /// </summary>
        /// <param name="pageTitleWords">The words to match.</param>
        /// <returns></returns>
        private bool BrowserIsOpenOnPageWithTitleMatching(List<string> pageTitleWords)
        {
            // get list of all processes on the system
            var allProcesses = Process.GetProcesses();

            // find processes whose main window title matches our keywords
            var matchingProcesses = allProcesses
                                        .Select(x => x.MainWindowTitle)
                                        .Where(title => ContainsAllTargetWords(title))
                                        .ToList();

            // check that at least one process matches our search
            return matchingProcesses.Any();
        }

        /// <summary>
        /// Checks if a supplied string contains all target substrings
        /// </summary>
        /// <param name="source">The string to check</param>
        /// <returns></returns>
        private bool ContainsAllTargetWords(string source)
        {
            return TargetWords.TrueForAll(x => source.ToUpper().Contains(x)) ||
                   TargetWordsLoggedOutFallback.TrueForAll(x => source.ToUpper().Contains(x));
        }
    }
}
