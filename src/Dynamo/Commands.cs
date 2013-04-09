using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows;
using System.Collections;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.IO;
using Dynamo.Controls;
using Dynamo.Utilities;
using Dynamo.Nodes;
using Dynamo.Connectors;

namespace Dynamo.Commands
{

    public static partial class DynamoCommands
    {
        private static ToggleShowingClassicNodeNavigatorCommand toggleShowingClassicNodeNavigatorCmd;
        public static ToggleShowingClassicNodeNavigatorCommand ShowClassicNodeNavigatorCmd
        {
            get
            {
                if (toggleShowingClassicNodeNavigatorCmd == null)
                    toggleShowingClassicNodeNavigatorCmd = new ToggleShowingClassicNodeNavigatorCommand();
                return toggleShowingClassicNodeNavigatorCmd;
            }
        }

        private static ShowNewFunctionDialogCommand showNewFunctionDialogCmd;
        public static ShowNewFunctionDialogCommand ShowNewFunctionDialogCmd
        {
            get
            {
                if (showNewFunctionDialogCmd == null)
                    showNewFunctionDialogCmd = new ShowNewFunctionDialogCommand();
                return showNewFunctionDialogCmd;
            }
        }

        private static ShowSaveImageDialogAndSaveResultCommand showSaveImageDialogAndSaveResultCmd;
        public static ShowSaveImageDialogAndSaveResultCommand ShowSaveImageDialogAndSaveResultCmd
        {
            get
            {
                if (showSaveImageDialogAndSaveResultCmd == null)
                    showSaveImageDialogAndSaveResultCmd = new ShowSaveImageDialogAndSaveResultCommand();
                return showSaveImageDialogAndSaveResultCmd;
            }
        }

        private static ShowOpenDialogAndOpenResultCommand showOpenDialogAndOpenResultCmd;
        public static ShowOpenDialogAndOpenResultCommand ShowOpenDialogAndOpenResultCmd
        {
            get
            {
                if (showOpenDialogAndOpenResultCmd == null)
                    showOpenDialogAndOpenResultCmd = new ShowOpenDialogAndOpenResultCommand();
                return showOpenDialogAndOpenResultCmd;
            }
        }

        private static ShowSaveDialogIfNeededAndSaveResultCommand showSaveDialogIfNeededAndSaveResultCmd;
        public static ShowSaveDialogIfNeededAndSaveResultCommand ShowSaveDialogIfNeededAndSaveResultCmd
        {
            get
            {
                if (showSaveDialogIfNeededAndSaveResultCmd == null)
                    showSaveDialogIfNeededAndSaveResultCmd = new ShowSaveDialogIfNeededAndSaveResultCommand();
                return showSaveDialogIfNeededAndSaveResultCmd;
            }
        }

        private static ShowSaveDialogAndSaveResultCommand showSaveDialogAndSaveResultCmd;
        public static ShowSaveDialogAndSaveResultCommand ShowSaveDialogAndSaveResultCmd
        {
            get
            {
                if (showSaveDialogAndSaveResultCmd == null)
                    showSaveDialogAndSaveResultCmd = new ShowSaveDialogAndSaveResultCommand();
                return showSaveDialogAndSaveResultCmd;
            }
        }

        private static GoToWorkspaceCommand goToWorkspaceCmd;
        public static GoToWorkspaceCommand GoToWorkspaceCmd
        {
            get
            {
                if (goToWorkspaceCmd == null)
                    goToWorkspaceCmd = new GoToWorkspaceCommand();
                return goToWorkspaceCmd;
            }
        }

        private static GoToSourceCodeCommand goToSourceCodeCmd;
        public static GoToSourceCodeCommand GoToSourceCodeCmd
        {
            get
            {
                if (goToSourceCodeCmd == null)
                    goToSourceCodeCmd = new GoToSourceCodeCommand();
                return goToSourceCodeCmd;
            }
        }

