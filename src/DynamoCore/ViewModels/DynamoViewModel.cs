using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.PackageManager;
using Dynamo.Selection;
using Dynamo.UI.Commands;
using Dynamo.Utilities;

namespace Dynamo.ViewModels
{

    public delegate void WorkspaceSaveEventHandler(object sender, WorkspaceSaveEventArgs e);
    public delegate void RequestPackagePublishDialogHandler(PublishPackageViewModel publishViewModel);

    public class DynamoViewModel:ViewModelBase
    {
        #region events


        public event EventHandler RequestManagePackagesDialog;
        public virtual void OnRequestManagePackagesDialog(Object sender, EventArgs e)
        {
            if (RequestManagePackagesDialog != null)
            {
                RequestManagePackagesDialog(this, e);
            }
        }

        public event RequestPackagePublishDialogHandler RequestPackagePublishDialog;
        public void OnRequestPackagePublishDialog(PublishPackageViewModel vm)
        {
            if (RequestPackagePublishDialog != null)
                RequestPackagePublishDialog(vm);
        }

        public event EventHandler RequestPackageManagerSearchDialog;
        public virtual void OnRequestPackageManagerSearchDialog(Object sender, EventArgs e)
        {
            if (RequestPackageManagerSearchDialog != null)
            {
                RequestPackageManagerSearchDialog(this, e);
            }
        }

        public event AuthenticationRequestHandler RequestAuthentication;
        public void OnRequestAuthentication()
        {
            if (RequestAuthentication != null)
                RequestAuthentication(dynSettings.PackageManagerClient);
        }

        public event ImageSaveEventHandler RequestSaveImage;
        public virtual void OnRequestSaveImage(Object sender, ImageSaveEventArgs e)
        {
            if (RequestSaveImage != null)
            {
                RequestSaveImage(this, e);
            }
        }

        public event EventHandler WorkspaceChanged;
        public virtual void OnWorkspaceChanged(object sender, EventArgs e)
        {
            if (WorkspaceChanged != null)
            {
                WorkspaceChanged(this, e);
            }
        }

        public event EventHandler RequestClose;
        public virtual void OnRequestClose(Object sender, EventArgs e)
        {
            if (RequestClose != null)
            {
                RequestClose(this, e);
            }
        }

        public event WorkspaceSaveEventHandler RequestUserSaveWorkflow;
        public virtual void OnRequestUserSaveWorkflow(Object sender, WorkspaceSaveEventArgs e)
        {
            if (RequestUserSaveWorkflow != null)
            {
                RequestUserSaveWorkflow(this, e);
            }
        }

        #endregion

        #region properties

        private DynamoModel _model;        
        private Point transformOrigin;
        private DynamoController controller;
        private bool runEnabled = true;
        protected bool canRunDynamically = true;
        protected bool debug = false;
        protected bool dynamicRun = false;
        
        private bool fullscreenWatchShowing = false;
        private bool canNavigateBackground = false;

