using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Controls;
using System.ComponentModel;
using Dynamo.Utilities;
using System.Windows.Input;

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
    }

    public class NodeFromSelectionCommand : ICommand
    {
        public NodeFromSelectionCommand()
        {
            //dynSettings.Bench.WorkBench.Selection.
        }

        public void Execute(object parameters)
        {
            dynSettings.Bench.Controller.NodeFromSelection(
                dynSettings.Bench.WorkBench.Selection.Where(x => x is dynNodeUI)
                    .Select(x => (x as dynNodeUI).NodeLogic));
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameters)
        {
            //if (dynSettings.Bench != null)
            //{
            //    return dynSettings.Bench.WorkBench.Selection.Count > 0;
            //}
            //else
            //{
            //    return false;
            //}
            return true;
        }
    }
}
