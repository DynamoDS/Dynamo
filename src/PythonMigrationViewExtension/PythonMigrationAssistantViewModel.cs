using Dynamo.PythonMigration.MigrationAssistant;
using Dynamo.ViewModels;
using PythonNodeModels;
using System.IO;
using PythonNodeModelsWpf;
using System.Collections.Generic;

namespace Dynamo.PythonMigration
{
    internal class PythonMigrationAssistantViewModel
    {
        public string OldCode { get; set; }
        public string NewCode { get; set; }
        private PythonNode PythonNode { get; set; }
        public DynamoViewModel DynamoViewModel { get; set; }

        public PythonMigrationAssistantViewModel(PythonNode pythonNode, DynamoViewModel dynamoViewModel)
        {
            PythonNode = pythonNode;
            OldCode = pythonNode.Script;
            DynamoViewModel = dynamoViewModel;
            MigrateCode();
        }

        private void MigrateCode()
        {
            NewCode = ScriptMigrator.MigrateCode(OldCode);
        }

        public void ChangeCode()
        {
            SavePythonMigrationBackup();
            PythonNode.MigrateCode(NewCode);
        }

        private void SavePythonMigrationBackup()
        {
            var path = GetPythonMigrationBackupPath();
            if (File.Exists(path))
                return;

            DynamoViewModel.SaveAs(path, true);
        }

        private string GetPythonMigrationBackupPath()
        {
            var workspaceName = DynamoViewModel.CurrentSpace.Name;
            var backupDirectory = DynamoViewModel.Model.PathManager.BackupDirectory;
            var fileName = Path.Combine(backupDirectory, workspaceName) + string.Concat(".", Properties.Resources.PythonMigrationBackupExtension, ".dyn");
            return fileName;
        }
    }
}
