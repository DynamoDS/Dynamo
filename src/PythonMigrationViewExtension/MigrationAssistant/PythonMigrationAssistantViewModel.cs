using Dynamo.Graph.Workspaces;
using Dynamo.Interfaces;
using PythonNodeModels;
using System.IO;
using System.Windows;

namespace Dynamo.PythonMigration.MigrationAssistant
{
    internal class PythonMigrationAssistantViewModel
    {
        /// <summary>
        /// The original Python 2 code
        /// </summary>
        public string OldCode { get; private set; }

        /// <summary>
        /// The Python code after the migration assistants changes has been applied
        /// </summary>
        public string NewCode { get; private set; }

        private readonly WorkspaceModel workspace;
        private readonly string backupDirectory;
        private PythonNode PythonNode;

        public PythonMigrationAssistantViewModel(PythonNode pythonNode, WorkspaceModel workspace, IPathManager pathManager)
        {
            this.PythonNode = pythonNode;
            this.OldCode = pythonNode.Script;

            this.workspace = workspace;
            this.backupDirectory = pathManager.BackupDirectory;

            MigrateCode();
        }

        /// <summary>
        /// Replaces the code in the Pyton node with the code changes made by the Migration Assistant.
        /// </summary>
        public void ChangeCode()
        {
            SavePythonMigrationBackup();
            this.PythonNode.MigrateCode(this.NewCode);
        }

        private void MigrateCode()
        {
            this.NewCode = ScriptMigrator.MigrateCode(this.OldCode);
        }

        private void SavePythonMigrationBackup()
        {
            // only create a backup file the first time a migration is performed on this graph/custom node file
            var path = GetPythonMigrationBackupPath();
            if (File.Exists(path))
                return;

            this.workspace.Save(path, true);

            // notify user a backup file has been created
            if (!Models.DynamoModel.IsTestMode)
            {
                var message = string.Format(Properties.Resources.PythonMigrationBackupFileCreatedMessage, path);
                MessageBox.Show(message);
            }
        }

        private string GetPythonMigrationBackupPath()
        {
            var extension = this.workspace is CustomNodeWorkspaceModel ? ".dyf" : ".dyn";
            var fileName = string.Concat(this.workspace.Name, ".", Properties.Resources.PythonMigrationBackupExtension, extension);

            return Path.Combine(this.backupDirectory, fileName);
        }
    }
}