        private static GoToWikiCommand goToWikiCmd;
        public static GoToWikiCommand GoToWikiCmd
        {
            get
            {
                if (goToWikiCmd == null)
                    goToWikiCmd = new GoToWikiCommand();
                return goToWikiCmd;
            }
        }

        private static ExitCommand exitCmd;
        public static ExitCommand ExitCmd
        {
            get
            {
                if (exitCmd == null)
                    exitCmd = new ExitCommand();
                return exitCmd;
            }
        }

        private static NodeFromSelectionCommand nodeFromSelectionCmd;
        public static NodeFromSelectionCommand NodeFromSelectionCmd
        {
            get
            {
                if (nodeFromSelectionCmd == null)
                    nodeFromSelectionCmd = new NodeFromSelectionCommand();

                return nodeFromSelectionCmd;
            }
        }

        private static SelectNeighborsCommand selectNeighborsCmd;
        public static SelectNeighborsCommand SelectNeighborsCmd
        {
            get
            {
                if (selectNeighborsCmd == null)
                    selectNeighborsCmd = new SelectNeighborsCommand();

                return selectNeighborsCmd;
            }
        }

        private static AddNoteCommand addNoteCmd;
        public static AddNoteCommand AddNoteCmd
        {
            get
            {
                if (addNoteCmd == null)
                    addNoteCmd = new AddNoteCommand();

                return addNoteCmd;
            }
        }

        private static DeleteCommand deleteCmd;
        public static DeleteCommand DeleteCmd
        {
            get
            {
                if (deleteCmd == null)
                    deleteCmd = new DeleteCommand();

                return deleteCmd;
            }
        }

        private static ShowSplashScreenCommand showSplashScreenCmd;
        public static ShowSplashScreenCommand ShowSplashScreenCmd
        {
            get
            {
                if (showSplashScreenCmd == null)
                    showSplashScreenCmd = new ShowSplashScreenCommand();

                return showSplashScreenCmd;
            }
        }

        private static CloseSplashScreenCommand closeSplashScreenCmd;
        public static CloseSplashScreenCommand CloseSplashScreenCmd
        {
            get
            {
                if (closeSplashScreenCmd == null)
                    closeSplashScreenCmd = new CloseSplashScreenCommand();

                return closeSplashScreenCmd;
            }
        }

        private static WriteToLogCommand writeToLogCmd;
        public static WriteToLogCommand WriteToLogCmd
        {
            get
            {
                if (writeToLogCmd == null)
                    writeToLogCmd = new WriteToLogCommand();

                return writeToLogCmd;
            }
        }

        private static CreateNodeCommand createNodeCmd;
        public static CreateNodeCommand CreateNodeCmd
        {
            get
            {
                if (createNodeCmd == null)
                    createNodeCmd = new CreateNodeCommand();

                return createNodeCmd;
            }
        }

        private static CreateConnectionCommand createConnectionCmd;
        public static CreateConnectionCommand CreateConnectionCmd
        {
            get
            {
                if (createConnectionCmd == null)
                    createConnectionCmd = new CreateConnectionCommand();

                return createConnectionCmd;
            }
        }

        private static RunExpressionCommand runExpressionCommand;
        public static RunExpressionCommand RunExpressionCmd
        {
            get
            {
                if (runExpressionCommand == null)
                    runExpressionCommand = new RunExpressionCommand();

                return runExpressionCommand;
            }
        }

        private static CopyCommand copyCmd;
        public static CopyCommand CopyCmd
        {
            get
            {
                if (copyCmd == null)
                    copyCmd = new CopyCommand();

                return copyCmd;
            }
        }

        private static PasteCommand pasteCmd;
        public static PasteCommand PasteCmd
        {
            get
            {
                if (pasteCmd == null)
                    pasteCmd = new PasteCommand();

                return pasteCmd;
            }
        }

        private static SelectCommand selectCmd;
        public static SelectCommand SelectCmd
        {
            get
            {
                if (selectCmd == null)
                    selectCmd = new SelectCommand();

                return selectCmd;
            }
        }

