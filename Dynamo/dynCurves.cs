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
   public class dynModelCurve : dynNode
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

         ModelCurve mc;

         if (this.Elements.Any())
         {
            Element e;
            if (dynUtils.TryGetElement(this.Elements[0], out e))
            {
               mc = e as ModelCurve;
               var loc = mc.Location as LocationCurve;
               loc.Curve = c;
            }
            else
            {
               mc = this.UIDocument.Document.IsFamilyDocument
                  ? this.UIDocument.Document.FamilyCreate.NewModelCurve(c, sp)
                  : this.UIDocument.Document.Create.NewModelCurve(c, sp);
               this.Elements[0] = mc.Id;
            }
         }
         else
         {
            mc = this.UIDocument.Document.IsFamilyDocument
               ? this.UIDocument.Document.FamilyCreate.NewModelCurve(c, sp)
               : this.UIDocument.Document.Create.NewModelCurve(c, sp);
            this.Elements.Add(mc.Id);
         }

         return Expression.NewContainer(mc);
      }
   }

   [ElementName("Loft Form")]
   [ElementCategory(BuiltinElementCategories.REVIT)]
   [ElementDescription("Creates a new loft form <doc.FamilyCreate.NewLoftForm>")]
   [RequiresTransaction(true)]
   public class dynLoftForm : dynNode
   {
      public dynLoftForm()
      {
         InPortData.Add(new PortData("solid/void", "True creates a solid, false a void", typeof(object)));
         InPortData.Add(new PortData("refListList", "ReferenceArrayArray", typeof(object)));
         InPortData.Add(new PortData("surface?", "Create a single surface or an extrusion if one loop", typeof(object)));

         OutPortData = new PortData("form", "Loft Form", typeof(object));

         base.RegisterInputsAndOutputs();
      }

      public override Expression Evaluate(FSharpList<Expression> args)
      {
         //If we already have a form stored...
         if (this.Elements.Any())
         {
            //Dissolve it, we will re-make it later.
            FormUtils.DissolveForms(this.UIDocument.Document, this.Elements.Take(1).ToList());
            //And register the form for deletion. Since we've already deleted it here manually, we can 
            //pass "true" as the second argument.
            this.DeleteElement(this.Elements[0], true);
         }

         //Solid argument
         bool isSolid = ((Expression.Number)args[0]).Item == 1;

         //Surface argument
         bool isSurface = ((Expression.Number)args[2]).Item == 1;

         //Build up our list of list of references for the form by...
         IEnumerable<IEnumerable<Reference>> refArrays = ((Expression.List)args[1]).Item.Select(
            //...first selecting everything in the topmost list...
            delegate(Expression x)
            {
               //If the element in the topmost list is a sub-list...
               if (x.IsList)
               {
                  //...then we return a new IEnumerable of References by converting the sub list.
                  return (x as Expression.List).Item.Select(
                     delegate(Expression y)
                     {
                        //Since we're in a sub-list, we can assume it's a container.
                        var item = ((Expression.Container)y).Item;
                        if (item is CurveElement)
                           return (item as CurveElement).GeometryCurve.Reference;
                        else
                           return (Reference)item;
                     }
                  );
               }
               //If the element is not a sub-list, then just assume it's a container.
               else
               {
                  var obj = ((Expression.Container)x).Item;
                  Reference r;
                  if (obj is CurveElement)
                  {
                     r = (obj as CurveElement).GeometryCurve.Reference;
                  }
                  else
                  {
                     r = (Reference)obj;
                  }
                  //We return a list here since it's expecting an IEnumerable<Reference>. In reality,
                  //just passing the element by itself instead of a sub-list is a shortcut for having
                  //a list with one element, so this is just performing that for the user.
                  return new List<Reference>() { r };
               }
            }
         );

         //Now we add all of those references into ReferenceArrays
         ReferenceArrayArray refArrArr = new ReferenceArrayArray();
         foreach (IEnumerable<Reference> refs in refArrays.Where(x => x.Any()))
         {
            var refArr = new ReferenceArray();
            foreach (Reference r in refs)
               refArr.Append(r);
            refArrArr.Append(refArr);
         }

         //We use the ReferenceArrayArray to make the form, and we store it for later runs.

         Form f;
          //if we only have a single refArr, we can make a capping surface or an extrusion
         if (refArrArr.Size == 1)
         {
             ReferenceArray refArr = refArrArr.get_Item(0);

             if (isSurface) // make a capping surface
             {
                 f = this.UIDocument.Document.FamilyCreate.NewFormByCap(true, refArr);
             }
             else  // make an extruded surface
             {
                 // The extrusion form direction
                 XYZ direction = new XYZ(0, 0, 50);
                 f = this.UIDocument.Document.FamilyCreate.NewExtrusionForm(true, refArr, direction);
             }
         }
         else // make a lofted surface
         {
             f = this.UIDocument.Document.FamilyCreate.NewLoftForm(isSolid, refArrArr);
         }

         this.Elements.Add(f.Id);

         return Expression.NewContainer(f);
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

   [ElementName("CurveElement Reference")]
   [ElementCategory(BuiltinElementCategories.REVIT)]
   [ElementDescription("CurveyPoints.GeometryCurve.Reference")]
   [RequiresTransaction(true)]
   public class dynCurveRef : dynNode
   {
      public dynCurveRef()
      {
         InPortData.Add(new PortData("curve", "CurveByPoints", typeof(object)));
         OutPortData = new PortData("curveRef", "Reference", typeof(object));

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

   //[ElementName("Planar Curve By Points")]
   //[ElementCategory(BuiltinElementCategories.REVIT)]
   //[ElementDescription("Node to create a planar model curve.")]
   //[RequiresTransaction(true)]
   //public class dynModelCurveByPoints : dynElement
   //{
   //   public dynModelCurveByPoints()
   //   {
   //      InPortData.Add(new PortData("pts", "The points from which to create the curve", typeof(object)));
   //      OutPortData = new PortData("cv", "The curve(s) by points created by this operation.", typeof(ModelNurbSpline));

   //      base.RegisterInputsAndOutputs();
   //   }

   //   public override Expression Evaluate(FSharpList<Expression> args)
   //   {
   //      var pts = ((Expression.List)args[0]).Item.Select(
   //         e => ((ReferencePoint)((Expression.Container)e).Item).Position
   //      ).ToList();

   //      if (pts.Count <= 1)
   //      {
   //         throw new Exception("Not enough reference points to make a curve.");
   //      }

   //      //make a curve
   //      NurbSpline ns = this.UIDocument.Application.Application.Create.NewNurbSpline(
   //         pts, Enumerable.Repeat(1.0, pts.Count).ToList()
   //      );

   //      double rawParam = ns.ComputeRawParameter(.5);
   //      Transform t = ns.ComputeDerivatives(rawParam, false);

   //      XYZ norm = t.BasisZ;

   //      if (norm.GetLength() == 0)
   //      {
   //         norm = XYZ.BasisZ;
   //      }

   //      Plane p = new Plane(norm, t.Origin);
   //      SketchPlane sp = this.UIDocument.Document.FamilyCreate.NewSketchPlane(p);
   //      //sps.Add(sp);

   //      ModelNurbSpline c = (ModelNurbSpline)this.UIDocument.Document.FamilyCreate.NewModelCurve(ns, sp);

   //      return Expression.NewContainer(c);
   //   }
   //}
}

