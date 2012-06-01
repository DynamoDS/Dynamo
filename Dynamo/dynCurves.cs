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

using Expression = Dynamo.FScheme.Expression;
using Microsoft.FSharp.Collections;
using Dynamo.FSchemeInterop;

namespace Dynamo.Elements
{
   [ElementName("Model Curve")]
   [ElementCategory(BuiltinElementCategories.REVIT)]
   [ElementDescription("An element which creates a model curve.")]
   [RequiresTransaction(true)]
   public class dynModelCurve : dynElement
   {
      public dynModelCurve()
      {
         InPortData.Add(new PortData(null, "c", "A curve.", typeof(Curve)));
         InPortData.Add(new PortData(null, "sp", "The sketch plane.", typeof(SketchPlane)));
         OutPortData = new PortData(null, "mc", "ModelCurve", typeof(ModelCurve));

         base.RegisterInputsAndOutputs();
      }

      public override Expression Evaluate(FSharpList<Expression> args)
      {
         Curve c = (Curve)((Expression.Container)args[0]).Item;
         SketchPlane sp = (SketchPlane)((Expression.Container)args[1]).Item;

         return Expression.NewContainer(
            this.UIDocument.Document.FamilyCreate.NewModelCurve(c, sp)
         );
      }
   }

   [ElementName("Loft Form")]
   [ElementCategory(BuiltinElementCategories.REVIT)]
   [ElementDescription("Creates a new loft form <doc.FamilyCreate.NewLoftForm>")]
   [RequiresTransaction(true)]
   public class dynLoftForm : dynElement
   {
      public dynLoftForm()
      {
         InPortData.Add(new PortData(null, "solid?", "Is solid?", typeof(object)));
         InPortData.Add(new PortData(null, "refListList", "ReferenceArrayArray", typeof(object)));

         OutPortData = new PortData(null, "form", "Loft Form", typeof(object));

         base.RegisterInputsAndOutputs();
      }

      public override Expression Evaluate(FSharpList<Expression> args)
      {
         bool isSolid = ((FScheme.Expression.Number)args[0]).Item == 1;

         IEnumerable<IEnumerable<Reference>> refArrays = ((FScheme.Expression.List)args[1]).Item.Select(
            x => ((FScheme.Expression.List)x).Item.Select(
               y => (Reference)((FScheme.Expression.Container)y).Item
            )
         );

         ReferenceArrayArray refArrArr = new ReferenceArrayArray();

         foreach (IEnumerable<Reference> refs in refArrays.Where(x => x.Any()))
         {
            var refArr = new ReferenceArray();
            foreach (Reference r in refs)
            {
               refArr.Append(r);
            }
            refArrArr.Append(refArr);
         }

         return FScheme.Expression.NewContainer(
            dynElementSettings.SharedInstance.Doc.Document.FamilyCreate.NewLoftForm(isSolid, refArrArr)
         );
      }
   }

   [ElementName("Curve By Points")]
   [ElementCategory(BuiltinElementCategories.REVIT)]
   [ElementDescription("doc.FamilyCreate.NewCurveByPoints")]
   [RequiresTransaction(true)]
   public class dynCurveByPoints : dynElement
   {
      public dynCurveByPoints()
      {
         InPortData.Add(new PortData(null, "refPts", "List of reference points", typeof(object)));
         OutPortData = new PortData(null, "curve", "Curve from ref points", typeof(object));

         base.RegisterInputsAndOutputs();
      }

      public override Expression Evaluate(FSharpList<Expression> args)
      {
         CurveByPoints c;

         IEnumerable<ReferencePoint> refPts = ((Expression.List)args[0]).Item.Select(
            x => (ReferencePoint)((Expression.Container)x).Item
         );

         ReferencePointArray refPtArr = new ReferencePointArray();

         foreach (var refPt in refPts)
         {
            refPtArr.Append(refPt);
         }

         if (this.Elements.Any())
         {
            c = (CurveByPoints)this.Elements[0];

            c.SetPoints(refPtArr);
         }
         else
         {
            c = dynElementSettings.SharedInstance.Doc.Document.FamilyCreate.NewCurveByPoints(refPtArr);

            this.Elements.Add(c);
         }
         
         return FScheme.Expression.NewContainer(c);
      }
   }

