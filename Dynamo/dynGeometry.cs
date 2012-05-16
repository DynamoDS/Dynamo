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

         OutPortData.Add(new PortData(null, "xyz", "XYZ", typeof(XYZ)));

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

      //public override void Draw()
      //{
      //   if (CheckInputs())
      //   {
      //      //create the xyz
      //      pt = new XYZ((double)InPortData[0].Object,
      //          (double)InPortData[1].Object,
      //          (double)InPortData[2].Object);

      //      //OutPortData[0].Object = pt;
      //      this.Tree.Trunk.Leaves.Add(pt);
      //   }
      //}

      //public override void Destroy()
      //{
      //   pt = null;
      //   base.Destroy();
      //}

      //public override void Update()
      //{
      //   OnDynElementReadyToBuild(EventArgs.Empty);
      //}
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
         OutPortData.Add(new PortData(null, "P", "Plane", typeof(Plane)));
         //OutPortData[0].Object = this.Tree;

         base.RegisterInputsAndOutputs();
      }

      public override FScheme.Expression Evaluate(Microsoft.FSharp.Collections.FSharpList<FScheme.Expression> args)
      {
         XYZ ptA = (XYZ)((FScheme.Expression.Container)args[0]).Item;
         XYZ ptB = (XYZ)((FScheme.Expression.Container)args[1]).Item;

         return FScheme.Expression.NewContainer(
            this.UIDocument.Application.Application.Create.NewPlane(
               ptA, ptB
            )
         );
      }

      //public override void Draw()
      //{
      //   if (CheckInputs())
      //   {

      //      DataTree a = InPortData[0].Object as DataTree;
      //      DataTree b = InPortData[1].Object as DataTree;

      //      if (a != null && b != null)
      //      {
      //         Process(this.Tree.Trunk, a.Trunk, b.Trunk);
      //      }

      //   }
      //}

      //void Process(DataTreeBranch currBranch, DataTreeBranch a, DataTreeBranch b)
      //{
      //   foreach (object o in a.Leaves)
      //   {

      //      if (b.Leaves.Count > a.Leaves.IndexOf(o))
      //      {
      //         XYZ ptA = o as XYZ;
      //         XYZ ptB = b.Leaves[a.Leaves.IndexOf(o)] as XYZ;

      //         if (ptA != null && ptB != null)
      //         {
      //            this.Tree.Trunk.Leaves.Add(dynElementSettings.SharedInstance.Doc.Application.Application.Create.NewPlane(ptA, ptB));
      //         }
      //      }

      //   }

      //   foreach (DataTreeBranch aChild in a.Branches)
      //   {
      //      DataTreeBranch subBranch = new DataTreeBranch();
      //      currBranch.Branches.Add(subBranch);

      //      int idx = a.Branches.IndexOf(aChild);

      //      if (b.Branches.Count > idx)
      //      {
      //         Process(subBranch, aChild, b.Branches[idx]);
      //      }
      //   }
      //}

      //public override void Destroy()
      //{
      //   base.Destroy();
      //}

      //public override void Update()
      //{
      //   OnDynElementReadyToBuild(EventArgs.Empty);
      //}
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
         OutPortData.Add(new PortData(null, "SP", "SketchPlane", typeof(dynSketchPlane)));
         //OutPortData[0].Object = this.Tree;

         base.RegisterInputsAndOutputs();
      }

      public override FScheme.Expression Evaluate(Microsoft.FSharp.Collections.FSharpList<FScheme.Expression> args)
      {
         Plane p = (Plane)((FScheme.Expression.Container)args[0]).Item;

         SketchPlane sp = (this.UIDocument.Document.IsFamilyDocument)
            ? this.UIDocument.Document.FamilyCreate.NewSketchPlane(p)
            : this.UIDocument.Document.Create.NewSketchPlane(p);

         return FScheme.Expression.NewContainer(sp);
      }

      //public override void Draw()
      //{
      //   if (CheckInputs())
      //   {
      //      //OutPortData[0].Object = dynElementSettings.SharedInstance.Doc.Document.FamilyCreate.NewSketchPlane(InPortData[0].Object as Plane);

      //      DataTree a = InPortData[0].Object as DataTree;
      //      if (a != null)
      //      {
      //         Process(this.Tree.Trunk, a.Trunk);
      //      }
      //   }
      //}

      //void Process(DataTreeBranch currBranch, DataTreeBranch a)
      //{
      //   foreach (object o in a.Leaves)
      //   {
      //      Plane p = o as Plane;
      //      if (p != null)
      //      {
      //         currBranch.Leaves.Add(dynElementSettings.SharedInstance.Doc.Document.FamilyCreate.NewSketchPlane(p));
      //      }
      //   }

      //   foreach (DataTreeBranch aChild in a.Branches)
      //   {
      //      DataTreeBranch subBranch = new DataTreeBranch();
      //      currBranch.Branches.Add(subBranch);

      //      Process(subBranch, aChild);
      //   }

      //}

      //public override void Destroy()
      //{
      //   base.Destroy();
      //}

      //public override void Update()
      //{
      //   OnDynElementReadyToBuild(EventArgs.Empty);
      //}
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
         OutPortData.Add(new PortData(null, "line", "Line", typeof(Line)));

         base.RegisterInputsAndOutputs();
      }

      public override FScheme.Expression Evaluate(Microsoft.FSharp.Collections.FSharpList<FScheme.Expression> args)
      {
         var ptA = (XYZ)((FScheme.Expression.Container)args[0]).Item;
         var ptB = (XYZ)((FScheme.Expression.Container)args[1]).Item;

         return FScheme.Expression.NewContainer(
            this.UIDocument.Application.Application.Create.NewLineBound(
               ptA, ptB
            )
         );
      }

      //   public override void Draw()
      //   {
      //      if (CheckInputs())
      //      {

      //         DataTree a = InPortData[0].Object as DataTree;
      //         DataTree b = InPortData[1].Object as DataTree;

      //         if (a != null && b != null)
      //         {
      //            Process(this.Tree.Trunk, a.Trunk, b.Trunk);
      //         }
      //      }
      //   }

      //   void Process(DataTreeBranch currBranch, DataTreeBranch a, DataTreeBranch b)
      //   {

      //      foreach (object o in a.Leaves)
      //      {
      //         if (b.Leaves.Count > a.Leaves.IndexOf(o))
      //         {
      //            XYZ ptA = o as XYZ;
      //            XYZ ptB = b.Leaves[a.Leaves.IndexOf(o)] as XYZ;

      //            if (ptA != null && ptB != null)
      //            {
      //               Curve c = dynElementSettings.SharedInstance.Doc.Application.Application.Create.NewLineBound(ptA, ptB);
      //               currBranch.Leaves.Add(c);
      //            }
      //         }
      //      }

      //      foreach (DataTreeBranch aChild in a.Branches)
      //      {
      //         DataTreeBranch subBranch = new DataTreeBranch();
      //         currBranch.Branches.Add(subBranch);

      //         int idx = a.Branches.IndexOf(aChild);

      //         if (b.Branches.Count > idx)
      //         {
      //            Process(subBranch, aChild, b.Branches[idx]);
      //         }
      //      }
      //   }

      //   public override void Destroy()
      //   {
      //      base.Destroy();
      //   }

      //   public override void Update()
      //   {
      //      OnDynElementReadyToBuild(EventArgs.Empty);
      //   }
   }
}
