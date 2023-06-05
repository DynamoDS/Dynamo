using DynamoAnalyzer.Helper;
using DynamoAnalyzer.Models;
using DynamoAnalyzer.Models.UpgradeAssistant;

namespace DynamoAnalyzer.Analyzer
{
    /// <summary>
    /// Implementation of the binary analyzer using the analyzebinaries feature of the upgrade-assistant tool
    /// </summary>
    internal class BinaryAnalyzer : IAnalyze
    {
        private readonly FileInfo file;
        private readonly AnalyzedPackage package;
        private readonly string workspace;

        /// <summary>
        /// Name of the file being analyzed
        /// </summary>
        public string Name
        {
            get
            {
                return file?.Name;
            }
        }

        /// <summary>
        /// The path of the DLL currently being analyzed
        /// </summary>
        public string Path
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
        public BinaryAnalyzer(FileInfo file, AnalyzedPackage package, string workspace)
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
        /// Start the upgrade-assistant tool
        /// </summary>
        /// <returns></returns>
        public async Task<AnalyzedPackage> Process()
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
        /// The arguments required by the analyzebinaries tool
        /// </summary>
        /// <returns></returns>
        private string[] GetArgs()
        {
            string[] args = new string[] {
                "analyzebinaries",
                "-p Windows",
                "-t LTS",
                $"\"{file.FullName}\"",
            };

            return args;
        }

        public Task<AnalyzedPackage> GetAnalyzedPackage()
        {
            return Task.FromResult(package);
        }
    }
}
