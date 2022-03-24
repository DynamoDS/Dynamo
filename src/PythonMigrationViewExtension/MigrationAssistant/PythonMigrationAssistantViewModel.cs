using System;
using System.IO;
using System.Windows;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Dynamo.Core;
using Dynamo.Graph.Workspaces;
using Dynamo.Interfaces;
using Dynamo.PythonMigration.Controls;
using Dynamo.PythonMigration.Differ;
using Dynamo.PythonServices;
using Dynamo.Wpf.Utilities;
using Python.Runtime;
using PythonNodeModels;

namespace Dynamo.PythonMigration.MigrationAssistant
{
    internal class PythonMigrationAssistantViewModel : NotificationObject
    {
        private readonly string disableMigrationAssistantWarningFileName = @"MigrationAssistantWarningSetting.txt";
        private readonly string warningDismissPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Dynamo\");
        private readonly WorkspaceModel workspace;
        private readonly string backupDirectory;
        private readonly Version dynamoVersion;
        private PythonNode PythonNode;

        private IDiffViewViewModel currentViewModel;
        private SideBySideDiffModel diffModel;

        /// <summary>
        /// The original Python 2 code
        /// </summary>
        public string OldCode { get; private set; }

        /// <summary>
        /// The Python code after the migration assistants changes has been applied
        /// </summary>
        public string NewCode { get; private set; }

        public IDiffViewViewModel CurrentViewModel
        {
            get { return this.currentViewModel; }
            set { this.currentViewModel = value; RaisePropertyChanged(nameof(this.CurrentViewModel)); }
        }

        public PythonMigrationAssistantViewModel(PythonNode pythonNode, WorkspaceModel workspace, IPathManager pathManager, Version dynamoVersion)
        {
            PythonNode = pythonNode;
            OldCode = pythonNode.Script;

            this.workspace = workspace;
            backupDirectory = pathManager.BackupDirectory;
            this.dynamoVersion = dynamoVersion;

            try
            {
                MigrateCode();
            }
            catch (PythonException)
            {
                var sidebyside = new SideBySideDiffBuilder();
                diffModel = sidebyside.BuildDiffModel(OldCode, OldCode, false);

                SetSideBySideViewModel();
                
                CurrentViewModel.DiffState = State.Error;
                return;
            }
            SetSideBySideViewModel();

        }

        #region Code migration

        private void MigrateCode()
        {
            NewCode = ScriptMigrator.MigrateCode(OldCode);

            var sidebyside = new SideBySideDiffBuilder();
            diffModel = sidebyside.BuildDiffModel(OldCode, NewCode, false);
        }

        /// <summary>
        /// Replaces the code in the Python node with the code changes made by the Migration Assistant.
        /// </summary>
        public void ChangeCode()
        {
            if (CurrentViewModel.DiffState == State.NoChanges)
            {
                PythonNode.EngineName = PythonEngineManager.CPython3EngineName;
                return;
            }

            if (!Models.DynamoModel.IsTestMode && !File.Exists(GetMigrationAssistantDisclaimerDismissFile()))
            {
                var warningMessage = new MigrationAssistantDisclaimer(this);
                warningMessage.ShowDialog();
                if (!warningMessage.DisclaimerAccepted)
                    return;
            }

            SavePythonMigrationBackup();
            PythonNode.MigrateCode(this.NewCode);
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
            if (!Models.DynamoModel.IsTestMode)
            {
                var message = string.Format(Properties.Resources.PythonMigrationBackupFileCreatedMessage, path);
                MessageBoxService.Show(message, string.Empty, MessageBoxButton.OK, MessageBoxImage.None);
            }
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
            }
        }

        private void SetSideBySideViewModel()
        {
            CurrentViewModel = new SideBySideViewModel(diffModel);
        }

        private void SetInlineViewModel()
        {
            CurrentViewModel = new InLineViewModel(diffModel);
        }

        #endregion

        internal void DisableMigrationAssistantDisclaimer()
        {
            var filePath = GetMigrationAssistantDisclaimerDismissFile();

            var timeStamp = DateTime.Now.ToString();
            var machineName = Environment.MachineName;

            var file = new FileInfo(filePath);
            file.Directory.Create();
            File.WriteAllText(file.FullName, string.Format("{0} {1}", timeStamp, machineName));
        }

        private string GetMigrationAssistantDisclaimerDismissFile()
        {
            return Path.Combine(warningDismissPath, string.Format("{0}.{1}", dynamoVersion.Major,dynamoVersion.Minor), disableMigrationAssistantWarningFileName);
        }
    }
}
