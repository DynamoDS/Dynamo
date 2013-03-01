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

namespace Dynamo.Nodes
{
    public class dynWorkspace
    {
        public List<dynNode> Nodes { get; private set; }
        public List<dynConnector> Connectors { get; private set; }
        public List<dynNote> Notes { get; private set; }

        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public string FilePath { get; set; }

        public String Name { get; set; }

        public event Action OnModified;

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

        public virtual void Modified()
        {
            if (OnModified != null)
                OnModified();
        }

        public IEnumerable<dynNode> GetTopMostElements()
        {
            return this.Nodes.Where(
               x => !x.NodeUI.OutPort.Connectors.Any()
            );
        }
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

            dynSettings.Controller.SaveFunction(this);
        }
    }

    public class HomeWorkspace : dynWorkspace
    {
        #region Contructors

        public HomeWorkspace()
            : this(new List<dynNode>(), new List<dynConnector>(), 0, 0)
        { }

        public HomeWorkspace(double x, double y)
            : this(new List<dynNode>(), new List<dynConnector>(), x, y)
        { }

        public HomeWorkspace(List<dynNode> e, List<dynConnector> c, double x, double y)
            : base("Home", e, c, x, y)
        { }

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
                            controller.RunExpression(false, false);
                        else
                            controller.QueueRun();
                    }
                }));
        }
    }
}