        private static AddToSelectionCommand addToSelectionCmd;
        public static AddToSelectionCommand AddToSelectionCmd
        {
            get
            {
                if (addToSelectionCmd == null)
                    addToSelectionCmd = new AddToSelectionCommand();

                return addToSelectionCmd;
            }
        }

        private static ToggleConsoleShowingCommand _toggleConsoleShowingCmd;
        public static ToggleConsoleShowingCommand ToggleConsoleShowingCmd
        {
            get
            {
                if (_toggleConsoleShowingCmd == null)
                    _toggleConsoleShowingCmd = new ToggleConsoleShowingCommand();

                return _toggleConsoleShowingCmd;
            }
        }

        private static CancelRunCommand cancelRunCmd;
        public static CancelRunCommand CancelRunCmd
        {
            get
            {
                if (cancelRunCmd == null)
                    cancelRunCmd = new CancelRunCommand();

                return cancelRunCmd;
            }
        }

        private static SaveAsCommand saveAsCmd;
        public static SaveAsCommand SaveAsCmd
        {
            get
            {
                if (saveAsCmd == null)
                    saveAsCmd = new SaveAsCommand();

                return saveAsCmd;
            }
        }

        private static SaveCommand saveCmd;
        public static SaveCommand SaveCmd
        {
            get
            {
                if (saveCmd == null)
                    saveCmd = new SaveCommand();

                return saveCmd;
            }
        }

        private static OpenCommand openCmd;
        public static OpenCommand OpenCmd
        {
            get
            {
                if (openCmd == null)
                    openCmd = new OpenCommand();

                return openCmd;
            }
        }

        private static HomeCommand homeCmd;
        public static HomeCommand HomeCmd
        {
            get
            {
                if (homeCmd == null)
                    homeCmd = new HomeCommand();

                return homeCmd;
            }
        }

        private static SaveImageCommand saveImageCmd;
        public static SaveImageCommand SaveImageCmd
        {
            get
            {
                if (saveImageCmd == null)
                    saveImageCmd = new SaveImageCommand();

                return saveImageCmd;
            }
        }

        private static LayoutAllCommand layoutAllCmd;
        public static LayoutAllCommand LayoutAllCmd
        {
            get
            {
                if (layoutAllCmd == null)
                    layoutAllCmd = new LayoutAllCommand();

                return layoutAllCmd;
            }
        }

        private static ClearCommand clearCmd;
        public static ClearCommand ClearCmd
        {
            get
            {
                if (clearCmd == null)
                    clearCmd = new ClearCommand();

                return clearCmd;
            }
        }

        private static ClearLogCommand clearLogCmd;
        public static ClearLogCommand ClearLogCmd
        {
            get
            {
                if (clearLogCmd == null)
                    clearLogCmd = new ClearLogCommand();

                return clearLogCmd;
            }
        }

        private static DisplayFunctionCommand displayFunctionCmd;
        public static DisplayFunctionCommand DisplayFunctionCmd
        {
            get
            {
                if (displayFunctionCmd == null)
                    displayFunctionCmd = new DisplayFunctionCommand();

                return displayFunctionCmd;
            }
        }
    }

    public class ToggleShowingClassicNodeNavigatorCommand : ICommand
    {

        public void Execute(object parameters)
        {
            if (dynSettings.Bench.sidebarGrid.Visibility == Visibility.Visible)
            {
                dynSettings.Bench.sidebarGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                dynSettings.Bench.sidebarGrid.Visibility = Visibility.Visible;
            }
            
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameters)
        {
            return true;
        }
    }


    public class ShowSaveImageDialogAndSaveResultCommand : ICommand
    {
        private FileDialog _fileDialog;

        public void Execute(object parameters)
        {
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
            if (!string.IsNullOrEmpty(dynSettings.Controller.CurrentSpace.FilePath))
            {
                var fi = new FileInfo(dynSettings.Controller.CurrentSpace.FilePath);
                _fileDialog.InitialDirectory = fi.DirectoryName;
            }

            if (_fileDialog.ShowDialog() == DialogResult.OK)
            {
                DynamoCommands.SaveImageCmd.Execute(_fileDialog.FileName);
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameters)
        {
            return true;
        }
    }

