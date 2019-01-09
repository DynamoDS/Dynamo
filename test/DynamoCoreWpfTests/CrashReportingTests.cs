using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Dynamo.ViewModels;
using NUnit.Framework;

namespace Dynamo.Tests
{
    class CrashReportingTests : DynamoViewModelUnitTest
    {
        List<string> TargetWords = new List<string> { "New Issue", "DynamoDS" };

        [Test]
        void CanReportBugWithNoContent()
        {
            // report a bug with no details
            Assert.DoesNotThrow( () => DynamoViewModel.ReportABug());

            // give the system time to launch a browser & open the page
            Thread.Sleep(3000);

            // check browser is open on correct page
            Assert.True(BrowserIsOpenOnPageWithTitleMatching(TargetWords));
        }

        [Test]
        void CanReportBugWithContent()
        {
            // report a bug with details
            var details = "Exception thrown somewhere";
            Assert.IsNotNull(details);
            Assert.DoesNotThrow(() => DynamoViewModel.ReportABug(details));

            // give the system time to launch a browser & open the page
            Thread.Sleep(3000);

            // check browser is open on correct page
            Assert.True(BrowserIsOpenOnPageWithTitleMatching(TargetWords));
        }

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

        private bool ContainsAllTargetWords(string source)
        {
            return TargetWords.TrueForAll(x => source.Contains(x));
        }
    }
}
