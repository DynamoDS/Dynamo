using System;
using System.Windows;
using System.Windows.Input;
using Dynamo.Search;
using Dynamo.Utilities;

namespace Dynamo.Commands
{

    public static partial class DynamoCommands
    {

        private static ShowSearchCommand showSearchCmd;

        public static ShowSearchCommand ShowSearchCmd
        {
            get
            {
                if (showSearchCmd == null)
                    showSearchCmd = new ShowSearchCommand();
                return showSearchCmd;
            }
        }

        private static HideSearchCommand hideSearchCmd;

        public static HideSearchCommand HideSearchCmd
        {
            get
            {
                if (hideSearchCmd == null)
                    hideSearchCmd = new HideSearchCommand();
                return hideSearchCmd;
            }
        }
    }

    public class HideSearchCommand : ICommand {

        private SearchUI search;
        private static bool init = false;

        public HideSearchCommand()
        {

        }

        public void Execute(object parameters)
        {
            if (!init)
            {
                search = dynSettings.Controller.SearchController.View;
                init = true;
            }

            if (search.Visibility == Visibility.Visible)
            {
                search.Visibility = Visibility.Collapsed;
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

    public class ShowSearchCommand : ICommand
    {

        private SearchUI search;
        private static bool init = false;

        public ShowSearchCommand()
        {
            
        }

        public void Execute(object parameters)
        {
            if (!init)
            {
                search = dynSettings.Controller.SearchController.View;
                dynSettings.Bench.outerCanvas.Children.Add(search);
                init = true;
            }

            search.Visibility = Visibility.Visible;

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
