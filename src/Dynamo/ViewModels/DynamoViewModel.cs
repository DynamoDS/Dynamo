using System;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using Dynamo.Commands;
using Dynamo.Connectors;
using Dynamo.FSchemeInterop;
using Dynamo.FSchemeInterop.Node;
using Dynamo.Nodes;
using Dynamo.Selection;
using Dynamo.Utilities;
using Dynamo.Utilties;
using Microsoft.Practices.Prism.Commands;
using NUnit.Core;

namespace Dynamo.Controls
{

    public class DynamoViewModel:dynViewModelBase
    {

        #region properties

        public event EventHandler UILocked;
        public event EventHandler UIUnlocked;
        public event EventHandler RequestLayoutUpdate;
        public event EventHandler WorkspaceChanged;

        public virtual void OnUILocked(object sender, EventArgs e)
        {
            if (UILocked != null)
                UILocked(this, e);
        }
        public virtual void OnUIUnlocked(object sender, EventArgs e)
        {
            if (UIUnlocked != null)
                UIUnlocked(this, e);
        }
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
        private bool fullscreenWatchShowing = false;
        private DynamoController controller;
        public StringWriter sw;
        private bool runEnabled = true;
        protected bool canRunDynamically = true;
        protected bool debug = false;
        protected bool dynamicRun = false;
        
        private string UnlockLoadPath;
        private bool uiLocked = true;
        
        private string editName = "";

        /// <summary>
        /// An observable collection of workspace view models which tracks the model
        /// </summary>
        private ObservableCollection<dynWorkspaceViewModel> _workspaces = new ObservableCollection<dynWorkspaceViewModel>();

        public DelegateCommand GoToWikiCommand { get; set; }
        public DelegateCommand GoToSourceCodeCommand { get; set; }
        public DelegateCommand ExitCommand { get; set; }
        public DelegateCommand CleanupCommand { get; set; }
        public DelegateCommand ShowSaveImageDialogAndSaveResultCommand { get; set; }
        public DelegateCommand ShowOpenDialogAndOpenResultCommand { get; set; }
        public DelegateCommand ShowSaveDialogIfNeededAndSaveResultCommand { get; set; }
        public DelegateCommand ShowSaveDialogAndSaveResultCommand { get; set; }
        public DelegateCommand ShowNewFunctionDialogCommand { get; set; }
        public DelegateCommand<object> OpenCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand<object> SaveAsCommand { get; set; }
        public DelegateCommand ClearCommand { get; set; }
        public DelegateCommand HomeCommand { get; set; }
        public DelegateCommand LayoutAllCommand { get; set; }
        public DelegateCommand<object> CopyCommand { get; set; }
        public DelegateCommand<object> PasteCommand { get; set; }
        public DelegateCommand ToggleConsoleShowingCommand { get; set; }
        public DelegateCommand ToggleFullscreenWatchShowingCommand { get; set; }
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
        public DelegateCommand PostUIActivationCommand { get; set; }
        public DelegateCommand RefactorCustomNodeCommand { get; set; }
        public DelegateCommand RunUITestsCommand { get; set; }

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

