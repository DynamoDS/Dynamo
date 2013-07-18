using System;
using System.Collections.Generic;
using System.Globalization;
using Dynamo.Controls;
using Dynamo.Utilities;
using Microsoft.Practices.Prism.Commands;

namespace Dynamo.Commands
{

    public static partial class DynamoCommands
    {
        private static DynamoViewModel _vm = dynSettings.Controller.DynamoViewModel;
        private static readonly Queue<Tuple<object, object>> commandQueue = new Queue<Tuple<object, object>>();
        private static bool isProcessingCommandQueue = false;

        public static bool IsProcessingCommandQueue
        {
            get { return isProcessingCommandQueue; }
        }

        public static Queue<Tuple<object, object>> CommandQueue
        {
            get { return commandQueue; }
        }

        #region fields
        private static DelegateCommand<object> writeToLogCmd;
        private static DelegateCommand _reportABug;
        private static DelegateCommand _gotoWikiCommand;
        private static DelegateCommand _gotoSourceCommand;
        private static DelegateCommand<object> _exitCommand;
        private static DelegateCommand _cleanupCommand;
        private static DelegateCommand _showSaveImageDialogAndSaveResultCommand;
        private static DelegateCommand _showOpenDialogueAndOpenResultCommand;
        private static DelegateCommand _showSaveDialogIfNeededAndSaveResultCommand;
        private static DelegateCommand _showSaveDialogAndSaveResultCommand;
        private static DelegateCommand<object> _runExpressionCommand;
        private static DelegateCommand _showPackageManagerCommand;
        private static DelegateCommand _showNewFunctionDialogCommand;
        private static DelegateCommand<object> _openCommand;
        private static DelegateCommand _saveCommand;
        private static DelegateCommand<object> _saveAsCommand;
        private static DelegateCommand _clearCommand;
        private static DelegateCommand _homeCommand;
        private static DelegateCommand _layoutAllCommand;
        private static DelegateCommand _newHomeWorkspaceCommand;
        private static DelegateCommand<object> _copyCommand;
        private static DelegateCommand<object> _pasteCommand;
        private static DelegateCommand _toggleConsoleShowingCommand;
        private static DelegateCommand _cancelRunCommand;
        private static DelegateCommand<object> _saveImageCommand;
        private static DelegateCommand _clearLogCommand;
        private static DelegateCommand<object> _goToWorkspaceCommand;
        private static DelegateCommand<object> _displayFunctionCommand;
        private static DelegateCommand<object> _setConnectorTypeCommand;
        private static DelegateCommand<object> _createNodeCommand;
        private static DelegateCommand<object> _createConnectionCommand;
        private static DelegateCommand<object> _addNoteCommand;
        private static DelegateCommand<object> _deleteCommand;
        private static DelegateCommand<object> _selectNeighborsCommand;
        private static DelegateCommand<object> _addToSelectionCommand;
        private static DelegateCommand<string> _alignSelectedCommand;
        private static DelegateCommand _postUiActivationCommand;
        private static DelegateCommand _refactorCustomNodeCommand;
        private static DelegateCommand _showHideConnectorsCommand;
        private static DelegateCommand _toggleFullscreenWatchShowingCommand;
        private static DelegateCommand _toggleCanNavigateBackgroundCommand;
        private static DelegateCommand _goHomeCommand;
        private static DelegateCommand _selectAllCommand;

        #endregion

        public static DelegateCommand SelectAllCommand
        {
            get
            {
                if(_selectAllCommand == null)
                    _selectAllCommand = new DelegateCommand(_vm.SelectAll, _vm.CanSelectAll);
                return _selectAllCommand;
            }
        }

        public static DelegateCommand<object> WriteToLogCmd
        {
            get
            {
                if (writeToLogCmd == null)
                    writeToLogCmd = new DelegateCommand<object>(_vm.WriteToLog, _vm.CanWriteToLog);
                return writeToLogCmd;
            }
        }

        public static DelegateCommand ReportABugCommand
        {
            get
            {
                if(_reportABug == null)
                    _reportABug = new DelegateCommand(_vm.ReportABug, _vm.CanReportABug);
                return _reportABug;
            }
        }

        public  static DelegateCommand GoToWikiCommand
        {
            get
            {
                if (_gotoWikiCommand == null)
                    _gotoWikiCommand = new DelegateCommand(_vm.GoToWiki, _vm.CanGoToWiki);
                return _gotoWikiCommand;
            }
        }

        public static DelegateCommand GoToSourceCodeCommand
        {
            get
            {
                if (_gotoSourceCommand == null)
                    _gotoSourceCommand = new DelegateCommand(_vm.GoToSourceCode, _vm.CanGoToSourceCode);
                return _gotoSourceCommand;
            }
        }

