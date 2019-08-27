using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamo.Graph.Nodes.CustomNodes;
using Microsoft.Practices.Prism.Commands;
using DelegateCommand = Dynamo.UI.Commands.DelegateCommand;

namespace Dynamo.Wpf.ViewModels
{
    public interface IDynamoViewModelDelegateCommands
    {
        DelegateCommand OpenIfSavedCommand { get; set; }
        DelegateCommand OpenCommand { get; set; }
        DelegateCommand ShowOpenDialogAndOpenResultCommand { get; set; }
        DelegateCommand WriteToLogCmd { get; set; }
        DelegateCommand PostUiActivationCommand { get; set; }
        DelegateCommand AddNoteCommand { get; set; }
        DelegateCommand AddAnnotationCommand { get; set; }
        DelegateCommand UngroupAnnotationCommand { get; set; }
        DelegateCommand UngroupModelCommand { get; set; }
        DelegateCommand AddModelsToGroupModelCommand { get; set; }
        DelegateCommand UndoCommand { get; set; }
        DelegateCommand RedoCommand { get; set; }
        DelegateCommand CopyCommand { get; set; }
        DelegateCommand PasteCommand { get; set; }
        DelegateCommand AddToSelectionCommand { get; set; }
        DelegateCommand ShowNewFunctionDialogCommand { get; set; }
        DelegateCommand SaveRecordedCommand { get; set; }
        DelegateCommand InsertPausePlaybackCommand { get; set; }
        DelegateCommand GraphAutoLayoutCommand { get; set; }
        DelegateCommand GoHomeCommand { get; set; }
        DelegateCommand ShowPackageManagerSearchCommand { get; set; }
        DelegateCommand ShowInstalledPackagesCommand { get; set; }
        DelegateCommand ManagePackagePathsCommand { get; set; }
        DelegateCommand HomeCommand { get; set; }
        DelegateCommand ExitCommand { get; set; }
        DelegateCommand ShowSaveDialogIfNeededAndSaveResultCommand { get; set; }
        DelegateCommand ShowSaveDialogAndSaveResultCommand { get; set; }
        DelegateCommand SaveCommand { get; set; }
        DelegateCommand SaveAsCommand { get; set; }
        DelegateCommand NewHomeWorkspaceCommand { get; set; }
        DelegateCommand CloseHomeWorkspaceCommand { get; set; }
        DelegateCommand GoToWorkspaceCommand { get; set; }
        DelegateCommand DeleteCommand { get; set; }
        DelegateCommand AlignSelectedCommand { get; set; }
        DelegateCommand PostUIActivationCommand { get; set; }
        DelegateCommand ToggleFullscreenWatchShowingCommand { get; set; }
        DelegateCommand ToggleBackgroundGridVisibilityCommand { get; set; }
        DelegateCommand SelectAllCommand { get; set; }
        DelegateCommand SaveImageCommand { get; set; }
        DelegateCommand ShowSaveImageDialogAndSaveResultCommand { get; set; }
        DelegateCommand ToggleConsoleShowingCommand { get; set; }
        DelegateCommand ShowPackageManagerCommand { get; set; }
        DelegateCommand ForceRunExpressionCommand { get; set; }
        DelegateCommand MutateTestDelegateCommand { get; set; }
        DelegateCommand DisplayFunctionCommand { get; set; }
        DelegateCommand SetConnectorTypeCommand { get; set; }
        DelegateCommand ReportABugCommand { get; set; }
        DelegateCommand GoToWikiCommand { get; set; }
        DelegateCommand GoToDictionaryCommand { get; set; }
        DelegateCommand GoToSourceCodeCommand { get; set; }
        DelegateCommand DisplayStartPageCommand { get; set; }
        DelegateCommand ChangeScaleFactorCommand { get; set; }
        DelegateCommand ShowHideConnectorsCommand { get; set; }
        DelegateCommand SelectNeighborsCommand { get; set; }
        DelegateCommand ClearLogCommand { get; set; }
        DelegateCommand SubmitCommand { get; set; }
        DelegateCommand PublishNewPackageCommand { get; set; }
        DelegateCommand PublishCurrentWorkspaceCommand { get; set; }
        DelegateCommand PublishSelectedNodesCommand { get; set; }
        DelegateCommand<Function> PublishCustomNodeCommand { get; set; }
        DelegateCommand PanCommand { get; set; }
        DelegateCommand ZoomInCommand { get; set; }
        DelegateCommand ZoomOutCommand { get; set; }
        DelegateCommand FitViewCommand { get; set; }
        DelegateCommand EscapeCommand { get; set; }
        DelegateCommand ExportToSTLCommand { get; set; }
        DelegateCommand ImportLibraryCommand { get; set; }
        DelegateCommand ShowAboutWindowCommand { get; set; }
        DelegateCommand SetNumberFormatCommand { get; set; }
        DelegateCommand OpenRecentCommand { get; set; }
        DelegateCommand CheckForLatestRenderCommand { get; set; }
        DelegateCommand DumpLibraryToXmlCommand { get; set; }
        DelegateCommand ShowGalleryCommand { get; set; }
        DelegateCommand CloseGalleryCommand { get; set; }
        DelegateCommand ShowNewPresetsDialogCommand { get; set; }
        DelegateCommand NodeFromSelectionCommand { get; set; }
    }
}
