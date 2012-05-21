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

namespace Dynamo.Elements
{
   public abstract class dynReferencePoint : dynElement
   {
      public dynReferencePoint()
      {
         OutPortData = new PortData(null, "pt", "The Reference Point(s) created from this operation.", typeof(ReferencePoint));
         //OutPortData[0].Object = this.Tree;
      }

      //public override void Draw()
      //{

      //}

      //public override void Destroy()
      //{
      //   //base destroys all elements in the collection
      //   base.Destroy();
      //}

      //public override void Update()
      //{
      //   OnDynElementReadyToBuild(EventArgs.Empty);
      //}
   }

   [ElementName("Reference Point")]
   [ElementCategory(BuiltinElementCategories.REVIT)]
   [ElementDescription("An element which creates a reference point.")]
   [RequiresTransaction(true)]
   public class dynReferencePointByXYZ : dynReferencePoint
   {
      public dynReferencePointByXYZ()
      {
         InPortData.Add(new PortData(null, "xyz", "The point(s) from which to create reference points.", typeof(XYZ)));

         //outport already added in parent

         base.RegisterInputsAndOutputs();
      }

      public override FScheme.Expression Evaluate(Microsoft.FSharp.Collections.FSharpList<FScheme.Expression> args)
      {
         var input = args[0];

         if (input.IsList)
         {
            var xyzList = ((FScheme.Expression.List)input).Item;

            return FScheme.Expression.NewList(
               FSchemeInterop.Utils.convertSequence(
                  xyzList.Select(
                     delegate (FScheme.Expression x)
                     {
                        var pt = this.UIDocument.Document.FamilyCreate.NewReferencePoint(
                           (XYZ)((FScheme.Expression.Container)x).Item
                        );
                        this.Elements.Add(pt);
                        return FScheme.Expression.NewContainer(pt);
                     }
                  )
               )
            );
         }
         else
         {
            XYZ xyz = (XYZ)((FScheme.Expression.Container)input).Item;

            var pt = this.UIDocument.Document.FamilyCreate.NewReferencePoint(xyz);

            this.Elements.Add(pt);

            //return FScheme.Expression.NewContainer(promise);
            return FScheme.Expression.NewContainer(pt);
         }
      }

      //public override void Draw()
      //{
      //   if (CheckInputs())
      //   {
      //      DataTree a = InPortData[0].Object as DataTree;
      //      if (a != null)
      //      {
      //         Process(this.Tree.Trunk, a.Trunk);
      //      }

      //   }
      //}

      //public void Process(DataTreeBranch currBranch, DataTreeBranch a)
      //{
      //   //foreach (object o in a.Leaves)
      //   //{
      //   //   XYZ pt = o as XYZ;
      //   //   if (pt != null)
      //   //   {
      //   //      ReferencePoint rp = dynElementSettings.SharedInstance.Doc.Document.FamilyCreate.NewReferencePoint(pt);
      //   //      currBranch.Leaves.Add(rp);
      //   //      Elements.Append(rp);
      //   //   }
      //   //}

      //   //foreach (DataTreeBranch aChild in a.Branches)
      //   //{
      //   //   DataTreeBranch subBranch = new DataTreeBranch();
      //   //   currBranch.Branches.Add(subBranch);

      //   //   Process(subBranch, aChild);
      //   //}
      //}

      //public override void Destroy()
      //{
      //   //base destroys all elements in the collection
      //   base.Destroy();
      //}

      //public override void Update()
      //{
      //   OnDynElementReadyToBuild(EventArgs.Empty);
      //}
   }

   [ElementName("Reference Point Grid")]
   [ElementCategory(BuiltinElementCategories.REVIT)]
   [ElementDescription("An element which creates a grid of reference points.")]
   [RequiresTransaction(true)]
   public class dynReferencePtGrid : dynReferencePoint
   {
      public dynReferencePtGrid()
      {
         InPortData.Add(new PortData(null, "x-count", "Number in the X direction.", typeof(double)));
         InPortData.Add(new PortData(null, "y-count", "Number in the Y direction.", typeof(double)));
         InPortData.Add(new PortData(null, "z-count", "Number in the Z direction.", typeof(double)));
         InPortData.Add(new PortData(null, "x0", "Starting X Coordinate", typeof(double)));
         InPortData.Add(new PortData(null, "y0", "Starting Y Coordinate", typeof(double)));
         InPortData.Add(new PortData(null, "z0", "Starting Z Coordinate", typeof(double)));
         InPortData.Add(new PortData(null, "x-space", "The X spacing.", typeof(double)));
         InPortData.Add(new PortData(null, "y-space", "The Y spacing.", typeof(double)));
         InPortData.Add(new PortData(null, "z-space", "The Z spacing.", typeof(double)));

         //outports already added in parent

         base.RegisterInputsAndOutputs();
      }

