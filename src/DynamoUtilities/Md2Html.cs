using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using DynamoUtilities.Properties;

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

    internal class Md2Html : IDisposable
    {
        private readonly Process process = new Process();
        private bool started;
        /// <summary>
        /// Constructor
        /// Start the CLI tool and keep it around
        /// </summary>
        internal Md2Html()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,

                UseShellExecute = false,
                Arguments = @"",
                FileName = GetToolPath()
            };

            process.StartInfo = startInfo;
            try
            {
                process.Start();
                started = true;
            }
            catch(Win32Exception)
            {
                // Do nothing
            }
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
        public void Dispose()
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

            var output = GetData();

            return output;
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

            var output = GetData();

           return output;
        }

        /// <summary>
        /// Read data from CLI tool
        /// <returns>Returns data read from CLI tool</returns>
        /// </summary>
        private string GetData()
        {
            using (var writer = new StringWriter())
            {
                var done = false;

                while (!done)
                {
                    try
                    {
                        var line = process.StandardOutput.ReadLine();

                        if (line == null || line == @"<<<<<Eod>>>>>")
                        {
                            done = true;
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                writer.WriteLine(line);
                            }
                        }
                    }
                    catch (Exception e) when (e is IOException || e is OutOfMemoryException)
                    {
                        KillProcess();
                        return GetCantCommunicateErrorMessage();
                    }
                }

                return writer.ToString();
            }
        }


        /// <summary>
        /// Compute the location of the CLI tool
        /// </summary>
        /// <returns>Returns full path to the CLI tool</returns>
        private static string GetToolPath ()
        {
            var rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new ArgumentNullException(nameof(Path.GetDirectoryName));
            var toolPath = Path.Combine(rootPath, @"Md2Html", @"Md2Html.exe");
            return toolPath;
        }

        /// <summary>
        /// Kill the CLI tool - if running
        /// </summary>
        private void KillProcess()
        {
            if (started)
            {
                if (!process.HasExited)
                {
                    process.Kill();
                }
                started = false;
            }
        }

        /// <summary>
        /// Can't start Error message
        /// </summary>
        /// <returns>Returns error message</returns>
        private string GetCantStartErrorMessage()
        {
            return string.Format(Resources.Md2HtmlCantStartError, GetToolPath());
        }

        /// <summary>
        /// Can't communicate Error message
        /// </summary>
        /// <returns>Returns error message</returns>
        private string GetCantCommunicateErrorMessage()
        {
            return string.Format(Resources.Md2HtmlCantCommunicateError, GetToolPath());
        }
    }
}
