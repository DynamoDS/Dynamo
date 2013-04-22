using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dynamo.Commands;
using Dynamo.Connectors;
using Dynamo.Controls;
using Dynamo.Selection;
using Dynamo.Utilities;
using Microsoft.Practices.Prism.Commands;

namespace Dynamo.Controls
{
    class dynBenchViewModel:dynViewModelBase
    {
        private DynamoModel _model;

        private string logText;
        private ConnectorType connectorType;
        private Point transformOrigin;
        private bool consoleShowing;
        private dynConnector activeConnector;
        private DynamoController controller;
        public StringWriter sw;
        private bool runEnabled = true;
        protected bool canRunDynamically = true;
        protected bool debug = false;
        protected bool dynamicRun = false;

        /// <summary>
        /// An observable collection of workspace view models which tracks the model
        /// </summary>
        private ObservableCollection<dynWorkspaceViewModel> _workspaces = new ObservableCollection<dynWorkspaceViewModel>();

        public DelegateCommand GoToWikiCommand { get; set; }
        public DelegateCommand GoToSourceCodeCommand { get; set; }
        public DelegateCommand ExitCommand { get; set; }
        public DelegateCommand ShowSaveImageDialogueAndSaveResultCommand { get; set; }
        public DelegateCommand ShowOpenDialogAndOpenResultCommand { get; set; }
        public DelegateCommand ShowSaveDialogIfNeededAndSaveResultCommand { get; set; }
        public DelegateCommand ShowSaveDialogAndSaveResultCommand { get; set; }
        public DelegateCommand ShowNewFunctionDialogCommand { get; set; }
        public DelegateCommand<object> OpenCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand SaveAsCommand { get; set; }
        public DelegateCommand ClearCommand { get; set; }
        public DelegateCommand HomeCommand { get; set; }
        public DelegateCommand LayoutAllCommand { get; set; }
        public DelegateCommand<object> CopyCommand { get; set; }
        public DelegateCommand<object> PasteCommand { get; set; }
        public DelegateCommand ToggleConsoleShowingCommand { get; set; }
        public DelegateCommand CancelRunCommand { get; set; }
        public DelegateCommand<object> SaveImageCommand { get; set; }
        public DelegateCommand ClearLogCommand { get; set; }
        public DelegateCommand RunExpressionCommand { get; set; }
        public DelegateCommand ShowPackageManagerCommand { get; set; }
        public DelegateCommand<object> GoToWorkspaceCommand { get; set; }
        public DelegateCommand<object> DisplayFunctionCommand { get; set; }
        public DelegateCommand<object> SetConnectorTypeCommand { get; set; }
        
