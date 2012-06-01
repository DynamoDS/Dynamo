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
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Connectors;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Dynamo.FSchemeInterop;
using Expression = Dynamo.FScheme.Expression;

namespace Dynamo.Elements
{
   [ElementName("Reference Point")]
   [ElementCategory(BuiltinElementCategories.REVIT)]
   [ElementDescription("An element which creates a reference point.")]
   [RequiresTransaction(true)]
   public class dynReferencePointByXYZ : dynElement
   {
      public dynReferencePointByXYZ()
      {
         InPortData.Add(new PortData("xyz", "The point(s) from which to create reference points.", typeof(XYZ)));
         OutPortData = new PortData("pt", "The Reference Point(s) created from this operation.", typeof(ReferencePoint));

         base.RegisterInputsAndOutputs();
      }

      public override Expression Evaluate(FSharpList<Expression> args)
      {
         var input = args[0];
         if (input.IsList)
         {
            int count = 0;
            var xyzList = ((Expression.List)input).Item;
            var result = Utils.convertSequence(
               xyzList.Select(
                  delegate(Expression x)
                  {
                     ReferencePoint pt;
                     if (this.Elements.Count > count)
                     {
                        pt = (ReferencePoint)this.Elements[count];
                        pt.Position = (XYZ)((Expression.Container)x).Item;
                     }
                     else
                     {
                        pt = this.UIDocument.Document.FamilyCreate.NewReferencePoint(
                           (XYZ)((Expression.Container)x).Item
                        );
                        this.Elements.Add(pt);
                     }
                     count++;
                     return Expression.NewContainer(pt);
                  }
               )
            );

            int delCount = 0;
            foreach (var e in this.Elements.Skip(count))
            {
               this.UIDocument.Document.Delete(e);
               delCount++;
            }
            if (delCount > 0)
               this.Elements.RemoveRange(count, delCount);

            return Expression.NewList(result);
         }
         else
         {
            XYZ xyz = (XYZ)((Expression.Container)input).Item;

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

            return Expression.NewContainer(pt);
         }
      }
   }

   [ElementName("Reference Point Distance")]
   [ElementCategory(BuiltinElementCategories.REVIT)]
   [ElementDescription("An element which measures a distance between reference point(s).")]
   [RequiresTransaction(false)]
   public class dynDistanceBetweenPoints : dynElement
   {
      public dynDistanceBetweenPoints()
      {
         InPortData.Add(new PortData("ptA", "Element to measure to.", typeof(Element)));
         InPortData.Add(new PortData("ptB", "A Reference point.", typeof(ReferencePoint)));

         OutPortData = new PortData("dist", "Distance between points.", typeof(dynDouble));

         base.RegisterInputsAndOutputs();

      }

      public override Expression Evaluate(FSharpList<Expression> args)
      {
         object arg0 = ((Expression.Container)args[0]).Item;
         XYZ ptB = ((ReferencePoint)((Expression.Container)args[1]).Item).Position;

         if (arg0 is ReferencePoint)
         {
            return Expression.NewNumber(((ReferencePoint)arg0).Position.DistanceTo(ptB));
         }
         else if (arg0 is FamilyInstance)
         {
            return Expression.NewNumber(
               ((LocationPoint)((FamilyInstance)arg0).Location).Point.DistanceTo(ptB)
            );
         }
         else
         {
            throw new Exception("Cannot cast first argument to ReferencePoint or FamilyInstance.");
         }
      }
   }

   [ElementName("Point On Edge")]
   [ElementCategory(BuiltinElementCategories.REVIT)]
   [ElementDescription("Create an element which owns a reference point on a selected edge.")]
   [RequiresTransaction(true)]
   public class dynPointOnEdge : dynElement
   {
      public dynPointOnEdge()
      {
         InPortData.Add(new PortData("curve", "ModelCurve", typeof(ModelCurve)));
         InPortData.Add(new PortData("t", "Parameter on edge.", typeof(double)));
         OutPortData = new PortData("pt", "PointOnEdge", typeof(PointOnEdge));

         base.RegisterInputsAndOutputs();
      }

      public override Expression Evaluate(FSharpList<Expression> args)
      {
         Reference r = ((ModelCurve)((Expression.Container)args[0]).Item).GeometryCurve.Reference;
         double t = ((Expression.Number)args[1]).Item;

         return Expression.NewContainer(
            this.UIDocument.Application.Application.Create.NewPointOnEdge(r, t)
         );
      }
   }
}

