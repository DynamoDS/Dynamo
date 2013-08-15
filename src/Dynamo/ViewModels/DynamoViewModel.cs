﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Globalization;

using Dynamo.Commands;
using Dynamo.Connectors;
using Dynamo.Nodes;
using Dynamo.PackageManager;
using Dynamo.Selection;
using Dynamo.Utilities;

using Microsoft.Practices.Prism.Commands;

using NUnit.Core;
using NUnit.Framework;
using Dynamo.PackageManager.UI;

namespace Dynamo.Controls
{

    public class DynamoViewModel:dynViewModelBase
    {

        #region properties

        public event EventHandler RequestLayoutUpdate;
        public event EventHandler WorkspaceChanged;

        public virtual void OnRequestLayoutUpdate(object sender, EventArgs e)
        {
            if (RequestLayoutUpdate != null)
                RequestLayoutUpdate(this, e);
        }
        public virtual void OnWorkspaceChanged(object sender, EventArgs e)
        {
            if (WorkspaceChanged != null)
            {
                WorkspaceChanged(this, e);
            }
        }
        
        private DynamoModel _model;
        private string logText;
        private ConnectorType connectorType;
        private Point transformOrigin;
        private bool consoleShowing;
        private DynamoController controller;
        public StringWriter sw;
        private bool runEnabled = true;
        protected bool canRunDynamically = true;
        protected bool debug = false;
        protected bool dynamicRun = false;
        private string UnlockLoadPath;
        private bool uiLocked = true;
        private string editName = "";
        private bool isShowingConnectors = true;
        private bool fullscreenWatchShowing = false;
        private bool canNavigateBackground = false;

        /// <summary>
        /// An observable collection of workspace view models which tracks the model
        /// </summary>
        private ObservableCollection<dynWorkspaceViewModel> _workspaces = new ObservableCollection<dynWorkspaceViewModel>();
        public DelegateCommand ReportABugCommand { get; set; }
        public DelegateCommand GoToWikiCommand { get; set; }
        public DelegateCommand GoToSourceCodeCommand { get; set; }
        public DelegateCommand<object> ExitCommand { get; set; }
        public DelegateCommand CleanupCommand { get; set; }
        public DelegateCommand SelectAllCommand { get; set; }
        public DelegateCommand ShowSaveImageDialogAndSaveResultCommand { get; set; }
        public DelegateCommand ShowOpenDialogAndOpenResultCommand { get; set; }
        public DelegateCommand ShowSaveDialogIfNeededAndSaveResultCommand { get; set; }
        public DelegateCommand ShowSaveDialogAndSaveResultCommand { get; set; }
        public DelegateCommand ShowNewFunctionDialogCommand { get; set; }
        public DelegateCommand<object> OpenCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand<object> SaveAsCommand { get; set; }
        public DelegateCommand ClearCommand { get; set; }
        public DelegateCommand ShowPackageManagerSearchCommand { get; set; }
        public DelegateCommand ShowInstalledPackagesCommand { get; set; }
        public DelegateCommand HomeCommand { get; set; }
        public DelegateCommand LayoutAllCommand { get; set; }
        public DelegateCommand NewHomeWorkspaceCommand { get; set; }
        public DelegateCommand<object> CopyCommand { get; set; }
        public DelegateCommand<object> PasteCommand { get; set; }
        public DelegateCommand ToggleConsoleShowingCommand { get; set; }
        public DelegateCommand CancelRunCommand { get; set; }
        public DelegateCommand<object> SaveImageCommand { get; set; }
        public DelegateCommand ClearLogCommand { get; set; }
        public DelegateCommand<object> RunExpressionCommand { get; set; }
        public DelegateCommand ShowPackageManagerCommand { get; set; }
        public DelegateCommand<object> GoToWorkspaceCommand { get; set; }
        public DelegateCommand<object> DisplayFunctionCommand { get; set; }
        public DelegateCommand<object> SetConnectorTypeCommand { get; set; }
        public DelegateCommand<object> CreateNodeCommand { get; set; }
        public DelegateCommand<object> CreateConnectionCommand { get; set; }
        public DelegateCommand<object> AddNoteCommand { get; set; }
        public DelegateCommand<object> DeleteCommand { get; set; }
        public DelegateCommand<object> SelectNeighborsCommand { get; set; }
        public DelegateCommand<object> AddToSelectionCommand { get; set; }
        public DelegateCommand<string> AlignSelectedCommand { get; set; }
        public DelegateCommand PostUIActivationCommand { get; set; }
        public DelegateCommand ShowHideConnectorsCommand { get; set; }
        public DelegateCommand ToggleFullscreenWatchShowingCommand { get; set; }
        public DelegateCommand ToggleCanNavigateBackgroundCommand { get; set; }
        public DelegateCommand GoHomeCommand { get; set; }

        public ObservableCollection<dynWorkspaceViewModel> Workspaces
        {
            get { return _workspaces; }
            set
            {
                _workspaces = value;
                RaisePropertyChanged("Workspaces");
            }
        }

        public DynamoModel Model
        {
            get { return _model; }
        }

        public string LogText
        {
            get { return logText; }
            set
            {
                logText = value;
                RaisePropertyChanged("LogText");
            }
        }

        public ConnectorType ConnectorType
        {
            get { return connectorType; }
            set
            {
                connectorType = value;
                RaisePropertyChanged("ConnectorType");
            }
        }

        public Point TransformOrigin
        {
            get { return transformOrigin; }
            set
            {
                transformOrigin = value;
                RaisePropertyChanged("TransformOrigin");
            }
        }

        public bool ConsoleShowing
        {
            get { return consoleShowing; }
            set
            {
                consoleShowing = value;
                RaisePropertyChanged("ConsoleShowing");
            }
        }

        public DynamoController Controller
        {
            get { return controller; }
            set
            {
                controller = value;
                RaisePropertyChanged("ViewModel");
            }
        }

        public bool RunEnabled
        {
            get { return runEnabled; }
            set
            {
                runEnabled = value;
                RaisePropertyChanged("RunEnabled");
            }
        }

        public virtual bool CanRunDynamically
        {
            get
            {
                //we don't want to be able to run
                //dynamically if we're in debug mode
                return !debug;
            }
            set
            {
                canRunDynamically = value;
                RaisePropertyChanged("CanRunDynamically");
            }
        }

        public virtual bool DynamicRunEnabled
        {
            get
            {
                return dynamicRun; //selecting debug now toggles this on/off
            }
            set
            {
                dynamicRun = value;
                RaisePropertyChanged("DynamicRunEnabled");
            }
        }

        public bool ViewingHomespace
        {
            get { return _model.CurrentSpace == _model.HomeSpace; }
        }

        public bool IsAbleToGoHome { get; set; }

        public dynWorkspaceModel CurrentSpace
        {
            get { return _model.CurrentSpace; }
        }

        /// <summary>
        /// The index in the collection of workspaces of the current workspace.
        /// This property is bound to the SelectedIndex property in the workspaces tab control
        /// </summary>
        public int CurrentWorkspaceIndex
        {
            get
            {
                var index = _model.Workspaces.IndexOf(_model.CurrentSpace);
                return index;
            }
            set
            {
                _model.CurrentSpace = _model.Workspaces[value];
            }
        }

        /// <summary>
        /// Get the workspace view model whose workspace model is the model's current workspace
        /// </summary>
        public dynWorkspaceViewModel CurrentSpaceViewModel
        {
            get
            {
                return Workspaces.First(x => x.Model == _model.CurrentSpace);
            }
        }

        public string EditName
        {
            get { return editName; }
            set 
            { 
                editName = value;
                RaisePropertyChanged("EditName");
            }

        }

        public Visibility DebugMenuVisibility
        {
            get 
            {
                bool showDebugMenu = false;
#if DEBUG
                showDebugMenu = true;
#endif
                if (showDebugMenu)
                    return Visibility.Visible;

                return Visibility.Hidden;
            }
        }

        public bool IsUILocked
        {
            get { return uiLocked; }
            set
            {
                uiLocked = value;
                RaisePropertyChanged("IsUILocked");
            }
        }
        
        public bool IsShowingConnectors
        {
            get { return isShowingConnectors; }
            set
            {
                isShowingConnectors = value;
                RaisePropertyChanged("IsShowingConnectors");
            }
        }

        public bool FullscreenWatchShowing
        {
            get { return fullscreenWatchShowing; }
            set
            {
                fullscreenWatchShowing = value;
                RaisePropertyChanged("FullscreenWatchShowing");

                // NOTE: I couldn't get the binding to work in the XAML so
                //       this is a temporary hack
                foreach (dynWorkspaceViewModel workspace in dynSettings.Controller.DynamoViewModel.Workspaces)
                {
                    workspace.FullscreenChanged();
                }

                if (!fullscreenWatchShowing && canNavigateBackground)
                    CanNavigateBackground = false;
            }
        }

        public bool CanNavigateBackground
        {
            get { return canNavigateBackground; }
            set
            {
                canNavigateBackground = value;
                RaisePropertyChanged("CanNavigateBackground");

                int workspace_index = CurrentWorkspaceIndex;

                dynWorkspaceViewModel view_model = Workspaces[workspace_index];

                view_model.WatchEscapeIsDown = value;
            }
        }
        
        #endregion

