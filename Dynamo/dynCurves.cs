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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Connectors;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Expression = Dynamo.FScheme.Expression;
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
         InPortData.Add(new PortData("c", "A curve.", typeof(Curve)));
         InPortData.Add(new PortData("sp", "The sketch plane.", typeof(SketchPlane)));
         OutPortData = new PortData("mc", "ModelCurve", typeof(ModelCurve));

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
         InPortData.Add(new PortData("solid?", "Is solid?", typeof(object)));
         InPortData.Add(new PortData("refListList", "ReferenceArrayArray", typeof(object)));

         OutPortData = new PortData("form", "Loft Form", typeof(object));

         base.RegisterInputsAndOutputs();
      }

      public override Expression Evaluate(FSharpList<Expression> args)
      {
         bool isSolid = ((Expression.Number)args[0]).Item == 1;

         IEnumerable<IEnumerable<Reference>> refArrays = ((Expression.List)args[1]).Item.Select(
            x => ((Expression.List)x).Item.Select(
               y => (Reference)((Expression.Container)y).Item
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

         return Expression.NewContainer(
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
         InPortData.Add(new PortData("refPts", "List of reference points", typeof(object)));
         OutPortData = new PortData("curve", "Curve from ref points", typeof(object));

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

         return Expression.NewContainer(c);
      }
   }

   [ElementName("CurveElement Reference")]
   [ElementCategory(BuiltinElementCategories.REVIT)]
   [ElementDescription("CurveyPoints.GeometryCurve.Reference")]
   [RequiresTransaction(true)]
   public class dynCurveRef : dynElement
   {
      public dynCurveRef()
      {
         InPortData.Add(new PortData("curve", "CurveByPoints", typeof(object)));
         OutPortData = new PortData("curveRef", "Reference", typeof(object));

         base.RegisterInputsAndOutputs();
      }

      public override Expression Evaluate(FSharpList<Expression> args)
      {
         var input = args[0];

         if (input.IsList)
         {
            return Expression.NewList(
               Utils.convertSequence(
                  ((Expression.List)input).Item.Select(
                     x =>
                        Expression.NewContainer(
                           ((CurveElement)((Expression.Container)x).Item).GeometryCurve.Reference
                        )
                  )
               )
            );
         }
         else
         {
            return Expression.NewContainer(
               ((CurveElement)((Expression.Container)args[0]).Item).GeometryCurve.Reference
            );
         }
      }
   }

   [ElementName("Planar Curve By Points")]
   [ElementCategory(BuiltinElementCategories.REVIT)]
   [ElementDescription("Node to create a planar model curve.")]
   [RequiresTransaction(true)]
   public class dynModelCurveByPoints : dynElement
   {
      public dynModelCurveByPoints()
      {
         InPortData.Add(new PortData("pts", "The points from which to create the curve", typeof(object)));
         OutPortData = new PortData("cv", "The curve(s) by points created by this operation.", typeof(ModelNurbSpline));

         base.RegisterInputsAndOutputs();
      }

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
   }
}