        public static DelegateCommand<object> ExitCommand
        {
            get
            {
                if(_exitCommand == null)
                    _exitCommand = new DelegateCommand<object>(_vm.Exit, _vm.CanExit);
                return _exitCommand;
            }
        }

        public static DelegateCommand CleanupCommand
        {
            get
            {
                if(_cleanupCommand == null)
                    _cleanupCommand = new DelegateCommand(_vm.Cleanup, _vm.CanCleanup);
                return _cleanupCommand;
            }
        }

        public static DelegateCommand ShowSaveImageDialogAndSaveResultCommand
        {
            get
            {
                if(_showSaveImageDialogAndSaveResultCommand == null)
                    _showSaveImageDialogAndSaveResultCommand = new DelegateCommand(_vm.ShowSaveImageDialogAndSaveResult, _vm.CanShowSaveImageDialogAndSaveResult);
                return _showSaveImageDialogAndSaveResultCommand;
            }
        }

        public static DelegateCommand ShowOpenDialogAndOpenResultCommand 
        {
            get
            {
                if(_showOpenDialogueAndOpenResultCommand == null)
                    _showOpenDialogueAndOpenResultCommand = 
                        new DelegateCommand(_vm.ShowOpenDialogAndOpenResult, _vm.CanShowOpenDialogAndOpenResultCommand);
                return _showOpenDialogueAndOpenResultCommand; 
            }
        }

        public static DelegateCommand ShowSaveDialogIfNeededAndSaveResultCommand
        {
            get
            {
                if(_showSaveDialogIfNeededAndSaveResultCommand == null)
                    _showSaveDialogIfNeededAndSaveResultCommand =
                        new DelegateCommand(_vm.ShowSaveDialogIfNeededAndSaveResult, _vm.CanShowSaveDialogIfNeededAndSaveResultCommand);
                 return _showSaveDialogIfNeededAndSaveResultCommand; 
            }
        }

        public static DelegateCommand ShowSaveDialogAndSaveResultCommand
        {
            get
            {
                if(_showSaveDialogAndSaveResultCommand == null)
                    _showSaveDialogAndSaveResultCommand =
                        new DelegateCommand(_vm.ShowSaveDialogAndSaveResult, _vm.CanShowSaveDialogAndSaveResult);
                return _showSaveDialogAndSaveResultCommand; 
            }
        }

        public static DelegateCommand ShowNewFunctionDialogCommand
        {
            get
            {
                if(_showNewFunctionDialogCommand == null)
                    _showNewFunctionDialogCommand = new DelegateCommand(_vm.ShowNewFunctionDialogAndMakeFunction, _vm.CanShowNewFunctionDialogCommand);
                return _showNewFunctionDialogCommand; 
            }
        }

        public static DelegateCommand<object> OpenCommand
        {
            get
            {
                if(_openCommand == null)
                    _openCommand = new DelegateCommand<object>(_vm.Open, _vm.CanOpen);
                return _openCommand;
            }
        }

        public static DelegateCommand SaveCommand
        {
            get
            {
                if(_saveCommand == null)
                    _saveCommand = new DelegateCommand(_vm.Save, _vm.CanSave);
                return _saveCommand;
            }
        }

        public static DelegateCommand<object> SaveAsCommand
        {
            get
            {
                if(_saveAsCommand == null)
                    _saveAsCommand = new DelegateCommand<object>(_vm.SaveAs, _vm.CanSaveAs);
                return _saveAsCommand; 
            }
        }

        public static DelegateCommand ClearCommand
        {
            get
            {
                if(_clearCommand == null)
                    _clearCommand = new DelegateCommand(_vm.Clear, _vm.CanClear);
                return _clearCommand;
            }
        }

        public static DelegateCommand HomeCommand
        {
            get
            {
                if(_homeCommand == null)
                    _homeCommand = new DelegateCommand(_vm.Home, _vm.CanGoHome);
                return _homeCommand;
            }
        }

        public static DelegateCommand LayoutAllCommand
        {
            get
            {
                if (_layoutAllCommand == null)
                    _layoutAllCommand = new DelegateCommand(_vm.LayoutAll, _vm.CanLayoutAll);
                return _layoutAllCommand;
            }
        }

        public static DelegateCommand NewHomeWorkspaceCommand
        {
            get
            {
                if (_newHomeWorkspaceCommand == null)
                    _newHomeWorkspaceCommand =
                        new DelegateCommand(_vm.MakeNewHomeWorkspace, _vm.CanMakeNewHomeWorkspace);

                return _newHomeWorkspaceCommand;
            }
        }

