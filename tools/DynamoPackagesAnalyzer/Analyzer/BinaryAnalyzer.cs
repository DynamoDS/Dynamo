using DynamoPackagesAnalyzer.Helper;
using DynamoPackagesAnalyzer.Models;
using DynamoPackagesAnalyzer.Models.UpgradeAssistant;

namespace DynamoPackagesAnalyzer.Analyzer
{
    /// <summary>
    /// Implementation of the binary analyzer using the analyzebinaries feature of the upgrade-assistant tool
    /// </summary>
    internal class BinaryAnalyzer
    {
        private readonly FileInfo file;
        private readonly AnalyzedPackage package;
        private readonly string workspace;

        /// <summary>
        /// Name of the file being analyzed
        /// </summary>
        internal string Name
        {
            get
            {
                return file?.Name;
            }
        }

        /// <summary>
        /// The path of the DLL currently being analyzed
        /// </summary>
        internal string Path
        {
            get
            {
                if (file != null)
                {
                    string workdir = System.IO.Path.Combine(WorkspaceHelper.GetWorkspace().FullName, System.IO.Path.GetFileNameWithoutExtension(package.ArchiveName));
                    return file.FullName.Replace(workdir + "\\", "").Replace(file.Name, "");
                }
                return null;
            }
        }

        /// <summary>
        /// Creates a new BinaryAnalyzer instance
        /// </summary>
        /// <param name="file"></param>
        /// <param name="package"></param>
        /// <param name="workspace"></param>
        /// <exception cref="InvalidOperationException"></exception>
        internal BinaryAnalyzer(FileInfo file, AnalyzedPackage package, string workspace)
        {
            this.file = file;
            this.package = package;
            if (string.IsNullOrEmpty(workspace))
            {
                throw new InvalidOperationException($"{nameof(workspace)} can't be null");
            }
            this.workspace = workspace;
        }

        /// <summary>
        /// Starts the upgrade-assistant tool with the dll defined in <see cref="file"/>
        /// </summary>
        /// <returns></returns>
        internal async Task<AnalyzedPackage> StartAnalysis()
        {
            AnalyzedPackage dllAnalysis = package.Copy();
            dllAnalysis.ArtifactName = Name;
            dllAnalysis.ArtifactPath = Path;

            try
            {
                await ProcessHelper.StartAnalyzeProcess(workspace, GetArgs());
                FileInfo sarifFile = new FileInfo(System.IO.Path.Combine(workspace, ConfigHelper.GetConfiguration()["SarifFileName"]));
                Sarif result = await JsonHelper.Read<Sarif>(sarifFile);
                Result[] res = result.Runs.SelectMany(f => f.Results).ToArray();
                dllAnalysis.RequirePort = res.Any();

                //If the dll has a different assembly name, this line can get that name reported by upgrade-assistant
                List<string> f_res = new List<string>(res.SelectMany(f => f.Locations.Select(g => g.PhysicalLocation.ArtifactLocation.URI.Replace("file:///", "").Replace(workspace.Replace("\\", "/"), ""))).Distinct());
                f_res.AddRange(res.Select(f => f.Message.Text.Replace("\"", "'")));

                dllAnalysis.Result = f_res.ToArray();
            }
            catch (Exception ex)
            {
                dllAnalysis.HasAnalysisError = true;
                dllAnalysis.Result = new string[] { (ex.InnerException ?? ex).Message };
            }

            return dllAnalysis;
        }

        /// <summary>
        /// The arguments required by analyzebinaries feature of the Microsoft's upgrade-assistant tool 
        /// </summary>
        /// <returns></returns>
        private string[] GetArgs()
        {
            string[] args = new string[] {
                "analyzebinaries",//Experimental feature to analyze binaries
                "-p Windows",//Target platform Windows | Linux
                "-t LTS",//Latest Long Term Support framework (net6)
                $"\"{file.FullName}\"",//Path to the binary file
            };

            return args;
        }

        /// <summary>
        /// Returns the package information of the file being analyzed
        /// </summary>
        /// <returns></returns>
        internal Task<AnalyzedPackage> GetAnalyzedPackage()
        {
            return Task.FromResult(package);
        }
    }
}
