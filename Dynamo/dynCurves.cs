//Copyright 2012 Ian Keough

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
    [ElementName("ModelCurve")]
    [ElementDescription("An element which creates a model curve.")]
    [RequiresTransaction(true)]
    public class dynModelCurve : dynElement, IDynamic
    {

        public dynModelCurve()
        {
            InPortData.Add(new PortData(null, "c", "A curve.", typeof(dynCurve)));
            InPortData.Add(new PortData(null, "sp", "The sketch plane.", typeof(dynSketchPlane)));
            OutPortData.Add(new PortData(null, "mc", "ModelCurve", typeof(dynModelCurve)));

            base.RegisterInputsAndOutputs();
        }

        public override void Draw()
        {
            if (CheckInputs())
            {
                ModelCurve mc = dynElementSettings.SharedInstance.Doc.Document.FamilyCreate.NewModelCurve(InPortData[0].Object as Curve, InPortData[1].Object as SketchPlane);
                OutPortData[0].Object = mc;

                //add the element to the collection
                Elements.Append(mc);
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

    [ElementName("CurveByRefPoints")]
    [ElementDescription("Node to create a planar model curve.")]
    [RequiresTransaction(true)]
    public class dynModelCurveByPoints : dynElement, IDynamic
    {
        List<SketchPlane> sps = new List<SketchPlane>();

        public dynModelCurveByPoints()
        {
            InPortData.Add(new PortData(null, "pt", "The points from which to create the curve", typeof(dynReferencePoint)));

            OutPortData.Add(new PortData(null, "cv", "The curve(s) by points created by this operation.", typeof(dynModelCurveByPoints)));
            OutPortData[0].Object = this.Tree;

            base.RegisterInputsAndOutputs();

        }

        public override void Draw()
        {
            if (CheckInputs())
            {
                DataTree ptTree = InPortData[0].Object as DataTree;
                Process(ptTree.Trunk, this.Tree.Trunk);
            }
        }

        public void Process(DataTreeBranch b, DataTreeBranch currentBranch)
        {
           
            List<XYZ> ptArr = new List<XYZ>();
            List<double> weights = new List<double>();

            foreach (object o in b.Leaves)
            {
                ReferencePoint pt = o as ReferencePoint;
                ptArr.Add(pt.Position);
                weights.Add(1);
            }

            //only make a curve if
            //there's enough points
            if (ptArr.Count > 1)
            {
                //make a curve
                NurbSpline ns = dynElementSettings.SharedInstance.Doc.Application.Application.Create.NewNurbSpline(ptArr, weights);
                double rawParam = ns.ComputeRawParameter(.5);
                Transform t = ns.ComputeDerivatives(rawParam, false);

                XYZ norm = t.BasisZ;
                
                if(norm.GetLength()==0)
                {
                    norm = XYZ.BasisZ;
                }

                Plane p = new Plane(norm, t.Origin);
                SketchPlane sp = dynElementSettings.SharedInstance.Doc.Document.FamilyCreate.NewSketchPlane(p);
                sps.Add(sp);

                ModelNurbSpline c = (ModelNurbSpline)dynElementSettings.SharedInstance.Doc.Document.FamilyCreate.NewModelCurve(ns, sp);

                //add the element to the collection
                //so it can be deleted later
                Elements.Append(c);

                //create a leaf node on the local branch
                currentBranch.Leaves.Add(c);
            }

            foreach (DataTreeBranch b1 in b.Branches)
            {
                //every time you read a branch
                //create a branch
                DataTreeBranch newBranch = new DataTreeBranch();
                this.Tree.Trunk.Branches.Add(newBranch);

                Process(b1, newBranch);
            }
        }

        public override void Destroy()
        {
            
            //base destroys all elements in the collection
            base.Destroy();

            foreach (SketchPlane sp in sps)
            {
                dynElementSettings.SharedInstance.Doc.Document.Delete(sp);
            }

            sps.Clear();
        }

        public override void Update()
        {
            OnDynElementReadyToBuild(EventArgs.Empty);
        }
    }

    
}
