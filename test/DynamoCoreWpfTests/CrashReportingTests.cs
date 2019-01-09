using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Dynamo.ViewModels;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    class CrashReportingTests : DynamoViewModelUnitTest
    {
        /// <summary>
        /// Browser tab open on the GitHub new issue page should contain these words in the title
        /// </summary>
        List<string> TargetWords = new List<string> { "NEW ISSUE", "DYNAMODS" };
        /// <summary>
        /// If user is not logged in to GitHub, browser tab trying to open the GitHub new issue page should contain these words in the title
        /// </summary>
        List<string> TargetWordsLoggedOutFallback = new List<string> { "SIGN IN", "GITHUB" };

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