   [ElementName("Curve Reference")]
   [ElementCategory(BuiltinElementCategories.REVIT)]
   [ElementDescription("CurveyPoints.GeometryCurve.Reference")]
   [RequiresTransaction(true)]
   public class dynCurveRef : dynElement
   {
      public dynCurveRef()
      {
         InPortData.Add(new PortData(null, "curve", "CurveByPoints", typeof(object)));
         OutPortData = new PortData(null, "curveRef", "Reference", typeof(object));

         base.RegisterInputsAndOutputs();
      }

   //   public override FScheme.Expression Evaluate(Microsoft.FSharp.Collections.FSharpList<FScheme.Expression> args)
   //   {
   //       object arg0 = ((FScheme.Expression.Container)args[0]).Item;

   //       if (arg0 is CurveByPoints)
   //       {

   //           CurveByPoints curve = (args[0] as FScheme.Expression.Container).Item as CurveByPoints;

   //           return FScheme.Expression.NewContainer(curve.GeometryCurve.Reference);
   //       }

   //       if (arg0 is ModelCurve)
   //       {

   //           ModelCurve curve = (args[0] as FScheme.Expression.Container).Item as ModelCurve;

   //           return FScheme.Expression.NewContainer(curve.GeometryCurve.Reference);
   //       }
   //       else
   //       {
   //           throw new Exception("Cannot cast first argument to Mpdel Curve or Curve by Points.");
   //       }
   //   }

      public override FScheme.Expression Evaluate(Microsoft.FSharp.Collections.FSharpList<FScheme.Expression> args)
      {
          var input = args[0];

          if (input.IsList)
          {
              var curveList = ((FScheme.Expression.List)input).Item;

              int count = 0;

              var result = Utils.convertSequence(
                 curveList.Select(
                    delegate(FScheme.Expression x)
                    {
                        ModelCurve cv;
                        if (this.Elements.Count > count)
                        {
                            cv = (ModelCurve)this.Elements[count];
                            cv.GeometryCurve = (Curve)((FScheme.Expression.Container)x).Item;
                        }
                        else
                        {
                            cv = (ModelCurve)((FScheme.Expression.Container)x).Item;
                            this.Elements.Add(cv);
                        }
                        count++;
                        return FScheme.Expression.NewContainer(cv.GeometryCurve.Reference);
                    }
                 )
              );

              //int delCount = 0;
              //foreach (var e in this.Elements.Skip(count))
              //{
              //    this.UIDocument.Document.Delete(e);
              //    delCount++;
              //}
              //if (delCount > 0)
              //    this.Elements.RemoveRange(count, delCount);

              return FScheme.Expression.NewList(result);
          }
          else
          {
              XYZ xyz = (XYZ)((FScheme.Expression.Container)input).Item;

              ReferencePoint pt;

              if (this.Elements.Any())
              {
                  pt = (ReferencePoint)this.Elements[0];
                  pt.Position = xyz;

                  int count = 0;
                  foreach (var e in this.Elements.Skip(1))
                  {
                      this.UIDocument.Document.Delete(e);
                      count++;
                  }
                  if (count > 0)
                      this.Elements.RemoveRange(1, count);
              }
              else
              {
                  pt = this.UIDocument.Document.FamilyCreate.NewReferencePoint(xyz);
                  this.Elements.Add(pt);
              }

              return FScheme.Expression.NewContainer(pt);
          }
      }

   }

   [ElementName("Planar Curve By Points")]
   [ElementCategory(BuiltinElementCategories.REVIT)]
   [ElementDescription("Node to create a planar model curve.")]
   [RequiresTransaction(true)]
   public class dynModelCurveByPoints : dynElement
   {
      //List<SketchPlane> sps = new List<SketchPlane>();

