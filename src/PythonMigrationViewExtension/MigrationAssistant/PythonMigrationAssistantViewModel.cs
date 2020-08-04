using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Dynamo.Core;
using Dynamo.Graph.Workspaces;
using Dynamo.Interfaces;
using Dynamo.PythonMigration.Differ;
using PythonNodeModels;
using System;
using System.IO;
using System.Windows;

namespace Dynamo.PythonMigration.MigrationAssistant
{
    internal class PythonMigrationAssistantViewModel : NotificationObject
    {
        internal readonly string disableMigrationAssistantWarningFileName = @"Dynamo\MigrationAssistantWarningSetting.txt";
        internal readonly string localFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        public string OldCode { get; set; }
        public string NewCode { get; set; }

        private readonly WorkspaceModel workspace;
        private readonly string backupDirectory;
        private PythonNode PythonNode;

        private IDiffViewViewModel currentViewModel;
        private SideBySideDiffModel diffModel;

        public IDiffViewViewModel CurrentViewModel
        {
            get { return this.currentViewModel; }
            set { this.currentViewModel = value; RaisePropertyChanged(nameof(this.CurrentViewModel)); }
        }

        public PythonMigrationAssistantViewModel(PythonNode pythonNode, WorkspaceModel workspace, IPathManager pathManager)
        {
            this.PythonNode = pythonNode;
            this.OldCode = pythonNode.Script;

            this.workspace = workspace;
            this.backupDirectory = pathManager.BackupDirectory;

            MigrateCode();
            SetSideBySideViewModel();
        }

        #region Code migration

        private void MigrateCode()
        {
            this.NewCode = ScriptMigrator.MigrateCode(this.OldCode);

            var sidebyside = new SideBySideDiffBuilder();
            this.diffModel = sidebyside.BuildDiffModel(this.OldCode, this.NewCode);
        }

        public void ChangeCode()
        {
            SavePythonMigrationBackup();
            this.PythonNode.MigrateCode(this.NewCode);
        }

        #endregion

        #region Backup

        private void SavePythonMigrationBackup()
        {
            // only create a backup file the first time a migration is performed on this graph/custom node file
            var path = GetPythonMigrationBackupPath();
            if (File.Exists(path))
                return;

            this.workspace.Save(path, true);

            // notify user a backup file has been created
            var message = string.Format(Properties.Resources.PythonMigrationBackupFileCreatedMessage, path);
            MessageBox.Show(message);
        }

        private string GetPythonMigrationBackupPath()
        {
            var extension = this.workspace is CustomNodeWorkspaceModel ? ".dyf" : ".dyn";
            var fileName = string.Concat(this.workspace.Name, ".", Properties.Resources.PythonMigrationBackupExtension, extension);

            return Path.Combine(this.backupDirectory, fileName);
        }

        #endregion

        #region View mode

        internal void ChangeViewModel(ViewMode viewMode)
        {
            switch (viewMode)
            {
                case ViewMode.Inline:
                    SetInlineViewModel();
                    break;
                case ViewMode.SideBySide:
                    SetSideBySideViewModel();
                    break;
                default:
                    break;
            }
        }

        private void SetSideBySideViewModel()
        {
            this.CurrentViewModel = new SideBySideViewModel(this.diffModel);
        }

        private void SetInlineViewModel()
        {
            this.CurrentViewModel = new InLineViewModel(this.diffModel);
        }

        #endregion

        internal void DisableMigrationAssistanWarning()
        {
            var filePath = Path.Combine(localFolder, disableMigrationAssistantWarningFileName);

            var timeStamp = DateTime.Now.ToString();
            var machineName = Environment.MachineName;

            var file = new FileInfo(filePath);
            file.Directory.Create();
            File.WriteAllText(file.FullName, string.Format("{0} {1}", timeStamp, machineName));
        }
    }
}
