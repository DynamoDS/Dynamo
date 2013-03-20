using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows;
using System.Collections;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Reflection;
using System.IO;

using Dynamo.Controls;
using Dynamo.Utilities;
using Dynamo.Nodes;
using Dynamo.Connectors;

//http://msdn.microsoft.com/en-us/library/ms752308.aspx

namespace Dynamo.Commands
{
    public static class DynamoCommands
    {
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

        private static ShowConsoleCommand showConsoleCmd;
        public static ShowConsoleCommand ShowConsoleCmd
        {
            get
            {
                if (showConsoleCmd == null)
                    showConsoleCmd = new ShowConsoleCommand();

                return showConsoleCmd;
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
        public SelectNeighborsCommand()
        {

        }

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
        public AddNoteCommand()
        {

        }

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
        public DeleteCommand()
        {

        }

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
        public CloseSplashScreenCommand()
        {

        }

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
        public WriteToLogCommand()
        {

        }

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

    public class CreateNodeCommand : ICommand
    {
        public CreateNodeCommand()
        {

        }

        public void Execute(object parameters)
        {

            Dictionary<string, object> data = parameters as Dictionary<string, object>;
            if (data == null)
            {
                return;
            }

            TypeLoadData tld = dynSettings.Controller.BuiltInTypesByNickname[data["name"].ToString()];

            var obj = Activator.CreateInstanceFrom(tld.Assembly.Location, tld.Type.FullName);
            var node = (dynNode)obj.Unwrap();
            node.NodeUI.DisableInteraction();

            var el = node.NodeUI;

            dynSettings.Workbench.Children.Add(el);
            dynSettings.Controller.Nodes.Add(el.NodeLogic);
            el.NodeLogic.WorkSpace = dynSettings.Controller.CurrentSpace;
            el.Opacity = 1;

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
            }

            //override the guid so we can store
            //for connection lookup
            if (data.ContainsKey("guid"))
            {
                node.NodeUI.GUID = (Guid)data["guid"];
            }

            Point dropPt = new Point((double)data["x"], (double)data["y"]);
            Canvas.SetLeft(el, dropPt.X);
            Canvas.SetTop(el, dropPt.Y);

            el.EnableInteraction();

            if (dynSettings.Controller.ViewingHomespace)
            {
                el.NodeLogic.SaveResult = true;
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
                dynSettings.Controller.BuiltInTypesByNickname.ContainsKey(data["name"].ToString()))
            {
                return true;
            }

            return false;
        }
    }

    public class CreateConnectionCommand : ICommand
    {
        public CreateConnectionCommand()
        {

        }

        public void Execute(object parameters)
        {
            Dictionary<string,object> connectionData = parameters as Dictionary<string,object>;
            
            dynNodeUI start = (dynNodeUI)connectionData["start"];
            dynNodeUI end = (dynNodeUI)connectionData["end"];
            int startIndex = (int)connectionData["port_start"];
            int endIndex = (int)connectionData["port_end"];

            dynConnector c = new dynConnector(start, end, startIndex, endIndex, 0);
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
        public RunExpressionCommand()
        {

        }

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
        public CopyCommand()
        {

        }

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
        public PasteCommand()
        {

        }

        public void Execute(object parameters)
        {
            //make a lookup table to store the guids of the
            //old nodes and the guids of their pasted versions
            Hashtable nodeLookup = new Hashtable();

            //clear the selection so we can put the
            //paste contents in
            dynSettings.Bench.WorkBench.Selection.RemoveAll();

            var nodes = dynSettings.Controller.ClipBoard.Select(x => x).Where(x=>x.GetType().IsAssignableFrom(typeof(dynNodeUI)));
            var connectors = dynSettings.Controller.ClipBoard.Select(x => x).Where(x => x.GetType() == typeof(dynConnector));

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

                dynSettings.Controller.CommandQueue.Add(Tuple.Create<object, object>(DynamoCommands.CreateNodeCmd, nodeData));
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
                    startNode = c.Start.Owner;
                }

                connectionData.Add("start", startNode);

                connectionData.Add("end", dynSettings.Controller.CurrentSpace.Nodes
                    .Select(x=>x.NodeUI)
                    .Where(x=>x.GUID == (Guid)nodeLookup[c.End.Owner.GUID]).FirstOrDefault());

                connectionData.Add("port_start", c.Start.Index);
                connectionData.Add("port_end", c.End.Index);

                dynSettings.Controller.CommandQueue.Add(Tuple.Create<object, object>(DynamoCommands.CreateConnectionCmd, connectionData));
            }
            
            //process the queue again to create the connectors
            dynSettings.Controller.ProcessCommandQueue();

            foreach (DictionaryEntry de in nodeLookup)
            {
                dynSettings.Controller.CommandQueue.Add(Tuple.Create<object, object>(DynamoCommands.AddToSelectionCmd, 
                    dynSettings.Controller.CurrentSpace.Nodes
                    .Select(x => x.NodeUI)
                    .Where(x => x.GUID == (Guid)de.Value).FirstOrDefault()));
            }

            dynSettings.Controller.ProcessCommandQueue();

            //dynSettings.Controller.ClipBoard.Clear();
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
        public SelectCommand()
        {

        }

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
        public AddToSelectionCommand()
        {

        }

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

    public class ShowConsoleCommand : ICommand
    {
        public ShowConsoleCommand()
        {

        }

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
        public CancelRunCommand()
        {

        }

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
        public SaveAsCommand()
        {

        }

        public void Execute(object parameters)
        {
            dynSettings.Controller.SaveAs();
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
        public SaveCommand()
        {

        }

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
        public OpenCommand()
        {

        }

        public void Execute(object parameters)
        {
            //string xmlPath = "C:\\test\\myWorkbench.xml";
            string xmlPath = "";

            System.Windows.Forms.OpenFileDialog openDialog = new OpenFileDialog()
            {
                Filter = "Dynamo Definitions (*.dyn; *.dyf)|*.dyn;*.dyf|All files (*.*)|*.*"
            };

            if (openDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                xmlPath = openDialog.FileName;
            }

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
        public HomeCommand()
        {

        }

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
        public SaveImageCommand()
        {

        }

        public void Execute(object parameters)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PNG Image|*.png";
            sfd.Title = "Save your Workbench to an Image";
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string imagePath = sfd.FileName;

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

                using (var stm = System.IO.File.Create(sfd.FileName))
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
        public LayoutAllCommand()
        {

        }

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
                //AddNote(de.Key.ToString(), x, y, Controller.CurrentSpace);
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

                    el.DisableReporting();

                    maxWidth = Math.Max(el.NodeUI.Width, maxWidth);

                    colCount++;

                    y += el.NodeUI.Height + rowGutter;
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
        public ClearCommand()
        {

        }

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
        public ClearLogCommand()
        {

        }

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
}