      public override FScheme.Expression Evaluate(FSharpList<FScheme.Expression> args)
      {
         double xi, yi, zi, x0, y0, z0, xs, ys, zs;

         xi = ((FScheme.Expression.Number)args[0]).Item;
         yi = ((FScheme.Expression.Number)args[1]).Item;
         zi = ((FScheme.Expression.Number)args[2]).Item;
         x0 = ((FScheme.Expression.Number)args[3]).Item;
         y0 = ((FScheme.Expression.Number)args[4]).Item;
         z0 = ((FScheme.Expression.Number)args[5]).Item;
         xs = ((FScheme.Expression.Number)args[6]).Item;
         ys = ((FScheme.Expression.Number)args[7]).Item;
         zs = ((FScheme.Expression.Number)args[8]).Item;

         FSharpList<FScheme.Expression> result = FSharpList<FScheme.Expression>.Empty;

         double z = z0;
         for (int zCount = 0; zCount < zi; zCount++)
         {
            double y = y0;
            for (int yCount = 0; yCount < yi; yCount++)
            {
               double x = x0;
               for (int xCount = 0; xCount < xi; xCount++)
               {
                  result = FSharpList<FScheme.Expression>.Cons(
                     FScheme.Expression.NewContainer(
                        this.UIDocument.Document.FamilyCreate.NewReferencePoint(new XYZ(x, y, z))
                     ),
                     result
                  );
                  x += xs;
               }
               y += ys;
            }
            z += zs;
         }

         return FScheme.Expression.NewList(result);
      }

      //public override void Draw()
      //{
      //   if (CheckInputs())
      //   {
      //      DataTree xyzTree = InPortData[2].Object as DataTree;
      //      if (xyzTree != null)
      //      {
      //         Process(xyzTree.Trunk, this.Tree.Trunk);
      //      }
      //   }
      //}

      //public void Process(DataTreeBranch bIn, DataTreeBranch currentBranch)
      //{

      //   //use each XYZ leaf on the input
      //   //to define a new origin
      //   foreach (object o in bIn.Leaves)
      //   {
      //      ReferencePoint rp = o as ReferencePoint;

      //      if (rp != null)
      //      {
      //         for (int i = 0; i < (int)InPortData[0].Object; i++)
      //         {
      //            //create a branch for the data tree for
      //            //this row of points
      //            DataTreeBranch b = new DataTreeBranch();
      //            currentBranch.Branches.Add(b);

      //            for (int j = 0; j < (int)InPortData[1].Object; j++)
      //            {
      //               XYZ pt = new XYZ(rp.Position.X + i * (double)InPortData[3].Object,
      //                   rp.Position.Y + j * (double)InPortData[4].Object,
      //                   rp.Position.Z);

      //               ReferencePoint rpNew = dynElementSettings.SharedInstance.Doc.Document.FamilyCreate.NewReferencePoint(pt);

      //               //add the point as a leaf on the branch
      //               b.Leaves.Add(rpNew);

      //               //add the element to the collection
      //               Elements.Append(rpNew);
      //            }
      //         }
      //      }
      //   }

      //   foreach (DataTreeBranch b1 in bIn.Branches)
      //   {
      //      DataTreeBranch newBranch = new DataTreeBranch();
      //      currentBranch.Branches.Add(newBranch);

      //      Process(b1, newBranch);
      //   }

      //}

      //public override void Destroy()
      //{
      //   //base destroys all elements in the collection
      //   base.Destroy();
      //}

      //public override void Update()
      //{
      //   OnDynElementReadyToBuild(EventArgs.Empty);
      //}
   }

   [ElementName("Reference Point Distance")]
   [ElementCategory(BuiltinElementCategories.REVIT)]
   [ElementDescription("An element which measures a distance between reference point(s).")]
   [RequiresTransaction(false)]
   public class dynDistanceBetweenPoints : dynElement
   {
      public dynDistanceBetweenPoints()
      {
         InPortData.Add(new PortData(null, "ptA", "Element to measure to.", typeof(Element)));
         InPortData.Add(new PortData(null, "ptB", "A Reference point.", typeof(ReferencePoint)));

         OutPortData = new PortData(null, "dist", "Distance between points.", typeof(dynDouble));
         //OutPortData[0].Object = this.Tree;

         base.RegisterInputsAndOutputs();

      }

      public override FScheme.Expression Evaluate(FSharpList<FScheme.Expression> args)
      {
         object arg0 = ((FScheme.Expression.Container)args[0]).Item;
         XYZ ptB = ((ReferencePoint)((FScheme.Expression.Container)args[1]).Item).Position;

         if (arg0 is ReferencePoint)
         {
            return FScheme.Expression.NewNumber(((ReferencePoint)arg0).Position.DistanceTo(ptB));
         }
         else if (arg0 is FamilyInstance)
         {
            return FScheme.Expression.NewNumber(
               ((LocationPoint)((FamilyInstance)arg0).Location).Point.DistanceTo(ptB)
            );
         }
         else
         {
            throw new Exception("Cannot cast first argument to ReferencePoint or FamilyInstance.");
         }
      }