        public bool FullscreenWatchShowing
        {
            get { return fullscreenWatchShowing; }
            set
            {
                fullscreenWatchShowing = value;
                RaisePropertyChanged("FullscreenWatchShowing");

                // NOTE: I couldn't get the binding to work in the XAML so
                //       this is a temporary hack
                foreach (dynWorkspaceViewModel workspace in Workspaces)
                {
                    workspace.FullscreenChanged();
                }
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
            get { return _model.Workspaces.IndexOf(_model.CurrentSpace); }
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

            #region Initialize Commands
            GoToWikiCommand = new DelegateCommand(GoToWiki, CanGoToWiki);
            GoToSourceCodeCommand = new DelegateCommand(GoToSourceCode,  CanGoToSourceCode);
            CleanupCommand = new DelegateCommand(Cleanup, CanCleanup);
            ExitCommand = new DelegateCommand(Exit, CanExit);
            ShowSaveImageDialogAndSaveResultCommand = new DelegateCommand(ShowSaveImageDialogAndSaveResult, CanShowSaveImageDialogAndSaveResult);
            ShowOpenDialogAndOpenResultCommand = new DelegateCommand(ShowOpenDialogAndOpenResult, CanShowOpenDialogAndOpenResultCommand);
            ShowSaveDialogIfNeededAndSaveResultCommand = new DelegateCommand(ShowSaveDialogIfNeededAndSaveResult, CanShowSaveDialogIfNeededAndSaveResultCommand);
            ShowSaveDialogAndSaveResultCommand = new DelegateCommand(ShowSaveDialogAndSaveResult, CanShowSaveDialogAndSaveResultCommand);
            ShowNewFunctionDialogCommand = new DelegateCommand(ShowNewFunctionDialog, CanShowNewFunctionDialogCommand);
            SaveCommand = new DelegateCommand(Save, CanSave);
            OpenCommand = new DelegateCommand<object>(Open, CanOpen);
            SaveAsCommand = new DelegateCommand<object>(SaveAs, CanSaveAs);
            ClearCommand = new DelegateCommand(Clear, CanClear);
            HomeCommand = new DelegateCommand(Home, CanGoHome);
            LayoutAllCommand = new DelegateCommand(LayoutAll, CanLayoutAll);
            CopyCommand = new DelegateCommand<object>(Copy, CanCopy);
            PasteCommand = new DelegateCommand<object>(Paste, CanPaste);
            ToggleConsoleShowingCommand = new DelegateCommand(ToggleConsoleShowing, CanToggleConsoleShowing);
            ToggleFullscreenWatchShowingCommand = new DelegateCommand(ToggleFullscreenWatchShowing, CanToggleFullscreenWatchShowing);
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
            RefactorCustomNodeCommand = new DelegateCommand(RefactorCustomNode, CanRefactorCustomNode);
            RunUITestsCommand = new DelegateCommand(RunUITests, CanRunUITests);
            #endregion
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

        private bool CanSave()
        {
            return true;
        }

        private void Open(object parameters)
        {
            string xmlPath = parameters as string;

            if (!string.IsNullOrEmpty(xmlPath))
            {

                OnUILocked(this, EventArgs.Empty);

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

                OnUIUnlocked(this, EventArgs.Empty);
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
                fltr = "Dynamo Function (*.dyf)|*.dyf";
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

        private void ShowNewFunctionDialog()
        {
            //First, prompt the user to enter a name
            string name, category;
            string error = "";

            do
            {
                var dialog = new FunctionNamePrompt(dynSettings.Controller.SearchViewModel.Categories, error);
                if (dialog.ShowDialog() != true)
                {
                    return;
                }

                name = dialog.Text;
                category = dialog.Category;

                if (Controller.CustomNodeLoader.Contains(name))
                {
                    error = "A function with this name already exists.";
                }
                else if (category.Equals(""))
                {
                    error = "Please enter a valid category.";
                }
                else
                {
                    error = "";
                }
            } while (!error.Equals(""));

            NewFunction(Guid.NewGuid(), name, category, true);
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
        public bool AskUserToSaveWorkspaceOrCancel(dynWorkspaceModel workspace)
        {
            var result = System.Windows.MessageBox.Show("You have unsaved changes to " + workspace.Name + "\n\n Would you like to save your changes?", "Confirmation", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
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
        /// <returns>Whether the cleanup was completed or cancelled.</returns>
        public bool AskUserToSaveWorkspacesOrCancel()
        {
            foreach (var wvm in Workspaces.Where((wvm) => wvm.Model.HasUnsavedChanges))
            {
                if (!AskUserToSaveWorkspaceOrCancel(wvm.Model))
                    return false;
            }
            return true;
        }

        public void Cleanup()
        {
            DynamoLogger.Instance.FinishLogging();
        }

        private bool CanCleanup()
        {
            return true;
        }

        private void Exit()
        {
            if (!AskUserToSaveWorkspacesOrCancel())
                return;
            this.Cleanup();
            dynSettings.Bench.Close();
        }

        private bool CanExit()
        {
            return true;
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
            //dynSettings.Bench.LockUI();
            OnUILocked(this, EventArgs.Empty);

            CleanWorkbench();

            //don't save the file path
            _model.CurrentSpace.FilePath = "";

            //dynSettings.Bench.UnlockUI();
            OnUIUnlocked(this, EventArgs.Empty);
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
            //dynSettings.Bench.LockUI();
            OnUILocked(this, EventArgs.Empty);

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

            //dynSettings.Bench.UnlockUI();
            OnUIUnlocked(this, EventArgs.Empty);
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
            Hashtable nodeLookup = new Hashtable();

            //clear the selection so we can put the
            //paste contents in
            DynamoSelection.Instance.Selection.RemoveAll();

            var nodes = dynSettings.Controller.ClipBoard.Select(x => x).Where(x => x is dynNodeModel);

            var connectors = dynSettings.Controller.ClipBoard.Select(x => x).Where(x => x is dynConnectorModel);

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
                    nodeData.Add("name", node.NickName);
                nodeData.Add("guid", newGuid);

                if (typeof(dynBasicInteractive<double>).IsAssignableFrom(node.GetType()))
                {
                    nodeData.Add("value", (node as dynBasicInteractive<double>).Value);
                }
                else if (typeof(dynBasicInteractive<string>).IsAssignableFrom(node.GetType()))
                {
                    nodeData.Add("value", (node as dynBasicInteractive<string>).Value);
                }
                else if (typeof(dynBasicInteractive<bool>).IsAssignableFrom(node.GetType()))
                {
                    nodeData.Add("value", (node as dynBasicInteractive<bool>).Value);
                }
                else if (typeof(dynVariableInput).IsAssignableFrom(node.GetType()))
                {
                    //for list type nodes send the number of ports
                    //as the value - so we can setup the new node with
                    //the right number of ports
                    nodeData.Add("value", node.InPorts.Count);
                }

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
                Dictionary<string, object> connectionData = new Dictionary<string, object>();

                dynNodeModel startNode = null;

                try
                {
                    startNode = _model.CurrentSpace.Nodes
                        .Where(x => x.GUID == (Guid)nodeLookup[c.Start.Owner.GUID]).FirstOrDefault();
                }
                catch
                {
                    //don't let users paste connectors between workspaces
                    if (c.Start.Owner.WorkSpace == _model.CurrentSpace)
                    {
                        startNode = c.Start.Owner;
                    }
                    else
                    {
                        continue;
                    }

                }

                connectionData.Add("start", startNode);

                connectionData.Add("end", _model.CurrentSpace.Nodes.FirstOrDefault(x => x.GUID == (Guid)nodeLookup[c.End.Owner.GUID]));

                connectionData.Add("port_start", c.Start.Index);
                connectionData.Add("port_end", c.End.Index);

                dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(CreateConnectionCommand, connectionData));
            }

            //process the queue again to create the connectors
            dynSettings.Controller.ProcessCommandQueue();

            foreach (DictionaryEntry de in nodeLookup)
            {
                dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(AddToSelectionCommand,
                    _model.CurrentSpace.Nodes.FirstOrDefault(x => x.GUID == (Guid)de.Value)));
            }

            dynSettings.Controller.ProcessCommandQueue();

            //dynSettings.ViewModel.ClipBoard.Clear();
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

        private void ToggleFullscreenWatchShowing()
        {
            if (FullscreenWatchShowing)
            {
                FullscreenWatchShowing = false;
            }
            else
            {
                FullscreenWatchShowing = true;
            }
        }

        private bool CanToggleFullscreenWatchShowing()
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

        private void SaveImage(object parameters)
        {
            string imagePath = parameters as string;

            if (!string.IsNullOrEmpty(imagePath))
            {
                Transform trans = dynSettings.Workbench.LayoutTransform;
                dynSettings.Workbench.LayoutTransform = null;
                Size size = new Size(dynSettings.Workbench.Width, dynSettings.Workbench.Height);
                dynSettings.Workbench.Measure(size);
                dynSettings.Workbench.Arrange(new Rect(size));

                //calculate the necessary width and height
                double width = 0;
                double height = 0;
                foreach (dynNodeModel n in _model.Nodes)
                {
                    width = Math.Max(n.X + n.Width, width);
                    height = Math.Max(n.Y + n.Height, height);
                }

                Rect rect = VisualTreeHelper.GetDescendantBounds(dynSettings.Bench.border);

                RenderTargetBitmap rtb = new RenderTargetBitmap((int)rect.Right + 50,
                  (int)rect.Bottom + 50, 96, 96, System.Windows.Media.PixelFormats.Default);
                rtb.Render(dynSettings.Workbench);
                //endcode as PNG
                BitmapEncoder pngEncoder = new PngBitmapEncoder();
                pngEncoder.Frames.Add(BitmapFrame.Create(rtb));

                using (var stm = File.Create(imagePath))
                {
                    pngEncoder.Save(stm);
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
            dynSettings.Bench.PackageManagerLoginStateContainer.Visibility = Visibility.Visible;
            dynSettings.Bench.PackageManagerMenu.Visibility = Visibility.Visible;
        }

        private bool CanShowPackageManager()
        {
            return true;
        }

        private void GoToWorkspace(object parameter)
        {
            if (parameter is Guid && dynSettings.Controller.CustomNodeLoader.Contains((Guid)parameter))
            {
                ViewCustomNodeWorkspace( dynSettings.Controller.CustomNodeLoader.GetFunctionDefinition((Guid)parameter) );
            }
        }

        private bool CanGoToWorkspace(object parameter)
        {
            return true;
        }

        private void DisplayFunction(object parameters)
        {
            Controller.CustomNodeLoader.GetFunctionDefinition((Guid)parameters);
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

            _model.CurrentSpace.Nodes.Add(node);
            node.WorkSpace = dynSettings.Controller.DynamoViewModel.CurrentSpace;

            //if we've received a value in the dictionary
            //try to set the value on the node
            if (data.ContainsKey("value"))
            {
                if (typeof(dynBasicInteractive<double>).IsAssignableFrom(node.GetType()))
                {
                    (node as dynBasicInteractive<double>).Value = (double)data["value"];
                }
                else if (typeof(dynBasicInteractive<string>).IsAssignableFrom(node.GetType()))
                {
                    (node as dynBasicInteractive<string>).Value = data["value"].ToString();
                }
                else if (typeof(dynBasicInteractive<bool>).IsAssignableFrom(node.GetType()))
                {
                    (node as dynBasicInteractive<bool>).Value = (bool)data["value"];
                }
                else if (typeof(dynVariableInput).IsAssignableFrom(node.GetType()))
                {
                    int desiredPortCount = (int)data["value"];
                    if (node.InPortData.Count < desiredPortCount)
                    {
                        int portsToCreate = desiredPortCount - node.InPortData.Count;

                        for (int i = 0; i < portsToCreate; i++)
                        {
                            (node as dynVariableInput).AddInput();
                        }
                        (node as dynVariableInput).RegisterAllPorts();
                    }
                }
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

            dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.OnRequestNodeCentered(this, new NodeEventArgs(node, data));

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
                    || (Guid.TryParse(name, out guid) && dynSettings.Controller.CustomNodeLoader.Contains(guid)))
            {
                return true;
            }

            return false;
        }

        void CreateConnection(object parameters)
        {
            Dictionary<string, object> connectionData = parameters as Dictionary<string, object>;

            dynNodeModel start = (dynNodeModel)connectionData["start"];
            dynNodeModel end = (dynNodeModel)connectionData["end"];
            int startIndex = (int)connectionData["port_start"];
            int endIndex = (int)connectionData["port_end"];

            dynConnectorModel c = new dynConnectorModel(start, end, startIndex, endIndex, 0);

            _model.CurrentSpace.Connectors.Add(c);
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

        private bool CanDelete(object parameters)
        {
            return DynamoSelection.Instance.Selection.Count > 0;
        }

        private void AddNote(object parameters)
        {
            var inputs = (Dictionary<string, object>)parameters;

            // by default place note at center
            var x = 0.0;
            var y = 0.0;

            if (inputs != null && inputs.ContainsKey("x"))
                x = (double)inputs["x"];

            if (inputs != null && inputs.ContainsKey("y"))
                y = (double)inputs["y"];


            var n = new dynNoteModel(x, y);

            n.Text = (inputs == null || !inputs.ContainsKey("text")) ? "New Note" : inputs["text"].ToString();
            var ws = (inputs == null || !inputs.ContainsKey("workspace")) ? _model.CurrentSpace : (dynWorkspaceModel)inputs["workspace"];

            ws.Notes.Add(n);
            
            if (!ViewingHomespace)
            {
                _model.CurrentSpace.Modified();
            }
            
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

            //MVVM: Replaced with event handler on source changed
            //if (LogScroller != null)
            //    LogScroller.ScrollToBottom();
        }

        private void PostUIActivation()
        {
            DynamoLoader.LoadCustomNodes(dynSettings.Bench, Controller.CustomNodeLoader, Controller.SearchViewModel);

            dynSettings.Controller.DynamoViewModel.Log("Welcome to Dynamo!");

            if (UnlockLoadPath != null && !OpenWorkbench(UnlockLoadPath))
            {
                dynSettings.Controller.DynamoViewModel.Log("Workbench could not be opened.");

                if (DynamoCommands.WriteToLogCmd.CanExecute(null))
                {
                    DynamoCommands.WriteToLogCmd.Execute("Workbench could not be opened.");
                    DynamoCommands.WriteToLogCmd.Execute(UnlockLoadPath);
                }
            }

            UnlockLoadPath = null;

            OnUIUnlocked(this, EventArgs.Empty);

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
                double cx = DynamoView.CANVAS_OFFSET_X;
                double cy = DynamoView.CANVAS_OFFSET_Y;
                string id = "";

                // load the header
                foreach (XmlNode node in xmlDoc.GetElementsByTagName("dynWorkspace"))
                {
                    foreach (XmlAttribute att in node.Attributes)
                    {
                        if (att.Name.Equals("X"))
                            cx = Convert.ToDouble(att.Value);
                        else if (att.Name.Equals("Y"))
                            cy = Convert.ToDouble(att.Value);
                        else if (att.Name.Equals("Name"))
                            funName = att.Value;
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
                if (funName == null)
                {
                    //View the home workspace, then open the bench file
                    if (!ViewingHomespace)
                        ViewHomeWorkspace(); //TODO: Refactor
                    return OpenWorkbench(xmlPath);
                }
                else if (Controller.CustomNodeLoader.Contains(funName))
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
                        : BuiltinNodeCategories.SCRIPTING_CUSTOMNODES,
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

                    double x = Convert.ToDouble(xAttrib.Value);
                    double y = Convert.ToDouble(yAttrib.Value);

                    //Type t = Type.GetType(typeName);
                    TypeLoadData tData;
                    Type t;
                    
                    if (!Controller.BuiltInTypesByName.TryGetValue(typeName, out tData))
                    {
                        t = Type.GetType(typeName);
                        if (t == null)
                        {
                            Log("Error loading definition. Could not load node of type: " + typeName);
                            return false;
                        }
                    }
                    else
                        t = tData.Type;

                    dynNodeModel el = CreateInstanceAndAddNodeToWorkspace(t, nickname, guid, x, y, ws);

                    if (lacingAttrib != null)
                    {
                        LacingStrategy lacing = LacingStrategy.First;
                        Enum.TryParse(lacingAttrib.Value, out lacing);
                        el.ArgumentLacing = lacing;
                    }

                    if (el == null)
                        return false;

                    el.DisableReporting();
                    el.LoadElement(elNode);

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

                        if (dynSettings.Controller.CustomNodeLoader.IsInitialized(funId))
                            fun.Definition = dynSettings.Controller.CustomNodeLoader.GetFunctionDefinition(funId);
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
                            var newConnector = new dynConnectorModel(
                                start, end,
                                startIndex, endIndex,
                                portType, false
                                );

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
                        double x = Convert.ToDouble(xAttrib.Value);
                        double y = Convert.ToDouble(yAttrib.Value);

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

                Controller.PackageManagerClient.LoadPackageHeader(def, funName);
                nodeWorkspaceWasLoaded(def, children, parents);

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
                    var def = dynSettings.Controller.CustomNodeLoader.GetDefinitionFromWorkspace(workspace);
                    def.Workspace.FilePath = path;
                    if (def != null)
                    {
                        this.SaveFunction(def, true);
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

        /// <summary>
        ///     Update a custom node after refactoring.  Updates search and all instances of the node.
        /// </summary>
        /// <param name="selectedNodes"> The function definition for the user-defined node </param>
        private void RefactorCustomNode()
        {

            //Bench.workspaceLabel.Content = Bench.editNameBox.Text;
            var def = Controller.CustomNodeLoader.GetDefinitionFromWorkspace(CurrentSpace);
            Controller.SearchViewModel.Refactor(def, editName, (_model.CurrentSpace).Name);

            //Update existing function nodes
            foreach (dynNodeModel el in AllNodes)
            {
                if (el is dynFunction)
                {
                    var node = (dynFunction)el;

                    if (node.Definition == null)
                    {
                        node.Definition = Controller.CustomNodeLoader.GetFunctionDefinition(Guid.Parse(node.Symbol));
                    }

                    if (!node.Definition.Workspace.Name.Equals(CurrentSpace.Name))
                        continue;

                    //Rename nickname only if it's still referring to the old name
                    if (node.NickName.Equals(CurrentSpace.Name))
                        node.NickName = editName;
                }
            }

            Controller.FSchemeEnvironment.RemoveSymbol(CurrentSpace.Name);

            //TODO: Delete old stored definition
            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string pluginsPath = Path.Combine(directory, "definitions");

            if (Directory.Exists(pluginsPath))
            {
                string oldpath = Path.Combine(pluginsPath, CurrentSpace.Name + ".dyf");
                if (File.Exists(oldpath))
                {
                    string newpath = dynSettings.FormatFileName(
                        Path.Combine(pluginsPath, editName + ".dyf")
                        );

                    File.Move(oldpath, newpath);
                }
            }

            (_model.CurrentSpace).Name = editName;

            SaveFunction(def);
        }

        private bool CanRefactorCustomNode()
        {
            return true;
        }

        public IEnumerable<dynNodeModel> AllNodes
        {
            get
            {
                return _model.HomeSpace.Nodes.Concat(
                    Controller.CustomNodeLoader.GetLoadedDefinitions().Aggregate(
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
        /// <param name="definition">The definition to saveo</param>
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
                string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string pluginsPath = Path.Combine(directory, "definitions");

                try
                {
                    if (!Directory.Exists(pluginsPath))
                        Directory.CreateDirectory(pluginsPath);

                    string path = Path.Combine(pluginsPath, dynSettings.FormatFileName(functionWorkspace.Name) + ".dyf");
                    dynWorkspaceModel.SaveWorkspace(path, functionWorkspace);

                    if (addToSearch)
                    {
                        Controller.SearchViewModel.Add(functionWorkspace.Name, functionWorkspace.Category, definition.FunctionId);
                    }

                    if (compileFunction)
                    {
                        dynSettings.Controller.FSchemeEnvironment.DefineSymbol(definition.FunctionId.ToString(), CustomNodeLoader.CompileFunction(definition));
                    }

                    Controller.CustomNodeLoader.SetNodeInfo(functionWorkspace.Name, functionWorkspace.Category, definition.FunctionId, path);
                }
                catch (Exception e)
                {
                    Log("Error saving:" + e.GetType());
                    Log(e);
                }
            }

            try
            {
                #region Find outputs

                // Find output elements for the node
                IEnumerable<dynNodeModel> outputs = functionWorkspace.Nodes.Where(x => x is dynOutput);

                var topMost = new List<Tuple<int, dynNodeModel>>();

                IEnumerable<string> outputNames;

                // if we found output nodes, add select their inputs
                // these will serve as the function output
                if (outputs.Any())
                {
                    topMost.AddRange(
                        outputs.Where(x => x.HasInput(0)).Select(x => x.Inputs[0]));

                    outputNames = outputs.Select(x => (x as dynOutput).Symbol);
                }
                else
                {
                    // if there are no explicitly defined output nodes
                    // get the top most nodes and set THEM as tht output
                    IEnumerable<dynNodeModel> topMostNodes = functionWorkspace.GetTopMostNodes();

                    var outNames = new List<string>();

                    foreach (dynNodeModel topNode in topMostNodes)
                    {
                        foreach (int output in Enumerable.Range(0, topNode.OutPortData.Count))
                        {
                            if (!topNode.HasOutput(output))
                            {
                                topMost.Add(Tuple.Create(output, topNode));
                                outNames.Add(topNode.OutPortData[output].NickName);
                            }
                        }
                    }

                    outputNames = outNames;
                }

                #endregion

                // color the node to define its connectivity
                foreach (var ele in topMost)
                {
                    ele.Item2.ValidateConnections();
                }

                //Find function entry point, and then compile the function and add it to our environment
                IEnumerable<dynNodeModel> variables = functionWorkspace.Nodes.Where(x => x is dynSymbol);
                IEnumerable<string> inputNames = variables.Select(x => (x as dynSymbol).Symbol);

                INode top;
                var buildDict = new Dictionary<dynNodeModel, Dictionary<int, INode>>();

                if (topMost.Count > 1)
                {
                    InputNode node = new ExternalFunctionNode(
                        FScheme.Value.NewList,
                        Enumerable.Range(0, topMost.Count).Select(x => x.ToString()));

                    int i = 0;
                    foreach (var topNode in topMost)
                    {
                        string inputName = i.ToString();
                        node.ConnectInput(inputName, topNode.Item2.Build(buildDict, topNode.Item1));
                        i++;
                    }

                    top = node;
                }
                else
                    top = topMost[0].Item2.BuildExpression(buildDict);

                // if the node has any outputs, we create a BeginNode in order to evaluate all of them
                // sequentially (begin evaluates a list of expressions)
                if (outputs.Any())
                {
                    var beginNode = new BeginNode();
                    List<dynNodeModel> hangingNodes = functionWorkspace.GetTopMostNodes().ToList();
                    foreach (var tNode in hangingNodes.Select((x, index) => new { Index = index, Node = x }))
                    {
                        beginNode.AddInput(tNode.Index.ToString());
                        beginNode.ConnectInput(tNode.Index.ToString(), tNode.Node.Build(buildDict, 0));
                    }
                    beginNode.AddInput(hangingNodes.Count.ToString());
                    beginNode.ConnectInput(hangingNodes.Count.ToString(), top);

                    top = beginNode;
                }

                // make the anonymous function
                FScheme.Expression expression = Utils.MakeAnon(variables.Select(x => x.GUID.ToString()),
                                                               top.Compile());

                // make it accessible in the FScheme environment
                Controller.FSchemeEnvironment.DefineSymbol(definition.FunctionId.ToString(), expression);

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

            }
            catch (Exception ex)
            {
                Log(ex.GetType() + ": " + ex.Message);
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
            double x, double y, dynWorkspaceModel ws)    //Visibility vis = Visibility.Visible)
        {
            try
            {
                var node = CreateNodeInstance(elementType, nickName, guid);

                ws.Nodes.Add(node);
                node.WorkSpace = ws;

                node.X = x;
                node.Y = y;

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
        }

        public bool OpenWorkbench(string xmlPath)
        {
            Log("Opening home workspace " + xmlPath + "...");
            CleanWorkbench();

            try
            {
                #region read xml file

                var xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlPath);

                foreach (XmlNode node in xmlDoc.GetElementsByTagName("dynWorkspace"))
                {
                    foreach (XmlAttribute att in node.Attributes)
                    {
                        if (att.Name.Equals("X"))
                        {
                            _model.CurrentSpace.X = Convert.ToDouble(att.Value);
                        }
                        else if (att.Name.Equals("Y"))
                        {
                            _model.CurrentSpace.Y = Convert.ToDouble(att.Value);
                        }
                    }
                }

                XmlNodeList elNodes = xmlDoc.GetElementsByTagName("dynElements");
                XmlNodeList cNodes = xmlDoc.GetElementsByTagName("dynConnectors");
                XmlNodeList nNodes = xmlDoc.GetElementsByTagName("dynNotes");

                XmlNode elNodesList = elNodes[0];
                XmlNode cNodesList = cNodes[0];
                XmlNode nNodesList = nNodes[0];

                foreach (XmlNode elNode in elNodesList.ChildNodes)
                {
                    XmlAttribute typeAttrib = elNode.Attributes[0];
                    XmlAttribute guidAttrib = elNode.Attributes[1];
                    XmlAttribute nicknameAttrib = elNode.Attributes[2];
                    XmlAttribute xAttrib = elNode.Attributes[3];
                    XmlAttribute yAttrib = elNode.Attributes[4];

                    XmlAttribute lacingAttrib = null;
                    if (elNode.Attributes.Count > 5)
                    {
                        lacingAttrib = elNode.Attributes[5];
                    }

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

                    double x = Convert.ToDouble(xAttrib.Value);
                    double y = Convert.ToDouble(yAttrib.Value);

                    if (typeName.StartsWith("Dynamo.Elements."))
                        typeName = "Dynamo.Nodes." + typeName.Remove(0, 16);

                    TypeLoadData tData;
                    Type t;

                    if (!Controller.BuiltInTypesByName.TryGetValue(typeName, out tData))
                    {
                        t = Type.GetType(typeName);
                        if (t == null)
                        {
                            Log("Error loading workspace. Could not load node of type: " + typeName);
                            return false;
                        }
                    }
                    else
                        t = tData.Type;

                    dynNodeModel el = CreateInstanceAndAddNodeToWorkspace(
                        t, nickname, guid, x, y,
                        _model.CurrentSpace
                        );

                    if (lacingAttrib != null)
                    {
                        LacingStrategy lacing = LacingStrategy.Disabled;
                        Enum.TryParse(lacingAttrib.Value, out lacing);
                        el.ArgumentLacing = lacing;
                    }

                    el.DisableReporting();

                    el.LoadElement(elNode);

                    if (ViewingHomespace)
                        el.SaveResult = true;

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

                        fun.Definition = dynSettings.Controller.CustomNodeLoader.GetFunctionDefinition(funId);

                    }

                    //read the sub elements
                    //set any numeric values 
                    //foreach (XmlNode subNode in elNode.ChildNodes)
                    //{
                    //   if (subNode.Name == "System.Double")
                    //   {
                    //      double val = Convert.ToDouble(subNode.Attributes[0].Value);
                    //      el.OutPortData[0].Object = val;
                    //      el.Update();
                    //   }
                    //   else if (subNode.Name == "System.Int32")
                    //   {
                    //      int val = Convert.ToInt32(subNode.Attributes[0].Value);
                    //      el.OutPortData[0].Object = val;
                    //      el.Update();
                    //   }
                    //}
                }

                //dynSettings.Workbench.UpdateLayout();
                OnRequestLayoutUpdate(this, EventArgs.Empty);

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

                    //don't connect if the end element is an instance map
                    //those have a morphing set of inputs
                    //dynInstanceParameterMap endTest = end as dynInstanceParameterMap;

                    //if (endTest != null)
                    //{
                    //    continue;
                    //}

                    if (start != null && end != null && start != end)
                    {
                        var newConnector = new dynConnectorModel(start, end,
                                                            startIndex, endIndex, portType);

                        _model.CurrentSpace.Connectors.Add(newConnector);
                    }
                }

                //MVVM: redraw should be automatic with connector bindings
                //_model.CurrentSpace.Connectors.ForEach(x => x.Redraw());

                #region instantiate notes

                if (nNodesList != null)
                {
                    foreach (XmlNode note in nNodesList.ChildNodes)
                    {
                        XmlAttribute textAttrib = note.Attributes[0];
                        XmlAttribute xAttrib = note.Attributes[1];
                        XmlAttribute yAttrib = note.Attributes[2];

                        string text = textAttrib.Value;
                        double x = Convert.ToDouble(xAttrib.Value);
                        double y = Convert.ToDouble(yAttrib.Value);

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

                foreach (dynNodeModel e in _model.CurrentSpace.Nodes)
                    e.EnableReporting();

                #endregion

                _model.HomeSpace.FilePath = xmlPath;
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
            _model.CurrentSpace.Modified();
        }

        internal FunctionDefinition NewFunction(Guid id,
                                                string name,
                                                string category,
                                                bool display,
                                                double workspaceOffsetX = DynamoView.CANVAS_OFFSET_X,
                                                double workspaceOffsetY = DynamoView.CANVAS_OFFSET_Y)
        {
            //Add an entry to the funcdict
            var workSpace = new FuncWorkspace(
                name, category, workspaceOffsetX, workspaceOffsetY);

            _model.Workspaces.Add(workSpace);

            List<dynNodeModel> newElements = workSpace.Nodes.ToList();
            List<dynConnectorModel> newConnectors = workSpace.Connectors.ToList();

            var functionDefinition = new FunctionDefinition(id)
            {
                Workspace = workSpace
            };

            Controller.CustomNodeLoader.AddFunctionDefinition(functionDefinition.FunctionId, functionDefinition);

            // add the element to search
            Controller.SearchViewModel.Add(name, category, id);

            if (display)
            {
                if (!ViewingHomespace)
                {
                    var def = Controller.CustomNodeLoader.GetDefinitionFromWorkspace(CurrentSpace);
                    if (def != null)
                        SaveFunction(def);
                }

                _model.CurrentSpace = workSpace;
            }

            return functionDefinition;
        }

        protected virtual dynFunction CreateFunction(IEnumerable<string> inputs, IEnumerable<string> outputs,
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

                if (Controller.CustomNodeLoader.GetNodeInstance(Controller, Guid.Parse(name), out func))
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
                    foreach (FunctionDefinition funcDef in Controller.CustomNodeLoader.GetLoadedDefinitions())
                    {
                        if (funcDef.Workspace.Nodes.Contains(e))
                        {
                            ViewCustomNodeWorkspace(funcDef);
                            break;
                        }
                    }
                }
            }

            dynSettings.Controller.DynamoViewModel.CurrentSpaceViewModel.OnRequestCenterViewOnElement(this, new NodeEventArgs(e,null));
            
        }

        private void RunUITests()
        {
            string assLoc = Assembly.GetExecutingAssembly().Location;
            string testsLoc = Path.Combine(Path.GetDirectoryName(assLoc), "DynamoElementsTests.dll");
            TestPackage testPackage = new TestPackage(testsLoc);
            RemoteTestRunner remoteTestRunner = new RemoteTestRunner();
            remoteTestRunner.Load(testPackage);

            TestResult testResult = remoteTestRunner.Run(new NullListener(), new DynamoUITestFilter(), false, LoggingThreshold.All);
            OutputResult(testResult);
        }

        private bool CanRunUITests()
        {
            return true;
        }

        static void OutputResult(TestResult result)
        {
            if (result.HasResults)
            {
                foreach (var childResult in result.Results)
                {
                    OutputResult((TestResult)childResult);
                }
                return;
            }

            dynSettings.Controller.DynamoViewModel.Log(string.Format("{0}:{1}", result.FullName, result.ResultState));
        }
    }

    public class DynamoUITestFilter : ITestFilter
    {
        public bool IsEmpty
        {
            get;
            set;
        }

        public bool Pass(ITest test)
        {
            foreach (var cat in test.Categories)
            {
                if (cat == "DynamoUI")
                    return true;
            }

            return false;
        }

        public bool Match (ITest test)
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

    public class NodeEventArgs : EventArgs
    {
        public dynNodeModel Node { get; set; }
        public Dictionary<string, object> Data { get; set; } 
        public NodeEventArgs(dynNodeModel n, Dictionary<string, object> d )
        {
            Node = n;
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
