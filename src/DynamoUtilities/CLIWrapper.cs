using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.Utilities
{
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
                StandardInputEncoding = Encoding.UTF8,
                StandardOutputEncoding = Encoding.UTF8,
                UseShellExecute = false,
                Arguments = argString,
                FileName = GetToolPath(relativeEXEPath)
            };

            process.StartInfo = startInfo;
            try
            {
                process.Start();
                started = true;
                //the only purpose here is to avoid deadlocks when std err gets filled up 4kb
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
        /// <summary>
        /// Read data from CLI tool
        /// </summary>
        /// <param name="timeoutms">will return empty string if we don't finish reading all data in the timeout provided in milliseconds.</param>
        /// <param name="mockReadLine"> if this delegate is non null, it will be used instead of communicating with std out of the process. Used for testing only.</param>
        ///  <param name="decodeData"> if this flag is true means that the process is returning base64 encoded data then we need to decode it</param>
        /// <returns></returns>
        protected virtual string GetData(int timeoutms, Func<string> mockReadLine = null, bool decodeData = false)
        {
            var readStdOutTask = Task.Run(() =>
            {
                if (CheckIfProcessHasExited())
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
                            string line = null;
                            if(mockReadLine != null)
                            {
                                line = mockReadLine.Invoke();
                            }
                            else
                            {
                                line = process.StandardOutput.ReadLine();
                            }
                            
                           
                            MessageLogged?.Invoke(line);
                            if (line == null || line == startofDataToken)
                            {
                                start = true;
                                continue; //don't record start token to stream.
                            }

                            if (line == null || line == endOfDataToken)
                            {
                                done = true;
                            }
                            else
                            {
                                //if we have started recieving valid data, start recording
                                if (!string.IsNullOrWhiteSpace(line) && start)
                                {
                                    if(decodeData == true)
                                    {
                                        //Encode the info to base64 and send it to the Md2Html tool
                                        var base64EncodedBytes = Convert.FromBase64String(line);
                                        line = Encoding.UTF8.GetString(base64EncodedBytes);
                                    }                                 
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
            });
            var completedTask = Task.WhenAny(readStdOutTask, Task.Delay(TimeSpan.FromMilliseconds(timeoutms))).Result;
            //if the completed task was our read std out task, then return the data
            //else we timed out, so return an empty string.
            return completedTask == readStdOutTask ? readStdOutTask.Result : string.Empty;
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
 
        protected virtual bool CheckIfProcessHasExited()
        {
            return process.HasExited;
        }

    }
}
