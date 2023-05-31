using System.Diagnostics;

namespace DynamoAnalyzer.Helper
{
    /// <summary>
    /// Provides methods to start new processes
    /// </summary>
    public static class ProcessHelper
    {
        /// <summary>
        /// Allows to start a new instance of the upgrade-assistant
        /// </summary>
        public static async Task<string> StartAnalyzeProcess(string workingDir, params string[] args)
        {
            Process proc = new Process();

            await Task.Run(() =>
            {
                proc.StartInfo.FileName = "upgrade-assistant";
                proc.StartInfo.Arguments = string.Join(" ", args);
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.UseShellExecute = false;
                if (!string.IsNullOrEmpty(workingDir))
                {
                    proc.StartInfo.WorkingDirectory = workingDir;
                }
                proc.Start();
                proc.WaitForExit();
            });

            string output = proc.StandardOutput.ReadToEnd();

            if (proc.ExitCode != 0)
            {
                throw new InvalidOperationException("Unable to run upgrade-assistant");
            }

            return output;
        }
    }
}
