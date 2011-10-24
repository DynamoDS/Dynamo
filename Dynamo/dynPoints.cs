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
    public abstract class dynReferencePoint:dynElement,IDynamic
    {
        public dynReferencePoint(string nickName)
            : base(nickName)
        {

            OutPortData.Add(new PortData(null, "pt", "The Reference Point(s) created from this operation.", typeof(dynReferencePoint)));
            OutPortData[0].Object = this.Tree;

            //base.RegisterInputsAndOutputs();
        }

        public override void Draw()
        {

        }

        public override void Destroy()
        {
            //base destroys all elements in the collection
            base.Destroy();
        }

        public override void Update()
        {
            OnDynElementReadyToBuild(EventArgs.Empty);
        }
    }

    [ElementName("ReferencePointByXYZ")]
    [ElementDescription("An element which creates a reference point.")]
    [RequiresTransaction(true)]
    public class dynReferencePointByXYZ : dynReferencePoint, IDynamic
    {
        public dynReferencePointByXYZ(string nickName)
            : base(nickName)
        {
            InPortData.Add(new PortData(null, "xyz", "The point(s) from which to create reference points.", typeof(dynXYZ)));

            //outport already added in parent

            base.RegisterInputsAndOutputs();
        }

        public override void Draw()
        {
            if (CheckInputs())
            {
                DataTreeBranch b = new DataTreeBranch();
                this.Tree.Trunk.Branches.Add(b);

                XYZ pt = InPortData[0].Object as XYZ;
                if (pt != null)
                {
                    ReferencePoint rp = dynElementSettings.SharedInstance.Doc.Document.FamilyCreate.NewReferencePoint(pt);
                    b.Leaves.Add(rp);

                    //add the element to the collection
                    Elements.Append(rp);
                }
            }
        }

        public override void Destroy()
        {
            //base destroys all elements in the collection
            base.Destroy();
        }

        public override void Update()
        {
            OnDynElementReadyToBuild(EventArgs.Empty);
        }
    }

    [ElementName("ReferencePointGridXYZ")]
    [ElementDescription("An element which creates a grid of reference points.")]
    [RequiresTransaction(true)]
    public class dynReferencePtGrid : dynReferencePoint, IDynamic
    {
        public dynReferencePtGrid(string nickName)
            : base(nickName)
        {
            InPortData.Add(new PortData(null, "xi", "Number in the X direction.", typeof(dynInt)));
            InPortData.Add(new PortData(null, "yi", "Number in the Y direction.", typeof(dynInt)));
            InPortData.Add(new PortData(null, "pt", "Origin.", typeof(dynReferencePoint)));
            InPortData.Add(new PortData(null, "x", "The X spacing.", typeof(dynDouble)));
            InPortData.Add(new PortData(null, "y", "The Y spacing.", typeof(dynDouble)));
            InPortData.Add(new PortData(null, "z", "The Z offset.", typeof(dynDouble)));

            //outports already added in parent

            base.RegisterInputsAndOutputs();

        }

        public override void Draw()
        {
            if (CheckInputs())
            {
                DataTree xyzTree = InPortData[2].Object as DataTree;
                if (xyzTree != null)
                {
                    Process(xyzTree.Trunk, this.Tree.Trunk);
                }
            }
        }

        public void Process(DataTreeBranch bIn, DataTreeBranch currentBranch)
        {

            //use each XYZ leaf on the input
            //to define a new origin
            foreach (object o in bIn.Leaves)
            {
                ReferencePoint rp = o as ReferencePoint;

                if (rp != null)
                {
                    for (int i = 0; i < (int)InPortData[0].Object; i++)
                    {
                        //create a branch for the data tree for
                        //this row of points
                        DataTreeBranch b = new DataTreeBranch();
                        currentBranch.Branches.Add(b);

                        for (int j = 0; j < (int)InPortData[1].Object; j++)
                        {
                            XYZ pt = new XYZ(rp.Position.X + i * (double)InPortData[3].Object,
                                rp.Position.Y + j * (double)InPortData[4].Object,
                                rp.Position.Z);

                            ReferencePoint rpNew = dynElementSettings.SharedInstance.Doc.Document.FamilyCreate.NewReferencePoint(pt);

                            //add the point as a leaf on the branch
                            b.Leaves.Add(rpNew);

                            //add the element to the collection
                            Elements.Append(rpNew);
                        }
                    }
                }
            }

            foreach (DataTreeBranch b1 in bIn.Branches)
            {
                DataTreeBranch newBranch = new DataTreeBranch();
                currentBranch.Branches.Add(newBranch);

                Process(b1, newBranch);
            }

        }

        public override void Destroy()
        {
            //base destroys all elements in the collection
            base.Destroy();
        }

        public override void Update()
        {
            OnDynElementReadyToBuild(EventArgs.Empty);
        }
    }

    //[ElementName("PtOnEdge")]
    //[ElementDescription("Create an element which owns a reference point on a selected edge.")]
    //[RequiresTransaction(true)]
    //public class dynPointOnEdge : dynElement, IDynamic
    //{
    //    public dynPointOnEdge(string nickName)
    //        : base(nickName)
    //    {
    //        InPortData.Add(new PortData(null, "cv", "ModelCurve", typeof(ModelCurve)));
    //        InPortData.Add(new PortData(null, "t", "Parameter on edge.", typeof(double)));
    //        OutPortData.Add(new PortData(null, "pt", "PointOnEdge", typeof(dynPointOnEdge)));

    //        base.RegisterInputsAndOutputs();
    //    }

    //    public override void Draw()
    //    {
    //        if (CheckInputs())
    //        {

    //            Reference r = (InPortData[0].Object as ModelCurve).GeometryCurve.Reference;
    //            OutPortData[0].Object = dynElementSettings.SharedInstance.Doc.Application.Application.Create.NewPointOnEdge(r, (double)InPortData[1].Object);

    //        }
    //    }

    //    public override void Destroy()
    //    {
    //        base.Destroy();
    //    }

    //    public override void Update()
    //    {
    //        OnDynElementReadyToBuild(EventArgs.Empty);
    //    }
    //}
}
