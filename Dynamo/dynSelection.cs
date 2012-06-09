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
using Dynamo.FSchemeInterop;

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
                        lst.Add(fi);
                     }
                     v = v + 1;
                  }

                  result.Add(lst);

                  u = u + 1;
               }
            }

            this.data = Expression.NewList(
               Utils.convertSequence(
                  result.Select(
                     row => Expression.NewList(
                        Utils.convertSequence(
                           row.Select(Expression.NewContainer)
                        )
                     )
                  )
               )
            );
         }
      }
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

         f = SelectionHelper.RequestFaceReferenceSelection(this.UIDocument, "Select a face.", dynElementSettings.SharedInstance);

         this.data = Expression.NewContainer(f);
      }
   }

   [ElementName("Curve by Selection")]
   [ElementCategory(BuiltinElementCategories.REVIT)]
   [ElementDescription("An element which allows the user to select a curve.")] //or set of curves in the future
   [RequiresTransaction(true)]
   public class dynCurvesBySelection : dynElement
   {
      CurveElement mc;

      Expression data = Expression.NewList(FSharpList<Expression>.Empty);
      FSharpList<Expression> result = FSharpList<Expression>.Empty;

      public dynCurvesBySelection()
      {
         this.topControl.Width = 300;

         OutPortData = new PortData("curve", "The curve", typeof(CurveElement));
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

      public override Expression Evaluate(FSharpList<Expression> args)
      {

         return data;

         //return Expression.NewList(result); // MDJ downstream form element breaks unless this is a list
      }

      void paramMapButt_Click(object sender, System.Windows.RoutedEventArgs e)
      {
         data = Expression.NewList(FSharpList<Expression>.Empty);

         mc = SelectionHelper.RequestModelCurveSelection(dynElementSettings.SharedInstance.Doc, "Select a curve.", dynElementSettings.SharedInstance);
         //this.result = FSharpList<Expression>.Cons(
         //          Expression.NewContainer(mc),
         //          result);

         //dynElementSettings.SharedInstance.UserSelectedElements.Insert(mc); // MDJ HOOK remember the one we selected for comparison in DMU code. 
         this.RegisterEvalOnModified(mc.Id);

         this.data = Expression.NewContainer(mc);
         this.IsDirty = true;
      }
   }

   [ElementName("Point by Selection")]
   [ElementCategory(BuiltinElementCategories.REVIT)]
   [ElementDescription("An element which allows the user to select a reference point.")]
   [RequiresTransaction(true)]
   public class dynPointBySelection : dynElement
   {
      ReferencePoint rp;

      Expression data = Expression.NewList(FSharpList<Expression>.Empty);
      FSharpList<Expression> result = FSharpList<Expression>.Empty;

      public dynPointBySelection()
      {
         this.topControl.Width = 300;

         OutPortData = new PortData("pt", "The point", typeof(ReferencePoint));
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

         //return Expression.NewList(result);
      }

      void paramMapButt_Click(object sender, System.Windows.RoutedEventArgs e)
      {
         data = Expression.NewList(FSharpList<Expression>.Empty);

         rp = SelectionHelper.RequestReferencePointSelection(dynElementSettings.SharedInstance.Doc, "Select a reference point.", dynElementSettings.SharedInstance);
         // this.result = FSharpList<Expression>.Cons(
         //           Expression.NewContainer(rp),
         //           result);

         //dynElementSettings.SharedInstance.UserSelectedElements.Insert(rp); // MDJ HOOK remember the one we selected for comparison in DMU code. 
         this.RegisterEvalOnModified(rp.Id);
         this.data = Expression.NewContainer(rp);
         this.IsDirty = true;
      }
   }
}
