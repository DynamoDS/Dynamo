//Copyright 2011 Ian Keough

//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at

//http://www.apache.org/licenses/LICENSE-2.0

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.IO;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Events;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Data;
using TextBox = System.Windows.Controls.TextBox;
using System.Windows.Forms;
using Dynamo.Controls;
using Dynamo.Connectors;
using Dynamo.Utilities;
using System.IO.Ports;

namespace Dynamo.Elements
{
    /// <summary>
    /// A DataTree represents a branching structure of branches and leaves. 
    /// It containes at least one branch which is the trunk.
    /// </summary>
    public class DataTree
    {
        DataTreeBranch trunk;

        public DataTreeBranch Trunk
        {
            get { return trunk; }
            set { trunk = value; }
        }

        public DataTree()
        {
            trunk = new DataTreeBranch();
        }

        public void Clear()
        {
            trunk.Clear();
        }

        public string Graph()
        {
            string graph = "";

            this.trunk.Graph(0,ref graph);

            return graph;
        }
    }

    /// <summary>
    /// A DataTree is comprised of DataTreeBranches. A DataTreeBranch contains a set of leaves and a set of branches.
    /// </summary>
    public class DataTreeBranch
    {
        List<DataTreeBranch> branches = new List<DataTreeBranch>();
        List<object> leaves = new List<object>();

        public List<DataTreeBranch> Branches
        {
            get { return branches; }
            set { branches = value; }
        }

        public List<object> Leaves
        {
            get { return leaves; }
            set { leaves = value; }
        }

        public DataTreeBranch()
        {
        }

        public void Clear()
        {
            //Debug.WriteLine("Item has " + this.branches.Count + " branches.");
            for (int i = this.branches.Count - 1; i>=0 ; i--)
            {
                DataTreeBranch b = this.branches[i];
                b.Clear();
                this.branches.Remove(b);
            }
            this.branches.Clear();
            leaves.Clear();
        }

        public void Graph(int branchIndex, ref string graph)
        {
            //n
            graph += branchIndex.ToString();

            int leafCount = 0;
            foreach (object o in this.Leaves)
            {
                //n:n--xxx
                graph += ":" + leafCount.ToString();
                if(o != null)
                    graph += "--" + o.ToString() + "\n";
                leafCount++;
            }

            int branchCount = 0;
            foreach (DataTreeBranch b1 in this.Branches)
            {
                //n:n:n
                b1.Graph(branchCount, ref graph);
                branchCount++;
            }

            graph += "\n";
        }

    }
}
