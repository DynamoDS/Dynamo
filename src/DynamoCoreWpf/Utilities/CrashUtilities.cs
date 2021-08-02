﻿using Dynamo.Configuration;
using Dynamo.PackageManager;
using Dynamo.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dynamo.Wpf.Utilities
{
    static class CrashUtilities
    {
        internal static string GithubNewIssueUrlFromCrashContent(object crashContent)
        {
            var baseUri = new UriBuilder(Configurations.GitHubBugReportingLink);

            // provide fallback values for text content in case Resources or Assembly calls fail
            var issueTitle = Properties.Resources.CrashPromptGithubNewIssueTitle ?? "Crash report from Dynamo {0}";
            var dynamoVersion = AssemblyHelper.GetDynamoVersion().ToString() ?? "2.1.0+";
            var content = GitHubCrashReportBody(crashContent);

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
        /// Converts packages information into markdown format for use in Github issue body
        /// </summary>
        /// <param name="packageLoader">Package loader</param>
        /// <returns>A markdown format string to use in issue body.</returns>
        internal static string PackagesToMakrdown(PackageLoader packageLoader)
        {
            if (packageLoader != null)
            {
                //List of the names of all the loaded packages with their corresponding version
                var packagesNames = packageLoader.LocalPackages.Select(o => o.Name + " " + o.VersionName).ToList();
                //Package's issue section in markdown format
                string markdownText;
                if (packagesNames.Any())
                {
                    markdownText = "### Loaded Packages" + Environment.NewLine;
                    foreach (var name in packagesNames)
                    {
                        markdownText += "- " + name + Environment.NewLine;
                    }
                }
                else
                {
                    markdownText = "No loaded packages were found.";
                }
                return markdownText;
            }
            else
            {
                return "(Fill in here)";
            }
        }

        /// <summary>
        /// Formats crash details and adds metadata for use in Github issue body
        /// </summary>
        /// <param name="details">Crash details, such as a stack trace.</param>
        /// <returns>A formatted, but not escaped, string to use as issue body.</returns>
        private static string GitHubCrashReportBody(object details)
        {
            var markdownPackages = details?.ToString() ?? string.Empty;

            // This functionality was not available prior to version 2.1.0, so it should be the fallback value
            var dynamoVersion = AssemblyHelper.GetDynamoVersion().ToString() ?? "2.1.0+";

            return BuildMarkdownContent(dynamoVersion, markdownPackages);
        }

        /// <summary>
        /// Builds a Markdown string that will be posted to our new GitHub issue
        /// </summary>
        /// <param name="dynamoVersion">Dynamo version that should be recorded in the issue report</param>
        /// <param name="markdownPackages">Section of the issue with the loaded packages in markdown format</param>
        /// <returns></returns>
        internal static string BuildMarkdownContent(string dynamoVersion, string markdownPackages)
        {
            return
                "# Issue Description" + Environment.NewLine +
                "Please fill in the following information to help us reproduce the issue:" + Environment.NewLine +
                Environment.NewLine +

                "## Dynamo version" + Environment.NewLine +
                "Dynamo: " + "`" + dynamoVersion + "`" + Environment.NewLine + Environment.NewLine +

                "## Operating system" + Environment.NewLine +
                "OS: " + "`" + Environment.OSVersion + "`" + Environment.NewLine + Environment.NewLine +

                "## What did you do?" + Environment.NewLine +
                "(Fill in here)" + Environment.NewLine + Environment.NewLine +

                "## What did you expect to see?" + Environment.NewLine +
                "(Fill in here)" + Environment.NewLine + Environment.NewLine +

                "## What did you see instead?" + Environment.NewLine +
                "(Fill in here)" + Environment.NewLine + Environment.NewLine +

                "## What packages or external references (if any) were used?" + Environment.NewLine +
                markdownPackages + Environment.NewLine + Environment.NewLine +

                "## Stack Trace" + Environment.NewLine +
                "```" + Environment.NewLine +
                "(From the Dynamo crash window select 'Details' -> 'Copy' and paste here)" + Environment.NewLine +
                "```" + Environment.NewLine + Environment.NewLine +

                "---" + Environment.NewLine +
                "CLR: " + "`" + Environment.Version + "`" + Environment.NewLine;
        }
    }
}
