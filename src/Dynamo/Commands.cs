using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows;
using System.Collections;

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
            dynSettings.Controller.RunExpression(false);
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
                nodeData.Add("x", Canvas.GetLeft(node) + 100);
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
}
