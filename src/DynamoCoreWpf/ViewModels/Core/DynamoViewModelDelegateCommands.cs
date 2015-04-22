using Dynamo.Nodes;
using Dynamo.Wpf.ViewModels;
using Microsoft.Practices.Prism.Commands;
using DelegateCommand = Dynamo.UI.Commands.DelegateCommand;

namespace Dynamo.ViewModels
{
    partial class DynamoViewModel
    {
        private void InitializeDelegateCommands()
        {
            OpenCommand = new DelegateCommand(Open, CanOpen);
            OpenRecentCommand = new DelegateCommand(OpenRecent, CanOpenRecent);
            SaveCommand = new DelegateCommand(Save, CanSave);
            SaveAsCommand = new DelegateCommand(SaveAs, CanSaveAs);
            ShowOpenDialogAndOpenResultCommand = new DelegateCommand(ShowOpenDialogAndOpenResult, CanShowOpenDialogAndOpenResultCommand);
            ShowSaveDialogAndSaveResultCommand = new DelegateCommand(ShowSaveDialogAndSaveResult, CanShowSaveDialogAndSaveResult);
            ShowSaveDialogIfNeededAndSaveResultCommand = new DelegateCommand(ShowSaveDialogIfNeededAndSaveResult, CanShowSaveDialogIfNeededAndSaveResultCommand);
            SaveImageCommand = new DelegateCommand(SaveImage, CanSaveImage);
            ShowSaveImageDialogAndSaveResultCommand = new DelegateCommand(ShowSaveImageDialogAndSaveResult, CanShowSaveImageDialogAndSaveResult);
            WriteToLogCmd = new DelegateCommand(o => model.Logger.Log(o.ToString()), CanWriteToLog);
            PostUiActivationCommand = new DelegateCommand(model.PostUIActivation);
            AddNoteCommand = new DelegateCommand(AddNote, CanAddNote);
            AddAnnotationCommand = new DelegateCommand(AddAnnotation,CanAddAnnotation);
            UngroupAnnotationCommand = new DelegateCommand(UngroupAnnotation,CanUngroupAnnotation);
            UngroupModelCommand = new DelegateCommand(UngroupModel,CanUngroupModel);
            AddToSelectionCommand = new DelegateCommand(model.AddToSelection, CanAddToSelection);
            ShowNewFunctionDialogCommand = new DelegateCommand(ShowNewFunctionDialogAndMakeFunction, CanShowNewFunctionDialogCommand);
            SaveRecordedCommand = new DelegateCommand(SaveRecordedCommands, CanSaveRecordedCommands);
            InsertPausePlaybackCommand = new DelegateCommand(ExecInsertPausePlaybackCommand, CanInsertPausePlaybackCommand);
            GraphAutoLayoutCommand = new DelegateCommand(DoGraphAutoLayout, CanDoGraphAutoLayout);
            GoHomeCommand = new DelegateCommand(GoHomeView, CanGoHomeView);
            SelectAllCommand = new DelegateCommand(SelectAll, CanSelectAll);
            HomeCommand = new DelegateCommand(GoHome, CanGoHome);
            NewHomeWorkspaceCommand = new DelegateCommand(MakeNewHomeWorkspace, CanMakeNewHomeWorkspace);
            CloseHomeWorkspaceCommand = new DelegateCommand(CloseHomeWorkspace, CanCloseHomeWorkspace);
            GoToWorkspaceCommand = new DelegateCommand(GoToWorkspace, CanGoToWorkspace);
            DeleteCommand = new DelegateCommand(Delete, CanDelete);
            ExitCommand = new DelegateCommand(Exit, CanExit);
            ToggleFullscreenWatchShowingCommand = new DelegateCommand(ToggleFullscreenWatchShowing, CanToggleFullscreenWatchShowing);
            ToggleCanNavigateBackgroundCommand = new DelegateCommand(ToggleCanNavigateBackground, CanToggleCanNavigateBackground);
            AlignSelectedCommand = new DelegateCommand(AlignSelected, CanAlignSelected); ;
            UndoCommand = new DelegateCommand(Undo, CanUndo);
            RedoCommand = new DelegateCommand(Redo, CanRedo);
            CopyCommand = new DelegateCommand(_ => model.Copy(), CanCopy);
            PasteCommand = new DelegateCommand(_ => model.Paste(), CanPaste);
            ToggleConsoleShowingCommand = new DelegateCommand(ToggleConsoleShowing, CanToggleConsoleShowing);
            ForceRunExpressionCommand = new DelegateCommand(ForceRunExprCmd, RunSettingsViewModel.CanRunExpression);
            MutateTestDelegateCommand = new DelegateCommand(MutateTestCmd, RunSettingsViewModel.CanRunExpression);
            DisplayFunctionCommand = new DelegateCommand(DisplayFunction, CanDisplayFunction);
            SetConnectorTypeCommand = new DelegateCommand(SetConnectorType, CanSetConnectorType);
            ReportABugCommand = new DelegateCommand(ReportABug, CanReportABug);
            GoToWikiCommand = new DelegateCommand(GoToWiki, CanGoToWiki);
            GoToSourceCodeCommand = new DelegateCommand(GoToSourceCode, CanGoToSourceCode);
            DisplayStartPageCommand = new DelegateCommand(DisplayStartPage, CanDisplayStartPage);
            ShowPackageManagerSearchCommand = new DelegateCommand(ShowPackageManagerSearch, CanShowPackageManagerSearch);
            PublishNewPackageCommand = new DelegateCommand(PackageManagerClientViewModel.PublishNewPackage, PackageManagerClientViewModel.CanPublishNewPackage);
            ShowInstalledPackagesCommand = new DelegateCommand(ShowInstalledPackages, CanShowInstalledPackages);
            PublishCurrentWorkspaceCommand = new DelegateCommand(PackageManagerClientViewModel.PublishCurrentWorkspace, PackageManagerClientViewModel.CanPublishCurrentWorkspace);
            PublishSelectedNodesCommand = new DelegateCommand(PackageManagerClientViewModel.PublishSelectedNodes, PackageManagerClientViewModel.CanPublishSelectedNodes);
            PublishCustomNodeCommand = new DelegateCommand<Function>(PackageManagerClientViewModel.PublishCustomNode, PackageManagerClientViewModel.CanPublishCustomNode);
            ShowHideConnectorsCommand = new DelegateCommand(ShowConnectors, CanShowConnectors);
            SelectNeighborsCommand = new DelegateCommand(SelectNeighbors, CanSelectNeighbors);
            ClearLogCommand = new DelegateCommand(ClearLog, CanClearLog);
            PanCommand = new DelegateCommand(Pan, CanPan);
            ZoomInCommand = new DelegateCommand(ZoomIn, CanZoomIn);
            ZoomOutCommand = new DelegateCommand(ZoomOut, CanZoomOut);
            FitViewCommand = new DelegateCommand(FitView, CanFitView);
            TogglePanCommand = new DelegateCommand(TogglePan, CanTogglePan);
            ToggleOrbitCommand = new DelegateCommand(ToggleOrbit, CanToggleOrbit);
            EscapeCommand = new DelegateCommand(Escape, CanEscape);
            ExportToSTLCommand = new DelegateCommand(ExportToSTL, CanExportToSTL);
            ImportLibraryCommand = new DelegateCommand(ImportLibrary, CanImportLibrary);
            ShowAboutWindowCommand = new DelegateCommand(ShowAboutWindow, CanShowAboutWindow);
            SetNumberFormatCommand = new DelegateCommand(SetNumberFormat, CanSetNumberFormat);
            GetBranchVisualizationCommand = new DelegateCommand(GetBranchVisualization, CanGetBranchVisualization);
            DumpLibraryToXmlCommand = new DelegateCommand(model.DumpLibraryToXml, model.CanDumpLibraryToXml);
            ShowIncanvasSearchCommand = new DelegateCommand(ShowIncanvasSearch, CanShowIncanvasSearch);
        }