      public dynModelCurveByPoints()
      {
         InPortData.Add(new PortData(null, "pts", "The points from which to create the curve", typeof(object)));

         OutPortData = new PortData(null, "cv", "The curve(s) by points created by this operation.", typeof(ModelNurbSpline));
         //OutPortData[0].Object = this.Tree;

         base.RegisterInputsAndOutputs();
      }

      //public override void Draw()
      //{
      //   if (CheckInputs())
      //   {
      //      DataTree ptTree = InPortData[0].Object as DataTree;
      //      Process(ptTree.Trunk, this.Tree.Trunk);
      //   }
      //}

      public override Expression Evaluate(FSharpList<Expression> args)
      {
         var pts = ((Expression.List)args[0]).Item.Select(
            e => ((ReferencePoint)((Expression.Container)e).Item).Position
         ).ToList();

         if (pts.Count <= 1)
         {
            throw new Exception("Not enough reference points to make a curve.");
         }

         //make a curve
         NurbSpline ns = this.UIDocument.Application.Application.Create.NewNurbSpline(
            pts, Enumerable.Repeat(1.0, pts.Count).ToList()
         );

         double rawParam = ns.ComputeRawParameter(.5);
         Transform t = ns.ComputeDerivatives(rawParam, false);

         XYZ norm = t.BasisZ;

         if (norm.GetLength() == 0)
         {
            norm = XYZ.BasisZ;
         }

         Plane p = new Plane(norm, t.Origin);
         SketchPlane sp = this.UIDocument.Document.FamilyCreate.NewSketchPlane(p);
         //sps.Add(sp);

         ModelNurbSpline c = (ModelNurbSpline)this.UIDocument.Document.FamilyCreate.NewModelCurve(ns, sp);

         return Expression.NewContainer(c);
      }

      //public void Process(DataTreeBranch b, DataTreeBranch currentBranch)
      //{

      //   List<XYZ> ptArr = new List<XYZ>();
      //   List<double> weights = new List<double>();

      //   foreach (object o in b.Leaves)
      //   {
      //      ReferencePoint pt = o as ReferencePoint;
      //      ptArr.Add(pt.Position);
      //      weights.Add(1);
      //   }

      //   //only make a curve if
      //   //there's enough points
      //   if (ptArr.Count > 1)
      //   {
      //      //make a curve
      //      NurbSpline ns = dynElementSettings.SharedInstance.Doc.Application.Application.Create.NewNurbSpline(ptArr, weights);
      //      double rawParam = ns.ComputeRawParameter(.5);
      //      Transform t = ns.ComputeDerivatives(rawParam, false);

      //      XYZ norm = t.BasisZ;

      //      if (norm.GetLength() == 0)
      //      {
      //         norm = XYZ.BasisZ;
      //      }

      //      Plane p = new Plane(norm, t.Origin);
      //      SketchPlane sp = dynElementSettings.SharedInstance.Doc.Document.FamilyCreate.NewSketchPlane(p);
      //      sps.Add(sp);

      //      ModelNurbSpline c = (ModelNurbSpline)dynElementSettings.SharedInstance.Doc.Document.FamilyCreate.NewModelCurve(ns, sp);

      //      //add the element to the collection
      //      //so it can be deleted later
      //      Elements.Append(c);

      //      //create a leaf node on the local branch
      //      currentBranch.Leaves.Add(c);
      //   }

      //   foreach (DataTreeBranch b1 in b.Branches)
      //   {
      //      //every time you read a branch
      //      //create a branch
      //      DataTreeBranch newBranch = new DataTreeBranch();
      //      this.Tree.Trunk.Branches.Add(newBranch);

      //      Process(b1, newBranch);
      //   }
      //}

      //public override void Destroy()
      //{

      //   //base destroys all elements in the collection
      //   base.Destroy();

      //   foreach (SketchPlane sp in sps)
      //   {
      //      dynElementSettings.SharedInstance.Doc.Document.Delete(sp);
      //   }

      //   sps.Clear();
      //}

      //public override void Update()
      //{
      //   OnDynElementReadyToBuild(EventArgs.Empty);
      //}
   }
}
