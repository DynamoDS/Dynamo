using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using Dynamo.Utilities;
using Dynamo.Connectors;

using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using Expression = Dynamo.FScheme.Expression;

namespace Dynamo.Elements
{
   [ElementName("Divided Surface by Selection")]
   [ElementCategory(BuiltinElementCategories.REVIT)]
   [ElementDescription("An element which allows the user to select a divided surface.")]
   [RequiresTransaction(true)]
    public class dynDividedSurfaceBySelection : dynElement
   {
      Form f;
      DividedSurfaceData dsd;
      //DataTree seedPts = new DataTree();

      Expression data = Expression.NewList(FSharpList<Expression>.Empty);

      public dynDividedSurfaceBySelection()
      {
         this.topControl.Width = 300;

         OutPortData = new PortData("srf", "The divided surface family instance(s)", typeof(dynElement));
         //OutPortData[0].Object = this.Tree;

         //add a button to the inputGrid on the dynElement
         System.Windows.Controls.Button paramMapButt = new System.Windows.Controls.Button();
         this.inputGrid.Children.Add(paramMapButt);
         paramMapButt.Margin = new System.Windows.Thickness(0, 0, 0, 0);
         paramMapButt.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
         paramMapButt.VerticalAlignment = System.Windows.VerticalAlignment.Center;
         paramMapButt.Click += new System.Windows.RoutedEventHandler(paramMapButt_Click);
         paramMapButt.Content = "Select";
         paramMapButt.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
         paramMapButt.VerticalAlignment = System.Windows.VerticalAlignment.Center;

         base.RegisterInputsAndOutputs();

      }

      public override Expression Evaluate(FSharpList<Expression> args)
      {
         return data;
      }

      void paramMapButt_Click(object sender, System.Windows.RoutedEventArgs e)
      {
         //clear the existing tree and the seed pts tree
         //this.Tree.Clear();
         //seedPts.Clear();

         //foreach (Element el in this.Elements)
         //{
         //    dynElementSettings.SharedInstance.Doc.Document.Delete(el);
         //}

         data = Expression.NewList(FSharpList<Expression>.Empty);

         var result = new List<List<FamilyInstance>>();

         f = SelectionHelper.RequestFormSelection(dynElementSettings.SharedInstance.Doc, "Select a form element.", dynElementSettings.SharedInstance);
         dsd = f.GetDividedSurfaceData();
         if (dsd != null)
         {
            foreach (Reference r in dsd.GetReferencesWithDividedSurfaces())
            {
               DividedSurface ds = dsd.GetDividedSurfaceForReference(r);

               GridNode gn = new GridNode();

               int u = 0;
               while (u < ds.NumberOfUGridlines)
               {
                  //add a new tree branch for every node
                  //DataTreeBranch dtb = new DataTreeBranch();
                  //this.Tree.Trunk.Branches.Add(dtb);
                  //DataTreeBranch seedBranch = new DataTreeBranch();
                  //seedPts.Trunk.Branches.Add(seedBranch);

                  var lst = new List<FamilyInstance>();

                  gn.UIndex = u;

                  int v = 0;
                  while (v < ds.NumberOfVGridlines)
                  {
                     gn.VIndex = v;

                     if (ds.IsSeedNode(gn))
                     {
                        FamilyInstance fi
                          = ds.GetTileFamilyInstance(gn, 0);

                        //put the family instance into the tree
                        //dtb.Leaves.Add(fi);
                        lst.Add(fi);

                        ////add a reference point for the seed node
                        //Point p = ds.GetGridNodeReference(gn).GeometryObject as Point;
                        //if (p != null)
                        //{
                        //    ReferencePoint rp = dynElementSettings.SharedInstance.Doc.Document.FamilyCreate.NewReferencePoint(p.Coord);
                        //    seedBranch.Leaves.Add(rp);
                        //    Elements.Append(rp);
                        //}
                     }
                     v = v + 1;
                  }

                  result.Add(lst);

                  u = u + 1;
               }
            }

            this.data = Expression.NewList(
               FSchemeInterop.Utils.convertSequence(
                  result.Select(
                     row => Expression.NewList(
                        FSchemeInterop.Utils.convertSequence(
                           row.Select(Expression.NewContainer)
                        )
                     )
                  )
               )
            );
         }

         //OnDynElementReadyToBuild(EventArgs.Empty);
      }

