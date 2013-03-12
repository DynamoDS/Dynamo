using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows;

using Dynamo.Controls;
using Dynamo.Utilities;
using Dynamo.Controls;
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

        public event EventHandler CanExecuteChanged;

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

        public event EventHandler CanExecuteChanged;

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

        public event EventHandler CanExecuteChanged;

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

            dynSettings.Workbench.Selection.Remove(node);
            dynSettings.Controller.Nodes.Remove(node.NodeLogic);
            dynSettings.Workbench.Children.Remove(node);
        }

        public event EventHandler CanExecuteChanged;

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

        public event EventHandler CanExecuteChanged;

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

        public event EventHandler CanExecuteChanged;

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

        public event EventHandler CanExecuteChanged;

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
            TypeLoadData tld = dynSettings.Controller.BuiltInTypesByNickname[parameters.ToString()];

            var obj = Activator.CreateInstanceFrom(tld.Assembly.Location, tld.Type.FullName);
            var node = (dynNode)obj.Unwrap();
            node.NodeUI.DisableInteraction();

            var el = node.NodeUI;

            dynSettings.Workbench.Children.Add(el);
            dynSettings.Controller.Nodes.Add(el.NodeLogic);
            el.NodeLogic.WorkSpace = dynSettings.Controller.CurrentSpace;
            el.Opacity = 1;

            Point pt = new Point((int)(dynSettings.Bench.overlayCanvas.ActualWidth / 2), (int)(dynSettings.Bench.overlayCanvas.ActualHeight / 2));
            Point dropPt = dynSettings.Bench.overlayCanvas.TransformToVisual(dynSettings.Workbench).Transform(pt);
            Canvas.SetLeft(el, dropPt.X);
            Canvas.SetTop(el, dropPt.Y);

            el.EnableInteraction();

            if (dynSettings.Controller.ViewingHomespace)
            {
                el.NodeLogic.SaveResult = true;
            }
            
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameters)
        {
            if (parameters != null && dynSettings.Controller.BuiltInTypesByNickname.ContainsKey(parameters.ToString()))
            {
                return true;
            }

            return false;
        }
    }
}
