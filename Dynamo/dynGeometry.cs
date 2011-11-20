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
    [ElementName("XYZ")]
    [ElementDescription("An element which creates an XYZ from three double values.")]
    [RequiresTransaction(true)]
    public class dynXYZ : dynElement, IDynamic
    {
        XYZ pt;

        public dynXYZ()
        {
            InPortData.Add(new PortData(null, "X", "X", typeof(dynDouble)));
            InPortData.Add(new PortData(null, "Y", "Y", typeof(dynDouble)));
            InPortData.Add(new PortData(null, "Z", "Z", typeof(dynDouble)));

            OutPortData.Add(new PortData(null, "xyz", "XYZ", typeof(dynXYZ)));
            OutPortData[0].Object = this.Tree;

            base.RegisterInputsAndOutputs();

        }

        public override void Draw()
        {
            if (CheckInputs())
            {

                //create the xyz
                pt = new XYZ((double)InPortData[0].Object,
                    (double)InPortData[1].Object,
                    (double)InPortData[2].Object);

                //OutPortData[0].Object = pt;
                this.Tree.Trunk.Leaves.Add(pt);
            }
        }

        public override void Destroy()
        {
            pt = null;
            base.Destroy();
        }

        public override void Update()
        {
            OnDynElementReadyToBuild(EventArgs.Empty);
        }
    }

    [ElementName("Plane")]
    [ElementDescription("An element which creates a geometric plane.")]
    [RequiresTransaction(true)]
    public class dynPlane : dynElement, IDynamic
    {
        public dynPlane()
        {
            InPortData.Add(new PortData(null, "XYZ", "Normal", typeof(dynXYZ)));
            InPortData.Add(new PortData(null, "XYZ", "Origin", typeof(dynXYZ)));
            OutPortData.Add(new PortData(null, "P", "Plane", typeof(dynPlane)));
            OutPortData[0].Object = this.Tree;

            base.RegisterInputsAndOutputs();
        }

        public override void Draw()
        {
            if (CheckInputs())
            {

                DataTree a = InPortData[0].Object as DataTree;
                DataTree b = InPortData[1].Object as DataTree;

                if (a != null && b != null)
                {
                    Process(this.Tree.Trunk, a.Trunk, b.Trunk);
                }

            }
        }

        void Process(DataTreeBranch currBranch, DataTreeBranch a, DataTreeBranch b)
        {
            foreach (object o in a.Leaves)
            {

                if (b.Leaves.Count > a.Leaves.IndexOf(o))
                {
                    XYZ ptA = o as XYZ;
                    XYZ ptB = b.Leaves[a.Leaves.IndexOf(o)] as XYZ;

                    if (ptA != null && ptB != null)
                    {
                        this.Tree.Trunk.Leaves.Add(dynElementSettings.SharedInstance.Doc.Application.Application.Create.NewPlane(ptA, ptB));
                    }
                }
                
            }

            foreach (DataTreeBranch aChild in a.Branches)
            {
                DataTreeBranch subBranch = new DataTreeBranch();
                currBranch.Branches.Add(subBranch);

                int idx = a.Branches.IndexOf(aChild);

                if (b.Branches.Count > idx)
                {
                    Process(subBranch, aChild, b.Branches[idx]);
                }
            }
        }

        public override void Destroy()
        {
            base.Destroy();
        }

        public override void Update()
        {
            OnDynElementReadyToBuild(EventArgs.Empty);
        }
    }

    [ElementName("SketchPlane")]
    [ElementDescription("An element which creates a geometric sketch plane.")]
    [RequiresTransaction(true)]
    public class dynSketchPlane : dynElement, IDynamic
    {
        public dynSketchPlane()
        {
            InPortData.Add(new PortData(null, "P", "The plane in which to define the sketch.", typeof(dynPlane)));
            OutPortData.Add(new PortData(null, "SP", "SketchPlane", typeof(dynSketchPlane)));
            OutPortData[0].Object = this.Tree;

            base.RegisterInputsAndOutputs();
        }

        public override void Draw()
        {
            if (CheckInputs())
            {
                //OutPortData[0].Object = dynElementSettings.SharedInstance.Doc.Document.FamilyCreate.NewSketchPlane(InPortData[0].Object as Plane);

                DataTree a = InPortData[0].Object as DataTree;
                if(a != null)
                {
                    Process(this.Tree.Trunk, a.Trunk);
                }
            }
        }

        void Process(DataTreeBranch currBranch, DataTreeBranch a)
        {
            foreach (object o in a.Leaves)
            {
                Plane p = o as Plane;
                if (p != null)
                {
                    currBranch.Leaves.Add(dynElementSettings.SharedInstance.Doc.Document.FamilyCreate.NewSketchPlane(p));
                }
            }

            foreach (DataTreeBranch aChild in a.Branches)
            {
                DataTreeBranch subBranch = new DataTreeBranch();
                currBranch.Branches.Add(subBranch);

                Process(subBranch, aChild);
            }

        }

        public override void Destroy()
        {
            base.Destroy();
        }

        public override void Update()
        {
            OnDynElementReadyToBuild(EventArgs.Empty);
        }
    }

    [ElementName("Line")]
    [ElementDescription("An element which creates a geometric line.")]
    [RequiresTransaction(true)]
    public class dynLineBound : dynCurve, IDynamic
    {
        public dynLineBound()
        {
            InPortData.Add(new PortData(null, "xyz", "Start", typeof(dynXYZ)));
            InPortData.Add(new PortData(null, "xyz", "End", typeof(dynXYZ)));
            OutPortData.Add(new PortData(null, "C", "Line", typeof(dynCurve)));
            OutPortData[0].Object = this.Tree;

            base.RegisterInputsAndOutputs();
        }

        public override void Draw()
        {
            if (CheckInputs())
            {
                
                DataTree a = InPortData[0].Object as DataTree;
                DataTree b = InPortData[1].Object as DataTree;

                if (a != null && b != null)
                {
                    Process(this.Tree.Trunk, a.Trunk, b.Trunk);
                }
            }
        }

        void Process(DataTreeBranch currBranch, DataTreeBranch a, DataTreeBranch b)
        {

            foreach (object o in a.Leaves)
            {
                if (b.Leaves.Count > a.Leaves.IndexOf(o))
                {
                    XYZ ptA = o as XYZ;
                    XYZ ptB = b.Leaves[a.Leaves.IndexOf(o)] as XYZ;

                    if (ptA != null && ptB != null)
                    {
                        Curve c = dynElementSettings.SharedInstance.Doc.Application.Application.Create.NewLineBound(ptA, ptB);
                        currBranch.Leaves.Add(c);
                    }
                }
            }

            foreach (DataTreeBranch aChild in a.Branches)
            {
                DataTreeBranch subBranch = new DataTreeBranch();
                currBranch.Branches.Add(subBranch);

                int idx = a.Branches.IndexOf(aChild);

                if(b.Branches.Count > idx)
                {
                    Process(subBranch, aChild, b.Branches[idx]);
                }
            }
        }

        public override void Destroy()
        {
            base.Destroy();
        }

        public override void Update()
        {
            OnDynElementReadyToBuild(EventArgs.Empty);
        }
    }
}
