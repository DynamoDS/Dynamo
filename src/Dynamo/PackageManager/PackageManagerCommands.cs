//Copyright © Autodesk, Inc. 2012. All rights reserved.
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Dynamo.Controls;
using Dynamo.Nodes;
using Dynamo.PackageManager;
using Dynamo.Selection;
using Dynamo.Utilities;
using System.Windows.Controls;

namespace Dynamo.Commands
{

    public class ShowNodePublishInfoCommand : ICommand
    {

        public void Execute(object funcDef)
        {

            if (funcDef is List<FunctionDefinition>)
            {
                var fs = funcDef as List<FunctionDefinition>;

                foreach (var f in fs)
                {
                    var pkg = dynSettings.PackageLoader.GetOwnerPackage(f);

                    if (dynSettings.PackageLoader.GetOwnerPackage(f) != null)
                    {
                        var m = MessageBox.Show("The node is part of the dynamo package called \"" + pkg.Name + 
                            "\" - do you want to submit a new version of this package?  \n\nIf not, this node will be moved to the new package you are creating.", 
                            "Package Warning", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (m == MessageBoxResult.Yes)
                        {
                            pkg.PublishNewPackageVersionCommand.Execute();
                            return;
                        }
                    }
                }

                dynSettings.Controller.PublishPackageViewModel = new PublishPackageViewModel(dynSettings.Controller.PackageManagerClient);
                dynSettings.Controller.PublishPackageViewModel.FunctionDefinitions = fs;

            }
            else
            {
                dynSettings.Controller.DynamoViewModel.Log("Failed to obtain function definition from node.");
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

    public class PublishSelectedNodesCommand : ICommand
    {
        private PackageManagerClient _client;

        public void Execute(object parameters)
        {
            this._client = dynSettings.Controller.PackageManagerClient;

            var nodeList = DynamoSelection.Instance.Selection
                                .Where(x => x is dynFunction)
                                .Cast<dynFunction>()
                                .Select(x => x.Definition.FunctionId )
                                .ToList();

            if (!nodeList.Any())
            {
                MessageBox.Show("You must select at least one custom node.", "Selection Error", MessageBoxButton.OK, MessageBoxImage.Question);
                return;
            }

            var defs = nodeList.Select(dynSettings.CustomNodeManager.GetFunctionDefinition).ToList();

            if (defs.Any(x=> x == null))
                MessageBox.Show("There was a problem getting the node from the workspace.", "Selection Error", MessageBoxButton.OK, MessageBoxImage.Question);

            DynamoCommands.ShowNodeNodePublishInfoCmd.Execute(defs);
            
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

        public void Execute(object parameters)
        {
            this._client = dynSettings.Controller.PackageManagerClient;
            
            if ( dynSettings.Controller.DynamoViewModel.ViewingHomespace )
            {
                MessageBox.Show("You can only publish Custom Node workspaces.", "Workspace Error", MessageBoxButton.OK, MessageBoxImage.Question);
                return;
            }

            var currentFunDef =
                dynSettings.Controller.CustomNodeManager.GetDefinitionFromWorkspace(dynSettings.Controller.DynamoViewModel.CurrentSpace);

            if ( currentFunDef != null )
            {
                DynamoCommands.ShowNodeNodePublishInfoCmd.Execute(new List<FunctionDefinition> {currentFunDef});
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

        private static ShowNodePublishInfoCommand _showNodePublishInfoCmd;
        public static ShowNodePublishInfoCommand ShowNodeNodePublishInfoCmd
        {
            get
            {
                if (_showNodePublishInfoCmd == null)
                    _showNodePublishInfoCmd = new ShowNodePublishInfoCommand();
                return _showNodePublishInfoCmd;
            }
        }

        private static PublishCurrentWorkspaceCommand publishCurrentWorkspaceCmd;
        public static PublishCurrentWorkspaceCommand PublishCurrentWorkspaceCmd
        {
            get
            {
                if (publishCurrentWorkspaceCmd == null)
                    publishCurrentWorkspaceCmd = new PublishCurrentWorkspaceCommand();
                return publishCurrentWorkspaceCmd;
            }
        }

        private static PublishSelectedNodesCommand _publishSelectedNodesCmd;
        public static PublishSelectedNodesCommand PublishSelectedNodesCmd
        {
            get
            {
                if (_publishSelectedNodesCmd == null)
                    _publishSelectedNodesCmd = new PublishSelectedNodesCommand();
                return _publishSelectedNodesCmd;
            }
        }

    }
}