        public DelegateCommand OpenCommand { get; set; }
        public DelegateCommand ShowOpenDialogAndOpenResultCommand { get; set; }
        public DelegateCommand WriteToLogCmd { get; set; }
        public DelegateCommand PostUiActivationCommand { get; set; }
        public DelegateCommand AddNoteCommand { get; set; }
        public DelegateCommand AddAnnotationCommand { get; set; }
        public DelegateCommand UngroupAnnotationCommand { get; set; }
        public DelegateCommand UngroupModelCommand { get; set; }
        public DelegateCommand UndoCommand { get; set; }
        public DelegateCommand RedoCommand { get; set; }
        public DelegateCommand CopyCommand { get; set; }
        public DelegateCommand PasteCommand { get; set; }
        public DelegateCommand AddToSelectionCommand { get; set; }
        public DelegateCommand ShowNewFunctionDialogCommand { get; set; }
        public DelegateCommand SaveRecordedCommand { get; set; }
        public DelegateCommand InsertPausePlaybackCommand { get; set; }
        public DelegateCommand GraphAutoLayoutCommand { get; set; }
        public DelegateCommand GoHomeCommand { get; set; }
        public DelegateCommand ShowPackageManagerSearchCommand { get; set; }
        public DelegateCommand ShowInstalledPackagesCommand { get; set; }
        public DelegateCommand HomeCommand { get; set; }
        public DelegateCommand ExitCommand { get; set; }
        public DelegateCommand ShowSaveDialogIfNeededAndSaveResultCommand { get; set; }
        public DelegateCommand ShowSaveDialogAndSaveResultCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand SaveAsCommand { get; set; }
        public DelegateCommand NewHomeWorkspaceCommand { get; set; }
        public DelegateCommand CloseHomeWorkspaceCommand { get; set; }
        public DelegateCommand GoToWorkspaceCommand { get; set; }
        public DelegateCommand DeleteCommand { get; set; }
        public DelegateCommand AlignSelectedCommand { get; set; }
        public DelegateCommand PostUIActivationCommand { get; set; }
        public DelegateCommand ToggleFullscreenWatchShowingCommand { get; set; }
        public DelegateCommand ToggleCanNavigateBackgroundCommand { get; set; }
        public DelegateCommand SelectAllCommand { get; set; }
        public DelegateCommand SaveImageCommand { get; set; }
        public DelegateCommand ShowSaveImageDialogAndSaveResultCommand { get; set; }
        public DelegateCommand ToggleConsoleShowingCommand { get; set; }
        public DelegateCommand ShowPackageManagerCommand { get; set; }
        public DelegateCommand ForceRunExpressionCommand { get; set; }
        public DelegateCommand MutateTestDelegateCommand { get; set; }
        public DelegateCommand DisplayFunctionCommand { get; set; }
        public DelegateCommand SetConnectorTypeCommand { get; set; }
        public DelegateCommand ReportABugCommand { get; set; }
        public DelegateCommand GoToWikiCommand { get; set; }
        public DelegateCommand GoToSourceCodeCommand { get; set; }
        public DelegateCommand DisplayStartPageCommand { get; set; }
        public DelegateCommand ShowHideConnectorsCommand { get; set; }
        public DelegateCommand SelectNeighborsCommand { get; set; }
        public DelegateCommand ClearLogCommand { get; set; }
        public DelegateCommand SubmitCommand { get; set; }
        public DelegateCommand PublishNewPackageCommand { get; set; }
        public DelegateCommand PublishCurrentWorkspaceCommand { get; set; }
        public DelegateCommand PublishSelectedNodesCommand { get; set; }
        public DelegateCommand<Function> PublishCustomNodeCommand { get; set; }
        public DelegateCommand PanCommand { get; set; }
        public DelegateCommand ZoomInCommand { get; set; }
        public DelegateCommand ZoomOutCommand { get; set; }
        public DelegateCommand FitViewCommand { get; set; }
        public DelegateCommand TogglePanCommand { get; set; }
        public DelegateCommand ToggleOrbitCommand { get; set; }
        public DelegateCommand EscapeCommand { get; set; }
        public DelegateCommand ExportToSTLCommand { get; set; }
        public DelegateCommand ImportLibraryCommand { get; set; }
        public DelegateCommand ShowAboutWindowCommand { get; set; }
        public DelegateCommand SetNumberFormatCommand { get; set; }
        public DelegateCommand OpenRecentCommand { get; set; }
        public DelegateCommand GetBranchVisualizationCommand { get; set; }
        public DelegateCommand CheckForLatestRenderCommand { get; set; }
        public DelegateCommand DumpLibraryToXmlCommand { get; set; }
        public DelegateCommand ShowIncanvasSearchCommand { get; set; }
    }
}