    public class ShowOpenDialogAndOpenResultCommand : ICommand
    {
        private FileDialog _fileDialog;

        public void Execute(object parameters)
        {
            if (_fileDialog == null)
            {
                _fileDialog = new OpenFileDialog()
                {
                    Filter = "Dynamo Definitions (*.dyn; *.dyf)|*.dyn;*.dyf|All files (*.*)|*.*",
                    Title = "Open Dynamo Definition..."
                };
            }

            // if you've got the current space path, use it as the inital dir
            if (!string.IsNullOrEmpty(dynSettings.Controller.CurrentSpace.FilePath))
            {
                var fi = new FileInfo(dynSettings.Controller.CurrentSpace.FilePath);
                _fileDialog.InitialDirectory = fi.DirectoryName;
            }

            if (_fileDialog.ShowDialog() == DialogResult.OK)
            {
                DynamoCommands.OpenCmd.Execute(_fileDialog.FileName);
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameters)
        {
            return true;
        }
    }

    public class ShowSaveDialogIfNeededAndSaveResultCommand : ICommand
    {
        public void Execute(object parameters)
        {
            if (dynSettings.Controller.CurrentSpace.FilePath != null)
            {
                DynamoCommands.SaveCmd.Execute(null);
            }
            else
            {
                DynamoCommands.ShowSaveDialogAndSaveResultCmd.Execute(null);
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameters)
        {
            return true;
        }
    }

    public class ShowSaveDialogAndSaveResultCommand : ICommand
    {

        private FileDialog _fileDialog;

        public void Execute(object parameters)
        {
            if (_fileDialog == null)
            {
                _fileDialog = new SaveFileDialog
                {
                    AddExtension = true,
                };
            }

            string ext, fltr;
            if ( dynSettings.Controller.ViewingHomespace )
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
          
            _fileDialog.FileName = dynSettings.Controller.CurrentSpace.Name + ext;
            _fileDialog.AddExtension = true;
            _fileDialog.DefaultExt = ext;
            _fileDialog.Filter = fltr;

            //if the xmlPath is not empty set the default directory
            if (!string.IsNullOrEmpty(dynSettings.Controller.CurrentSpace.FilePath))
            {
                var fi = new FileInfo(dynSettings.Controller.CurrentSpace.FilePath);
                _fileDialog.InitialDirectory = fi.DirectoryName;
            }

            if (_fileDialog.ShowDialog() == DialogResult.OK)
            {
                dynSettings.Controller.SaveAs(_fileDialog.FileName);
            }
          
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameters)
        {
            return true;
        }
    }

    public class ShowNewFunctionDialogCommand : ICommand
    {

        public void Execute(object parameters)
        {
            //First, prompt the user to enter a name
            string name, category;
            string error = "";

            do
            {
                var dialog = new FunctionNamePrompt( dynSettings.Bench.addMenuCategoryDict.Keys, error);
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

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameters)
        {
            return true;
        }
    }

    public class GoToWikiCommand : ICommand
    {

        public void Execute(object parameters)
        {
            System.Diagnostics.Process.Start("https://github.com/ikeough/Dynamo/wiki");
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameters)
        {
            return true;
        }
    }

    public class GoToSourceCodeCommand : ICommand
    {
        public void Execute(object parameters)
        {
            System.Diagnostics.Process.Start("https://github.com/ikeough/Dynamo");
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameters)
        {
            return true;
        }
    }

    public class ExitCommand : ICommand
    {
        public void Execute(object parameters)
        {
            dynSettings.Bench.Close();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameters)
        {
            return true;
        }
    }

    public class NodeFromSelectionCommand : ICommand
    {
        public NodeFromSelectionCommand()
        {
            //TODO: figure out how to wire the selection changed event to 
            //evaluate if this can be executed. we can't do this currently
            //as dynSettings.Bench is null when the commands are instantiated
            //dynSettings.Bench.WorkBench.Selection.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Selection_CollectionChanged);
        }

