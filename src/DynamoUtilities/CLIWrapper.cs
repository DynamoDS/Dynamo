using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;

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
            var toolPath = Path.Combine(rootPath, relativePath);
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
                    catch
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
}
