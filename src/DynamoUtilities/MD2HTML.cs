using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Dynamo.Utilities
{
    public class MD2HTML
    {
        private readonly Process process = new Process();
        public MD2HTML()
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(ProcessExit);

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,

                UseShellExecute = false,
                Arguments = @"",
                FileName = ToolPath()
            };

            process.StartInfo = startInfo;
            process.Start();
        }

        ~MD2HTML()
        {
            KillProcess();
        }

        public void ParseToHtml(ref StringWriter writer, string mdString, string mdPath)
        {
            process.StandardInput.WriteLine("<<<<<Convert>>>>>");
            process.StandardInput.WriteLine(mdPath);
            process.StandardInput.WriteLine(mdString);
            process.StandardInput.WriteLine("<<<<<Eod>>>>>");
            var done = false;
            while(!done)
            {
                var line = process.StandardOutput.ReadLine();
                if (line == null || line == "<<<<<Eod>>>>>")
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
        }

        private static string ToolPath ()
        {
            var rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new ArgumentNullException("Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)");
            var toolPath = Path.Combine(rootPath, @"MD2HTML\MD2HTML.exe");
            return toolPath;
        }

        private void KillProcess()
        {
            if (!process.HasExited)
            {
                process.Kill();
            }
        }
        private void ProcessExit(object sender, EventArgs e)
        {
            KillProcess();
        }
    }
}
