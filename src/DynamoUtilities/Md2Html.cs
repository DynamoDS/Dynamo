using System;
using System.IO;
using System.Threading;
using DynamoUtilities.Properties;
using Newtonsoft.Json.Linq;

namespace Dynamo.Utilities
{
    /// <summary>
    /// Utilities for converting Markdown to html and for sanitizing html
    /// The Md2Html command line tool is used for doing the actual conversion/santizing
    /// (This tool is delivered as part of Dynamo)
    /// This class is not thread safe so please instantiate this class in the same thread that
    /// you intend to use.
    /// But multiple instances of the class is supported.
    /// </summary>

    internal class Md2Html : CLIWrapper
    {
        private string relativePath = Path.Combine(@"Md2Html", @"Md2Html.exe");
        private int processCommunicationTimeoutms = 5000;

        /// <summary>
        /// Constructor
        /// Start the CLI tool and keep it around
        /// </summary>
        internal Md2Html()
        {
            StartProcess(relativePath,string.Empty);
        }

        /// <summary>
        /// Kill the CLI tool, if still running
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            KillProcess();
        }

        /// <summary>
        /// Kill the CLI tool, if still running
        /// </summary>
        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Converts a markdown string into Html.
        /// </summary>
        /// <param name="mdString"></param>
        /// <param name="mdPath"></param>
        /// <returns>Returns converted markdown as html</returns>
        internal string ParseMd2Html(string mdString, string mdPath)
        {
            if (!started)
            {
                return GetCantStartErrorMessage();
            }

            try
            {
                process.StandardInput.WriteLine(@"<<<<<Convert>>>>>");
                process.StandardInput.WriteLine(mdPath);
                process.StandardInput.WriteLine(mdString);
                process.StandardInput.WriteLine(@"<<<<<Eod>>>>>");
            }
            catch (Exception e) when (e is IOException || e is ObjectDisposedException)
            {
                KillProcess();
                return GetCantCommunicateErrorMessage();
            }

            return GetData(processCommunicationTimeoutms);
        }

        /// <summary>
        /// Sanitize Html
        /// </summary>
        /// <param name="content"></param>
        /// <returns>Returns Sanitized Html or an empty string if no sanitization was needed</returns>
        internal string SanitizeHtml(string content)
        {
            if (!started)
            {
                return GetCantStartErrorMessage();
            }

            try
            {
                process.StandardInput.WriteLine(@"<<<<<Sanitize>>>>>");
                process.StandardInput.WriteLine(content);
                process.StandardInput.WriteLine(@"<<<<<Eod>>>>>");
            }
            catch (Exception e) when (e is IOException || e is ObjectDisposedException)
            {
                KillProcess();
                return GetCantCommunicateErrorMessage();
            }

            var output = GetData(processCommunicationTimeoutms);

           return output;
        }

        /// <summary>
        /// Can't start Error message
        /// </summary>
        /// <returns>Returns error message</returns>
        protected override string GetCantStartErrorMessage()
        {
            return string.Format(Resources.Md2HtmlCantStartError, GetToolPath(relativePath));
        }

        /// <summary>
        /// Can't communicate Error message
        /// </summary>
        /// <returns>Returns error message</returns>
        protected override string GetCantCommunicateErrorMessage()
        {
            return string.Format(Resources.Md2HtmlCantCommunicateError, GetToolPath(relativePath));
        }
    }
}