        public DynamoViewModel(DynamoController controller)
        {
            //MVVM: Instantiate the model
            _model = new DynamoModel();
            _model.Workspaces.CollectionChanged += Workspaces_CollectionChanged;

            _model.PropertyChanged += _model_PropertyChanged;

            dynSettings.Controller.DynamoModel = _model;

            _model.AddHomeWorkspace();
            _model.CurrentSpace = _model.HomeSpace;

            Controller = controller;
            sw = new StringWriter();
            ConnectorType = ConnectorType.BEZIER;

            DynamoSelection.Instance.Selection.CollectionChanged += new NotifyCollectionChangedEventHandler(Selection_CollectionChanged);
            #region Initialize Commands
            GoToWikiCommand = new DelegateCommand(GoToWiki, CanGoToWiki);
            ReportABugCommand = new DelegateCommand(ReportABug, CanReportABug);
            GoToSourceCodeCommand = new DelegateCommand(GoToSourceCode,  CanGoToSourceCode);
            CleanupCommand = new DelegateCommand(Cleanup, CanCleanup);
            ExitCommand = new DelegateCommand<object>(Exit, CanExit);
            NewHomeWorkspaceCommand = new DelegateCommand(MakeNewHomeWorkspace, CanMakeNewHomeWorkspace);
            ShowSaveImageDialogAndSaveResultCommand = new DelegateCommand(ShowSaveImageDialogAndSaveResult, CanShowSaveImageDialogAndSaveResult);
            ShowOpenDialogAndOpenResultCommand = new DelegateCommand(ShowOpenDialogAndOpenResult, CanShowOpenDialogAndOpenResultCommand);
            ShowSaveDialogIfNeededAndSaveResultCommand = new DelegateCommand(ShowSaveDialogIfNeededAndSaveResult, CanShowSaveDialogIfNeededAndSaveResultCommand);
            ShowSaveDialogAndSaveResultCommand = new DelegateCommand(ShowSaveDialogAndSaveResult, CanShowSaveDialogAndSaveResultCommand);
            ShowPackageManagerSearchCommand = new DelegateCommand(ShowPackageManagerSearch, CanShowPackageManagerSearch);
            ShowInstalledPackagesCommand = new DelegateCommand(ShowInstalledPackages, CanShowInstalledPackages);
            ShowNewFunctionDialogCommand = new DelegateCommand(ShowNewFunctionDialogAndMakeFunction, CanShowNewFunctionDialogCommand);
            SaveCommand = new DelegateCommand(Save, CanSave);
            SelectAllCommand = new DelegateCommand(SelectAll, CanSelectAll);
            OpenCommand = new DelegateCommand<object>(Open, CanOpen);
            AlignSelectedCommand = new DelegateCommand<string>(AlignSelected, CanAlignSelected);
            SaveAsCommand = new DelegateCommand<object>(SaveAs, CanSaveAs);
            ClearCommand = new DelegateCommand(Clear, CanClear);
            HomeCommand = new DelegateCommand(Home, CanGoHome);
            LayoutAllCommand = new DelegateCommand(LayoutAll, CanLayoutAll);
            CopyCommand = new DelegateCommand<object>(Copy, CanCopy);
            PasteCommand = new DelegateCommand<object>(Paste, CanPaste);
            ToggleConsoleShowingCommand = new DelegateCommand(ToggleConsoleShowing, CanToggleConsoleShowing);
            CancelRunCommand = new DelegateCommand(CancelRun, CanCancelRun);
            SaveImageCommand = new DelegateCommand<object>(SaveImage, CanSaveImage);
            ClearLogCommand = new DelegateCommand(ClearLog, CanClearLog);
            RunExpressionCommand = new DelegateCommand<object>(RunExpression,CanRunExpression);
            ShowPackageManagerCommand = new DelegateCommand(ShowPackageManager,CanShowPackageManager);
            GoToWorkspaceCommand = new DelegateCommand<object>(GoToWorkspace, CanGoToWorkspace);
            DisplayFunctionCommand = new DelegateCommand<object>(DisplayFunction, CanDisplayFunction);
            SetConnectorTypeCommand = new DelegateCommand<object>(SetConnectorType, CanSetConnectorType);
            CreateNodeCommand = new DelegateCommand<object>(CreateNode, CanCreateNode);
            CreateConnectionCommand = new DelegateCommand<object>(CreateConnection, CanCreateConnection);
            AddNoteCommand = new DelegateCommand<object>(AddNote, CanAddNote);
            DeleteCommand = new DelegateCommand<object>(Delete, CanDelete);
            SelectNeighborsCommand = new DelegateCommand<object>(SelectNeighbors, CanSelectNeighbors);
            AddToSelectionCommand = new DelegateCommand<object>(AddToSelection, CanAddToSelection);
            PostUIActivationCommand = new DelegateCommand(PostUIActivation, CanDoPostUIActivation);
            ShowHideConnectorsCommand = new DelegateCommand(ShowConnectors, CanShowConnectors);
            ToggleFullscreenWatchShowingCommand = new DelegateCommand(ToggleFullscreenWatchShowing, CanToggleFullscreenWatchShowing);
            ToggleCanNavigateBackgroundCommand = new DelegateCommand(ToggleCanNavigateBackground, CanToggleCanNavigateBackground);
            GoHomeCommand = new DelegateCommand(GoHomeView, CanGoHomeView);

            #endregion
        }

        void Selection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CopyCommand.RaiseCanExecuteChanged();
            PasteCommand.RaiseCanExecuteChanged();
        }

