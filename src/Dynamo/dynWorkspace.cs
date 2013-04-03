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
using System.Text;

using Dynamo.Connectors;
using System.Windows;
using Dynamo.Utilities;
using Dynamo.Controls;
using Dynamo.Nodes;

namespace Dynamo
{
    public abstract class dynWorkspace
    {
        public List<dynNode> Nodes { get; private set; }
        public List<dynConnector> Connectors { get; private set; }
        public List<dynNote> Notes { get; private set; }

        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public string FilePath { get; set; }
        public String Name { get; set; }

        public event Action OnModified;

        public abstract void OnDisplayed();

        //Hide default constructor.
        private dynWorkspace() { }

        protected dynWorkspace(String name, List<dynNode> e, List<dynConnector> c, double x, double y)
        {
            this.Name = name;
            this.Nodes = e;
            this.Connectors = c;
            this.PositionX = x;
            this.PositionY = y;
            this.Notes = new List<dynNote>();
        }

        public void DisableReporting()
        {
            Nodes.ForEach(x => x.DisableReporting());
        }

        public void EnableReporting()
        {
            Nodes.ForEach(x => x.EnableReporting());
        }

        public virtual void Modified()
        {
            if (OnModified != null)
                OnModified();
        }

        public IEnumerable<dynNode> GetTopMostNodes()
        {
            return this.Nodes.Where(
               x => x.OutPortData.Any() && x.NodeUI.OutPorts.All(y => !y.Connectors.Any())
            );
        }
    }

    internal static class WorkspaceHelpers
    {
        public static Dictionary<string, dynNodeUI> hiddenNodes =
            new Dictionary<string, dynNodeUI>();
    }



    public class FuncWorkspace : dynWorkspace
    {
        public String Category { get; set; }


        #region Contructors

        public FuncWorkspace()
            : this("", "", new List<dynNode>(), new List<dynConnector>(), 0, 0)
        { }

        public FuncWorkspace(String name, String category)
            : this(name, category, new List<dynNode>(), new List<dynConnector>(), 0, 0)
        { }

        public FuncWorkspace(String name, String category, double x, double y)
            : this(name, category, new List<dynNode>(), new List<dynConnector>(), x, y)
        { }

        public FuncWorkspace(String name, String category, List<dynNode> e, List<dynConnector> c, double x, double y)
            : base(name, e, c, x, y)
        {
            this.Category = category;
        }

        #endregion

        public override void Modified()
        {
            base.Modified();

            dynSettings.Controller.SaveFunction(
                dynSettings.FunctionDict.Values.First(x => x.Workspace == this));
        }

        public override void OnDisplayed()
        {
            var bench = dynSettings.Bench;

            if (bench.addMenuItemsDictNew.ContainsKey("Variable"))
                return;

            var variable = WorkspaceHelpers.hiddenNodes["Variable"];
            var output = WorkspaceHelpers.hiddenNodes["Output"];
            WorkspaceHelpers.hiddenNodes.Remove("Variable");
            WorkspaceHelpers.hiddenNodes.Remove("Output");
            variable.Visibility = Visibility.Visible;
            variable.Visibility = Visibility.Visible;
            bench.addMenuItemsDictNew["Variable"] = variable;
            bench.addMenuItemsDictNew["Output"] = output;

            dynSettings.Controller.UpdateSearch(bench.SearchBox.Text.Trim());
        }
    }

    public class HomeWorkspace : dynWorkspace
    {
        #region Contructors

        private static bool initializedFunctionDefinition = false;

        public HomeWorkspace()
            : this(new List<dynNode>(), new List<dynConnector>(), 0, 0)
        { }

        public HomeWorkspace(double x, double y)
            : this(new List<dynNode>(), new List<dynConnector>(), x, y)
        { }

        public HomeWorkspace(List<dynNode> e, List<dynConnector> c, double x, double y)
            : base("Home", e, c, x, y)
        {
            if (!initializedFunctionDefinition)
            {
                var homeGuid = Guid.Parse("32AAC852-90A7-4FBD-B78A-8FDB69302670");
                var homeWorkspaceFuncDef = new FunctionDefinition();
                homeWorkspaceFuncDef.Workspace = this;
                dynSettings.FunctionDict.Add(homeGuid, homeWorkspaceFuncDef);
                initializedFunctionDefinition = true;
            }
        }

        #endregion

        public override void Modified()
        {
            base.Modified();

            var controller = dynSettings.Controller;
            controller.Bench.Dispatcher.BeginInvoke(new Action(
                () =>
                {
                    if (controller.DynamicRunEnabled)
                    {
                        if (!controller.Running)
                            controller.RunExpression(false);
                        else
                            controller.QueueRun();
                    }
                }));
        }

        public override void OnDisplayed()
        {
            var bench = dynSettings.Bench;
            
            if (!bench.addMenuItemsDictNew.ContainsKey("Variable"))
                return;

            var variable = bench.addMenuItemsDictNew["Variable"];
            var output = bench.addMenuItemsDictNew["Output"];
            bench.addMenuItemsDictNew.Remove("Variable");
            bench.addMenuItemsDictNew.Remove("Output");
            variable.Visibility = Visibility.Collapsed;
            variable.Visibility = Visibility.Collapsed;
            WorkspaceHelpers.hiddenNodes["Variable"] = variable;
            WorkspaceHelpers.hiddenNodes["Output"] = output;

            dynSettings.Controller.UpdateSearch(bench.SearchBox.Text.Trim());
        }
    }
}