        public DelegateCommand OpenCommand { get; set; }
        public DelegateCommand ShowOpenDialogAndOpenResultCommand { get; set; }
        public DelegateCommand WriteToLogCmd { get; set; }
        public DelegateCommand PostUiActivationCommand { get; set; }
        public DelegateCommand AddNoteCommand { get; set; }
        public DelegateCommand LayoutAllCommand { get; set; }
        public DelegateCommand CopyCommand { get; set; }
        public DelegateCommand PasteCommand { get; set; }
        public DelegateCommand AddToSelectionCommand { get; set; }
        public DelegateCommand ShowNewFunctionDialogCommand { get; set; }
        public DelegateCommand CreateNodeCommand { get; set; }
        public DelegateCommand CreateConnectionCommand { get; set; }
        public DelegateCommand ClearCommand { get; set; }
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
        public DelegateCommand GoToWorkspaceCommand { get; set; }
        public DelegateCommand DeleteCommand { get; set; }
        public DelegateCommand AlignSelectedCommand { get; set; }
        public DelegateCommand RefactorCustomNodeCommand { get; set; }
        public DelegateCommand PostUIActivationCommand { get; set; }
        public DelegateCommand ToggleFullscreenWatchShowingCommand { get; set; }
        public DelegateCommand ToggleCanNavigateBackgroundCommand { get; set; }
        public DelegateCommand SelectAllCommand { get; set; }
        public DelegateCommand SaveImageCommand { get; set; }
        public DelegateCommand ShowSaveImageDialogAndSaveResultCommand { get; set; }
        public DelegateCommand ToggleConsoleShowingCommand { get; set; }
        public DelegateCommand ShowPackageManagerCommand { get; set; }
        public DelegateCommand CancelRunCommand { get; set; }
        public DelegateCommand RunExpressionCommand { get; set; }
        public DelegateCommand DisplayFunctionCommand { get; set; }
        public DelegateCommand SetConnectorTypeCommand { get; set; }
        public DelegateCommand ReportABugCommand { get; set; }
        public DelegateCommand GoToWikiCommand { get; set; }
        public DelegateCommand GoToSourceCodeCommand { get; set; }
        public DelegateCommand ShowHideConnectorsCommand { get; set; }
        public DelegateCommand SelectNeighborsCommand { get; set; }
        public DelegateCommand ClearLogCommand { get; set; }
        public DelegateCommand SubmitCommand { get; set; }
        public DelegateCommand PublishCurrentWorkspaceCommand { get; set; }
        public DelegateCommand PublishSelectedNodesCommand { get; set; }

        public DelegateCommand PanCommand { get; set; }

        /// <summary>
        /// An observable collection of workspace view models which tracks the model
        /// </summary>
        private ObservableCollection<WorkspaceViewModel> _workspaces = new ObservableCollection<WorkspaceViewModel>();

        public ObservableCollection<WorkspaceViewModel> Workspaces
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

        public Point TransformOrigin
        {
            get { return transformOrigin; }
            set
            {
                transformOrigin = value;
                RaisePropertyChanged("TransformOrigin");
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
            get { return _model.CurrentWorkspace == _model.HomeSpace; }
        }

        public bool IsAbleToGoHome { get; set; }

        public WorkspaceModel CurrentSpace
        {
            get { return _model.CurrentWorkspace; }
        }

        /// <summary>
        /// The index in the collection of workspaces of the current workspace.
        /// This property is bound to the SelectedIndex property in the workspaces tab control
        /// </summary>
        public int CurrentWorkspaceIndex
        {
            get
            {
                var index = _model.Workspaces.IndexOf(_model.CurrentWorkspace);
                return index;
            }
            set
            {
                _model.CurrentWorkspace = _model.Workspaces[value];
            }
        }

        /// <summary>
        /// Get the workspace view model whose workspace model is the model's current workspace
        /// </summary>
        public WorkspaceViewModel CurrentSpaceViewModel
        {
            get
            {
                return Workspaces.First(x => x.Model == _model.CurrentWorkspace);
            }
        }

        public string EditName
        {
            get { return _model.editName; }
            set 
            { 
                _model.editName = value;
                RaisePropertyChanged("EditName");
            }
        }

        public bool IsUILocked
        {
            get { return dynSettings.Controller.IsUILocked; }
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
                foreach (WorkspaceViewModel workspace in dynSettings.Controller.DynamoViewModel.Workspaces)
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

                WorkspaceViewModel view_model = Workspaces[workspace_index];

                view_model.WatchEscapeIsDown = value;
            }
        }

        private bool _consoleShowing;

        public string LogText
        {
            get { return DynamoLogger.Instance.LogText; }
        }

        public bool ConsoleShowing
        {
            get { return _consoleShowing; }
            set
            {
                _consoleShowing = value;
                RaisePropertyChanged("ConsoleShowing");
            }
        }