        void Selection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CanExecute(null);
        }

        public void Execute(object parameters)
        {
            if (dynSettings.Bench.WorkBench.Selection.Count > 0)
            {
                dynSettings.Bench.Controller.NodeFromSelection(
                    dynSettings.Bench.WorkBench.Selection.Where(x => x is dynNodeUI)
                        .Select(x => (x as dynNodeUI).NodeLogic));
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameters)
        {
            return true;
        }
    }

    public class SelectNeighborsCommand : ICommand
    {

        public void Execute(object parameters)
        {
            List<ISelectable> sels = dynSettings.Workbench.Selection.ToList<ISelectable>();

            foreach (ISelectable sel in sels)
            {
                ((dynNodeUI)sel).SelectNeighbors();
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameters)
        {
            return true;
        }
    }

    public class AddNoteCommand : ICommand
    {
        public void Execute(object parameters)
        {
            Dictionary<string,object> inputs = (Dictionary<string,object>) parameters;

            dynNote n = new dynNote();
            Canvas.SetLeft(n, (double)inputs["x"]);
            Canvas.SetTop(n, (double)inputs["y"]);
            n.noteText.Text = inputs["text"].ToString();
            dynWorkspace ws = (dynWorkspace)inputs["workspace"];

            ws.Notes.Add(n);
            dynSettings.Bench.WorkBench.Children.Add(n);

            if (!dynSettings.Bench.Controller.ViewingHomespace)
            {
                dynSettings.Bench.Controller.CurrentSpace.Modified();
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameters)
        {
            return true;
        }
    }

    public class DeleteCommand : ICommand
    {
        public void Execute(object parameters)
        {
            //if you get an object in the parameters, just delete that object
            if (parameters != null)
            {
                dynNote note = parameters as dynNote;
                dynNodeUI node = parameters as dynNodeUI;

                if (node != null)
                {
                    DeleteNode(node);
                }
                else if (note != null)
                {
                    DeleteNote(note);
                }
            }
            else
            {
                for (int i = dynSettings.Workbench.Selection.Count - 1; i >= 0; i--)
                {
                    dynNote note = dynSettings.Workbench.Selection[i] as dynNote;
                    dynNodeUI node = dynSettings.Workbench.Selection[i] as dynNodeUI;

                    if (node != null)
                    {
                        DeleteNode(node);
                    }
                    else if (note != null)
                    {
                        DeleteNote(note);
                    }
                }
            }
        }

        private static void DeleteNote(dynNote note)
        {
            dynSettings.Workbench.Selection.Remove(note);
            dynSettings.Controller.CurrentSpace.Notes.Remove(note);
            dynSettings.Workbench.Children.Remove(note);
        }

        private static void DeleteNode(dynNodeUI node)
        {
            foreach (var port in node.OutPorts)
            {
                for (int j = port.Connectors.Count - 1; j >= 0; j--)
                {
                    port.Connectors[j].Kill();
                }
            }

            foreach (dynPort p in node.InPorts)
            {
                for (int j = p.Connectors.Count - 1; j >= 0; j--)
                {
                    p.Connectors[j].Kill();
                }
            }

            node.NodeLogic.Cleanup();
            dynSettings.Workbench.Selection.Remove(node);
            dynSettings.Controller.Nodes.Remove(node.NodeLogic);
            dynSettings.Workbench.Children.Remove(node);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameters)
        {
            return dynSettings.Workbench.Selection.Count > 0;
        }
    }

    public class ShowSplashScreenCommand : ICommand
    {
        public ShowSplashScreenCommand()
        {

        }

        public void Execute(object parameters)
        {
            dynSettings.Controller.SplashScreen = new Controls.DynamoSplash();
            dynSettings.Controller.SplashScreen.Show();
        }

        public event EventHandler CanExecuteChanged 
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameters)
        {
            if (dynSettings.Controller != null)
            {
                return true;
            }

            return false;
        }
    }