        public static DelegateCommand<object> CopyCommand
        {
            get
            {
                if(_copyCommand == null)
                    _copyCommand = new DelegateCommand<object>(_vm.Copy, _vm.CanCopy);
                return _copyCommand;
            }
        }

        public static DelegateCommand<object> PasteCommand
        {
            get
            {
                if(_pasteCommand == null)
                    _pasteCommand = new DelegateCommand<object>(_vm.Paste, _vm.CanPaste);
                return _pasteCommand;
            }
        }

        public static DelegateCommand ToggleConsoleShowingCommand
        {
            get
            {
                if(_toggleConsoleShowingCommand == null)
                    _toggleConsoleShowingCommand = 
                        new DelegateCommand(_vm.ToggleConsoleShowing, _vm.CanToggleConsoleShowing);
                return _toggleConsoleShowingCommand;
            }
        }

        public static DelegateCommand CancelRunCommand
        {
            get
            {
                if(_cancelRunCommand == null)
                    _cancelRunCommand = new DelegateCommand(_vm.CancelRun, _vm.CanCancelRun);
                return _cancelRunCommand;
            }
        }

        public static DelegateCommand<object> SaveImageCommand
        {
            get
            {
                if(_saveImageCommand == null)
                    _saveImageCommand = new DelegateCommand<object>(_vm.SaveImage, _vm.CanSaveImage);
                return _saveImageCommand;
            }
        }

        public static DelegateCommand ClearLogCommand
        {
            get
            {
                if(_clearLogCommand == null)
                    _clearLogCommand = new DelegateCommand(_vm.ClearLog, _vm.CanClearLog);
                return _clearLogCommand;
            }
        }

        public static DelegateCommand<object> RunExpressionCommand
        {
            get
            {
                if(_runExpressionCommand == null)
                    _runExpressionCommand = 
                        new DelegateCommand<object>(_vm.RunExpression, _vm.CanRunExpression);
                return _runExpressionCommand;
            }
        }

        public static DelegateCommand ShowPackageManagerCommand
        {
            get
            {
                if(_showPackageManagerCommand == null)
                    _showPackageManagerCommand = new DelegateCommand(_vm.ShowPackageManager, _vm.CanShowPackageManager);
                return _showPackageManagerCommand;
            }
        }

        public static DelegateCommand<object> GoToWorkspaceCommand
        {
            get
            {
                if(_goToWorkspaceCommand == null)
                    _goToWorkspaceCommand = new DelegateCommand<object>(_vm.GoToWorkspace, _vm.CanGoToWorkspace);

                return _goToWorkspaceCommand;
            }
        }

        public static DelegateCommand<object> DisplayFunctionCommand
        {
            get
            {
                if(_displayFunctionCommand == null)
                    _displayFunctionCommand = 
                        new DelegateCommand<object>(_vm.DisplayFunction, _vm.CanDisplayFunction);

                return _displayFunctionCommand;
            }
        }

        public static DelegateCommand<object> SetConnectorTypeCommand
        {
            get
            {
                if(_setConnectorTypeCommand == null)
                    _setConnectorTypeCommand = 
                        new DelegateCommand<object>(_vm.SetConnectorType, _vm.CanSetConnectorType);

                return _setConnectorTypeCommand;
            }
        }

        public static DelegateCommand<object> CreateNodeCommand
        {
            get
            {
                if(_createNodeCommand == null)
                    _createNodeCommand = new DelegateCommand<object>(_vm.CreateNode, _vm.CanCreateNode);

                return _createNodeCommand;
            }
        }

        public static DelegateCommand<object> CreateConnectionCommand
        {
            get
            {
                if(_createConnectionCommand == null)
                    _createConnectionCommand = 
                        new DelegateCommand<object>(_vm.CreateConnection, _vm.CanCreateConnection);

                return _createConnectionCommand;
            }
        }

        public static DelegateCommand<object> AddNoteCommand
        {
            get
            {
                if(_addNoteCommand == null)
                    _addNoteCommand = new DelegateCommand<object>(_vm.AddNote, _vm.CanAddNote);

                return _addNoteCommand;
            }
        }

        public static DelegateCommand<object> DeleteCommand
        {
            get
            {
                if(_deleteCommand == null)
                    _deleteCommand = new DelegateCommand<object>(_vm.Delete, _vm.CanDelete);
                return _deleteCommand;
            }
        }

        public static DelegateCommand<object> SelectNeighborsCommand
        {
            get
            {
                if(_selectNeighborsCommand == null)
                    _selectNeighborsCommand = 
                        new DelegateCommand<object>(_vm.SelectNeighbors, _vm.CanSelectNeighbors);

                return _selectNeighborsCommand;
            }
        }

