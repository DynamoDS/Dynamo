namespace DynamoAnalyzer.Helper
{
    /// <summary>
    /// Provides methods to handle the workspace
    /// </summary>
    internal static class WorkspaceHelper
    {
        private static DirectoryInfo workspace;

        public static DirectoryInfo GetWorkspace()
        {
            string ws = string.IsNullOrEmpty(ConfigHelper.GetConfiguration()["Workspace"]) ? Path.Combine(Path.GetTempPath(), "DynamoDS") : ConfigHelper.GetConfiguration()["Workspace"];
            return workspace ??= new DirectoryInfo(ws);
        }
    }
}
