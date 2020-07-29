using DiffPlex.DiffBuilder;
using Dynamo.Core;
using Dynamo.Graph.Workspaces;
using Dynamo.Interfaces;
using Dynamo.PythonMigration.Differ;
using PythonNodeModels;
using System.IO;
using System.Windows;

namespace Dynamo.PythonMigration.MigrationAssistant
{
    internal class PythonMigrationAssistantViewModel : NotificationObject
    {
        public string OldCode { get; set; }
        public string NewCode { get; set; }

        private readonly WorkspaceModel workspace;
        private readonly string backupDirectory;
        private PythonNode PythonNode;

        private IDiffViewViewModel currentViewModel;
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
            var sidebyside = new SideBySideDiffBuilder();
            var sidebysideModel = sidebyside.BuildDiffModel(this.OldCode, this.NewCode);
            this.CurrentViewModel = new SideBySideViewModel(sidebysideModel);
        }

        private void SetInlineViewModel()
        {
            var inline = new InlineDiffBuilder();
            var inlineModel = inline.BuildDiffModel(this.OldCode, this.NewCode);
            this.CurrentViewModel = new InLineViewModel(inlineModel);
        }

        #endregion
    }
}