      //public override void Draw()
      //{
      //   if (CheckInputs())
      //   {
      //      DataTree treeA = InPortData[0].Object as DataTree;
      //      DataTree treeB = InPortData[1].Object as DataTree;

      //      if (treeB != null && treeB.Trunk.Leaves.Count > 0)
      //      {
      //         //we're only using the first point in the tree right now.
      //         if (treeB.Trunk.Leaves.Count > 0)
      //         {
      //            ReferencePoint pt = treeB.Trunk.FindFirst() as ReferencePoint;

      //            if (treeA != null && pt != null)
      //            {
      //               //find out what kind of elements the tree hash
      //               ReferencePoint rp = treeA.Trunk.FindFirst() as ReferencePoint;
      //               FamilyInstance fi = treeA.Trunk.FindFirst() as FamilyInstance;
      //               if (rp != null)
      //               {
      //                  Process(treeA.Trunk, this.Tree.Trunk);
      //               }
      //               else if (fi != null)
      //               {
      //                  ProcessInstances(treeA.Trunk, this.Tree.Trunk);
      //               }

      //            }
      //         }
      //      }
      //   }
      //}

      //public void Process(DataTreeBranch bIn, DataTreeBranch currentBranch)
      //{
      //   DataTree dt = InPortData[1].Object as DataTree;
      //   ReferencePoint attractor = dt.Trunk.FindFirst() as ReferencePoint;

      //   //use each XYZ leaf on the input
      //   //to define a new origin
      //   foreach (object o in bIn.Leaves)
      //   {
      //      ReferencePoint rp = o as ReferencePoint;

      //      if (rp != null)
      //      {
      //         //get the distance betweent the points

      //         double dist = rp.Position.DistanceTo(attractor.Position);
      //         currentBranch.Leaves.Add(dist);
      //      }
      //   }

      //   foreach (DataTreeBranch b1 in bIn.Branches)
      //   {
      //      DataTreeBranch newBranch = new DataTreeBranch();
      //      currentBranch.Branches.Add(newBranch);

      //      Process(b1, newBranch);
      //   }

      //}

      //public void ProcessInstances(DataTreeBranch bIn, DataTreeBranch currentBranch)
      //{
      //   DataTree dt = InPortData[1].Object as DataTree;
      //   ReferencePoint attractor = dt.Trunk.FindFirst() as ReferencePoint;

      //   //use each XYZ leaf on the input
      //   //to define a new origin
      //   foreach (object o in bIn.Leaves)
      //   {
      //      FamilyInstance fi = o as FamilyInstance;

      //      if (fi != null)
      //      {
      //         //get the distance betweent the points
      //         LocationPoint lp = fi.Location as LocationPoint;
      //         if (lp != null)
      //         {
      //            double dist = lp.Point.DistanceTo(attractor.Position);
      //            currentBranch.Leaves.Add(dist);
      //         }
      //      }
      //   }

      //   foreach (DataTreeBranch b1 in bIn.Branches)
      //   {
      //      DataTreeBranch newBranch = new DataTreeBranch();
      //      currentBranch.Branches.Add(newBranch);

      //      ProcessInstances(b1, newBranch);
      //   }

      //}

      //public override void Destroy()
      //{
      //   //base destroys all elements in the collection
      //   base.Destroy();
      //}

      //public override void Update()
      //{
      //   OnDynElementReadyToBuild(EventArgs.Empty);
      //}
   }

   [ElementName("Point On Edge")]
   [ElementCategory(BuiltinElementCategories.REVIT)]
   [ElementDescription("Create an element which owns a reference point on a selected edge.")]
   [RequiresTransaction(true)]
   public class dynPointOnEdge : dynElement
   {
      public dynPointOnEdge()
      {
         InPortData.Add(new PortData(null, "curve", "ModelCurve", typeof(ModelCurve)));
         InPortData.Add(new PortData(null, "t", "Parameter on edge.", typeof(double)));
         OutPortData = new PortData(null, "pt", "PointOnEdge", typeof(PointOnEdge));

         base.RegisterInputsAndOutputs();
      }

      public override FScheme.Expression Evaluate(FSharpList<FScheme.Expression> args)
      {
         Reference r = ((ModelCurve)((FScheme.Expression.Container)args[0]).Item).GeometryCurve.Reference;
         double t = ((FScheme.Expression.Number)args[1]).Item;

         return FScheme.Expression.NewContainer(
            this.UIDocument.Application.Application.Create.NewPointOnEdge(r, t)
         );
      }

      //public override void Draw()
      //{
      //   if (CheckInputs())
      //   {

      //      Reference r = (InPortData[0].Object as ModelCurve).GeometryCurve.Reference;
      //      OutPortData[0].Object = dynElementSettings.SharedInstance.Doc.Application.Application.Create.NewPointOnEdge(r, (double)InPortData[1].Object);

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
}