        public ObservableCollection<dynWorkspaceViewModel> Workspaces
        {
            get { return _workspaces; }
            set
            {
                _workspaces = value;
                RaisePropertyChanged("Workspaces");
            }
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

        public dynConnector ActiveConnector
        {
            get { return activeConnector; }
            set
            {
                activeConnector = value;
                RaisePropertyChanged("ActiveConnector");
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

        public Point CurrentOffset
        {
            get { return zoomBorder.GetTranslateTransformOrigin(); }
            set
            {
                if (zoomBorder != null)
                {
                    zoomBorder.SetTranslateTransformOrigin(value);
                }
                NotifyPropertyChanged("CurrentOffset");
            }
        }

        public dynBenchViewModel(DynamoController controller)
        {
            //MVVM: Instantiate the model
            _model = new DynamoModel();
            _model.Workspaces.CollectionChanged += Workspaces_CollectionChanged;
            _model.PropertyChanged += _model_PropertyChanged;

            Controller = controller;
            sw = new StringWriter();
            ConnectorType = ConnectorType.BEZIER;

            GoToWikiCommand = new DelegateCommand(GoToWiki, CanGoToWiki);
            GoToSourceCodeCommand = new DelegateCommand(GoToSourceCode,  CanGoToSourceCode);
            ExitCommand = new DelegateCommand(Exit, CanExit);
            ShowSaveImageDialogueAndSaveResultCommand = new DelegateCommand(ShowSaveImageDialogueAndSaveResult, CanShowSaveImageDialogueAndSaveResult);
            ShowOpenDialogAndOpenResultCommand = new DelegateCommand(ShowOpenDialogAndOpenResult, CanShowOpenDialogAndOpenResultCommand);
            ShowSaveDialogIfNeededAndSaveResultCommand = new DelegateCommand(ShowSaveDialogIfNeededAndSaveResult, CanShowSaveDialogIfNeededAndSaveResultCommand);
            ShowSaveDialogAndSaveResultCommand = new DelegateCommand(ShowSaveDialogAndSaveResult, CanShowSaveDialogAndSaveResultCommand);
            ShowNewFunctionDialogCommand = new DelegateCommand(ShowNewFunctionDialog, CanShowNewFunctionDialogCommand);
            SaveCommand = new DelegateCommand(Save, CanSave);
            OpenCommand = new DelegateCommand<object>(Open, CanOpen);
            SaveAsCommand = new DelegateCommand<string>(SaveAs, CanSaveAs);
            ClearCommand = new DelegateCommand(Clear, CanClear);
            HomeCommand = new DelegateCommand(Home, CanGoHome);
            LayoutAllCommand = new DelegateCommand(LayoutAll, CanLayoutAll);
            CopyCommand = new DelegateCommand<object>(Copy, CanCopy);
            PasteCommand = new DelegateCommand<object>(Paste, CanPaste);
            ToggleConsoleShowingCommand = new DelegateCommand(ToggleConsoleShowing, CanToggleConsoleShowing);
            CancelRunCommand = new DelegateCommand(CancelRun, CanCancelRun);
            SaveImageCommand = new DelegateCommand<object>(SaveImage, CanSaveImage);
            ClearLogCommand = new DelegateCommand(ClearLog, CanClearLog);
            RunExpressionCommand = new DelegateCommand(RunExpression,CanRunExpression);
            ShowPackageManagerCommand = new DelegateCommand(ShowPackageManager,CanShowPackageManager);
            GoToWorkspaceCommand = new DelegateCommand<object>(GoToWorkspace, CanGoToWorkspace);
            DisplayFunctionCommand = new DelegateCommand<object>(DisplayFunction, CanDisplayFunction);
            SetConnectorTypeCommand = new DelegateCommand<object>(SetConnectorType, CanSetConnectorType);
        }

        void _model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentSpace")
                RaisePropertyChanged("CanGoHome");
        }

        /// <summary>
        /// Responds to change in the workspaces collection, creating or deleting workspace model views.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Workspaces_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems)
                        _workspaces.Add(new dynWorkspaceViewModel(item as dynWorkspace));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                        _workspaces.Remove(_workspaces.ToList().Where(x => x.Workspace == item));
                    break;
            }
        }

        private void Save()
        {
            _model.Save();
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
                if (dynSettings.Bench.UILocked)
                {
                    dynSettings.Controller.QueueLoad(xmlPath);
                    return;
                }

                dynSettings.Bench.LockUI();

                if (!_model.OpenDefinition(xmlPath))
                {
                    //MessageBox.Show("Workbench could not be opened.");
                    dynSettings.Bench.Log("Workbench could not be opened.");

                    //dynSettings.Writer.WriteLine("Workbench could not be opened.");
                    //dynSettings.Writer.WriteLine(xmlPath);

                    if (DynamoCommands.WriteToLogCmd.CanExecute(null))
                    {
                        DynamoCommands.WriteToLogCmd.Execute("Workbench could not be opened.");
                        DynamoCommands.WriteToLogCmd.Execute(xmlPath);
                    }
                }
                dynSettings.Bench.UnlockUI();
            }

            //clear the clipboard to avoid copying between dyns
            dynSettings.Controller.ClipBoard.Clear();
        }

        private bool CanOpen(object parameters)
        {
            return true;
        }
        
        private void ShowSaveImageDialogueAndSaveResult()
        {
            FileDialog _fileDialog;

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
            if (!string.IsNullOrEmpty(DynamoModel.Instance.CurrentSpace.FilePath))
            {
                var fi = new FileInfo(DynamoModel.Instance.CurrentSpace.FilePath);
                _fileDialog.InitialDirectory = fi.DirectoryName;
            }

            if (_fileDialog.ShowDialog() == DialogResult.OK)
            {
                DynamoCommands.SaveImageCmd.Execute(_fileDialog.FileName);
            }
        
        }

        private bool CanShowSaveImageDialogueAndSaveResult()
        {
            return true;
        }

