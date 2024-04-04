using System.Diagnostics;

namespace DynamoPackagesAnalyzer.Helper
{
    /// <summary>
    /// Provides methods to start new processes
    /// </summary>
    internal static class ProcessHelper
    {
        /// <summary>
        /// Allows to start a new instance of the upgrade-assistant
        /// </summary>
        internal static async Task<string> StartAnalyzeProcess(string workingDir, params string[] args)
        {
            Process proc = new Process();
            bool procWasKilled = false;
            int processTimeOutInMins = int.Parse(ConfigHelper.GetConfiguration()["ProcessTimeOut"]);
            Stopwatch stopWatch = Stopwatch.StartNew();

            await Task.Run(() =>
            {
                proc.StartInfo.FileName = "upgrade-assistant";
                proc.StartInfo.Arguments = string.Join(" ", args);
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.UseShellExecute = false;
                if (!string.IsNullOrEmpty(workingDir))
                {
                    proc.StartInfo.WorkingDirectory = workingDir;
                }
                proc.Start();

                while (!proc.HasExited)
                {
                    if (stopWatch.Elapsed.Minutes >= processTimeOutInMins)
                    {
                        proc.Kill();
                        procWasKilled = true;
                    }
                    Thread.Sleep(1000);
                }
            });

            string output = proc.StandardOutput.ReadToEnd();

            if (proc.ExitCode != 0)
            {
                string error = proc.StandardError.ReadToEnd()?.Trim();

                throw procWasKilled switch
                {
                    true => new InvalidOperationException($"upgrade-assistant process was killed after {stopWatch.Elapsed}"),
                    _ => new InvalidOperationException(string.IsNullOrEmpty(error) ? "Unable to run upgrade-assistant" : error),
                };
            }

            return output;
        }
    }
}
