using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Dynamo.Controls;
using Dynamo.Utilities;
using Dynamo.Connectors;
using Dynamo.Selection;

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

    }

    public class SelectNeighborsCommand : ICommand
    {
        public void Execute(object parameters)
        {
            List<ISelectable> sels = DynamoSelection.Instance.Selection.ToList<ISelectable>();

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

    public class SelectCommand : ICommand
    {
        public void Execute(object parameters)
        {
            dynNodeViewModel node = parameters as dynNodeViewModel;

            if (!node.IsSelected)
            {
                if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
                {
                    dynSettings.Bench.WorkBench.ClearSelection();
                }

                if (!DynamoSelection.Instance.Selection.Contains(node))
                    DynamoSelection.Instance.Selection.Add(node);
            }
            else
            {
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    DynamoSelection.Instance.Selection.Remove(node);
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
            dynNodeViewModel node = parameters as dynNodeViewModel;

            if (!node.IsSelected)
            {
                if (!DynamoSelection.Instance.Selection.Contains(node))
                    DynamoSelection.Instance.Selection.Add(node);
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