        public static DelegateCommand<object> AddToSelectionCommand
        {
            get
            {
                if(_addToSelectionCommand == null)
                    _addToSelectionCommand = 
                        new DelegateCommand<object>(_vm.AddToSelection, _vm.CanAddToSelection);

                return _addToSelectionCommand;
            }
        }

        public static DelegateCommand<string> AlignSelectedCommand
        {
            get
            {
                if(_alignSelectedCommand == null)
                    _alignSelectedCommand = new DelegateCommand<string>(_vm.AlignSelected, _vm.CanAlignSelected);;
                return _alignSelectedCommand;
            }
        }

        public static DelegateCommand PostUiActivationCommand
        {
            get
            {
                if(_postUiActivationCommand == null)
                    _postUiActivationCommand = new DelegateCommand(_vm.PostUIActivation, _vm.CanDoPostUIActivation);

                return _postUiActivationCommand;
            }
        }

        public static DelegateCommand RefactorCustomNodeCommand
        {
            get
            {
                if(_refactorCustomNodeCommand == null)
                    _refactorCustomNodeCommand = 
                        new DelegateCommand(_vm.RefactorCustomNode, _vm.CanRefactorCustomNode);

                return _refactorCustomNodeCommand;
            }
        }

        public static DelegateCommand ShowHideConnectorsCommand
        {
            get
            {
                if(_showHideConnectorsCommand == null)
                    _showHideConnectorsCommand = 
                        new DelegateCommand(_vm.ShowConnectors, _vm.CanShowConnectors);

                return _showHideConnectorsCommand;
            }
        }

        public static DelegateCommand ToggleFullscreenWatchShowingCommand
        {
            get
            {
                if(_toggleFullscreenWatchShowingCommand == null)
                    _toggleFullscreenWatchShowingCommand = 
                        new DelegateCommand(_vm.ToggleFullscreenWatchShowing, _vm.CanToggleFullscreenWatchShowing);

                return _toggleFullscreenWatchShowingCommand;
            }
        }

        public static DelegateCommand ToggleCanNavigateBackgroundCommand
        {
            get
            {
                if(_toggleCanNavigateBackgroundCommand == null)
                    _toggleCanNavigateBackgroundCommand = 
                        new DelegateCommand(_vm.ToggleCanNavigateBackground, _vm.CanToggleCanNavigateBackground);

                return _toggleCanNavigateBackgroundCommand;
            }
        }

        public static DelegateCommand GoHomeCommand
        {
            get
            {
                if(_goHomeCommand == null)
                    _goHomeCommand = new DelegateCommand(_vm.GoHomeView, _vm.CanGoHomeView);

                return _goHomeCommand;
            }
        }

        #region CommandQueue

        /// <summary>
        /// Add a command to the CommandQueue and run ProcessCommandQueue(), providing null as the 
        /// command arguments
        /// </summary>
        /// <param name="command">The command to run</param>
        public static void RunCommand(DelegateCommand<object> command)
        {
            RunCommand(command, null);
        }

        /// <summary>
        /// Add a command to the CommandQueue and run ProcessCommandQueue(), providing the given
        /// arguments to the command
        /// </summary>
        /// <param name="command">The command to run</param>
        /// <param name="args">Arguments to give to the command</param>
        public static void RunCommand(DelegateCommand<object> command, object args)
        {
            var commandAndParams = Tuple.Create<object, object>(command, args);
            CommandQueue.Enqueue(commandAndParams);
            ProcessCommandQueue();
        }

        //private void Hooks_DispatcherInactive(object sender, EventArgs e)
        //{
        //    ProcessCommandQueue();
        //}

        /// <summary>
        ///     Run all of the commands in the CommandQueue
        /// </summary>
        public static void ProcessCommandQueue()
        {
            while (commandQueue.Count > 0)
            {
                var cmdData = commandQueue.Dequeue();
                var cmd = cmdData.Item1 as DelegateCommand<object>;
                if (cmd != null)
                {
                    if (cmd.CanExecute(cmdData.Item2))
                    {
                        cmd.Execute(cmdData.Item2);
                    }
                }
            }
            commandQueue.Clear();

            if (dynSettings.Controller.UIDispatcher != null)
            {
                DynamoLogger.Instance.Log(string.Format("dynSettings.Bench Thread : {0}",
                                                       dynSettings.Controller.UIDispatcher.Thread.ManagedThreadId.ToString(CultureInfo.InvariantCulture)));
            }
        }

        #endregion
    }

}