        private void ShowOpenDialogAndOpenResult()
        {
            FileDialog _fileDialog;

            if (_fileDialog == null)
            {
                _fileDialog = new OpenFileDialog()
                {
                    Filter = "Dynamo Definitions (*.dyn; *.dyf)|*.dyn;*.dyf|All files (*.*)|*.*",
                    Title = "Open Dynamo Definition..."
                };
            }

            // if you've got the current space path, use it as the inital dir
            if (!string.IsNullOrEmpty(DynamoModel.Instance.CurrentSpace.FilePath))
            {
                var fi = new FileInfo(DynamoModel.Instance.CurrentSpace.FilePath);
                _fileDialog.InitialDirectory = fi.DirectoryName;
            }

            if (_fileDialog.ShowDialog() == DialogResult.OK)
            {
                DynamoCommands.OpenCmd.Execute(_fileDialog.FileName);
            }
        }

        private bool CanShowOpenDialogAndOpenResultCommand()
        {
            return true;
        }

        private void ShowSaveDialogIfNeededAndSaveResult()
        {
            if (DynamoModel.Instance.CurrentSpace.FilePath != null)
            {
                DynamoCommands.SaveCmd.Execute(null);
            }
            else
            {
                DynamoCommands.ShowSaveDialogAndSaveResultCmd.Execute(null);
            }
        }

        private bool CanShowSaveDialogIfNeededAndSaveResultCommand()
        {
            return true;
        }

