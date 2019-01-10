using Dynamo.Configuration;
using Dynamo.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.Wpf.Utilities
{
    static class CrashUtilities
    {

        internal static string GithubNewIssueUrlFromCrashContent(object crashContent)
        {
            var baseUri = new UriBuilder(Configurations.GitHubBugReportingLink);

            // provide fallback values for text content in case Resources or Assembly calls fail
            var issueTitle = Properties.Resources.CrashPromptGithubNewIssueTitle ?? "Crash report from Dynamo {0}";
            var dynamoVersion = AssemblyHelper.GetDynamoVersion().ToString() ?? "2.2+";
            var content = GithhubCrashReportBody(crashContent);

            // append the title and body to the URL as query parameters
            // making sure we properly escape content since stack traces may contain characters not suitable
            // for use in URLs
            var title = "title=" + Uri.EscapeDataString(string.Format(issueTitle, dynamoVersion));
            var body = "body=" + Uri.EscapeDataString(content);
            baseUri.Query = title + "&" + body;

            // this will properly format the string for use as a uri
            return baseUri.ToString();
        }

        /// <summary>
        /// Formats crash details & adds metadata for use in Github issue body
        /// </summary>
        /// <param name="details">Crash details, such as a stack trace.</param>
        /// <returns>A formatted but not excaped string to use as issue body.</returns>
        private static string GithhubCrashReportBody(object details)
        {
            var content = details?.ToString() ?? string.Empty;

            // This functionality was not available prior to version 2.2, so it should be the fallback value
            var dynamoVersion = AssemblyHelper.GetDynamoVersion().ToString() ?? "2.2+";

            return
                "Description: Please outline all steps required to reproduce the crash including all files and packages" + Environment.NewLine +
                "---" + Environment.NewLine +
                "OS: "      + "`"   + Environment.OSVersion + "`"   + Environment.NewLine +
                "CLR: "     + "`"   + Environment.Version   + "`"   + Environment.NewLine +
                "Dynamo: "  + "`"   + dynamoVersion         + "`"   + Environment.NewLine +
                "Details: "         + Environment.NewLine   + "```" + Environment.NewLine + content + Environment.NewLine + "```";
        }

    }
}
