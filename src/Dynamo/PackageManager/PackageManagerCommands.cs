using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Dynamo.Controls;
using Dynamo.Nodes;
using Dynamo.PackageManager;
using Dynamo.Utilities;

namespace Dynamo.Commands
{

    public class LoginCommand : ICommand
    {
        public LoginCommand()
        {

        }

        public void Execute(object parameters)
        {
            
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

    public class ShowNodePublishInfoCommand : ICommand
    {
        private bool init;
        private PackageManagerPublishUI ui;

        public ShowNodePublishInfoCommand()
        {
            this.init = false;
        }

        public void Execute(object funcDef)
        {

            if (dynSettings.Controller.PackageManagerClient.IsLoggedIn == false)
            {
                DynamoCommands.ShowLoginCmd.Execute(null);
                dynSettings.Bench.Log("Must login first to publish a node.");
                return;
            }

            if (!init)
            {
                ui = new PackageManagerPublishUI(dynSettings.Controller.PackageManagerPublishViewModel);
                dynSettings.Bench.outerCanvas.Children.Add(ui);
                init = true;
            }
            
            if (funcDef is FunctionDefinition)
            {
                var f = funcDef as FunctionDefinition;

                dynSettings.Controller.PackageManagerPublishViewModel.FunctionDefinition =
                    f;

                // we're submitting a new version
                if ( dynSettings.Controller.PackageHeaders.ContainsKey(f) )
                {
                    dynSettings.Controller.PackageManagerPublishViewModel.PackageHeader =
                        dynSettings.Controller.PackageHeaders[f];
                }
            }
            else
            {
                dynSettings.Bench.Log("Failed to obtain function definition from node.");
                return;
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

    public class ShowLoginCommand : ICommand
    {
        private bool init;
        private PackageManager.PackageManagerLoginUI ui;

        public ShowLoginCommand()
        {
            this.init = false;
        }

        public void Execute(object parameters)
        {
            if (!init)
            {
                ui = dynSettings.Controller.PackageManagerLoginController.View;
                dynSettings.Bench.outerCanvas.Children.Add(ui);
                init = true;
            }

            ui.Visibility = Visibility.Visible;
           
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

    public class RefreshRemotePackagesCommand : ICommand
    {
        public RefreshRemotePackagesCommand()
        {

        }

        public void Execute(object parameters)
        {
            Dynamo.Utilities.dynSettings.Controller.PackageManagerClient.RefreshAvailable();
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

    public class PublishSelectedNodeCommand : ICommand
    {
        private PackageManagerClient _client;

        public PublishSelectedNodeCommand()
        {

        }

        public void Execute(object parameters)
        {
            this._client = dynSettings.Controller.PackageManagerClient;

            var nodeList = dynSettings.Bench.WorkBench.Selection.Where(x => x is dynNodeUI && ((dynNodeUI)x).NodeLogic is dynFunction )
                                        .Select(x => ( ((dynNodeUI)x).NodeLogic as dynFunction ).Definition.FunctionId ).ToList();

            if (nodeList.Count != 1)
            {
                MessageBox.Show("You must select a single user-defined node.  You selected " + nodeList.Count + " nodes." , "Selection Error", MessageBoxButton.OK, MessageBoxImage.Question);
                return;
            }

            if ( dynSettings.FunctionDict.ContainsKey( nodeList[0] ) )
            {
                DynamoCommands.ShowNodeNodePublishInfoCmd.Execute( dynSettings.FunctionDict[nodeList[0]] );
            }
            else
            {
                MessageBox.Show("The selected symbol was not found in the workspace", "Selection Error", MessageBoxButton.OK, MessageBoxImage.Question);
            }

        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameters)
        {
            // todo: should check authentication state, connected to internet
            return true;
        }
    }

    public class PublishCurrentWorkspaceCommand : ICommand
    {

        private PackageManagerClient _client;

        public PublishCurrentWorkspaceCommand()
        {

        }

        public void Execute(object parameters)
        {
            this._client = dynSettings.Controller.PackageManagerClient;
            
            if ( dynSettings.Controller.ViewingHomespace )
            {
                MessageBox.Show("You can't publish your the home workspace.", "Workspace Error", MessageBoxButton.OK, MessageBoxImage.Question);
                return;
            }

            var currentFunDef =
                dynSettings.FunctionDict.FirstOrDefault(x => x.Value.Workspace == dynSettings.Controller.CurrentSpace).Value;

            if ( currentFunDef != null )
            {
                DynamoCommands.ShowNodeNodePublishInfoCmd.Execute(currentFunDef);
            }
            else
            {
                MessageBox.Show("The selected symbol was not found in the workspace", "Selection Error", MessageBoxButton.OK, MessageBoxImage.Question);
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
    
    public static partial class DynamoCommands
    {

        private static Dynamo.Commands.ShowNodePublishInfoCommand _showNodePublishInfoCmd;
        public static Dynamo.Commands.ShowNodePublishInfoCommand ShowNodeNodePublishInfoCmd
        {
            get
            {
                if (_showNodePublishInfoCmd == null)
                    _showNodePublishInfoCmd = new Dynamo.Commands.ShowNodePublishInfoCommand();
                return _showNodePublishInfoCmd;
            }
        }

        private static Dynamo.Commands.PublishCurrentWorkspaceCommand publishCurrentWorkspaceCmd;
        public static Dynamo.Commands.PublishCurrentWorkspaceCommand PublishCurrentWorkspaceCmd
        {
            get
            {
                if (publishCurrentWorkspaceCmd == null)
                    publishCurrentWorkspaceCmd = new Dynamo.Commands.PublishCurrentWorkspaceCommand();
                return publishCurrentWorkspaceCmd;
            }
        }

        private static Dynamo.Commands.PublishSelectedNodeCommand publishSelectedNodeCmd;
        public static Dynamo.Commands.PublishSelectedNodeCommand PublishSelectedNodeCmd
        {
            get
            {
                if (publishSelectedNodeCmd == null)
                    publishSelectedNodeCmd = new Dynamo.Commands.PublishSelectedNodeCommand();
                return publishSelectedNodeCmd;
            }
        }

        private static Dynamo.Commands.RefreshRemotePackagesCommand refreshRemotePackagesCmd;
        public static Dynamo.Commands.RefreshRemotePackagesCommand RefreshRemotePackagesCmd
        {
            get
            {
                if (refreshRemotePackagesCmd == null)
                    refreshRemotePackagesCmd = new Dynamo.Commands.RefreshRemotePackagesCommand();
                return refreshRemotePackagesCmd;
            }
        }

        private static Dynamo.Commands.ShowLoginCommand showLoginCmd;
        public static Dynamo.Commands.ShowLoginCommand ShowLoginCmd
        {
            get
            {
                if (showLoginCmd == null)
                    showLoginCmd = new Dynamo.Commands.ShowLoginCommand();
                return showLoginCmd;
            }
        }

        private static Dynamo.Commands.LoginCommand loginCmd;
        public static Dynamo.Commands.LoginCommand LoginCmd
        {
            get
            {
                if (loginCmd == null)
                    loginCmd = new Dynamo.Commands.LoginCommand();
                return loginCmd;
            }
        }
    }
}
