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
   [ElementDescription("Creates a model curve.")]
   [RequiresTransaction(true)]
   public class dynModelCurve : dynNode
   {
      public dynModelCurve()
      {
         InPortData.Add(new PortData("c", "A Geometric Curve.", typeof(Curve)));
         InPortData.Add(new PortData("sp", "The Sketch Plane.", typeof(SketchPlane)));
         OutPortData = new PortData("mc", "Model Curve", typeof(ModelCurve));

         base.RegisterInputsAndOutputs();
      }

      public override Expression Evaluate(FSharpList<Expression> args)
      {
         Curve c = (Curve)((Expression.Container)args[0]).Item;
         SketchPlane sp = (SketchPlane)((Expression.Container)args[1]).Item;

         ModelCurve mc;
         XYZ spOrigin = sp.Plane.Origin;
         XYZ modelOrigin = XYZ.Zero;
         Transform trf = Transform.get_Translation(spOrigin);
         trf =  trf.Multiply(Transform.get_Rotation(spOrigin,XYZ.BasisZ,spOrigin.AngleOnPlaneTo(XYZ.BasisY,spOrigin)));
         Curve ct = c.get_Transformed(trf);



         if (this.Elements.Any())
         {
            Element e;
            if (dynUtils.TryGetElement(this.Elements[0], out e))
            {
               mc = e as ModelCurve;
               mc.SketchPlane = sp;
               var loc = mc.Location as LocationCurve;
               loc.Curve = ct;
              
            }
            else
            {
               mc = this.UIDocument.Document.IsFamilyDocument
                  ? this.UIDocument.Document.FamilyCreate.NewModelCurve(ct, sp)
                  : this.UIDocument.Document.Create.NewModelCurve(ct, sp);
               this.Elements[0] = mc.Id;
               mc.SketchPlane = sp;
               
               
            }
         }
         else
         {
            mc = this.UIDocument.Document.IsFamilyDocument
               ? this.UIDocument.Document.FamilyCreate.NewModelCurve(ct, sp)
               : this.UIDocument.Document.Create.NewModelCurve(ct, sp);
            this.Elements.Add(mc.Id);
            mc.SketchPlane = sp;
         }

         return Expression.NewContainer(mc);
      }
   }

   [ElementName("Curve By Points")]
   [ElementCategory(BuiltinElementCategories.REVIT)]
   [ElementDescription("doc.FamilyCreate.NewCurveByPoints")]
   [RequiresTransaction(true)]
   public class dynCurveByPoints : dynNode
   {
      public dynCurveByPoints()
      {
         InPortData.Add(new PortData("refPts", "List of reference points", typeof(object)));
         OutPortData = new PortData("curve", "Curve from ref points", typeof(object));

         base.RegisterInputsAndOutputs();
      }

      public override Expression Evaluate(FSharpList<Expression> args)
      {
         //Our eventual output.
         CurveByPoints c;

         //Build a sequence that unwraps the input list from it's Expression form.
         IEnumerable<ReferencePoint> refPts = ((Expression.List)args[0]).Item.Select(
            x => (ReferencePoint)((Expression.Container)x).Item
         );

         //Add all of the elements in the sequence to a ReferencePointArray.
         ReferencePointArray refPtArr = new ReferencePointArray();
         foreach (var refPt in refPts)
         {
            refPtArr.Append(refPt);
         }

         //Standard logic for updating an old result, if it exists.
         if (this.Elements.Any())
         {
            Element e;
            if (dynUtils.TryGetElement(this.Elements[0], out e))
            {
               c = e as CurveByPoints;
               c.SetPoints(refPtArr);
            }
            else
            {
               //TODO: This method of handling bad elements may cause problems. Instead of overwriting
               //      index in Elements, might be better to just add it the Elements and then do
               //      this.DeleteElement(id, true) on the old index.
               c = this.UIDocument.Document.FamilyCreate.NewCurveByPoints(refPtArr);
               this.Elements[0] = c.Id;
            }
         }
         else
         {
            c = this.UIDocument.Document.FamilyCreate.NewCurveByPoints(refPtArr);
            this.Elements.Add(c.Id);
         }

         return Expression.NewContainer(c);
      }
   }

   [ElementName("Curve Element Reference")]
   [ElementCategory(BuiltinElementCategories.REVIT)]
   [ElementDescription("Takes in a Model Curve or Geometry Curve, returns a Curve References")]
   [RequiresTransaction(true)]
   public class dynCurveRef : dynNode
   {
      public dynCurveRef()
      {
         InPortData.Add(new PortData("curve", "Model Curve Element", typeof(object)));
         OutPortData = new PortData("curveRef", "Curve Reference", typeof(object));

         base.RegisterInputsAndOutputs();
      }

      private Expression makeCurveRef(object c, int count)
      {
          Reference r = c is CurveElement
             ? (c as CurveElement).GeometryCurve.Reference // curve element
             : (c as Curve).Reference; // geometry curve

          //Reference r;
          //if (c is CurveElement)
          //{
          //    r = (c as CurveElement).GeometryCurve.Reference;
          //    return Expression.NewContainer(r);
          //}
          //else if (c is Curve)
          //{
          //    r = (c as Curve).Reference;
          //    return Expression.NewContainer(r);
          //}
          //else if (c is Reference)
          //{
          //    r = c as Reference;
          //    return Expression.NewContainer(r);
          //}
      
          return Expression.NewContainer(r);
      }


      public override Expression Evaluate(FSharpList<Expression> args)
      {
         var input = args[0];

         if (input.IsList)
         {
            int count = 0;
            var result =  Expression.NewList(
               Utils.convertSequence(
                  (input as Expression.List).Item.Select(
                     x =>
                            this.makeCurveRef(
                            ((Expression.Container)x).Item,
                            count++
                        )
                  )
               )
            );
            foreach (var e in this.Elements.Skip(count))
            {
                this.DeleteElement(e);
            }

            return result;
         }
         else
         {
            var result = this.makeCurveRef(
                   ((Expression.Container)input).Item,
                   0

                );

                foreach (var e in this.Elements.Skip(1))
                {
                    this.DeleteElement(e);
                }

                return result;
         }
      }

   }

   [ElementName("Planar Nurb Spline")]
   [ElementCategory(BuiltinElementCategories.REVIT)]
   [ElementDescription("Node to create a planar model curve.")]
   [RequiresTransaction(true)]
   public class dynModelCurveNurbSpline : dynNode
   {
       public dynModelCurveNurbSpline()
       {
           InPortData.Add(new PortData("pts", "The points from which to create the nurbes curve", typeof(object)));
           OutPortData = new PortData("cv", "The nurbs spline model curve created by this operation.", typeof(ModelNurbSpline));

           base.RegisterInputsAndOutputs();
       }

       public override Expression Evaluate(FSharpList<Expression> args)
       {
           var pts = ((Expression.List)args[0]).Item.Select(
              e => ((ReferencePoint)((Expression.Container)e).Item).Position
           ).ToList();


           foreach (ElementId el in this.Elements)
           {
               Element e;
               if (dynUtils.TryGetElement(el, out e))
               {
                   this.UIDocument.Document.Delete(el);
               }
           }
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

           this.Elements.Add(c.Id);

           return Expression.NewContainer(c);
       }
   }
}