    public class CloseSplashScreenCommand : ICommand
    {
        public void Execute(object parameters)
        {
            dynSettings.Controller.SplashScreen.Close();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameters)
        {
            if (dynSettings.Controller.SplashScreen != null)
            {
                return true;
            }

            return false;
        }
    }

    public class WriteToLogCommand : ICommand
    {
        public void Execute(object parameters)
        {
            if (parameters == null) return;

            string logText = parameters.ToString();
            dynSettings.Writer.WriteLine(logText);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameters)
        {
            if (dynSettings.Writer != null)
            {
                return true;
            }

            return false;
        }
    }

    public class GoToWorkspaceCommand : ICommand
    {
        public void Execute(object parameter)
        {
           if (parameter is Guid && dynSettings.FunctionDict.ContainsKey( (Guid)parameter ) )
           {
               dynSettings.Controller.DisplayFunction( dynSettings.FunctionDict[ (Guid) parameter] );   
           }     
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameters)
        {
            return true;
        }
    }

    public class CreateNodeCommand : ICommand
    {
        public void Execute(object parameters)
        {
            Dictionary<string, object> data = parameters as Dictionary<string, object>;
            if (data == null)
            {
                return;
            }

            dynNode node = dynSettings.Controller.CreateDragNode( data["name"].ToString() );
 
            dynNodeUI nodeUi = node.NodeUI; 
            if (dynSettings.Workbench != null)
            {
                dynSettings.Workbench.Children.Add(nodeUi);
            }
                
            dynSettings.Controller.Nodes.Add(nodeUi.NodeLogic);
            nodeUi.NodeLogic.WorkSpace = dynSettings.Controller.CurrentSpace;
            nodeUi.Opacity = 1;
            
            //if we've received a value in the dictionary
            //try to set the value on the node
            if(data.ContainsKey("value"))
            {
                if (typeof(dynBasicInteractive<double>).IsAssignableFrom(node.GetType()))
                {
                    (node as dynBasicInteractive<double>).Value = (double)data["value"];
                }
                else if (typeof(dynBasicInteractive<string>).IsAssignableFrom(node.GetType()))
                {
                    (node as dynBasicInteractive<string>).Value = data["value"].ToString();
                }
                else if(typeof(dynBasicInteractive<bool>).IsAssignableFrom(node.GetType()))
                {
                    (node as dynBasicInteractive<bool>).Value = (bool)data["value"];
                }
                else if(typeof(dynVariableInput).IsAssignableFrom(node.GetType()))
                {
                    int desiredPortCount = (int)data["value"];
                    if (node.InPortData.Count < desiredPortCount)
                    {
                        int portsToCreate = desiredPortCount - node.InPortData.Count;

                        for (int i = 0; i < portsToCreate; i++)
                        {
                            (node as dynVariableInput).AddInput();
                        }
                        (node as dynVariableInput).NodeUI.RegisterAllPorts();
                    }
                }
            }

            //override the guid so we can store
            //for connection lookup
            if (data.ContainsKey("guid"))
            {
                node.NodeUI.GUID = (Guid) data["guid"];
            }
            else
            {
                node.NodeUI.GUID = Guid.NewGuid();
            }

            // by default place node at center
            var x = 0.0;
            var y = 0.0;
            if (dynSettings.Bench != null)
            {
                x = dynSettings.Bench.outerCanvas.ActualWidth / 2.0;
                y = dynSettings.Bench.outerCanvas.ActualHeight / 2.0;
            }
            
            var transformFromOuterCanvas = data.ContainsKey("transformFromOuterCanvasCoordinates");
               
            if ( data.ContainsKey("x") )
                x = (double) data["x"];

            if ( data.ContainsKey("y") )
                y = (double) data["y"];
                
            Point dropPt = new Point(x, y);

            // Transform dropPt from outerCanvas space into zoomCanvas space
            if ( transformFromOuterCanvas )
            {
                var a = dynSettings.Bench.outerCanvas.TransformToDescendant(dynSettings.Bench.WorkBench);
                dropPt = a.Transform(dropPt);
            }

            // center the node at the drop point
            dropPt.X -= (nodeUi.Width / 2.0);
            dropPt.Y -= (nodeUi.Height / 2.0);

            Canvas.SetLeft(nodeUi, dropPt.X);
            Canvas.SetTop(nodeUi, dropPt.Y);

            nodeUi.EnableInteraction();

            if (dynSettings.Controller.ViewingHomespace)
            {
                nodeUi.NodeLogic.SaveResult = true;
            }
            
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameters)
        {
            Dictionary<string, object> data = parameters as Dictionary<string, object>;

            if (data != null &&
                dynSettings.Controller.BuiltInTypesByNickname.ContainsKey(data["name"].ToString()) || dynSettings.FunctionDict.ContainsKey( Guid.Parse( (string) data["name"] ) ) )
            {
                return true;
            }

            return false;
        }
    }

