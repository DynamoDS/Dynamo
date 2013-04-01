using System;
using System.Windows;
using System.Windows.Input;
using Dynamo.Search;
using Dynamo.Utilities;

namespace Dynamo.Commands
{

    public static partial class DynamoCommands
    {

        private static SearchCommand searchCmd;

        public static SearchCommand SearchCmd
        {
            get
            {
                if (searchCmd == null)
                    searchCmd = new SearchCommand();
                return searchCmd;
            }
        }

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

    public class SearchCommand : ICommand
    {
        public void Execute(object parameters)
        {
            dynSettings.Controller.SearchViewModel.SearchAndUpdateResults();
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

    public class HideSearchCommand : ICommand {

        public void Execute(object parameters)
        {
            dynSettings.Controller.SearchViewModel.Visible = Visibility.Collapsed;
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

        public void Execute(object parameters)
        {
            if (!init)
            {
                search = new SearchUI(dynSettings.Controller.SearchViewModel);
                dynSettings.Bench.outerCanvas.Children.Add(search);
                init = true;
            }

            dynSettings.Controller.SearchViewModel.Visible = Visibility.Visible;

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