        public bool IsShowingConnectors
        {
            get { return dynSettings.Controller.IsShowingConnectors; }
            set
            {
                dynSettings.Controller.IsShowingConnectors = value;
                RaisePropertyChanged("IsShowingConnectors");
            }
        }

        public ConnectorType ConnectorType
        {
            get { return dynSettings.Controller.ConnectorType; }
            set
            {
                dynSettings.Controller.ConnectorType = value;
                RaisePropertyChanged("ConnectorType");
            }
        }
        
        #endregion

        public DynamoViewModel(DynamoController controller)
        {
            ConnectorType = ConnectorType.BEZIER;
            
            //create the model
            _model = new DynamoModel();
            dynSettings.Controller.DynamoModel = _model;

            //register for property change notifications 
            //on the model and the controller
            _model.PropertyChanged += _model_PropertyChanged;
            dynSettings.Controller.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Controller_PropertyChanged);
            _model.Workspaces.CollectionChanged += Workspaces_CollectionChanged;

            _model.AddHomeWorkspace();
            _model.CurrentWorkspace = _model.HomeSpace;

            Controller = controller;

            OpenCommand = new DelegateCommand(_model.Open, _model.CanOpen);
            ShowOpenDialogAndOpenResultCommand = new DelegateCommand(_model.ShowOpenDialogAndOpenResult, _model.CanShowOpenDialogAndOpenResultCommand);
            WriteToLogCmd = new DelegateCommand(_model.WriteToLog, _model.CanWriteToLog);
            PostUiActivationCommand = new DelegateCommand(_model.PostUIActivation, _model.CanDoPostUIActivation);
            AddNoteCommand = new DelegateCommand(_model.AddNote, _model.CanAddNote);
            LayoutAllCommand = new DelegateCommand(_model.LayoutAll, _model.CanLayoutAll);
            AddToSelectionCommand = new DelegateCommand(_model.AddToSelection, _model.CanAddToSelection);
            ShowNewFunctionDialogCommand = new DelegateCommand(_model.ShowNewFunctionDialogAndMakeFunction, _model.CanShowNewFunctionDialogCommand);
            CreateNodeCommand = new DelegateCommand(_model.CreateNode, _model.CanCreateNode);
            CreateConnectionCommand = new DelegateCommand(_model.CreateConnection, _model.CanCreateConnection);
            ClearCommand = new DelegateCommand(_model.Clear, _model.CanClear);
            GoHomeCommand = new DelegateCommand(GoHomeView, CanGoHomeView);
            SelectAllCommand = new DelegateCommand(SelectAll, CanSelectAll);
            ShowSaveDialogAndSaveResultCommand = new DelegateCommand(ShowSaveDialogAndSaveResult, CanShowSaveDialogAndSaveResult);
            SaveCommand = new DelegateCommand(_model.Save, _model.CanSave);
            SaveAsCommand = new DelegateCommand(_model.SaveAs, _model.CanSaveAs);
            HomeCommand = new DelegateCommand(_model.Home, _model.CanGoHome);
            NewHomeWorkspaceCommand = new DelegateCommand(MakeNewHomeWorkspace, CanMakeNewHomeWorkspace);
            GoToWorkspaceCommand = new DelegateCommand(GoToWorkspace, CanGoToWorkspace);
            DeleteCommand = new DelegateCommand(_model.Delete, _model.CanDelete);
            ExitCommand = new DelegateCommand(Exit,CanExit);
            ToggleFullscreenWatchShowingCommand = new DelegateCommand(ToggleFullscreenWatchShowing, CanToggleFullscreenWatchShowing);
            ToggleCanNavigateBackgroundCommand = new DelegateCommand(ToggleCanNavigateBackground, CanToggleCanNavigateBackground);
            AlignSelectedCommand = new DelegateCommand(AlignSelected, CanAlignSelected); ;
            ShowSaveDialogIfNeededAndSaveResultCommand = new DelegateCommand(ShowSaveDialogIfNeededAndSaveResult, CanShowSaveDialogIfNeededAndSaveResultCommand);
            RefactorCustomNodeCommand = new DelegateCommand(_model.RefactorCustomNode, _model.CanRefactorCustomNode);
            SaveImageCommand = new DelegateCommand(SaveImage, CanSaveImage);
            ShowSaveImageDialogAndSaveResultCommand = new DelegateCommand(ShowSaveImageDialogAndSaveResult, CanShowSaveImageDialogAndSaveResult);
            CopyCommand = new DelegateCommand(_model.Copy, _model.CanCopy);
            PasteCommand = new DelegateCommand(_model.Paste, _model.CanPaste);
            ToggleConsoleShowingCommand = new DelegateCommand(ToggleConsoleShowing, CanToggleConsoleShowing);
            CancelRunCommand = new DelegateCommand(Controller.CancelRun, Controller.CanCancelRun);
            RunExpressionCommand = new DelegateCommand(Controller.RunExpression, Controller.CanRunExpression);
            DisplayFunctionCommand = new DelegateCommand(Controller.DisplayFunction, Controller.CanDisplayFunction);
            SetConnectorTypeCommand = new DelegateCommand(SetConnectorType, CanSetConnectorType);
            ReportABugCommand = new DelegateCommand(Controller.ReportABug, Controller.CanReportABug);
            GoToWikiCommand = new DelegateCommand(GoToWiki, CanGoToWiki);
            GoToSourceCodeCommand = new DelegateCommand(GoToSourceCode, CanGoToSourceCode);

