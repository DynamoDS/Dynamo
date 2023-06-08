namespace DynamoPackagesAnalyzer.Helper
{
    /// <summary>
    /// Provides methods to handle the workspace
    /// </summary>
    internal static class WorkspaceHelper
    {
        internal static DirectoryInfo workspace;

        internal static DirectoryInfo GetWorkspace()
        {
            var ws = string.IsNullOrEmpty(ConfigHelper.GetConfiguration()["Workspace"]) ? Path.Combine(Path.GetTempPath(), "DynamoDS") : ConfigHelper.GetConfiguration()["Workspace"];
            return workspace ??= new DirectoryInfo(ws);
        }
    }
}
