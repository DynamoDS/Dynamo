using DynamoAnalyzer.Helper;
using DynamoAnalyzer.Models;
using DynamoAnalyzer.Models.UpgradeAssistant;

namespace DynamoAnalyzer.Analyzer
{
    /// <summary>
    /// Implementation of the binary analyzer using the analyzebinaries of the upgrade-assistant tool
    /// </summary>
    public class BinaryAnalyzer : IAnalyze
    {
        private readonly FileInfo _file;
        private readonly AnalyzedPackage _package;
        private readonly string workspace;

        /// <summary>
        /// Name of the file being analyzed
        /// </summary>
        public string Name
        {
            get
            {
                return _file?.Name;
            }
        }

        /// <summary>
        /// The path of the DLL currently being analyzed
        /// </summary>
        public string Path
        {
            get
            {
                if (_file != null)
                {
                    string workdir = System.IO.Path.Combine(AnalyzeEnvironment.GetWorkspace().FullName, System.IO.Path.GetFileNameWithoutExtension(_package.ArchiveName));
                    return _file.FullName.Replace(workdir + "\\", "").Replace(_file.Name, "");
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
            _file = file;
            _package = package;
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
            AnalyzedPackage dllAnalysis = _package.Copy();
            dllAnalysis.ArtifactName = Name;
            dllAnalysis.ArtifactPath = Path;

            try
            {
                await ProcessHelper.StartAnalyzeProcess(workspace, GetArgs());
                FileInfo sarifFile = new FileInfo(System.IO.Path.Combine(workspace, ConfigHelper.GetConfiguration()["SarifFileName"]));
                Sarif result = await SarifHelper.ReadSarif(sarifFile);
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
                _file.FullName,
            };

            return args;
        }
    }
}