            ShowPackageManagerSearchCommand = new DelegateCommand(ShowPackageManagerSearch, CanShowPackageManagerSearch);
            ShowInstalledPackagesCommand = new DelegateCommand(ShowInstalledPackages, CanShowInstalledPackages);
            PublishCurrentWorkspaceCommand = new DelegateCommand(PublishCurrentWorkspace, CanPublishCurrentWorkspace);
            PublishSelectedNodesCommand = new DelegateCommand(PublishSelectedNodes, CanPublishSelectedNodes);

            ShowHideConnectorsCommand = new DelegateCommand(ShowConnectors, CanShowConnectors);
            SelectNeighborsCommand = new DelegateCommand(SelectNeighbors, CanSelectNeighbors);
            ClearLogCommand = new DelegateCommand(dynSettings.Controller.ClearLog, dynSettings.Controller.CanClearLog);
            PanCommand = new DelegateCommand(Pan, CanPan);

            DynamoLogger.Instance.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Instance_PropertyChanged);

            DynamoSelection.Instance.Selection.CollectionChanged += SelectionOnCollectionChanged;
        }

        private void SelectionOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            PublishSelectedNodesCommand.RaiseCanExecuteChanged();
            AlignSelectedCommand.RaiseCanExecuteChanged();
        }

        void Controller_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsUILocked":
                    RaisePropertyChanged("IsUILocked");
                    break;
            }
        }

        void Instance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

            switch (e.PropertyName)
            {
                case "LogText":
                    RaisePropertyChanged("LogText");
                    break;
            }

        }

        void _model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentWorkspace")
            {
                IsAbleToGoHome = _model.CurrentWorkspace != _model.HomeSpace;
                RaisePropertyChanged("IsAbleToGoHome");
                RaisePropertyChanged("CurrentSpace");
                RaisePropertyChanged("BackgroundColor");
                RaisePropertyChanged("CurrentWorkspaceIndex");
                RaisePropertyChanged("ViewingHomespace");
                if (this.PublishCurrentWorkspaceCommand != null)
                this.PublishCurrentWorkspaceCommand.RaiseCanExecuteChanged();
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
                        _workspaces.Add(new WorkspaceViewModel(item as WorkspaceModel, this));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                        _workspaces.Remove(_workspaces.ToList().First(x => x.Model == item));
                    break;
            }

            RaisePropertyChanged("Workspaces");
        }

        public FileDialog GetSaveDialog(WorkspaceModel workspace)
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

        /// <summary>
        ///     Attempts to save a given workspace.  Shows a save as dialog if the 
        ///     workspace does not already have a path associated with it
        /// </summary>
        /// <param name="workspace">The workspace for which to show the dialog</param>
        internal void ShowSaveDialogIfNeededAndSave(WorkspaceModel workspace)
        {
            if (workspace.FilePath != null)
            {
                _model.SaveAs(workspace.FilePath, workspace);
            }
            else
            {
                var fd = this.GetSaveDialog(workspace);
                if (fd.ShowDialog() == DialogResult.OK)
                {
                    _model.SaveAs(fd.FileName, workspace);
                }
            }
        }

        public bool exitInvoked = false;
        
        internal bool CanVisibilityBeToggled(object parameters)
        {
            return true;
        }

        internal bool CanUpstreamVisibilityBeToggled(object parameters)
        {
            return true;
        }

        private void PublishCurrentWorkspace(object parameters)
        {
            dynSettings.PackageManagerClient.PublishCurrentWorkspace();
        }

        private bool CanPublishCurrentWorkspace(object parameters)
        {
            return dynSettings.PackageManagerClient.CanPublishCurrentWorkspace();
        }

        private void PublishSelectedNodes(object parameters)
        {
            dynSettings.PackageManagerClient.PublishSelectedNode();
        }

        private bool CanPublishSelectedNodes(object parameters)
        {
            return dynSettings.PackageManagerClient.CanPublishSelectedNode(parameters);
        }

        private void ShowPackageManagerSearch(object parameters)
        {
            OnRequestPackageManagerSearchDialog(this, EventArgs.Empty);
        }

        private bool CanShowPackageManagerSearch(object parameters)
        {
            return true;
        }

        private void ShowInstalledPackages(object parameters)
        {
            OnRequestManagePackagesDialog(this, EventArgs.Empty);
        }

        private bool CanShowInstalledPackages(object parameters)
        {
            return true;
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
            WorkspaceModel functionWorkspace = definition.Workspace;

            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string pluginsPath = Path.Combine(directory, "definitions");

            try
            {
                if (!Directory.Exists(pluginsPath))
                    Directory.CreateDirectory(pluginsPath);

                string path = Path.Combine(pluginsPath, dynSettings.FormatFileName(functionWorkspace.Name) + ".dyf");
                WorkspaceModel.SaveWorkspace(path, functionWorkspace);
                return path;
            }
            catch (Exception e)
            {
                DynamoLogger.Instance.Log("Error saving:" + e.GetType());
                DynamoLogger.Instance.Log(e);
                return "";
            }

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

            if (_model.CurrentWorkspace.Name.Equals(symbol.Workspace.Name))
                return;

            WorkspaceModel newWs = symbol.Workspace;

            if ( !this._model.Workspaces.Contains(newWs) )
                this._model.Workspaces.Add(newWs);

            CurrentSpaceViewModel.OnStopDragging(this, EventArgs.Empty);

            _model.CurrentWorkspace = newWs;
            _model.CurrentWorkspace.OnDisplayed();

            //set the zoom and offsets events
            var vm = dynSettings.Controller.DynamoViewModel.Workspaces.First(x => x.Model == newWs);
            vm.OnCurrentOffsetChanged(this, new PointEventArgs(new Point(newWs.X, newWs.Y)));
            vm.OnZoomChanged(this, new ZoomEventArgs(newWs.Zoom));
        }

        public virtual Function CreateFunction(IEnumerable<string> inputs, IEnumerable<string> outputs,
                                                     FunctionDefinition functionDefinition)
        {
            return new Function(inputs, outputs, functionDefinition);
        }


        /// <summary>
        ///     Sets the load path
        /// </summary>
        internal void QueueLoad(string path)
        {
            _model.UnlockLoadPath = path;
        }

        internal void ShowElement(NodeModel e)
        {
            if (dynamicRun)
                return;

            if (!_model.Nodes.Contains(e))
            {
                if (_model.HomeSpace != null && _model.HomeSpace.Nodes.Contains(e))
                {
                    //Show the homespace
                    _model.ViewHomeWorkspace();
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
        
        public void ShowSaveDialogIfNeededAndSaveResult(object parameter)
        {
            var vm = dynSettings.Controller.DynamoViewModel;

            if (vm.Model.CurrentWorkspace.FilePath != null)
            {
                if(_model.CanSave(parameter))
                    _model.Save(parameter);
            }
            else
            {
                if (CanShowSaveDialogAndSaveResult(parameter))
                    ShowSaveDialogAndSaveResult(parameter);
            }
        }

        internal bool CanShowSaveDialogIfNeededAndSaveResultCommand(object parameter)
        {
            return true;
        }

        public void ShowSaveDialogAndSaveResult(object parameter)
        {
            var vm = dynSettings.Controller.DynamoViewModel;

            FileDialog _fileDialog = vm.GetSaveDialog(vm.Model.CurrentWorkspace);

            //if the xmlPath is not empty set the default directory
            if (!string.IsNullOrEmpty(vm.Model.CurrentWorkspace.FilePath))
            {
                var fi = new FileInfo(vm.Model.CurrentWorkspace.FilePath);
                _fileDialog.InitialDirectory = fi.DirectoryName;
                _fileDialog.FileName = fi.Name;
            }
            else if (vm.Model.CurrentWorkspace is FuncWorkspace && dynSettings.Controller.CustomNodeManager.SearchPath.Any())
            {
                _fileDialog.InitialDirectory = dynSettings.Controller.CustomNodeManager.SearchPath[0];
            }

            if (_fileDialog.ShowDialog() == DialogResult.OK)
            {
                _model.SaveAs(_fileDialog.FileName);
            }
        }

        internal bool CanShowSaveDialogAndSaveResult(object parameter)
        {
            return true;
        }
        
        public void ToggleCanNavigateBackground(object parameter)
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

        internal bool CanToggleCanNavigateBackground(object parameter)
        {
            return true;
        }

        public void ToggleFullscreenWatchShowing(object parameter)
        {
            if (FullscreenWatchShowing)
            {
                //delete the watches
                foreach (WorkspaceViewModel vm in dynSettings.Controller.DynamoViewModel.Workspaces)
                {
                    vm.Watch3DViewModels.Clear();
                }

                FullscreenWatchShowing = false;
            }
            else
            {
                //construct a watch
                foreach (WorkspaceViewModel vm in dynSettings.Controller.DynamoViewModel.Workspaces)
                {
                    vm.Watch3DViewModels.Add(new Watch3DFullscreenViewModel(vm));
                }

                //removed because we can reference the commands from here
                //and also because this behavior was not great. instead we'll
                //request just a redraw

                //run the expression to refresh
                //if (DynamoCommands.IsProcessingCommandQueue)
                //    return;

                //dynSettings.Controller.RunCommand(dynSettings.Controller.DynamoViewModel.RunExpressionCommand, null);
                //RunExpression(null);

                dynSettings.Controller.OnRequestsRedraw(this, EventArgs.Empty);

                FullscreenWatchShowing = true;
            }
        }

        internal bool CanToggleFullscreenWatchShowing(object parameter)
        {
            return true;
        }

        public void GoToWorkspace(object parameter)
        {
            if (parameter is Guid && dynSettings.Controller.CustomNodeManager.Contains((Guid)parameter))
            {
                ViewCustomNodeWorkspace(dynSettings.Controller.CustomNodeManager.GetFunctionDefinition((Guid)parameter));
            }
        }

        internal bool CanGoToWorkspace(object parameter)
        {
            return true;
        }

        public void AlignSelected(object param)
        {
            //this.CurrentSpaceViewModel.AlignSelectedCommand.Execute(param);
            this.CurrentSpaceViewModel.AlignSelected(param.ToString());
        }

        internal bool CanAlignSelected(object param)
        {
            return true;
        }

        /// <summary>
        /// Resets the offset and the zoom for a view
        /// </summary>
        public void GoHomeView(object parameter)
        {
            _model.CurrentWorkspace.Zoom = 1.0;
            var wsvm = dynSettings.Controller.DynamoViewModel.Workspaces.First(x => x.Model == _model.CurrentWorkspace);
            wsvm.OnCurrentOffsetChanged(this, new PointEventArgs(new Point(0, 0)));
        }

        internal bool CanGoHomeView(object parameter)
        {
            return true;
        }

        public void SelectAll(object parameter)
        {
            this.CurrentSpaceViewModel.SelectAll(null);
        }

        internal bool CanSelectAll(object parameter)
        {
            return this.CurrentSpaceViewModel.CanSelectAll(null);
        }

        public void MakeNewHomeWorkspace(object parameter)
        {
            // if the workspace is unsaved, prompt to save
            // otherwise overwrite the home workspace with new workspace
            if (!Model.HomeSpace.HasUnsavedChanges || AskUserToSaveWorkspaceOrCancel(this.Model.HomeSpace))
            {
                Model.CurrentWorkspace = this.Model.HomeSpace;
               
                _model.Clear(null);
            }
        }

        internal bool CanMakeNewHomeWorkspace(object parameter)
        {
            return true;
        }

        public void Exit(object allowCancel)
        {
            bool allowCancelBool = true;
            if (allowCancel != null)
            {
                allowCancelBool = (bool)allowCancel;
            }
            if (!AskUserToSaveWorkspacesOrCancel(allowCancelBool))
                return;

            exitInvoked = true;

            //request the UI to close its window
            OnRequestClose(this, EventArgs.Empty);

            dynSettings.Controller.ShutDown();

            DynamoLogger.Instance.FinishLogging();
        }

        internal bool CanExit(object allowCancel)
        {
            return !exitInvoked;
        }

        /// <summary>
        /// Requests a message box asking the user to save the workspace and allows saving.
        /// </summary>
        /// <param name="workspace">The workspace for which to show the dialog</param>
        /// <returns>False if the user cancels, otherwise true</returns>
        public bool AskUserToSaveWorkspaceOrCancel(WorkspaceModel workspace, bool allowCancel = true)
        {
            var args = new WorkspaceSaveEventArgs(workspace, allowCancel);
            OnRequestUserSaveWorkflow(this, args);
            if (!args.Success)
                return false;
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
                //if (!AskUserToSaveWorkspaceOrCancel(wvm.Model, allowCancel))
                //    return false;

                var args = new WorkspaceSaveEventArgs(wvm.Model, allowCancel);
                OnRequestUserSaveWorkflow(this, args);
                if (!args.Success)
                    return false;
            }
            return true;
        }

        public void SaveImage(object parameters)
        {
            OnRequestSaveImage(this, new ImageSaveEventArgs(parameters.ToString()));
        }

        internal bool CanSaveImage(object parameters)
        {
            return true;
        }

        public void ShowSaveImageDialogAndSaveResult(object parameter)
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
            if (!string.IsNullOrEmpty(_model.CurrentWorkspace.FilePath))
            {
                var fi = new FileInfo(_model.CurrentWorkspace.FilePath);
                _fileDialog.InitialDirectory = fi.DirectoryName;
            }

            if (_fileDialog.ShowDialog() == DialogResult.OK)
            {
                if (CanSaveImage(_fileDialog.FileName))
                    SaveImage(_fileDialog.FileName);
            }

        }

        internal bool CanShowSaveImageDialogAndSaveResult(object parameter)
        {
            return true;
        }

        public void ToggleConsoleShowing(object parameter)
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

        internal bool CanToggleConsoleShowing(object parameter)
        {
            return true;
        }

        public void SelectNeighbors(object parameters)
        {
            List<ISelectable> sels = DynamoSelection.Instance.Selection.ToList<ISelectable>();

            foreach (ISelectable sel in sels)
            {
                if (sel is NodeModel)
                    ((NodeModel)sel).SelectNeighbors();
            }
        }

        internal bool CanSelectNeighbors(object parameters)
        {
            return true;
        }

        public void ShowConnectors(object parameter)
        {
            if (IsShowingConnectors == false)
                IsShowingConnectors = true;
        }

        internal bool CanShowConnectors(object parameter)
        {
            return true;
        }

        public void SetConnectorType(object parameters)
        {
            if (parameters.ToString() == "BEZIER")
            {
                ConnectorType = ConnectorType.BEZIER;
            }
            else
            {
                ConnectorType = ConnectorType.POLYLINE;
            }
        }

        internal bool CanSetConnectorType(object parameters)
        {
            //parameter object will be BEZIER or POLYLINE
            if (string.IsNullOrEmpty(parameters.ToString()))
            {
                return false;
            }
            return true;
        }

        public void GoToWiki(object parameter)
        {
            Process.Start("https://github.com/ikeough/Dynamo/wiki");
        }

        internal bool CanGoToWiki(object parameter)
        {
            return true;
        }

        public void GoToSourceCode(object parameter)
        {
            Process.Start("https://github.com/ikeough/Dynamo");
        }

        internal bool CanGoToSourceCode(object parameter)
        {
            return true;
        }

        public void Pan(object parameter)
        {
            Debug.WriteLine(string.Format("Offset: {0},{1}, Zoom: {2}", _model.CurrentWorkspace.X, _model.CurrentWorkspace.Y, _model.CurrentWorkspace.Zoom));
            var panType = parameter.ToString();
            double pan = 10;
            var pt = new Point(_model.CurrentWorkspace.X, _model.CurrentWorkspace.Y);

            switch (panType)
            {
                case "Left":
                    pt.X += pan;
                    break;
                case "Right":
                    pt.X -= pan;
                    break;
                case "Up":
                    pt.Y += pan;
                    break;
                case "Down":
                    pt.Y -= pan;
                    break;
            }
            _model.CurrentWorkspace.X = pt.X;
            _model.CurrentWorkspace.Y = pt.Y;

            CurrentSpaceViewModel.OnCurrentOffsetChanged(this, new PointEventArgs(pt));
        }

        internal bool CanPan(object parameter)
        {
            return true;
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

    public class NoteEventArgs : EventArgs
    {
        public NoteModel Note { get; set; }
        public Dictionary<string, object> Data { get; set; }
        public NoteEventArgs(NoteModel n, Dictionary<string, object> d)
        {
            Note = n;
            Data = d;
        }
    }

    public class ViewEventArgs : EventArgs
    {
        public object View { get; set; }

        public ViewEventArgs(object v)
        {
            View = v;
        }
    }

    public class WorkspaceSaveEventArgs : EventArgs
    {
        public WorkspaceModel Workspace { get; set; }
        public bool AllowCancel { get; set; }
        public bool Success { get; set; }
        public WorkspaceSaveEventArgs(WorkspaceModel ws, bool allowCancel)
        {
            Workspace = ws;
            AllowCancel = allowCancel;
            Success = false;
        }
    }

    public class ImageSaveEventArgs : EventArgs
    {
        public string Path { get; set; }

        public ImageSaveEventArgs(string path)
        {
            Path = path;
        }
    }
}
