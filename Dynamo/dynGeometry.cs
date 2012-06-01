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

namespace Dynamo.Elements
{
   [ElementName("XYZ")]
   [ElementCategory(BuiltinElementCategories.REVIT)]
   [ElementDescription("An element which creates an XYZ from three double values.")]
   [RequiresTransaction(false)]
   public class dynXYZ : dynElement
   {
      public dynXYZ()
      {
         InPortData.Add(new PortData(null, "X", "X", typeof(double)));
         InPortData.Add(new PortData(null, "Y", "Y", typeof(double)));
         InPortData.Add(new PortData(null, "Z", "Z", typeof(double)));

         OutPortData = new PortData(null, "xyz", "XYZ", typeof(XYZ));

         base.RegisterInputsAndOutputs();
      }

      public override FScheme.Expression Evaluate(Microsoft.FSharp.Collections.FSharpList<FScheme.Expression> args)
      {
         double x, y, z;
         x = (args[0] as FScheme.Expression.Number).Item;
         y = (args[1] as FScheme.Expression.Number).Item;
         z = (args[2] as FScheme.Expression.Number).Item;

         return FScheme.Expression.NewContainer(new XYZ(x, y, z));
      }
   }

   [ElementName("Plane")]
   [ElementCategory(BuiltinElementCategories.REVIT)]
   [ElementDescription("An element which creates a geometric plane.")]
   [RequiresTransaction(false)]
   public class dynPlane : dynElement
   {
      public dynPlane()
      {
         InPortData.Add(new PortData(null, "normal", "Normal Point (XYZ)", typeof(XYZ)));
         InPortData.Add(new PortData(null, "origin", "Origin Point (XYZ)", typeof(XYZ)));
         OutPortData = new PortData(null, "P", "Plane", typeof(Plane));

         base.RegisterInputsAndOutputs();
      }

      public override FScheme.Expression Evaluate(Microsoft.FSharp.Collections.FSharpList<FScheme.Expression> args)
      {
         XYZ ptA = (XYZ)((FScheme.Expression.Container)args[0]).Item;
         XYZ ptB = (XYZ)((FScheme.Expression.Container)args[1]).Item;

         var plane = this.UIDocument.Application.Application.Create.NewPlane(
            ptA, ptB
         );

         return FScheme.Expression.NewContainer(plane);
      }
   }

   [ElementName("Sketch Plane")]
   [ElementCategory(BuiltinElementCategories.REVIT)]
   [ElementDescription("An element which creates a geometric sketch plane.")]
   [RequiresTransaction(true)]
   public class dynSketchPlane : dynElement
   {
      public dynSketchPlane()
      {
         InPortData.Add(new PortData(null, "plane", "The plane in which to define the sketch.", typeof(dynPlane)));
         OutPortData = new PortData(null, "SP", "SketchPlane", typeof(dynSketchPlane));

         base.RegisterInputsAndOutputs();
      }

      public override FScheme.Expression Evaluate(Microsoft.FSharp.Collections.FSharpList<FScheme.Expression> args)
      {
         Plane p = (Plane)((FScheme.Expression.Container)args[0]).Item;

         SketchPlane sp;

         //TODO: Handle Removal
         if (this.Elements.Any())
         {
            sp = (SketchPlane)this.Elements[0];
         }

         sp = (this.UIDocument.Document.IsFamilyDocument)
            ? this.UIDocument.Document.FamilyCreate.NewSketchPlane(p)
            : this.UIDocument.Document.Create.NewSketchPlane(p);

         //this.Elements.Add(sp);

         return FScheme.Expression.NewContainer(sp);
      }
   }

   [ElementName("Line")]
   [ElementCategory(BuiltinElementCategories.REVIT)]
   [ElementDescription("An element which creates a geometric line.")]
   [RequiresTransaction(true)]
   public class dynLineBound : dynElement
   {
      public dynLineBound()
      {
         InPortData.Add(new PortData(null, "start", "Start XYZ", typeof(XYZ)));
         InPortData.Add(new PortData(null, "end", "End XYZ", typeof(XYZ)));
         InPortData.Add(new PortData(null, "bound?", "Boolean: Is this line bounded?", typeof(bool)));
         OutPortData = new PortData(null, "line", "Line", typeof(Line));

         base.RegisterInputsAndOutputs();
      }

      public override FScheme.Expression Evaluate(Microsoft.FSharp.Collections.FSharpList<FScheme.Expression> args)
      {
         var ptA = (XYZ)((FScheme.Expression.Container)args[0]).Item;
         var ptB = (XYZ)((FScheme.Expression.Container)args[1]).Item;
         var bound = ((FScheme.Expression.Number)args[2]).Item == 1;

         return FScheme.Expression.NewContainer(
            this.UIDocument.Application.Application.Create.NewLine(
               ptA, ptB, bound
            )
         );
      }
   }
   [ElementName("UV")]
   [ElementCategory(BuiltinElementCategories.REVIT)]
   [ElementDescription("An element which creates a UV from two double values.")]
   [RequiresTransaction(false)]
   public class dynUV : dynElement
   {
       public dynUV()
       {
           InPortData.Add(new PortData(null, "U", "U", typeof(double)));
           InPortData.Add(new PortData(null, "V", "V", typeof(double)));


           OutPortData = new PortData(null, "uv", "UV", typeof(UV));

           base.RegisterInputsAndOutputs();
       }

       public override FScheme.Expression Evaluate(Microsoft.FSharp.Collections.FSharpList<FScheme.Expression> args)
       {
           double u, v;
           u = (args[0] as FScheme.Expression.Number).Item;
           v = (args[1] as FScheme.Expression.Number).Item;


           return FScheme.Expression.NewContainer(new UV(u, v));
       }
   }
}