      //public override void Update()
      //{
      //   OnDynElementReadyToBuild(EventArgs.Empty);
      //}

      //public override void Destroy()
      //{
      //   //don't call base destroy
      //   //base.Destroy();
      //}
   }

   [ElementName("Face by Selection")]
   [ElementCategory(BuiltinElementCategories.REVIT)]
   [ElementDescription("An element which allows the user to select a face.")]
   [RequiresTransaction(true)]
   public class dynFormElementBySelection : dynElement
   {
       Reference f;

       Expression data = Expression.NewList(FSharpList<Expression>.Empty);

       public dynFormElementBySelection()
       {
           this.topControl.Width = 300;

           OutPortData = new PortData("face", "The face", typeof(dynElement));
           //OutPortData[0].Object = this.Tree;

           //add a button to the inputGrid on the dynElement
           System.Windows.Controls.Button paramMapButt = new System.Windows.Controls.Button();
           this.inputGrid.Children.Add(paramMapButt);
           paramMapButt.Margin = new System.Windows.Thickness(0, 0, 0, 0);
           paramMapButt.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
           paramMapButt.VerticalAlignment = System.Windows.VerticalAlignment.Center;
           paramMapButt.Click += new System.Windows.RoutedEventHandler(paramMapButt_Click);
           paramMapButt.Content = "Select";
           paramMapButt.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
           paramMapButt.VerticalAlignment = System.Windows.VerticalAlignment.Center;

           base.RegisterInputsAndOutputs();

       }

       public override Expression Evaluate(FSharpList<Expression> args)
       {
           return data;
       }

       void paramMapButt_Click(object sender, System.Windows.RoutedEventArgs e)
       {


           data = Expression.NewList(FSharpList<Expression>.Empty);

           // MDJ TODO - this is really hacky. I want to just use the face but evaluating the ref fails later on in pointOnSurface, the ref just returns void, not sure why.

           f = SelectionHelper.RequestFaceReferenceSelection(dynElementSettings.SharedInstance.Doc, "Select a face.", dynElementSettings.SharedInstance);

           this.data = Expression.NewContainer(f);

       }


   }

   [ElementName("Curves by Selection")]
   [ElementCategory(BuiltinElementCategories.REVIT)]
   [ElementDescription("An element which allows the user to select a curve or set of curves.")]
   [RequiresTransaction(true)]
   public class dynCurvesBySelection : dynElement
   {
       ModelCurve mc;

       Expression data = Expression.NewList(FSharpList<Expression>.Empty);
       FSharpList<FScheme.Expression> result = FSharpList<FScheme.Expression>.Empty;

       public dynCurvesBySelection()
       {
           this.topControl.Width = 300;

           OutPortData = new PortData("curves", "The curves", typeof(ModelCurve));
           //OutPortData[0].Object = this.Tree;

           //add a button to the inputGrid on the dynElement
           System.Windows.Controls.Button paramMapButt = new System.Windows.Controls.Button();
           this.inputGrid.Children.Add(paramMapButt);
           paramMapButt.Margin = new System.Windows.Thickness(0, 0, 0, 0);
           paramMapButt.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
           paramMapButt.VerticalAlignment = System.Windows.VerticalAlignment.Center;
           paramMapButt.Click += new System.Windows.RoutedEventHandler(paramMapButt_Click);
           paramMapButt.Content = "Select";
           paramMapButt.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
           paramMapButt.VerticalAlignment = System.Windows.VerticalAlignment.Center;

           base.RegisterInputsAndOutputs();

       }
       //public override FScheme.Expression Evaluate(FSharpList<FScheme.Expression> args)

       public override FScheme.Expression Evaluate(FSharpList<Expression> args)
       {
           //return data;
           return FScheme.Expression.NewList(result);
       }

       void paramMapButt_Click(object sender, System.Windows.RoutedEventArgs e)
       {


           data = Expression.NewList(FSharpList<Expression>.Empty);

           mc = SelectionHelper.RequestModelCurveSelection(dynElementSettings.SharedInstance.Doc, "Select a curve.", dynElementSettings.SharedInstance);
           this.result = FSharpList<FScheme.Expression>.Cons(
                     FScheme.Expression.NewContainer(mc),
                     result);

       
           //this.data = Expression.NewContainer(mc);
           

       }


   }
}