        void _model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentSpace")
            {
                IsAbleToGoHome = _model.CurrentSpace != _model.HomeSpace;
                RaisePropertyChanged("IsAbleToGoHome");
                RaisePropertyChanged("CurrentSpace");
                RaisePropertyChanged("BackgroundColor");
                RaisePropertyChanged("CurrentWorkspaceIndex");
                RaisePropertyChanged("ViewingHomespace");
            } 
        }

        /// <summary>
        /// Responds to change in the model's workspaces collection, creating or deleting workspace model views.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Workspaces_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems)
                        _workspaces.Add(new dynWorkspaceViewModel(item as dynWorkspaceModel, this));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                        _workspaces.Remove(_workspaces.ToList().First(x => x.Model == item));
                    break;
            }
        }

        private void AlignSelected(string param)
        {
            this.CurrentSpaceViewModel.AlignSelectedCommand.Execute(param);
        }

        private bool CanAlignSelected(string param)
        {
            return true;
        }

        private void SelectAll()
        {
            this.CurrentSpaceViewModel.SelectAllCommand.Execute();
        }

        private bool CanSelectAll()
        {
            return this.CurrentSpaceViewModel.SelectAllCommand.CanExecute();
        }

        private bool CanSave()
        {
            return true;
        }

        private void ReportABug()
        {
            Process.Start("https://github.com/ikeough/Dynamo/issues?state=open");
        }

        private bool CanReportABug()
        {
            return true;
        }

        private void MakeNewHomeWorkspace()
        {
            // if the workspace is unsaved, prompt to save
            // otherwise overwrite the home workspace with new workspace
            if (!this.Model.HomeSpace.HasUnsavedChanges || AskUserToSaveWorkspaceOrCancel(this.Model.HomeSpace))
            {
                this.Model.CurrentSpace = this.Model.HomeSpace;
                ClearCommand.Execute();
            }
        }

        private bool CanMakeNewHomeWorkspace()
        {
            return true;
        }

        private void Open(object parameters)
        {
            string xmlPath = parameters as string;

            if (!string.IsNullOrEmpty(xmlPath))
            {
                IsUILocked = true;

                if (!OpenDefinition(xmlPath))
                {
                    //MessageBox.Show("Workbench could not be opened.");
                    Log("Workbench could not be opened.");

                    if (DynamoCommands.WriteToLogCmd.CanExecute(null))
                    {
                        DynamoCommands.WriteToLogCmd.Execute("Workbench could not be opened.");
                        DynamoCommands.WriteToLogCmd.Execute(xmlPath);
                    }
                }

                IsUILocked = false;
            }

            //clear the clipboard to avoid copying between dyns
            dynSettings.Controller.ClipBoard.Clear();
        }

        private bool CanOpen(object parameters)
        {
            return true;
        }
        
        private void ShowSaveImageDialogAndSaveResult()
        {
            FileDialog _fileDialog = null;

            if (_fileDialog == null)
            {
                _fileDialog = new SaveFileDialog()
                {
                    AddExtension = true,
                    DefaultExt = ".png",
                    FileName = "Capture.png",
                    Filter = "PNG Image|*.png",
                    Title = "Save your Workbench to an Image",
                };
            }

            // if you've got the current space path, use it as the inital dir
            if (!string.IsNullOrEmpty(_model.CurrentSpace.FilePath))
            {
                var fi = new FileInfo(_model.CurrentSpace.FilePath);
                _fileDialog.InitialDirectory = fi.DirectoryName;
            }

            if (_fileDialog.ShowDialog() == DialogResult.OK)
            {
                if (SaveImageCommand.CanExecute(_fileDialog.FileName))
                    SaveImageCommand.Execute(_fileDialog.FileName);
            }
        
        }

        private bool CanShowSaveImageDialogAndSaveResult()
        {
            return true;
        }

        private void ShowOpenDialogAndOpenResult()
        {

            if ( this.Model.HomeSpace.HasUnsavedChanges && !AskUserToSaveWorkspaceOrCancel(this.Model.HomeSpace))
            {
                return;
            }

            FileDialog _fileDialog = null;

            if (_fileDialog == null)
            {
                _fileDialog = new OpenFileDialog()
                {
                    Filter = "Dynamo Definitions (*.dyn; *.dyf)|*.dyn;*.dyf|All files (*.*)|*.*",
                    Title = "Open Dynamo Definition..."
                };
            }

            // if you've got the current space path, use it as the inital dir
            if (!string.IsNullOrEmpty(_model.CurrentSpace.FilePath))
            {
                var fi = new FileInfo(_model.CurrentSpace.FilePath);
                _fileDialog.InitialDirectory = fi.DirectoryName;
            }
            else // use the samples directory, if it exists
            {
                Assembly dynamoAssembly = Assembly.GetExecutingAssembly();
                string location = Path.GetDirectoryName(dynamoAssembly.Location);
                string path = Path.Combine(location, "samples");

                if (Directory.Exists(path))
                {
                    _fileDialog.InitialDirectory = path;
                }
            }

            if (_fileDialog.ShowDialog() == DialogResult.OK)
            {
                if (OpenCommand.CanExecute(_fileDialog.FileName))
                    OpenCommand.Execute(_fileDialog.FileName);
            }
        }

        private bool CanShowOpenDialogAndOpenResultCommand()
        {
            return true;
        }

        private void ShowSaveDialogIfNeededAndSaveResult()
        {
            if (_model.CurrentSpace.FilePath != null)
            {
                if(SaveCommand.CanExecute())
                    SaveCommand.Execute();
            }
            else
            {
                if(ShowSaveDialogAndSaveResultCommand.CanExecute())
                    ShowSaveDialogAndSaveResultCommand.Execute();
            }
        }

        private bool CanShowSaveDialogIfNeededAndSaveResultCommand()
        {
            return true;
        }

        private FileDialog GetSaveDialog(dynWorkspaceModel workspace)
        {
            FileDialog fileDialog = new SaveFileDialog
            {
                AddExtension = true,
            };

            string ext, fltr;
            if ( workspace == _model.HomeSpace )
            {
                ext = ".dyn";
                fltr = "Dynamo Workspace (*.dyn)|*.dyn";
            }
            else
            {
                ext = ".dyf";
                fltr = "Dynamo Custom Node (*.dyf)|*.dyf";
            }
            fltr += "|All files (*.*)|*.*";

            fileDialog.FileName = workspace.Name + ext;
            fileDialog.AddExtension = true;
            fileDialog.DefaultExt = ext;
            fileDialog.Filter = fltr;

            return fileDialog;
        }

        private void ShowSaveDialogAndSaveResult()
        {
            FileDialog _fileDialog = GetSaveDialog(_model.CurrentSpace);

            //if the xmlPath is not empty set the default directory
            if (!string.IsNullOrEmpty(_model.CurrentSpace.FilePath))
            {
                var fi = new FileInfo(_model.CurrentSpace.FilePath);
                _fileDialog.InitialDirectory = fi.DirectoryName;
                _fileDialog.FileName = fi.Name;
            }
            else if (_model.CurrentSpace is FuncWorkspace && dynSettings.Controller.CustomNodeManager.SearchPath.Any())
            {
                _fileDialog.InitialDirectory = dynSettings.Controller.CustomNodeManager.SearchPath[0];
            }

            if (_fileDialog.ShowDialog() == DialogResult.OK)
            {
                SaveAs(_fileDialog.FileName);
            }
        }

        private bool CanShowSaveDialogAndSaveResultCommand()
        {
            return true;
        }

        public bool ShowNewFunctionDialog(ref string name, ref string category, ref string description, bool isRefactor = false)
        {
            string error = "";

            do
            {
                var dialog = new FunctionNamePrompt(dynSettings.Controller.SearchViewModel.Categories);
                dialog.nameBox.Text = name;
                dialog.categoryBox.Text = category;
                dialog.DescriptionInput.Text = description;

                if (dialog.ShowDialog() != true)
                {
                    return false;
                }

                name = dialog.Text;
                category = dialog.Category;
                description = dialog.Description;

                if (Controller.CustomNodeManager.Contains(name) && !isRefactor)
                {
                    error = "A custom node with the given name already exists.";
                    System.Windows.MessageBox.Show(error, "Error Initializing Custom Node", MessageBoxButton.OK,
                                                   MessageBoxImage.Error);
                }
                else if ( Controller.BuiltInTypesByNickname.ContainsKey(name) )
                {
                    error = "A built-in node with the given name already exists.";
                    System.Windows.MessageBox.Show(error, "Error Initializing Custom Node", MessageBoxButton.OK,
                                                   MessageBoxImage.Error);
                }
                else if (category.Equals(""))
                {
                    error = "You must enter a new category or choose one from the existing categories.";
                    System.Windows.MessageBox.Show(error, "Error Initializing Custom Node", MessageBoxButton.OK,
                                                   MessageBoxImage.Error);

                }
                else
                {
                    error = "";
                }

            } while (!error.Equals(""));

            return true;
        }

        private void ShowNewFunctionDialogAndMakeFunction()
        {
            string name = "", category = "", description = "";
            if (ShowNewFunctionDialog(ref name, ref category, ref description))
            {
                NewFunction(Guid.NewGuid(), name, category, description, true);
            }
        }

        private bool CanShowNewFunctionDialogCommand()
        {
            return true;
        }

        public virtual bool RunInDebug
        {
            get { return debug; }
            set
            {
                debug = value;

                //toggle off dynamic run
                CanRunDynamically = !debug;

                if (debug)
                    DynamicRunEnabled = false;

                RaisePropertyChanged("RunInDebug");
            }
        }

        private void GoToWiki()
        {
            Process.Start("https://github.com/ikeough/Dynamo/wiki");
        }

        private bool CanGoToWiki()
        {
            return true;
        }

        private void GoToSourceCode()
        {
            Process.Start("https://github.com/ikeough/Dynamo");
        }

        private bool CanGoToSourceCode()
        {
            return true;
        }

        /// <summary>
        ///     Attempts to save a given workspace.  Shows a save as dialog if the 
        ///     workspace does not already have a path associated with it
        /// </summary>
        /// <param name="workspace">The workspace for which to show the dialog</param>
        private void ShowSaveDialogIfNeededAndSave(dynWorkspaceModel workspace)
        {
            if (workspace.FilePath != null)
            {
                this.SaveAs(workspace.FilePath, workspace);
            }
            else
            {
                var fd = this.GetSaveDialog(workspace);
                if (fd.ShowDialog() == DialogResult.OK)
                {
                    this.SaveAs(fd.FileName, workspace);
                }
            }
        }

        /// <summary>
        /// Shows a message box asking the user to save the workspace and allows saving.
        /// </summary>
        /// <param name="workspace">The workspace for which to show the dialog</param>
        /// <returns>False if the user cancels, otherwise true</returns>
        public bool AskUserToSaveWorkspaceOrCancel(dynWorkspaceModel workspace, bool allowCancel = true)
        {
            var dialogText = "";
            if (workspace is FuncWorkspace)
            {
                dialogText = "You have unsaved changes to custom node workspace " + workspace.Name +
                             "\n\n Would you like to save your changes?";
            }
            else // homeworkspace
            {
                if (string.IsNullOrEmpty(workspace.FilePath))
                {
                    dialogText = "You haven't saved your changes to the Home workspace. " +
                                 "\n\n Would you like to save your changes?";
                }
                else
                {
                    dialogText = "You have unsaved changes to " + Path.GetFileName( workspace.FilePath ) +
                    "\n\n Would you like to save your changes?";
                }
            }

            var buttons = allowCancel ? MessageBoxButton.YesNoCancel : MessageBoxButton.YesNo;
            var result = System.Windows.MessageBox.Show(dialogText, "Confirmation", buttons, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                this.ShowSaveDialogIfNeededAndSave(workspace);
            }
            else if (result == MessageBoxResult.Cancel)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        ///     Ask the user if they want to save any unsaved changes, return false if the user cancels.
        /// </summary>
        /// <param name="allowCancel">Whether to show cancel button to user. </param>
        /// <returns>Whether the cleanup was completed or cancelled.</returns>
        public bool AskUserToSaveWorkspacesOrCancel(bool allowCancel = true)
        {
            foreach (var wvm in Workspaces.Where((wvm) => wvm.Model.HasUnsavedChanges))
            {
                if (!AskUserToSaveWorkspaceOrCancel(wvm.Model, allowCancel))
                    return false;
            }
            return true;
        }

        public void Cleanup()
        {
            this.Model.OnCleanup(null);
            DynamoLogger.Instance.FinishLogging();
        }

        private bool CanCleanup()
        {
            return true;
        }

        private void Exit(object allowCancel)
        {
            bool allowCancelBool = true;
            if (allowCancel != null)
            {
                allowCancelBool = (bool)allowCancel;
            }
            if (!AskUserToSaveWorkspacesOrCancel(allowCancelBool))
                return;
            this.Cleanup();
            
            exitInvoked = true;
            dynSettings.Bench.Close();
        }

        public bool exitInvoked = false;
        private bool CanExit(object allowCancel)
        {
            return !exitInvoked;
        }

        private void SaveAs(object parameters)
        {
            SaveAs(parameters.ToString());
        }

        private bool CanSaveAs(object parameters)
        {
            return true;
        }

        private void Clear()
        {
            IsUILocked = true;

            CleanWorkbench();

            //don't save the file path
            _model.CurrentSpace.FilePath = "";
            _model.CurrentSpace.HasUnsavedChanges = false;

            //clear the renderables
            dynSettings.Controller.RenderDescriptions.Clear();
            dynSettings.Controller.OnRequestsRedraw(dynSettings.Controller, EventArgs.Empty);

            IsUILocked = false;
        }

        private bool CanClear()
        {
            return true;
        }

        private void Home()
        {
            ViewHomeWorkspace();
        }

        private bool CanGoHome()
        {
            return _model.CurrentSpace != _model.HomeSpace;
        }

        private void LayoutAll()
        {
            IsUILocked = true;

            CleanWorkbench();

            double x = 0;
            double y = 0;
            double maxWidth = 0;    //track max width of current column
            double colGutter = 40;     //the space between columns
            double rowGutter = 40;
            int colCount = 0;

            Hashtable typeHash = new Hashtable();

            foreach (KeyValuePair<string, TypeLoadData> kvp in dynSettings.Controller.BuiltInTypesByNickname)
            {
                Type t = kvp.Value.Type;

                object[] attribs = t.GetCustomAttributes(typeof(NodeCategoryAttribute), false);

                if (t.Namespace == "Dynamo.Nodes" &&
                    !t.IsAbstract &&
                    attribs.Length > 0 &&
                    t.IsSubclassOf(typeof(dynNodeModel)))
                {
                    NodeCategoryAttribute elCatAttrib = attribs[0] as NodeCategoryAttribute;

                    List<Type> catTypes = null;

                    if (typeHash.ContainsKey(elCatAttrib.ElementCategory))
                    {
                        catTypes = typeHash[elCatAttrib.ElementCategory] as List<Type>;
                    }
                    else
                    {
                        catTypes = new List<Type>();
                        typeHash.Add(elCatAttrib.ElementCategory, catTypes);
                    }

                    catTypes.Add(t);
                }
            }

            foreach (DictionaryEntry de in typeHash)
            {
                List<Type> catTypes = de.Value as List<Type>;

                //add the name of the category here
                //AddNote(de.Key.ToString(), x, y, ViewModel.CurrentSpace);
                Dictionary<string, object> paramDict = new Dictionary<string, object>();
                paramDict.Add("x", x);
                paramDict.Add("y", y);
                paramDict.Add("text", de.Key.ToString());
                paramDict.Add("workspace", _model.CurrentSpace);

                if(AddNoteCommand.CanExecute(paramDict))
                    AddNoteCommand.Execute(paramDict);

                y += 60;

                foreach (Type t in catTypes)
                {
                    object[] attribs = t.GetCustomAttributes(typeof(NodeNameAttribute), false);

                    NodeNameAttribute elNameAttrib = attribs[0] as NodeNameAttribute;
                    dynNodeModel el = CreateInstanceAndAddNodeToWorkspace(
                           t, elNameAttrib.Name, Guid.NewGuid(), x, y,
                           _model.CurrentSpace
                        );

                    if (el == null) continue;

                    el.DisableReporting();

                    maxWidth = Math.Max(el.Width, maxWidth);

                    colCount++;

                    y += el.Height + rowGutter;

                    if (colCount > 20)
                    {
                        y = 60;
                        colCount = 0;
                        x += maxWidth + colGutter;
                        maxWidth = 0;
                    }
                }

                y = 0;
                colCount = 0;
                x += maxWidth + colGutter;
                maxWidth = 0;

            }

            IsUILocked = false;
        }

        private bool CanLayoutAll()
        {
            return true;
        }
    
        private void Copy(object parameters)
        {
            dynSettings.Controller.ClipBoard.Clear();

            foreach (ISelectable sel in DynamoSelection.Instance.Selection)
            {
                //MVVM : selection and clipboard now hold view model objects
                //UIElement el = sel as UIElement;
                dynModelBase el = sel as dynModelBase;
                if (el != null)
                {
                    if (!dynSettings.Controller.ClipBoard.Contains(el))
                    {
                        dynSettings.Controller.ClipBoard.Add(el);

                        //dynNodeView n = el as dynNodeView;
                        dynNodeModel n = el as dynNodeModel;
                        if (n != null)
                        {
                            var connectors = n.InPorts.ToList().SelectMany(x => x.Connectors)
                                .Concat(n.OutPorts.ToList().SelectMany(x => x.Connectors))
                                .Where(x => x.End != null &&
                                    x.End.Owner.IsSelected &&
                                    !dynSettings.Controller.ClipBoard.Contains(x));

                            dynSettings.Controller.ClipBoard.AddRange(connectors);
                        }
                    }
                }
            }
        }

        private bool CanCopy(object parameters)
        {   
            if (DynamoSelection.Instance.Selection.Count == 0)
            {
                return false;
            }
            return true;
        }

        private void Paste(object parameters)
        {
            //make a lookup table to store the guids of the
            //old nodes and the guids of their pasted versions
            var nodeLookup = new Dictionary<Guid, Guid>();

            //clear the selection so we can put the
            //paste contents in
            DynamoSelection.Instance.Selection.RemoveAll();

            var nodes = dynSettings.Controller.ClipBoard.OfType<dynNodeModel>();

            var connectors = dynSettings.Controller.ClipBoard.OfType<dynConnectorModel>();

            foreach (dynNodeModel node in nodes)
            {
                //create a new guid for us to use
                Guid newGuid = Guid.NewGuid();
                nodeLookup.Add(node.GUID, newGuid);

                var nodeData = new Dictionary<string, object>();
                nodeData.Add("x", node.X);
                nodeData.Add("y", node.Y + 100);
                if(node is dynFunction)
                    nodeData.Add("name", (node as dynFunction).Definition.FunctionId);
                else
                    nodeData.Add("name", node.GetType() );
                nodeData.Add("guid", newGuid);

                var xmlDoc = new XmlDocument();
                var dynEl = xmlDoc.CreateElement(node.GetType().ToString());
                xmlDoc.AppendChild(dynEl);
                node.Save(xmlDoc, dynEl, SaveContext.Copy);

                nodeData.Add("data", dynEl);

                dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(CreateNodeCommand, nodeData));
            }

            //process the command queue so we have 
            //nodes to connect to
            dynSettings.Controller.ProcessCommandQueue();

            //update the layout to ensure that the visuals
            //are present in the tree to connect to
            //dynSettings.Bench.UpdateLayout();
            OnRequestLayoutUpdate(this, EventArgs.Empty);

            foreach (dynConnectorModel c in connectors)
            {
                var connectionData = new Dictionary<string, object>();

                // if in nodeLookup, the node is paste.  otherwise, use the existing node guid
                Guid startGuid = Guid.Empty;
                Guid endGuid = Guid.Empty;

                startGuid = nodeLookup.TryGetValue(c.Start.Owner.GUID, out startGuid) ? startGuid : c.Start.Owner.GUID;
                endGuid = nodeLookup.TryGetValue(c.End.Owner.GUID, out endGuid) ? endGuid : c.End.Owner.GUID;

                var startNode = _model.CurrentSpace.Nodes.FirstOrDefault(x => x.GUID == startGuid );
                var endNode = _model.CurrentSpace.Nodes.FirstOrDefault(x => x.GUID == endGuid );

                // do not form connector if the end nodes are null
                if (startNode == null || endNode == null)
                {
                    continue;
                }

                //don't let users paste connectors between workspaces
                if (startNode.WorkSpace != _model.CurrentSpace)
                {
                    continue;
                }

                connectionData.Add("start", startNode);
                connectionData.Add("end", endNode);

                connectionData.Add("port_start", c.Start.Index);
                connectionData.Add("port_end", c.End.Index);

                dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(CreateConnectionCommand, connectionData));
            }

            //process the queue again to create the connectors
            dynSettings.Controller.ProcessCommandQueue();

            var notes = dynSettings.Controller.ClipBoard.OfType<dynNoteModel>();

            foreach (dynNoteModel note in notes)
            {
                var newGUID = Guid.NewGuid();

                var sameSpace = _model.CurrentSpace.Notes.Any(x => x.GUID == note.GUID);
                var newX = sameSpace ? note.X + 20 : note.X;
                var newY = sameSpace ? note.Y + 20 : note.Y;

                var noteData = new Dictionary<string, object>()
                {
                    { "x", newX },
                    { "y", newY },
                    { "text", note.Text },
                    { "guid", newGUID }
                };

                dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(AddNoteCommand, noteData));
                dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(
                    AddToSelectionCommand, 
                    _model.CurrentSpace.Notes.FirstOrDefault(x => x.GUID == newGUID)));
            }

            dynSettings.Controller.ProcessCommandQueue();

            foreach (var de in nodeLookup)
            {
                dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(
                    AddToSelectionCommand,
                    _model.CurrentSpace.Nodes.FirstOrDefault(x => x.GUID == de.Value)));
            }

            dynSettings.Controller.ProcessCommandQueue();
        }

        private bool CanPaste(object parameters)
        {
            if (dynSettings.Controller.ClipBoard.Count == 0)
            {
                return false;
            }

            return true;
        }

        private void ToggleConsoleShowing()
        {
            if (ConsoleShowing)
            {
                ConsoleShowing = false;
            }
            else
            {
                ConsoleShowing = true;
            }
        }

        private bool CanToggleConsoleShowing()
        {
            return true;
        }

        private void CancelRun()
        {
            dynSettings.Controller.RunCancelled = true;
        }

        private bool CanCancelRun()
        {
            return true;
        }

        private void ShowPackageManagerSearch()
        {
            var pms = new PackageManagerSearchViewModel(Controller.PackageManagerClient);
            var window = new PackageManagerSearchView(pms);
            window.Show();
        }

        private bool CanShowPackageManagerSearch()
        {
            return true;
        }

        private void ShowInstalledPackages()
        {
            var window = new InstalledPackagesView();
            window.Show();
        }

        private bool CanShowInstalledPackages()
        {
            return true;
        }

        public void SaveImage(object parameters)
        {
            string imagePath = parameters as string;

            if (!string.IsNullOrEmpty(imagePath))
            {
                var bench = dynSettings.Bench;

                if (bench == null)
                {
                    DynamoLogger.Instance.Log("Cannot export bench as image without UI.  No image wil be exported.");
                    return;
                }

                var control = WPF.FindChild<DragCanvas>(bench, null);

                double width = 1;
                double height = 1;

                // connectors are most often within the bounding box of the nodes and notes

                foreach (dynNodeModel n in _model.CurrentSpace.Nodes)
                {
                    width = Math.Max(n.X + n.Width, width);
                    height = Math.Max(n.Y + n.Height, height);
                }

                foreach (dynNoteModel n in _model.CurrentSpace.Notes)
                {
                    width = Math.Max(n.X + n.Width, width);
                    height = Math.Max(n.Y + n.Height, height);
                }

                var rtb = new RenderTargetBitmap((int) width,
                                                 (int) height, 96, 96,
                                                 System.Windows.Media.PixelFormats.Default);

                rtb.Render(control);

                //endcode as PNG
                var pngEncoder = new PngBitmapEncoder();
                pngEncoder.Frames.Add(BitmapFrame.Create(rtb));

                try
                {
                    using (var stm = File.Create(imagePath))
                    {
                        pngEncoder.Save(stm);
                    }
                }
                catch
                {
                    DynamoLogger.Instance.Log("Failed to save the Workspace an image.");
                }
                

            }
        }

        private bool CanSaveImage(object parameters)
        {
            return true;
        }

        /// <summary>
        /// Clear the UI log.
        /// </summary>
        private void ClearLog()
        {
            sw.Flush();
            sw.Close();
            sw = new StringWriter();
            LogText = sw.ToString();
        }

        private bool CanClearLog()
        {
            return true;
        }

        private void RunExpression(object parameters)
        {
            dynSettings.Controller.RunExpression(Convert.ToBoolean(parameters));
        }

        private bool CanRunExpression(object parameters)
        {
            if (dynSettings.Controller == null)
            {
                return false;
            }
            return true;
        }

        private void ShowPackageManager()
        {


            //dynSettings.Bench.PackageManagerLoginStateContainer.Visibility = Visibility.Visible;
            //dynSettings.Bench.PackageManagerMenu.Visibility = Visibility.Visible;


        }

        private bool CanShowPackageManager()
        {
            return true;
        }

        private void GoToWorkspace(object parameter)
        {
            if (parameter is Guid && dynSettings.Controller.CustomNodeManager.Contains((Guid)parameter))
            {
                ViewCustomNodeWorkspace( dynSettings.Controller.CustomNodeManager.GetFunctionDefinition((Guid)parameter) );
            }
        }

        private bool CanGoToWorkspace(object parameter)
        {
            return true;
        }

        private void DisplayFunction(object parameters)
        {
            Controller.CustomNodeManager.GetFunctionDefinition((Guid)parameters);



        }

        private bool CanDisplayFunction(object parameters)
        { 
            var id = dynSettings.CustomNodes.FirstOrDefault(x => x.Value == (Guid) parameters).Value;

            if (id!=null)
                return true;

            return false;
        }

        private void SetConnectorType(object parameters)
        {
            if (parameters.ToString() == "BEZIER")
            {
                connectorType = ConnectorType.BEZIER;
            }
            else
            {
                connectorType = ConnectorType.POLYLINE;
            }
        }

        private bool CanSetConnectorType(object parameters)
        {
            //parameter object will be BEZIER or POLYLINE
            if (string.IsNullOrEmpty(parameters.ToString()))
            {
                return false;
            }
            return true;
        }

        void CreateNode(object parameters)
        {
            var data = parameters as Dictionary<string, object>;
            if (data == null)
            {
                return;
            }

            dynNodeModel node = CreateNode(data["name"].ToString());
            if (node == null)
            {
                DynamoCommands.WriteToLogCmd.Execute("Failed to create the node");
                return;
            } 

            if ( (node is dynSymbol || node is dynOutput) && _model.CurrentSpace is HomeWorkspace)
            {
                DynamoCommands.WriteToLogCmd.Execute("Cannot place dynSymbol or dynOutput in HomeWorkspace");
                return;
            }

            _model.CurrentSpace.Nodes.Add(node);
            node.WorkSpace = dynSettings.Controller.DynamoViewModel.CurrentSpace;

            //if we've received a value in the dictionary
            //try to set the value on the node
            if (data.ContainsKey("data"))
            {
                node.Load(data["data"] as XmlNode);
            }

            //override the guid so we can store
            //for connection lookup
            if (data.ContainsKey("guid"))
            {
                node.GUID = (Guid)data["guid"];
            }
            else
            {
                node.GUID = Guid.NewGuid();
            }

            dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.OnRequestNodeCentered(this, new ModelEventArgs(node, data));

            node.EnableInteraction();

            if (ViewingHomespace)
            {
                node.SaveResult = true;
            }
        }

        bool CanCreateNode(object parameters)
        {
            var data = parameters as Dictionary<string, object>;

            if (data == null)
                return false;

            Guid guid;
            var name = data["name"].ToString();

            if (dynSettings.Controller.BuiltInTypesByNickname.ContainsKey(name)
                    || dynSettings.Controller.BuiltInTypesByName.ContainsKey(name)
                    || (Guid.TryParse(name, out guid) && dynSettings.Controller.CustomNodeManager.Contains(guid)))
            {
                return true;
            }

            string message = string.Format("Can not create instance of node {0}.", data["name"]);
            DynamoCommands.WriteToLogCmd.Execute(message);
            Log(message);

            return false;
        }

        void CreateConnection(object parameters)
        {
            try
            {
                Dictionary<string, object> connectionData = parameters as Dictionary<string, object>;

                dynNodeModel start = (dynNodeModel)connectionData["start"];
                dynNodeModel end = (dynNodeModel)connectionData["end"];
                int startIndex = (int)connectionData["port_start"];
                int endIndex = (int)connectionData["port_end"];

                var c = dynConnectorModel.Make(start, end, startIndex, endIndex, 0);

                if (c != null)
                    _model.CurrentSpace.Connectors.Add(c);
            }
            catch (Exception e)
            {
                DynamoLogger.Instance.Log(e.Message);
                Log(e);
            }
        }

        bool CanCreateConnection(object parameters)
        {
            //make sure you have valid connection data
            Dictionary<string, object> connectionData = parameters as Dictionary<string, object>;
            if (connectionData != null && connectionData.Count == 4)
            {
                return true;
            }

            return false;
        }

        private void Delete(object parameters)
        {
            //if you get an object in the parameters, just delete that object
            if (parameters != null)
            {
                dynNoteModel note = parameters as dynNoteModel;
                dynNodeModel node = parameters as dynNodeModel;

                if (node != null)
                {
                    DeleteNodeAndItsConnectors(node);
                }
                else if (note != null)
                {
                    DeleteNote(note);
                }
            }
            else
            {
                for (int i = DynamoSelection.Instance.Selection.Count - 1; i >= 0; i--)
                {
                    dynNoteModel note = DynamoSelection.Instance.Selection[i] as dynNoteModel;
                    dynNodeModel node = DynamoSelection.Instance.Selection[i] as dynNodeModel;

                    if (node != null)
                    {
                        DeleteNodeAndItsConnectors(node);
                    }
                    else if (note != null)
                    {
                        DeleteNote(note);
                    }
                }
            }
        }

        private bool CanVisibilityBeToggled(object parameters)
        {
            return true;
        }

        private bool CanUpstreamVisibilityBeToggled(object parameters)
        {
            return true;
        }

        private bool CanDelete(object parameters)
        {
            return DynamoSelection.Instance.Selection.Count > 0;
        }

        private void AddNote(object parameters)
        {
            var inputs = parameters as Dictionary<string, object> ?? new Dictionary<string, object>();

            // by default place note at center
            var x = 0.0;
            var y = 0.0;

            if (inputs != null && inputs.ContainsKey("x"))
                x = (double)inputs["x"];

            if (inputs != null && inputs.ContainsKey("y"))
                y = (double)inputs["y"];


            var n = new dynNoteModel(x, y);

            //if we have null parameters, the note is being added
            //from the menu, center the view on the note
            if (parameters == null)
            {
                inputs.Add("transformFromOuterCanvasCoordinates", true);
                dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.OnRequestNodeCentered( this, new ModelEventArgs(n, inputs) );
            }

            object id;
            if (inputs.TryGetValue("guid", out id))
                n.GUID = (Guid)id;

            n.Text = (inputs == null || !inputs.ContainsKey("text")) ? "New Note" : inputs["text"].ToString();
            var ws = (inputs == null || !inputs.ContainsKey("workspace")) ? _model.CurrentSpace : (dynWorkspaceModel)inputs["workspace"];

            ws.Notes.Add(n);
        }

        private bool CanAddNote(object parameters)
        {
            return true;
        }

        private void DeleteNote(dynNoteModel note)
        {
            DynamoSelection.Instance.Selection.Remove(note);
            _model.CurrentSpace.Notes.Remove(note);
        }

        private void SelectNeighbors(object parameters)
        {
            List<ISelectable> sels = DynamoSelection.Instance.Selection.ToList<ISelectable>();

            foreach (ISelectable sel in sels)
            {
                ((dynNodeModel)sel).SelectNeighbors();
            }
        }

        private bool CanSelectNeighbors(object parameters)
        {
            return true;
        }

        private static void DeleteNodeAndItsConnectors(dynNodeModel node)
        {
            foreach (var conn in node.AllConnectors().ToList())
            {
                conn.NotifyConnectedPortsOfDeletion();
                dynSettings.Controller.DynamoViewModel.Model.CurrentSpace.Connectors.Remove(conn);
            }

            node.DisableReporting();
            node.Destroy();
            node.Cleanup();
            DynamoSelection.Instance.Selection.Remove(node);
            node.WorkSpace.Nodes.Remove(node);
        }

        private void AddToSelection(object parameters)
        {
            var node = parameters as dynNodeModel;

            if (!node.IsSelected)
            {
                if (!DynamoSelection.Instance.Selection.Contains(node))
                    DynamoSelection.Instance.Selection.Add(node);
            }
        }

        private bool CanAddToSelection(object parameters)
        {
            dynNodeModel node = parameters as dynNodeModel;
            if (node == null)
            {
                return false;
            }

            return true;
        }

        public void Log(Exception e)
        {
            Log(e.GetType() + ":");
            Log(e.Message);
            Log(e.StackTrace);
        }

        public void Log(string message)
        {
            sw.WriteLine(message);
            LogText = sw.ToString();

            if (DynamoCommands.WriteToLogCmd.CanExecute(null))
            {
                DynamoCommands.WriteToLogCmd.Execute(message);
            }
        }

        private void PostUIActivation()
        {
            DynamoLoader.LoadCustomNodes();

            dynSettings.Controller.DynamoViewModel.Log("Welcome to Dynamo!");

            if (UnlockLoadPath != null && !OpenWorkspace(UnlockLoadPath))
            {
                dynSettings.Controller.DynamoViewModel.Log("Workbench could not be opened.");

                if (DynamoCommands.WriteToLogCmd.CanExecute(null))
                {
                    DynamoCommands.WriteToLogCmd.Execute("Workbench could not be opened.");
                    DynamoCommands.WriteToLogCmd.Execute(UnlockLoadPath);
                }
            }

            UnlockLoadPath = null;

            //OnUIUnlocked(this, EventArgs.Empty);
            IsUILocked = false;

            DynamoCommands.ShowSearch.Execute(null);

            _model.HomeSpace.OnDisplayed();

        }

        private bool CanDoPostUIActivation()
        {
            return true;
        }

        internal bool OpenDefinition(string xmlPath)
        {
            return OpenDefinition(
                xmlPath,
                new Dictionary<Guid, HashSet<FunctionDefinition>>(),
                new Dictionary<Guid, HashSet<Guid>>());
        }

        // PB: This is deprecated, can't do it now, though...
        internal bool OpenDefinition(
            string xmlPath,
            Dictionary<Guid, HashSet<FunctionDefinition>> children,
            Dictionary<Guid, HashSet<Guid>> parents)
        {
            try
            {
                #region read xml file

                var xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlPath);

                string funName = null;
                string category = "";
                string description = "";
                double cx = DynamoView.CANVAS_OFFSET_X;
                double cy = DynamoView.CANVAS_OFFSET_Y;
                double zoom = 1.0;
                string id = "";

                // load the header
                foreach (XmlNode node in xmlDoc.GetElementsByTagName("dynWorkspace"))
                {
                    foreach (XmlAttribute att in node.Attributes)
                    {
                        if (att.Name.Equals("X"))
                            cx = double.Parse(att.Value, CultureInfo.InvariantCulture);
                        else if (att.Name.Equals("Y"))
                            cy = double.Parse(att.Value, CultureInfo.InvariantCulture);
                        else if (att.Name.Equals("zoom"))
                            zoom = double.Parse(att.Value, CultureInfo.InvariantCulture);
                        else if (att.Name.Equals("Name"))
                            funName = att.Value;
                        else if (att.Name.Equals("Description"))
                            description = att.Value;
                        else if (att.Name.Equals("Category"))
                            category = att.Value;
                        else if (att.Name.Equals("ID"))
                        {
                            id = att.Value;
                        }
                    }
                }

                // we have a dyf and it lacks an ID field, we need to assign it
                // a deterministic guid based on its name.  By doing it deterministically,
                // files remain compatible
                if (string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(funName))
                {
                    id = GuidUtility.Create(GuidUtility.UrlNamespace, funName).ToString();
                }

                #endregion

                //If there is no function name, then we are opening a home definition
                if (funName == null || funName == "Home")
                {
                    //View the home workspace, then open the bench file
                    if (!ViewingHomespace)
                        ViewHomeWorkspace();

                    var loaded = DynamoLoader.LoadCustomNodes(Path.GetDirectoryName(xmlPath));
                    //var loaded = Controller.CustomNodeManager.ScanNodeHeadersInDirectory(Path.GetDirectoryName(xmlPath));
                    return OpenWorkspace(xmlPath);
                }
                else if (Controller.CustomNodeManager.Contains(funName))
                {
                    Log("ERROR: Could not load definition for \"" + funName +
                              "\", a node with this name already exists.");
                    return false;
                }

                Log("Loading node definition for \"" + funName + "\" from: " + xmlPath);

                FunctionDefinition def = NewFunction(
                    Guid.Parse(id),
                    funName,
                    category.Length > 0
                        ? category
                        : BuiltinNodeCategories.SCRIPTING_CUSTOMNODES, description,
                    false, cx, cy
                    );

                dynWorkspaceModel ws = def.Workspace;

                //this.Log("Opening definition " + xmlPath + "...");

                XmlNodeList elNodes = xmlDoc.GetElementsByTagName("dynElements");
                XmlNodeList cNodes = xmlDoc.GetElementsByTagName("dynConnectors");
                XmlNodeList nNodes = xmlDoc.GetElementsByTagName("dynNotes");

                XmlNode elNodesList = elNodes[0];
                XmlNode cNodesList = cNodes[0];
                XmlNode nNodesList = nNodes[0];

                var dependencies = new Stack<Guid>();

                #region instantiate nodes

                //if there is any problem loading a node, then
                //add the node's guid to the bad nodes collection
                //so we can avoid attempting to make connections to it
                List<Guid> badNodes = new List<Guid>();

                foreach (XmlNode elNode in elNodesList.ChildNodes)
                {
                    XmlAttribute typeAttrib = elNode.Attributes[0];
                    XmlAttribute guidAttrib = elNode.Attributes[1];
                    XmlAttribute nicknameAttrib = elNode.Attributes[2];
                    XmlAttribute xAttrib = elNode.Attributes[3];
                    XmlAttribute yAttrib = elNode.Attributes[4];

                    XmlAttribute lacingAttrib = null;
                    if(elNode.Attributes.Count > 5)
                    {
                         lacingAttrib = elNode.Attributes[5];
                    }

                    string typeName = typeAttrib.Value;

                    string oldNamespace = "Dynamo.Elements.";
                    if (typeName.StartsWith(oldNamespace))
                        typeName = "Dynamo.Nodes." + typeName.Remove(0, oldNamespace.Length);

                    //test the GUID to confirm that it is non-zero
                    //if it is zero, then we have to fix it
                    //this will break the connectors, but it won't keep
                    //propagating bad GUIDs
                    var guid = new Guid(guidAttrib.Value);
                    if (guid == Guid.Empty)
                    {
                        guid = Guid.NewGuid();
                    }

                    string nickname = nicknameAttrib.Value;

                    double x = double.Parse(xAttrib.Value,CultureInfo.InvariantCulture);
                    double y = double.Parse(yAttrib.Value,CultureInfo.InvariantCulture);

                    //Type t = Type.GetType(typeName);
                    TypeLoadData tData;
                    Type t;
                    
                    if (!Controller.BuiltInTypesByName.TryGetValue(typeName, out tData))
                    {
                        //try and get a system type by this name
                        t = Type.GetType(typeName);

                        //if we still can't find the type, try the also known as attributes
                        if (t == null)
                        {
                            //try to get the also known as values
                            foreach (KeyValuePair<string, TypeLoadData> kvp in Controller.BuiltInTypesByName)
                            {
                                var akaAttribs = kvp.Value.Type.GetCustomAttributes(typeof(AlsoKnownAsAttribute), false);
                                if (akaAttribs.Count() > 0)
                                {
                                    if ((akaAttribs[0] as AlsoKnownAsAttribute).Values.Contains(typeName))
                                    {
                                        Log(string.Format("Found matching node for {0} also known as {1}", kvp.Key, typeName));
                                        t = kvp.Value.Type;
                                    }
                                }
                            }
                        }

                        if (t == null)
                        {
                            Log("Could not load node of type: " + typeName);
                            Log("Loading will continue but nodes might be missing from your workflow.");

                            //return false;
                            badNodes.Add(guid);
                            continue;
                        }
                    }
                    else
                        t = tData.Type;

                    dynNodeModel el = CreateInstanceAndAddNodeToWorkspace(t, nickname, guid, x, y, ws);

                    if (lacingAttrib != null)
                    {
                        //don't set the lacing strategy if the type
                        //wants it disabled
                        if (el.ArgumentLacing != LacingStrategy.Disabled)
                        {
                            LacingStrategy lacing = LacingStrategy.First;
                            Enum.TryParse(lacingAttrib.Value, out lacing);
                            el.ArgumentLacing = lacing;
                        }
                    }

                    el.DisableReporting();
                    el.Load(elNode);

                    if (el is dynFunction)
                    {
                        var fun = el as dynFunction;

                        // we've found a custom node, we need to attempt to load its guid.  
                        // if it doesn't exist (i.e. its a legacy node), we need to assign it one,
                        // deterministically
                        Guid funId;
                        try
                        {
                            funId = Guid.Parse(fun.Symbol);
                        }
                        catch
                        {
                            funId = GuidUtility.Create(GuidUtility.UrlNamespace, nicknameAttrib.Value);
                            fun.Symbol = funId.ToString();
                        }

                        if (dynSettings.Controller.CustomNodeManager.IsInitialized(funId))
                            fun.Definition = dynSettings.Controller.CustomNodeManager.GetFunctionDefinition(funId);
                        else
                            dependencies.Push(funId);
                    }
                }

                #endregion

                //Bench.WorkBench.UpdateLayout();
                OnRequestLayoutUpdate(this, EventArgs.Empty);

                #region instantiate connectors

                foreach (XmlNode connector in cNodesList.ChildNodes)
                {
                    XmlAttribute guidStartAttrib = connector.Attributes[0];
                    XmlAttribute intStartAttrib = connector.Attributes[1];
                    XmlAttribute guidEndAttrib = connector.Attributes[2];
                    XmlAttribute intEndAttrib = connector.Attributes[3];
                    XmlAttribute portTypeAttrib = connector.Attributes[4];

                    var guidStart = new Guid(guidStartAttrib.Value);
                    var guidEnd = new Guid(guidEndAttrib.Value);
                    int startIndex = Convert.ToInt16(intStartAttrib.Value);
                    int endIndex = Convert.ToInt16(intEndAttrib.Value);
                    int portType = Convert.ToInt16(portTypeAttrib.Value);

                    //find the elements to connect
                    dynNodeModel start = null;
                    dynNodeModel end = null;

                    if (badNodes.Contains(guidStart) || badNodes.Contains(guidEnd))
                        continue;

                    foreach (dynNodeModel e in ws.Nodes)
                    {
                        if (e.GUID == guidStart)
                        {
                            start = e;
                        }
                        else if (e.GUID == guidEnd)
                        {
                            end = e;
                        }
                        if (start != null && end != null)
                        {
                            break;
                        }
                    }

                    try
                    {
                        if (start != null && end != null && start != end)
                        {
                            var newConnector = dynConnectorModel.Make(
                                start, end,
                                startIndex, endIndex,
                                portType );

                            ws.Connectors.Add(newConnector);
                        }
                    }
                    catch
                    {
                        dynSettings.Controller.DynamoViewModel.Log(string.Format("ERROR : Could not create connector between {0} and {1}.", start.GUID, end.GUID));
                    }
                }

                #endregion

                #region instantiate notes

                if (nNodesList != null)
                {
                    foreach (XmlNode note in nNodesList.ChildNodes)
                    {
                        XmlAttribute textAttrib = note.Attributes[0];
                        XmlAttribute xAttrib = note.Attributes[1];
                        XmlAttribute yAttrib = note.Attributes[2];

                        string text = textAttrib.Value;
                        double x = double.Parse(xAttrib.Value, CultureInfo.InvariantCulture);
                        double y = double.Parse(yAttrib.Value, CultureInfo.InvariantCulture);

                        var paramDict = new Dictionary<string, object>();
                        paramDict.Add("x", x);
                        paramDict.Add("y", y);
                        paramDict.Add("text", text);
                        paramDict.Add("workspace", ws);
                        //DynamoCommands.AddNoteCmd.Execute(paramDict);
                        dynSettings.Controller.DynamoViewModel.AddNoteCommand.Execute(paramDict);
                    }
                }

                #endregion

                foreach (dynNodeModel e in ws.Nodes)
                    e.EnableReporting();

                //DynamoModel.hideWorkspace(ws);

                ws.FilePath = xmlPath;

                bool canLoad = true;

                //For each node this workspace depends on...
                foreach (Guid dep in dependencies)
                {
                    canLoad = false;
                    //Dep -> Ws
                    if (children.ContainsKey(dep))
                        children[dep].Add(def);
                    else
                        children[dep] = new HashSet<FunctionDefinition> { def };

                    //Ws -> Deps
                    if (parents.ContainsKey(def.FunctionId))
                        parents[def.FunctionId].Add(dep);
                    else
                        parents[def.FunctionId] = new HashSet<Guid> { dep };
                }

                if (canLoad)
                    SaveFunction(def, false);

                nodeWorkspaceWasLoaded(def, children, parents);

                //set the zoom and trigger events
                //to get the view to scale iteself
                ws.Zoom = zoom;
                var vm = dynSettings.Controller.DynamoViewModel.Workspaces.First(x => x.Model == ws);
                vm.OnCurrentOffsetChanged(this, new PointEventArgs(new Point(cx, cy)));

            }
            catch (Exception ex)
            {
                Log("There was an error opening the workbench.");
                Log(ex);
                Debug.WriteLine(ex.Message + ":" + ex.StackTrace);
                CleanWorkbench();

                if (controller.Testing)
                    Assert.Fail(ex.Message);

                return false;
            }

            return true;
        }

        /// <summary>
        ///     Save the current workspace to a specific file path, if the path is null or empty, does nothing.
        ///     If successful, the CurrentSpace.FilePath field is updated as a side effect
        /// </summary>
        /// <param name="path">The path to save to</param>
        internal void SaveAs(string path)
        {
            this.SaveAs(path, _model.CurrentSpace);
        }

        /// <summary>
        ///     Save to a specific file path, if the path is null or empty, does nothing.
        ///     If successful, the CurrentSpace.FilePath field is updated as a side effect
        /// </summary>
        /// <param name="path">The path to save to</param>
        /// <param name="workspace">The workspace to save</param>
        internal void SaveAs(string path, dynWorkspaceModel workspace)
        {
            if (!String.IsNullOrEmpty(path))
            {
                // if it's a custom node
                if (workspace is FuncWorkspace)
                {
                    var def = dynSettings.Controller.CustomNodeManager.GetDefinitionFromWorkspace(workspace);
                    def.Workspace.FilePath = path;

                    if (def != null)
                    {
                        this.SaveFunction(def, true);
                        workspace.FilePath = path;
                    }
                    return;
                }

                if (!dynWorkspaceModel.SaveWorkspace(path, workspace))
                {
                    Log("Workbench could not be saved.");
                }
                else
                {
                    workspace.FilePath = path;
                }
                
            }
        }

        /// <summary>
        ///     Attempts to save an element, assuming that the CurrentSpace.FilePath 
        ///     field is already  populated with a path has a filename associated with it. 
        /// </summary>
        internal void Save()
        {
            if (!String.IsNullOrEmpty(_model.CurrentSpace.FilePath))
                SaveAs(_model.CurrentSpace.FilePath);
        }


        public IEnumerable<dynNodeModel> AllNodes
        {
            get
            {
                return _model.Workspaces.Aggregate((IEnumerable<dynNodeModel>)new List<dynNodeModel>(), (a, x) => a.Concat(x.Nodes))
                    .Concat(Controller.CustomNodeManager.GetLoadedDefinitions().Aggregate(
                        (IEnumerable<dynNodeModel>)new List<dynNodeModel>(),
                        (a, x) => a.Concat(x.Workspace.Nodes)
                        )
                    );
            }
        }

        /// <summary>
        ///     Save a function.  This includes writing to a file and compiling the 
        ///     function and saving it to the FSchemeEnvironment
        /// </summary>
        /// <param name="definition">The definition to save</param>
        /// <param name="bool">Whether to write the function to file.</param>
        /// <returns>Whether the operation was successful</returns>
        public void SaveFunction(FunctionDefinition definition, bool writeDefinition = true, bool addToSearch = false, bool compileFunction = true)
        {
            if (definition == null)
                return;

            // Get the internal nodes for the function
            var functionWorkspace = definition.Workspace as FuncWorkspace;

            // If asked to, write the definition to file
            if (writeDefinition)
            {
                string path = "";
                if (String.IsNullOrEmpty(definition.Workspace.FilePath))
                {
                    var pluginsPath = this.Controller.CustomNodeManager.GetDefaultSearchPath();

                    if (!Directory.Exists(pluginsPath))
                        Directory.CreateDirectory(pluginsPath);

                    path = Path.Combine(pluginsPath, dynSettings.FormatFileName(functionWorkspace.Name) + ".dyf");
                }
                else
                {
                    path = definition.Workspace.FilePath;
                }
                
                try
                {
                
                    dynWorkspaceModel.SaveWorkspace(path, functionWorkspace);

                    if (addToSearch)
                    {
                        Controller.SearchViewModel.Add(functionWorkspace.Name, functionWorkspace.Category, functionWorkspace.Description, definition.FunctionId);
                    }

                    var customNodeInfo = new CustomNodeInfo(definition.FunctionId, functionWorkspace.Name, functionWorkspace.Category,
                                                            functionWorkspace.Description, path);
                    Controller.CustomNodeManager.SetNodeInfo(customNodeInfo);

                    #region Compile Function and update all nodes

                    IEnumerable<string> inputNames = new List<string>();
                    IEnumerable<string> outputNames = new List<string>();
                    dynSettings.Controller.FSchemeEnvironment.DefineSymbol(definition.FunctionId.ToString(), CustomNodeManager.CompileFunction(definition, ref inputNames, ref outputNames));

                    //Update existing function nodes which point to this function to match its changes
                    foreach (dynNodeModel el in AllNodes)
                    {
                        if (el is dynFunction)
                        {
                            var node = (dynFunction)el;

                            if (node.Definition != definition)
                                continue;

                            node.SetInputs(inputNames);
                            node.SetOutputs(outputNames);
                            el.RegisterAllPorts();
                        }
                    }

                    //Call OnSave for all saved elements
                    foreach (dynNodeModel el in functionWorkspace.Nodes)
                        el.onSave();


                    #endregion


                }
                catch (Exception e)
                {
                    Log("Error saving:" + e.GetType());
                    Log(e);
                }
            }

        }

        /// <summary>
        ///     Save a function.  This includes writing to a file and compiling the 
        ///     function and saving it to the FSchemeEnvironment
        /// </summary>
        /// <param name="definition">The definition to saveo</param>
        /// <param name="bool">Whether to write the function to file</param>
        /// <returns>Whether the operation was successful</returns>
        public string SaveFunctionOnly(FunctionDefinition definition)
        {
            if (definition == null)
                return "";

            // Get the internal nodes for the function
            dynWorkspaceModel functionWorkspace = definition.Workspace;

            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string pluginsPath = Path.Combine(directory, "definitions");

            try
            {
                if (!Directory.Exists(pluginsPath))
                    Directory.CreateDirectory(pluginsPath);

                string path = Path.Combine(pluginsPath, dynSettings.FormatFileName(functionWorkspace.Name) + ".dyf");
                dynWorkspaceModel.SaveWorkspace(path, functionWorkspace);
                return path;
            }
            catch (Exception e)
            {
                Log("Error saving:" + e.GetType());
                Log(e);
                return "";
            }

        }

        private void nodeWorkspaceWasLoaded(
            FunctionDefinition def,
            Dictionary<Guid, HashSet<FunctionDefinition>> children,
            Dictionary<Guid, HashSet<Guid>> parents)
        {
            //If there were some workspaces that depended on this node...
            if (children.ContainsKey(def.FunctionId))
            {
                //For each workspace...
                foreach (FunctionDefinition child in children[def.FunctionId])
                {
                    //Nodes the workspace depends on
                    HashSet<Guid> allParents = parents[child.FunctionId];
                    //Remove this workspace, since it's now loaded.
                    allParents.Remove(def.FunctionId);
                    //If everything the node depends on has been loaded...
                    if (!allParents.Any())
                    {
                        SaveFunction(child, false);
                        nodeWorkspaceWasLoaded(child, children, parents);
                    }
                }
            }
        }

        /// <summary>
        ///     Create a node from a type object in a given workspace.
        /// </summary>
        /// <param name="elementType"> The Type object from which the node can be activated </param>
        /// <param name="nickName"> A nickname for the node.  If null, the nickName is loaded from the NodeNameAttribute of the node </param>
        /// <param name="guid"> The unique identifier for the node in the workspace. </param>
        /// <param name="x"> The x coordinate where the dynNodeView will be placed </param>
        /// <param name="y"> The x coordinate where the dynNodeView will be placed</param>
        /// <returns> The newly instantiate dynNode</returns>
        public dynNodeModel CreateInstanceAndAddNodeToWorkspace(Type elementType, string nickName, Guid guid,
            double x, double y, dynWorkspaceModel ws, bool isVisible = true, bool isUpstreamVisible = true)    //Visibility vis = Visibility.Visible)
        {
            try
            {
                dynNodeModel node = CreateNodeInstance(elementType, nickName, guid);

                ws.Nodes.Add(node);
                node.WorkSpace = ws;

                node.X = x;
                node.Y = y;

                node.IsVisible = isVisible;
                node.IsUpstreamVisible = isUpstreamVisible;

                return node;
            }
            catch (Exception e)
            {
                Log("Could not create an instance of the selected type: " + elementType);
                Log(e);
                return null;
            }
        }

        /// <summary>
        ///     Create a build-in node from a type object in a given workspace.
        /// </summary>
        /// <param name="elementType"> The Type object from which the node can be activated </param>
        /// <param name="nickName"> A nickname for the node.  If null, the nickName is loaded from the NodeNameAttribute of the node </param>
        /// <param name="guid"> The unique identifier for the node in the workspace. </param>
        /// <returns> The newly instantiated dynNode</returns>
        public dynNodeModel CreateNodeInstance(Type elementType, string nickName, Guid guid)
        {
            var node = (dynNodeModel)Activator.CreateInstance(elementType);

            if (!string.IsNullOrEmpty(nickName))
            {
                node.NickName = nickName;
            }
            else
            {
                var elNameAttrib =
                    node.GetType().GetCustomAttributes(typeof(NodeNameAttribute), true)[0] as NodeNameAttribute;
                if (elNameAttrib != null)
                {
                    node.NickName = elNameAttrib.Name;
                }
            }

            node.GUID = guid;

            //string name = nodeUI.NickName;
            return node;
        }

        /// <summary>
        ///     Change the currently visible workspace to the home workspace
        /// </summary>
        /// <param name="symbol">The function definition for the custom node workspace to be viewed</param>
        internal void ViewHomeWorkspace()
        {
            _model.CurrentSpace = _model.HomeSpace;
            _model.CurrentSpace.OnDisplayed();
        }

        /// <summary>
        ///     Change the currently visible workspace to a custom node's workspace
        /// </summary>
        /// <param name="symbol">The function definition for the custom node workspace to be viewed</param>
        internal void ViewCustomNodeWorkspace(FunctionDefinition symbol)
        {
            if (symbol == null)
            {
                throw new Exception("There is a null function definition for this node.");
            }

            if (_model.CurrentSpace.Name.Equals(symbol.Workspace.Name))
                return;

            dynWorkspaceModel newWs = symbol.Workspace;

            if ( !this._model.Workspaces.Contains(newWs) )
                this._model.Workspaces.Add(newWs);

            CurrentSpaceViewModel.OnStopDragging(this, EventArgs.Empty);

            _model.CurrentSpace = newWs;
            _model.CurrentSpace.OnDisplayed();

            //set the zoom and offsets events
            var vm = dynSettings.Controller.DynamoViewModel.Workspaces.First(x => x.Model == newWs);
            vm.OnCurrentOffsetChanged(this, new PointEventArgs(new Point(newWs.X, newWs.Y)));
            vm.OnZoomChanged(this, new ZoomEventArgs(newWs.Zoom));
        }

        public bool OpenWorkspace(string xmlPath)
        {
            Log("Opening home workspace " + xmlPath + "...");

            CleanWorkbench();

            //clear the renderables
            dynSettings.Controller.RenderDescriptions.Clear();
            dynSettings.Controller.OnRequestsRedraw(dynSettings.Controller, EventArgs.Empty);

            Stopwatch sw = new Stopwatch();

            try
            {
                #region read xml file

                sw.Start();

                var xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlPath);

                TimeSpan previousElapsed = sw.Elapsed;
                Log(string.Format("{0} elapsed for loading xml.", sw.Elapsed));

                double cx = DynamoView.CANVAS_OFFSET_X;
                double cy = DynamoView.CANVAS_OFFSET_Y;
                double zoom = 1.0;
                string description = "";

                foreach (XmlNode node in xmlDoc.GetElementsByTagName("dynWorkspace"))
                {
                    foreach (XmlAttribute att in node.Attributes)
                    {
                        if (att.Name.Equals("X"))
                        {
                            cx = double.Parse(att.Value, CultureInfo.InvariantCulture);
                        }
                        else if (att.Name.Equals("Y"))
                        {
                            cy = double.Parse(att.Value, CultureInfo.InvariantCulture);
                        }
                        else if (att.Name.Equals("zoom"))
                        {
                            zoom = double.Parse(att.Value, CultureInfo.InvariantCulture);
                        }
                        else if (att.Name.Equals("Description"))
                        {
                            description = att.Value;
                        }
                    }
                }

                //set the zoom and offsets and trigger events
                //to get the view to position iteself
                _model.CurrentSpace.X = cx;
                _model.CurrentSpace.Y = cy;
                _model.CurrentSpace.Zoom = zoom;
                _model.CurrentSpace.Description = description;

                var vm = dynSettings.Controller.DynamoViewModel.Workspaces.First(x => x.Model == _model.CurrentSpace);
                vm.OnCurrentOffsetChanged(this, new PointEventArgs(new Point(cx, cy)));

                XmlNodeList elNodes = xmlDoc.GetElementsByTagName("dynElements");
                XmlNodeList cNodes = xmlDoc.GetElementsByTagName("dynConnectors");
                XmlNodeList nNodes = xmlDoc.GetElementsByTagName("dynNotes");

                XmlNode elNodesList = elNodes[0];
                XmlNode cNodesList = cNodes[0];
                XmlNode nNodesList = nNodes[0];

                //if there is any problem loading a node, then
                //add the node's guid to the bad nodes collection
                //so we can avoid attempting to make connections to it
                List<Guid> badNodes = new List<Guid>();

                foreach (XmlNode elNode in elNodesList.ChildNodes)
                {
                    XmlAttribute typeAttrib = elNode.Attributes["type"];
                    XmlAttribute guidAttrib = elNode.Attributes["guid"];
                    XmlAttribute nicknameAttrib = elNode.Attributes["nickname"];
                    XmlAttribute xAttrib = elNode.Attributes["x"];
                    XmlAttribute yAttrib = elNode.Attributes["y"];
                    XmlAttribute isVisAttrib = elNode.Attributes["isVisible"];
                    XmlAttribute isUpstreamVisAttrib = elNode.Attributes["isUpstreamVisible"];
                    XmlAttribute lacingAttrib = elNode.Attributes["lacing"];

                    string typeName = typeAttrib.Value;

                    //test the GUID to confirm that it is non-zero
                    //if it is zero, then we have to fix it
                    //this will break the connectors, but it won't keep
                    //propagating bad GUIDs
                    var guid = new Guid(guidAttrib.Value);
                    if (guid == Guid.Empty)
                    {
                        guid = Guid.NewGuid();
                    }

                    string nickname = nicknameAttrib.Value;

                    double x = double.Parse(xAttrib.Value, CultureInfo.InvariantCulture);
                    double y = double.Parse(yAttrib.Value, CultureInfo.InvariantCulture);

                    if (typeName.StartsWith("Dynamo.Elements."))
                        typeName = "Dynamo.Nodes." + typeName.Remove(0, 16);

                    TypeLoadData tData;
                    Type t;

                    if (!Controller.BuiltInTypesByName.TryGetValue(typeName, out tData))
                    {
                        //try and get a system type by this name
                        t = Type.GetType(typeName);

                        //if we still can't find the type, try the also known as attributes
                        if (t == null)
                        {
                            //try to get the also known as values
                            foreach (KeyValuePair<string, TypeLoadData> kvp in Controller.BuiltInTypesByName)
                            {
                                var akaAttribs = kvp.Value.Type.GetCustomAttributes(typeof(AlsoKnownAsAttribute), false);
                                if (akaAttribs.Count() > 0)
                                {
                                    if ((akaAttribs[0] as AlsoKnownAsAttribute).Values.Contains(typeName))
                                    {
                                        Log(string.Format("Found matching node for {0} also known as {1}", kvp.Key, typeName));
                                        t = kvp.Value.Type;
                                    }
                                }
                            }
                        }

                        if (t == null)
                        {
                            Log("Could not load node of type: " + typeName);
                            Log("Loading will continue but nodes might be missing from your workflow.");

                            //return false;
                            badNodes.Add(guid);
                            continue;
                        }
                    }
                    else
                        t = tData.Type;

                    bool isVisible = true;
                    if (isVisAttrib != null)
                        isVisible = isVisAttrib.Value == "true" ? true : false;

                    bool isUpstreamVisible = true;
                    if (isUpstreamVisAttrib != null)
                        isUpstreamVisible = isUpstreamVisAttrib.Value == "true" ? true : false;

                    dynNodeModel el = CreateNodeInstance(t, nickname, guid);
                    el.WorkSpace = _model.CurrentSpace;
                    el.Load(elNode);

                    _model.CurrentSpace.Nodes.Add(el);

                    el.X = x;
                    el.Y = y;

                    el.IsVisible = isVisible;
                    el.IsUpstreamVisible = isUpstreamVisible;

                    if (lacingAttrib != null)
                    {
                        if (el.ArgumentLacing != LacingStrategy.Disabled)
                        {
                            LacingStrategy lacing = LacingStrategy.Disabled;
                            Enum.TryParse(lacingAttrib.Value, out lacing);
                            el.ArgumentLacing = lacing;
                        }
                    }

                    el.DisableReporting();

                    if (ViewingHomespace)
                        el.SaveResult = true;

                    //el.LoadNode(elNode);
                }

                Log(string.Format("{0} ellapsed for loading nodes.", sw.Elapsed - previousElapsed));
                previousElapsed = sw.Elapsed;

                OnRequestLayoutUpdate(this, EventArgs.Empty);
                Log(string.Format("{0} ellapsed for updating layout.", sw.Elapsed - previousElapsed));
                previousElapsed = sw.Elapsed;

                foreach (XmlNode connector in cNodesList.ChildNodes)
                {
                    XmlAttribute guidStartAttrib = connector.Attributes[0];
                    XmlAttribute intStartAttrib = connector.Attributes[1];
                    XmlAttribute guidEndAttrib = connector.Attributes[2];
                    XmlAttribute intEndAttrib = connector.Attributes[3];
                    XmlAttribute portTypeAttrib = connector.Attributes[4];

                    var guidStart = new Guid(guidStartAttrib.Value);
                    var guidEnd = new Guid(guidEndAttrib.Value);
                    int startIndex = Convert.ToInt16(intStartAttrib.Value);
                    int endIndex = Convert.ToInt16(intEndAttrib.Value);
                    int portType = Convert.ToInt16(portTypeAttrib.Value);

                    //find the elements to connect
                    dynNodeModel start = null;
                    dynNodeModel end = null;

                    if (badNodes.Contains(guidStart) || badNodes.Contains(guidEnd))
                        continue;

                    foreach (dynNodeModel e in _model.Nodes)
                    {
                        if (e.GUID == guidStart)
                        {
                            start = e;
                        }
                        else if (e.GUID == guidEnd)
                        {
                            end = e;
                        }
                        if (start != null && end != null)
                        {
                            break;
                        }
                    }

                    var newConnector = dynConnectorModel.Make(start, end,
                                                        startIndex, endIndex, portType);

                    Stopwatch addTimer = new Stopwatch();
                    addTimer.Start();
                    if (newConnector != null)
                        _model.CurrentSpace.Connectors.Add(newConnector);
                    addTimer.Stop();
                    Debug.WriteLine(string.Format("{0} elapsed for add connector to collection.", addTimer.Elapsed));

                }

                Log(string.Format("{0} ellapsed for loading connectors.", sw.Elapsed - previousElapsed));
                previousElapsed = sw.Elapsed;

                #region instantiate notes

                if (nNodesList != null)
                {
                    foreach (XmlNode note in nNodesList.ChildNodes)
                    {
                        XmlAttribute textAttrib = note.Attributes[0];
                        XmlAttribute xAttrib = note.Attributes[1];
                        XmlAttribute yAttrib = note.Attributes[2];

                        string text = textAttrib.Value;
                        double x = double.Parse(xAttrib.Value, CultureInfo.InvariantCulture);
                        double y = double.Parse(yAttrib.Value, CultureInfo.InvariantCulture);

                        //dynNoteView n = Bench.AddNote(text, x, y, this.CurrentSpace);
                        //Bench.AddNote(text, x, y, this.CurrentSpace);

                        var paramDict = new Dictionary<string, object>();
                        paramDict.Add("x", x);
                        paramDict.Add("y", y);
                        paramDict.Add("text", text);
                        paramDict.Add("workspace", _model.CurrentSpace);
                        AddNoteCommand.Execute(paramDict);
                    }
                }

                #endregion

                Log(string.Format("{0} ellapsed for loading notes.", sw.Elapsed - previousElapsed));

                foreach (dynNodeModel e in _model.CurrentSpace.Nodes)
                    e.EnableReporting();

                #endregion

                _model.HomeSpace.FilePath = xmlPath;

                Log(string.Format("{0} ellapsed for loading workspace.", sw.Elapsed));
            }
            catch (Exception ex)
            {
                Log("There was an error opening the workbench.");
                Log(ex);
                Debug.WriteLine(ex.Message + ":" + ex.StackTrace);
                CleanWorkbench();
                return false;
            }
            return true;
        }

        internal void CleanWorkbench()
        {
            Log("Clearing workflow...");

            //Copy locally
            List<dynNodeModel> elements = _model.Nodes.ToList();

            foreach (dynNodeModel el in elements)
            {
                el.DisableReporting();
                try
                {
                    el.Destroy();
                }
                catch
                {
                }
            }

            foreach (dynNodeModel el in elements)
            {
                foreach (dynPortModel p in el.InPorts)
                {
                    for (int i = p.Connectors.Count - 1; i >= 0; i--)
                        p.Connectors[i].NotifyConnectedPortsOfDeletion();
                }
                foreach (dynPortModel port in el.OutPorts)
                {
                    for (int i = port.Connectors.Count - 1; i >= 0; i--)
                        port.Connectors[i].NotifyConnectedPortsOfDeletion();
                }
            }

            _model.CurrentSpace.Connectors.Clear();
            _model.CurrentSpace.Nodes.Clear();
            _model.CurrentSpace.Notes.Clear();
        }

        internal FunctionDefinition NewFunction(Guid id,
                                                string name,
                                                string category,
                                                string description,
                                                bool display,
                                                double workspaceOffsetX = DynamoView.CANVAS_OFFSET_X,
                                                double workspaceOffsetY = DynamoView.CANVAS_OFFSET_Y)
        {
            //Add an entry to the funcdict
            var workSpace = new FuncWorkspace(
                name, category, description, workspaceOffsetX, workspaceOffsetY)
                {
                    WatchChanges = true
                };

            _model.Workspaces.Add(workSpace);

            List<dynNodeModel> newElements = workSpace.Nodes.ToList();
            List<dynConnectorModel> newConnectors = workSpace.Connectors.ToList();

            var functionDefinition = new FunctionDefinition(id)
            {
                Workspace = workSpace
            };

            Controller.CustomNodeManager.AddFunctionDefinition(functionDefinition.FunctionId, functionDefinition);

            // add the element to search
            Controller.SearchViewModel.Add(name, category, description, id);

            if (display)
            {
                if (!ViewingHomespace)
                {
                    var def = Controller.CustomNodeManager.GetDefinitionFromWorkspace(CurrentSpace);
                    if (def != null)
                        SaveFunction(def);
                }

                _model.CurrentSpace = workSpace;
            }

            return functionDefinition;
        }

        public virtual dynFunction CreateFunction(IEnumerable<string> inputs, IEnumerable<string> outputs,
                                                     FunctionDefinition functionDefinition)
        {
            return new dynFunction(inputs, outputs, functionDefinition);
        }

        internal dynNodeModel CreateNode(string name)
        {
            dynNodeModel result;

            if (Controller.BuiltInTypesByName.ContainsKey(name))
            {
                TypeLoadData tld = Controller.BuiltInTypesByName[name];

                ObjectHandle obj = Activator.CreateInstanceFrom(tld.Assembly.Location, tld.Type.FullName);
                var newEl = (dynNodeModel)obj.Unwrap();
                newEl.DisableInteraction();
                result = newEl;
            }
            else if (Controller.BuiltInTypesByNickname.ContainsKey(name))
            {
                TypeLoadData tld = Controller.BuiltInTypesByNickname[name];
                try
                {

                    ObjectHandle obj = Activator.CreateInstanceFrom(tld.Assembly.Location, tld.Type.FullName);
                    var newEl = (dynNodeModel)obj.Unwrap();
                    newEl.DisableInteraction();
                    result = newEl;
                }
                catch (Exception ex)
                {
                    Log("Failed to load built-in type");
                    Log(ex);
                    result = null;
                }
            }
            else
            {
                dynFunction func;

                if (Controller.CustomNodeManager.GetNodeInstance(Controller, Guid.Parse(name), out func))
                {
                    result = func;
                }
                else
                {
                    Log("Failed to find FunctionDefinition.");
                    return null;
                }
            }

            return result;
        }


        /// <summary>
        ///     Sets the load path
        /// </summary>
        internal void QueueLoad(string path)
        {
            UnlockLoadPath = path;
        }

        internal void ShowElement(dynNodeModel e)
        {
            if (dynamicRun)
                return;

            if (!_model.Nodes.Contains(e))
            {
                if (_model.HomeSpace != null && _model.HomeSpace.Nodes.Contains(e))
                {
                    //Show the homespace
                    ViewHomeWorkspace();
                }
                else
                {
                    foreach (FunctionDefinition funcDef in Controller.CustomNodeManager.GetLoadedDefinitions())
                    {
                        if (funcDef.Workspace.Nodes.Contains(e))
                        {
                            ViewCustomNodeWorkspace(funcDef);
                            break;
                        }
                    }
                }
            }

            dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.OnRequestCenterViewOnElement(this, new ModelEventArgs(e,null));
            
        }

        private void ShowConnectors()
        {
            if (isShowingConnectors == false)
                isShowingConnectors = true;
        }

        private bool CanShowConnectors()
        {
            return true;
        }

        private void ToggleFullscreenWatchShowing()
        {
            if (FullscreenWatchShowing)
            {
                //delete the watches
                foreach (dynWorkspaceViewModel vm in dynSettings.Controller.DynamoViewModel.Workspaces)
                {
                    vm.Watch3DViewModels.Clear();
                }

                FullscreenWatchShowing = false;
            }
            else
            {
                //construct a watch
                foreach (dynWorkspaceViewModel vm in dynSettings.Controller.DynamoViewModel.Workspaces)
                {
                    vm.Watch3DViewModels.Add(new Watch3DFullscreenViewModel(vm));
                }

                //run the expression to refresh
                if (dynSettings.Controller.IsProcessingCommandQueue)
                    return;

                dynSettings.Controller.RunCommand(dynSettings.Controller.DynamoViewModel.RunExpressionCommand, null);

                FullscreenWatchShowing = true;
            }
        }

        private bool CanToggleFullscreenWatchShowing()
        {
            return true;
        }

        private void ToggleCanNavigateBackground()
        {
            if (!FullscreenWatchShowing)
                return;

            if (CanNavigateBackground)
            {
                CanNavigateBackground = false;
            }
            else
            {
                CanNavigateBackground = true;
            }
        }

        private bool CanToggleCanNavigateBackground()
        {
            return true;
        }

        /// <summary>
        /// Resets the offset and the zoom for a view
        /// </summary>
        private void GoHomeView()
        {
            _model.CurrentSpace.Zoom = 1.0;
            var wsvm = dynSettings.Controller.DynamoViewModel.Workspaces.First(x => x.Model == _model.CurrentSpace);
            wsvm.OnCurrentOffsetChanged(this, new PointEventArgs(new Point(0,0)));
        }

        private bool CanGoHomeView()
        {
            return true;
        }
    }

    public class TypeLoadData
    {
        public Assembly Assembly;
        public Type Type;

        public TypeLoadData(Assembly assemblyIn, Type typeIn)
        {
            Assembly = assemblyIn;
            Type = typeIn;
        }
    }

    public class PointEventArgs:EventArgs
    {
        public Point Point { get; set; }

        public PointEventArgs(Point p)
        {
            Point = p;
        }
    }

    public class ZoomEventArgs : EventArgs
    {
        public double Zoom { get; set; }

        public ZoomEventArgs(double zoom)
        {
            Zoom = zoom;
        }
    }

    public class ModelEventArgs : EventArgs
    {
        public dynModelBase Model { get; set; }
        public Dictionary<string, object> Data { get; set; }
        public ModelEventArgs(dynModelBase n, Dictionary<string, object> d)
        {
            Model = n;
            Data = d;
        }
    }

    public class NoteEventArgs : EventArgs
    {
        public dynNoteModel Note { get; set; }
        public Dictionary<string, object> Data { get; set; }
        public NoteEventArgs(dynNoteModel n, Dictionary<string, object> d)
        {
            Note = n;
            Data = d;
        }
    }

    public class ViewEventArgs : EventArgs
    {
        public System.Windows.Controls.UserControl View { get; set; }

        public ViewEventArgs(System.Windows.Controls.UserControl v)
        {
            View = v;
        }
    }

}
