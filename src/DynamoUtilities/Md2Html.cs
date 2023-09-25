using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using DynamoUtilities.Properties;

namespace Dynamo.Utilities
{
    //TODO move to new file.
    /// <summary>
    /// Base class for Dynamo CLI wrappers 
    /// </summary>
    internal abstract class CLIWrapper : IDisposable
    {
        protected const string endOfDataToken = @"<<<<<Eod>>>>>";
        protected const string startofDataToken = @"<<<<<Sod>>>>>";
        protected readonly Process process = new Process();
        protected bool started;
        internal event Action<string> MessageLogged;

        public virtual void Dispose()
        {
            process.ErrorDataReceived -= Process_ErrorDataReceived;
            KillProcess();
        }

        /// <summary>
        /// Start the process. 
        /// </summary>
        /// <param name="relativeEXEPath">relative path to the exe to start.</param>
        /// <param name="argString">argument string to pass to process.</param>
        protected virtual void StartProcess(string relativeEXEPath, string argString)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                RedirectStandardError = true,

                UseShellExecute = false,
                Arguments = argString,
                FileName = GetToolPath(relativeEXEPath)
            };

            process.StartInfo = startInfo;
            try
            {
                process.Start();
                started = true;
                //the only purspose here is to avoid deadlocks when std err gets filled up 4kb
                //in long running processes.
                process.ErrorDataReceived += Process_ErrorDataReceived;
                process.BeginErrorReadLine();
                
            }
            catch (Win32Exception)
            {
                // Do nothing
            }
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
           //do nothing, we just want to empty the error stream.
        }



        /// <summary>
        /// Kill the CLI tool - if running
        /// </summary>
        protected void KillProcess()
        {
            if (started)
            {
                if (!process.HasExited)
                {
                    process.Kill();
                }
                started = false;
            }
            process.Dispose();
        }
        /// <summary>
        /// Compute the location of the CLI tool.
        /// </summary>
        /// <returns>Returns full path to the CLI tool</returns>
        protected static string GetToolPath(string relativePath)
        {
            var rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new ArgumentNullException(nameof(Path.GetDirectoryName));
            var toolPath = Path.Combine(rootPath,relativePath);
            return toolPath;
        }
        //TODO if we see any issues with deadlocks we can try using a timeout on another thread.
        /// <summary>
        /// Read data from CLI tool
        /// <returns>Returns data read from CLI tool</returns>
        /// </summary>
        protected virtual string GetData()
        {
            if (process.HasExited)
            {
                return string.Empty;
            }
            using (var writer = new StringWriter())
            {
                var done = false;
                var start = false;
                while (!done)
                {
                    try
                    {
                        var line = process.StandardOutput.ReadLine();
                        MessageLogged?.Invoke(line);
                        if (line == null || line == startofDataToken)
                        {
                            start = true;
                            continue;//don't record start token to stream.
                        }
                        if (line == null || line == endOfDataToken)
                        {
                            done = true;
                        }
                        else
                        {   //if we have started recieving valid data, start recording
                            if (!string.IsNullOrWhiteSpace(line) && start)
                            {
                                writer.WriteLine(line);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        KillProcess();
                        return GetCantCommunicateErrorMessage();
                    }
                }

                return writer.ToString();
            }
        }

        protected void RaiseMessageLogged(string message)
        {
            MessageLogged?.Invoke(message);
        }

        /// <summary>
        /// Can't start Error message
        /// </summary>
        /// <returns>Returns error message</returns>
        protected abstract string GetCantStartErrorMessage();


        /// <summary>
        /// Can't communicate Error message
        /// </summary>
        /// <returns>Returns error message</returns>
        protected abstract string GetCantCommunicateErrorMessage();
 


    }
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
