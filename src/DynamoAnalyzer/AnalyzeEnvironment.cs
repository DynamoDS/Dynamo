using DynamoAnalyzer.Helper;

namespace DynamoAnalyzer
{
    /// <summary>
    /// Common dependencies used during the analysis
    /// </summary>
    internal static class AnalyzeEnvironment
    {
        private static DirectoryInfo _workspace;

        public static DirectoryInfo GetWorkspace()
        {
            string ws = string.IsNullOrEmpty(ConfigHelper.GetConfiguration()["Workspace"]) ? Path.Combine(Path.GetTempPath(), "DynamoDS") : ConfigHelper.GetConfiguration()["Workspace"];
            return _workspace ??= new DirectoryInfo(ws);
        }
    }
}