        private void ShowSaveDialogAndSaveResult()
        {
            FileDialog _fileDialog;

            if (_fileDialog == null)
            {
                _fileDialog = new SaveFileDialog
                {
                    AddExtension = true,
                };
            }

            string ext, fltr;
            if (DynamoModel.Instance.ViewingHomespace)
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

            _fileDialog.FileName = DynamoModel.Instance.CurrentSpace.Name + ext;
            _fileDialog.AddExtension = true;
            _fileDialog.DefaultExt = ext;
            _fileDialog.Filter = fltr;

            //if the xmlPath is not empty set the default directory
            if (!string.IsNullOrEmpty(DynamoModel.Instance.CurrentSpace.FilePath))
            {
                var fi = new FileInfo(DynamoModel.Instance.CurrentSpace.FilePath);
                _fileDialog.InitialDirectory = fi.DirectoryName;
            }

            if (_fileDialog.ShowDialog() == DialogResult.OK)
            {
                DynamoModel.Instance.SaveAs(_fileDialog.FileName);
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

                if (dynSettings.FunctionDict.Values.Any(x => x.Workspace.Name == name))
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

            dynSettings.Controller.NewFunction(Guid.NewGuid(), name, category, true);
        }

        private bool CanShowNewFunctionDialogCommand()
        {
            return true;
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
            System.Diagnostics.Process.Start("https://github.com/ikeough/Dynamo/wiki");
        }

        private bool CanGoToWiki()
        {
            return true;
        }

        private void GoToSourceCode()
        {
            System.Diagnostics.Process.Start("https://github.com/ikeough/Dynamo");
        }

        private bool CanGoToSourceCode()
        {
            return true;
        }

        private void Exit()
        {
            dynSettings.Bench.Close();
        }

        private bool CanExit()
        {
            return true;
        }

        private void SaveAs(object parameters)
        {
            _model.SaveAs(parameters.ToString());
        }

        private bool CanSaveAs()
        {
            return true;
        }

        private void Clear()
        {
            dynSettings.Bench.LockUI();

            DynamoModel.Instance.CleanWorkbench();

            //don't save the file path
            DynamoModel.Instance.CurrentSpace.FilePath = "";

            dynSettings.Bench.UnlockUI();
        }

        private bool CanClear()
        {
            return true;
        }

        private void Home()
        {
            DynamoModel.Instance.ViewHomeWorkspace();
        }

        private bool CanGoHome()
        {
            return DynamoModel.Instance.CurrentSpace != DynamoModel.Instance.HomeSpace;
        }

        private void LayoutAll()
        {
            dynSettings.Bench.LockUI();
            dynSettings.Controller.CleanWorkbench();

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
                    t.IsSubclassOf(typeof(dynNode)))
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
                paramDict.Add("workspace", DynamoModel.Instance.CurrentSpace);
                DynamoCommands.AddNoteCmd.Execute(paramDict);

                y += 60;

                foreach (Type t in catTypes)
                {
                    object[] attribs = t.GetCustomAttributes(typeof(NodeNameAttribute), false);

                    NodeNameAttribute elNameAttrib = attribs[0] as NodeNameAttribute;
                    dynNode el = dynSettings.Controller.CreateInstanceAndAddNodeToWorkspace(
                           t, elNameAttrib.Name, Guid.NewGuid(), x, y,
                           DynamoModel.Instance.CurrentSpace
                        );

                    if (el == null) continue;

                    el.DisableReporting();

                    maxWidth = Math.Max(el.NodeUI.Width, maxWidth);

                    colCount++;

                    y += el.NodeUI.Height + rowGutter;

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

            dynSettings.Bench.UnlockUI();
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
                UIElement el = sel as UIElement;
                if (el != null)
                {
                    if (!dynSettings.Controller.ClipBoard.Contains(el))
                    {
                        dynSettings.Controller.ClipBoard.Add(el);

                        dynNodeUI n = el as dynNodeUI;
                        if (n != null)
                        {
                            var connectors = n.InPorts.SelectMany(x => x.Connectors)
                                .Concat(n.OutPorts.SelectMany(x => x.Connectors))
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

            var nodes = dynSettings.Controller.ClipBoard.Select(x => x).Where(x => x is dynNodeViewModel);

            var connectors = dynSettings.Controller.ClipBoard.Select(x => x).Where(x => x is dynConnector);

            foreach (dynNodeViewModel node in nodes)
            {
                //create a new guid for us to use
                Guid newGuid = Guid.NewGuid();
                nodeLookup.Add(node.GUID, newGuid);

                Dictionary<string, object> nodeData = new Dictionary<string, object>();
                nodeData.Add("x", Canvas.GetLeft(node));
                nodeData.Add("y", Canvas.GetTop(node) + 100);
                nodeData.Add("name", node.NickName);
                nodeData.Add("guid", newGuid);

                if (typeof(dynBasicInteractive<double>).IsAssignableFrom(node.NodeLogic.GetType()))
                {
                    nodeData.Add("value", (node.NodeLogic as dynBasicInteractive<double>).Value);
                }
                else if (typeof(dynBasicInteractive<string>).IsAssignableFrom(node.NodeLogic.GetType()))
                {
                    nodeData.Add("value", (node.NodeLogic as dynBasicInteractive<string>).Value);
                }
                else if (typeof(dynBasicInteractive<bool>).IsAssignableFrom(node.NodeLogic.GetType()))
                {
                    nodeData.Add("value", (node.NodeLogic as dynBasicInteractive<bool>).Value);
                }
                else if (typeof(dynVariableInput).IsAssignableFrom(node.NodeLogic.GetType()))
                {
                    //for list type nodes send the number of ports
                    //as the value - so we can setup the new node with
                    //the right number of ports
                    nodeData.Add("value", node.InPorts.Count);
                }

                dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.CreateNodeCmd, nodeData));
            }

            //process the command queue so we have 
            //nodes to connect to
            dynSettings.Controller.ProcessCommandQueue();

            //update the layout to ensure that the visuals
            //are present in the tree to connect to
            dynSettings.Bench.UpdateLayout();

            foreach (dynConnector c in connectors)
            {
                Dictionary<string, object> connectionData = new Dictionary<string, object>();

                dynNodeUI startNode = null;

                try
                {
                    startNode = dynSettings.Controller.CurrentSpace.Nodes
                        .Select(x => x.NodeUI)
                        .Where(x => x.GUID == (Guid)nodeLookup[c.Start.Owner.GUID]).FirstOrDefault();
                }
                catch
                {
                    //don't let users paste connectors between workspaces
                    if (c.Start.Owner.NodeLogic.WorkSpace == dynSettings.Controller.CurrentSpace)
                    {
                        startNode = c.Start.Owner;
                    }
                    else
                    {
                        continue;
                    }

                }

                connectionData.Add("start", startNode);

                connectionData.Add("end", dynSettings.Controller.CurrentSpace.Nodes
                    .Select(x => x.NodeUI)
                    .Where(x => x.GUID == (Guid)nodeLookup[c.End.Owner.GUID]).FirstOrDefault());

                connectionData.Add("port_start", c.Start.Index);
                connectionData.Add("port_end", c.End.Index);

                dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.CreateConnectionCmd, connectionData));
            }

            //process the queue again to create the connectors
            dynSettings.Controller.ProcessCommandQueue();

            foreach (DictionaryEntry de in nodeLookup)
            {
                dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(DynamoCommands.AddToSelectionCmd,
                    dynSettings.Controller.CurrentSpace.Nodes
                    .Select(x => x.NodeUI)
                    .Where(x => x.GUID == (Guid)de.Value).FirstOrDefault()));
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
            if (dynSettings.Bench.ConsoleShowing)
            {
                dynSettings.Bench.consoleRow.Height = new GridLength(0.0);
                dynSettings.Bench.ConsoleShowing = false;
            }
            else
            {
                dynSettings.Bench.consoleRow.Height = new GridLength(100.0);
                dynSettings.Bench.ConsoleShowing = true;
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
                foreach (dynNodeUI n in dynSettings.Controller.Nodes.Select(x => x.NodeUI))
                {
                    Point relativePoint = n.TransformToAncestor(dynSettings.Workbench)
                          .Transform(new Point(0, 0));

                    width = Math.Max(relativePoint.X + n.Width, width);
                    height = Math.Max(relativePoint.Y + n.Height, height);
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

        private void ClearLog()
        {
            dynSettings.Bench.sw.Flush();
            dynSettings.Bench.sw.Close();
            dynSettings.Bench.sw = new StringWriter();
            dynSettings.Bench.LogText = dynSettings.Bench.sw.ToString();
        }

        private bool CanClearLog()
        {
            return true;
        }

        private void RunExpression()
        {
            dynSettings.Controller.RunExpression(Convert.ToBoolean(parameters));
        }

        private bool CanRunExpression()
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
            if (parameter is Guid && dynSettings.FunctionDict.ContainsKey((Guid)parameter))
            {
                _model.ViewCustomNodeWorkspace(dynSettings.FunctionDict[(Guid)parameter]);
            }
        }

        private bool CanGoToWorkspace(object parameter)
        {
            return true;
        }

        private void DisplayFunction(object parameters)
        {
            _model.ViewCustomNodeWorkspace((parameters as FunctionDefinition));
        }

        private bool CanDisplayFunction(object parameters)
        {
            FunctionDefinition fd = parameters as FunctionDefinition;
            if (fd == null)
            {
                return false;
            }

            return true;
        }

        private void SetConnectorType(object parameters)
        {
            if (parameters.ToString() == "BEZIER")
            {
                _model.CurrentSpace.Connectors.ForEach(x => x.ConnectorType = ConnectorType.BEZIER);
            }
            else
            {
                _model.CurrentSpace.Connectors.ForEach(x => x.ConnectorType = ConnectorType.POLYLINE);
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
    }

    //MVVM:Removed the splash screen commands
    //public class ShowSplashScreenCommand : ICommand
    //{
    //    public ShowSplashScreenCommand()
    //    {

    //    }

    //    public void Execute(object parameters)
    //    {
    //        if (dynSettings.Controller.SplashScreen == null)
    //        {
    //            dynSettings.Controller.SplashScreen = new Controls.DynamoSplash();
    //        }
    //        dynSettings.Controller.SplashScreen.Show();
    //    }

    //    public event EventHandler CanExecuteChanged
    //    {
    //        add { CommandManager.RequerySuggested += value; }
    //        remove { CommandManager.RequerySuggested -= value; }
    //    }

    //    public bool CanExecute(object parameters)
    //    {
    //        if (dynSettings.Controller != null)
    //        {
    //            return true;
    //        }

    //        return false;
    //    }
    //}

    //public class CloseSplashScreenCommand : ICommand
    //{
    //    public void Execute(object parameters)
    //    {
    //        dynSettings.Controller.SplashScreen.Close();
    //    }

    //    public event EventHandler CanExecuteChanged
    //    {
    //        add { CommandManager.RequerySuggested += value; }
    //        remove { CommandManager.RequerySuggested -= value; }
    //    }

    //    public bool CanExecute(object parameters)
    //    {
    //        if (dynSettings.Controller.SplashScreen != null)
    //        {
    //            return true;
    //        }

    //        return false;
    //    }
    //}
}