    public class CreateConnectionCommand : ICommand
    {
        public void Execute(object parameters)
        {
            Dictionary<string,object> connectionData = parameters as Dictionary<string,object>;
            
            dynNodeUI start = (dynNodeUI)connectionData["start"];
            dynNodeUI end = (dynNodeUI)connectionData["end"];
            int startIndex = (int)connectionData["port_start"];
            int endIndex = (int)connectionData["port_end"];

            dynConnector c = new dynConnector(start, end, startIndex, endIndex, 0);

            dynSettings.Controller.CurrentSpace.Connectors.Add(c);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameters)
        {
            //make sure you have valid connection data
            Dictionary<string,object> connectionData = parameters as Dictionary<string,object>;
            if (connectionData != null && connectionData.Count == 4)
            {
                return true;
            }

            return false;
        }
    }

    public class RunExpressionCommand : ICommand
    {
        public void Execute(object parameters)
        {
            dynSettings.Controller.RunExpression(Convert.ToBoolean(parameters));
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameters)
        {
            //TODO: Any reason we wouldn't be able to run an expression?
            if(dynSettings.Controller == null)
            {
                return false;
            }
            return true;
        }
    }

    public class CopyCommand : ICommand
    {
        public void Execute(object parameters)
        {
            dynSettings.Controller.ClipBoard.Clear();

            foreach (ISelectable sel in dynSettings.Workbench.Selection)
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
                                .Where(x=>x.End != null && 
                                    x.End.Owner.IsSelected &&
                                    !dynSettings.Controller.ClipBoard.Contains(x));

                            dynSettings.Controller.ClipBoard.AddRange(connectors);
                        }
                    }
                }
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameters)
        {
            //TODO: Any reason we wouldn't be able to run an expression?
            if (dynSettings.Workbench.Selection.Count == 0)
            {
                return false;
            }
            return true;
        }
    }

    public class PasteCommand : ICommand
    {
        public void Execute(object parameters)
        {
            //make a lookup table to store the guids of the
            //old nodes and the guids of their pasted versions
            Hashtable nodeLookup = new Hashtable();

            //clear the selection so we can put the
            //paste contents in
            dynSettings.Bench.WorkBench.Selection.RemoveAll();

            var nodes = dynSettings.Controller.ClipBoard.Select(x => x).Where(x=>x is dynNodeUI);
            var connectors = dynSettings.Controller.ClipBoard.Select(x => x).Where(x => x is dynConnector);

            foreach (dynNodeUI node in nodes)
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
                else if(typeof(dynVariableInput).IsAssignableFrom(node.NodeLogic.GetType()))
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
                    .Select(x=>x.NodeUI)
                    .Where(x=>x.GUID == (Guid)nodeLookup[c.End.Owner.GUID]).FirstOrDefault());

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

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameters)
        {
            if (dynSettings.Controller.ClipBoard.Count == 0)
            {
                return false;
            }

            return true;
        }
    }

