using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Controls;

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
            for (int i = dynSettings.Workbench.Selection.Count - 1; i >= 0; i--)
            {
                dynNote note = dynSettings.Workbench.Selection[i] as dynNote;
                dynNodeUI node = dynSettings.Workbench.Selection[i] as dynNodeUI;

                if (node != null)
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
                else if (note != null)
                {
                    dynSettings.Workbench.Selection.Remove(note);
                    dynSettings.Controller.CurrentSpace.Notes.Remove(note);
                    dynSettings.Workbench.Children.Remove(note);
                }
            }
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameters)
        {
            return dynSettings.Workbench.Selection.Count > 0;
        }
    }
}
