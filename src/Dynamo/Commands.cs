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

        private static SetConnectorTypeCommand setConnectorTypeCmd;
        public static SetConnectorTypeCommand SetConnectorTypeCmd
        {
            get
            {
                if (setConnectorTypeCmd == null)
                    setConnectorTypeCmd = new SetConnectorTypeCommand();

                return setConnectorTypeCmd;
            }
        }
    }

    public class SelectNeighborsCommand : ICommand
    {

        public void Execute(object parameters)
        {
            List<ISelectable> sels = dynSettings.Workbench.Selection.ToList<ISelectable>();

            foreach (ISelectable sel in sels)
            {

                ((dynNodeViewModel)sel).SelectNeighbors();
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
               DynamoModel.Instance.ViewCustomNodeWorkspace( dynSettings.FunctionDict[ (Guid) parameter] );   
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

    public class DisplayFunctionCommand : ICommand
    {
        public void Execute(object parameters)
        {
            DynamoModel.Instance.ViewCustomNodeWorkspace((parameters as FunctionDefinition));
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

    public class SetConnectorTypeCommand : ICommand
    {
        public void Execute(object parameters)
        {
            if (parameters.ToString() == "BEZIER")
            {
                dynSettings.Controller.CurrentSpace.Connectors.ForEach(x => x.ConnectorType = ConnectorType.BEZIER);
            }
            else
            {
                dynSettings.Controller.CurrentSpace.Connectors.ForEach(x => x.ConnectorType = ConnectorType.POLYLINE);
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameters)
        {
            //parameter object will be BEZIER or POLYLINE
            if(string.IsNullOrEmpty(parameters.ToString()))
            {
                return false;
            }
            return true;
        }
    }

}