    public class SelectCommand : ICommand
    {
        public void Execute(object parameters)
        {
            dynNodeUI node = parameters as dynNodeUI;

            if (!node.IsSelected)
            {
                if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
                {
                    dynSettings.Bench.WorkBench.ClearSelection();
                }

                if (!dynSettings.Bench.WorkBench.Selection.Contains(node))
                    dynSettings.Bench.WorkBench.Selection.Add(node);
            }
            else
            {
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    dynSettings.Bench.WorkBench.Selection.Remove(node);
                }
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameters)
        {
            dynNodeUI node = parameters as dynNodeUI;
            if (node == null)
            {
                return false;
            }

            return true;
        }
    }

    public class AddToSelectionCommand : ICommand
    {
        public void Execute(object parameters)
        {
            dynNodeUI node = parameters as dynNodeUI;

            if (!node.IsSelected)
            {
                if (!dynSettings.Bench.WorkBench.Selection.Contains(node))
                    dynSettings.Bench.WorkBench.Selection.Add(node);
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameters)
        {
            dynNodeUI node = parameters as dynNodeUI;
            if (node == null)
            {
                return false;
            }

            return true;
        }
    }

    public class ToggleConsoleShowingCommand : ICommand
    {
        public void Execute(object parameters)
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

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameters)
        {
            return true;
        }
    }

    public class CancelRunCommand : ICommand
    {
        public void Execute(object parameters)
        {
            dynSettings.Controller.RunCancelled = true;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameters)
        {
            return true;
        }
    }

    public class SaveAsCommand : ICommand
    {
        public void Execute(object parameters)
        {
            if (parameters is string)
                dynSettings.Controller.SaveAs( (string) parameters );
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameters)
        {
            return true;
        }
    }

    public class SaveCommand : ICommand
    {
        public void Execute(object parameters)
        {
            dynSettings.Controller.Save();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameters)
        {
            return true;
        }
    }

    public class OpenCommand : ICommand
    {
        public void Execute(object parameters)
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

                if (!dynSettings.Controller.OpenDefinition(xmlPath))
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

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameters)
        {
            return true;
        }
    }

    public class HomeCommand : ICommand
    {
        public void Execute(object parameters)
        {
            dynSettings.Controller.ViewHomeWorkspace();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameters)
        {
            return true;
        }
    }

    public class SaveImageCommand : ICommand
    {
        public void Execute(object parameters)
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

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameters)
        {
            return true;
        }
    }

    public class LayoutAllCommand : ICommand
    {
        public void Execute(object parameters)
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
                paramDict.Add("workspace", dynSettings.Controller.CurrentSpace);
                DynamoCommands.AddNoteCmd.Execute(paramDict);

                y += 60;

                foreach (Type t in catTypes)
                {
                    object[] attribs = t.GetCustomAttributes(typeof(NodeNameAttribute), false);

                    NodeNameAttribute elNameAttrib = attribs[0] as NodeNameAttribute;
                    dynNode el = dynSettings.Controller.AddDynElement(
                           t, elNameAttrib.Name, Guid.NewGuid(), x, y,
                           dynSettings.Controller.CurrentSpace
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

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameters)
        {
            return true;
        }
    }

    public class ClearCommand : ICommand
    {
        public void Execute(object parameters)
        {
            dynSettings.Bench.LockUI();
            dynSettings.Controller.CleanWorkbench();

            //don't save the file path
            dynSettings.Controller.CurrentSpace.FilePath = "";

            dynSettings.Bench.UnlockUI();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameters)
        {
            return true;
        }
    }

    public class ClearLogCommand : ICommand
    {
        public void Execute(object parameters)
        {
            dynSettings.Bench.sw.Flush();
            dynSettings.Bench.sw.Close();
            dynSettings.Bench.sw = new StringWriter();
            dynSettings.Bench.LogText = dynSettings.Bench.sw.ToString();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameters)
        {
            return true;
        }
    }

    public class DisplayFunctionCommand : ICommand
    {
        public void Execute(object parameters)
        {
            dynSettings.Controller.DisplayFunction((parameters as FunctionDefinition));
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameters)
        {
            FunctionDefinition fd = parameters as FunctionDefinition;
            if(fd == null)
            {
                return false;
            }

            return true;
        }
    }

}
