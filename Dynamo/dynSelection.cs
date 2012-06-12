using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Xml;
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
      System.Windows.Controls.Button curveButt;
      System.Windows.Controls.TextBox tb;

      Expression data = Expression.NewList(FSharpList<Expression>.Empty);
      FSharpList<Expression> result = FSharpList<Expression>.Empty;

      public dynCurvesBySelection()
      {
         this.topControl.Width = 300;

         OutPortData = new PortData("curve", "The curve", typeof(CurveElement));
         //OutPortData[0].Object = this.Tree;

         //add a button to the inputGrid on the dynElement
         curveButt = new System.Windows.Controls.Button();
         //this.inputGrid.Children.Add(curveButt);
         curveButt.Margin = new System.Windows.Thickness(0, 0, 0, 0);
         curveButt.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
         curveButt.VerticalAlignment = System.Windows.VerticalAlignment.Center;
         curveButt.Click += new System.Windows.RoutedEventHandler(curveButt_Click);
         curveButt.Content = "Select Curve";
         curveButt.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
         curveButt.VerticalAlignment = System.Windows.VerticalAlignment.Center;

         tb = new TextBox();
         tb.Text = "Nothing Selected";
         tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
         tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
         SolidColorBrush backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
         tb.Background = backgroundBrush;
         tb.BorderThickness = new Thickness(0);
         tb.IsReadOnly = true;
         tb.IsReadOnlyCaretVisible = false;

         this.inputGrid.RowDefinitions.Add(new RowDefinition());
         this.inputGrid.RowDefinitions.Add(new RowDefinition());

         this.inputGrid.Children.Add(tb);
         this.inputGrid.Children.Add(curveButt);

         System.Windows.Controls.Grid.SetRow(curveButt, 0);
         System.Windows.Controls.Grid.SetRow(tb, 1);

         base.RegisterInputsAndOutputs();

         this.topControl.Height = 60;
         UpdateLayoutDelegate uld = new UpdateLayoutDelegate(CallUpdateLayout);
         Dispatcher.Invoke(uld, System.Windows.Threading.DispatcherPriority.Background, new object[] { this });

      }
      //public override FScheme.Expression Evaluate(FSharpList<FScheme.Expression> args)

      //public override Expression Evaluate(FSharpList<Expression> args)
      //{

      //   return data;

      //   //return Expression.NewList(result); // MDJ downstream form element breaks unless this is a list
      //}

      //void paramMapButt_Click(object sender, System.Windows.RoutedEventArgs e)
      //{
      //   data = Expression.NewList(FSharpList<Expression>.Empty);

      //   mc = SelectionHelper.RequestModelCurveSelection(dynElementSettings.SharedInstance.Doc, "Select a curve.", dynElementSettings.SharedInstance);
      //   //this.result = FSharpList<Expression>.Cons(
      //   //          Expression.NewContainer(mc),
      //   //          result);

      //   //dynElementSettings.SharedInstance.UserSelectedElements.Insert(mc); // MDJ HOOK remember the one we selected for comparison in DMU code. 
      //   this.RegisterEvalOnModified(mc.Id);

      //   this.data = Expression.NewContainer(mc);
      //   this.IsDirty = true;
      //}

      public CurveElement pickedCurve;

      public CurveElement PickedCurve
      {
          get { return pickedCurve; }
          set
          {
              pickedCurve = value;
              NotifyPropertyChanged("PickedCurve");
          }
      }

      private ElementId curveID;

      private ElementId CurveID
      {
          get { return curveID; }
          set
          {
              curveID = value;
              NotifyPropertyChanged("CurveID");
          }
      }
      void curveButt_Click(object sender, System.Windows.RoutedEventArgs e)
      {
          data = Expression.NewList(FSharpList<Expression>.Empty);

          PickedCurve = SelectionHelper.RequestModelCurveSelection(dynElementSettings.SharedInstance.Doc, "Select a curve.", dynElementSettings.SharedInstance);
          // this.result = FSharpList<Expression>.Cons(
          //           Expression.NewContainer(rp),
          //           result);

          //dynElementSettings.SharedInstance.UserSelectedElements.Insert(rp); // MDJ HOOK remember the one we selected for comparison in DMU code. 



          if (PickedCurve != null)
          {
              if (CurveID != null) //we already had a curve before and are now repicking
              {
                  ElementId oldCurveID = CurveID;
                  // this.RegisterEvalOnModified(oldCurveID);//BUG - stale references after repicking - we do not have a way to deregistering in the system stephen setup
              }
              CurveID = pickedCurve.Id;
              this.RegisterEvalOnModified(CurveID);
              this.data = Expression.NewContainer(PickedCurve);

              curveButt.Content = "Change Curve";
              string successString = PickedCurve.Name + " " + PickedCurve.Id;
              this.tb.Text = successString;
              Bench.Log("Selected " + successString);
          }
          else
          {
              curveButt.Content = "Select Curve";
              string failureString = "Nothing Selected";
              this.tb.Text = failureString;
              Bench.Log(failureString);
          }
      }

      public override Expression Evaluate(FSharpList<Expression> args)
      {
          if (PickedCurve.Id.IntegerValue == curveID.IntegerValue) // sanity check
          {

              this.data = Expression.NewContainer(PickedCurve);
              return data;
          }
          else
              throw new Exception("SANITY CHECK FAILED");
      }

      public override void SaveElement(XmlDocument xmlDoc, XmlElement dynEl)
      {
          //Debug.WriteLine(pd.Object.GetType().ToString());
          if (this.PickedCurve != null)
          {
              XmlElement outEl = xmlDoc.CreateElement("instance");
              outEl.SetAttribute("id", this.PickedCurve.Id.ToString());
              dynEl.AppendChild(outEl);
          }
      }

      public override void LoadElement(XmlNode elNode)
      {
          foreach (XmlNode subNode in elNode.ChildNodes)
          {
              if (subNode.Name.Equals("instance"))
              {
                  try
                  {
                      this.PickedCurve = dynElementSettings.SharedInstance.Doc.Document.get_Element(
                         new ElementId(Convert.ToInt32(subNode.Attributes[0].Value))
                      ) as CurveElement;
                      if (this.PickedCurve != null)
                      {
                          curveID = PickedCurve.Id;
                          this.tb.Text = this.PickedCurve.Name;
                          this.curveButt.Content = "Select Curve";
                      }
                  }
                  catch { }
              }
          }
      }
   }

   [ElementName("Point by Selection")]
   [ElementCategory(BuiltinElementCategories.REVIT)]
   [ElementDescription("An element which allows the user to select a reference point.")]
   [RequiresTransaction(true)]
   public class dynPointBySelection : dynElement
   {
      ReferencePoint rp;
      System.Windows.Controls.Button pointButt;
      System.Windows.Controls.TextBox tb;

      Expression data = Expression.NewList(FSharpList<Expression>.Empty);
      FSharpList<Expression> result = FSharpList<Expression>.Empty;

      public dynPointBySelection()
      {
         this.topControl.Width = 300;

         OutPortData = new PortData("pt", "The point", typeof(ReferencePoint));
         //OutPortData[0].Object = this.Tree;

         //add a button to the inputGrid on the dynElement
         pointButt = new System.Windows.Controls.Button();
         //this.inputGrid.Children.Add(pointButt);
         pointButt.Margin = new System.Windows.Thickness(0, 0, 0, 0);
         pointButt.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
         pointButt.VerticalAlignment = System.Windows.VerticalAlignment.Center;
         pointButt.Click += new System.Windows.RoutedEventHandler(pointButt_Click);
         pointButt.Content = "Select Point";
         pointButt.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
         pointButt.VerticalAlignment = System.Windows.VerticalAlignment.Center;

         tb = new TextBox();
         tb.Text = "Nothing Selected";
         tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
         tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
         SolidColorBrush backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
         tb.Background = backgroundBrush;
         tb.BorderThickness = new Thickness(0);
         tb.IsReadOnly = true;
         tb.IsReadOnlyCaretVisible = false;

         this.inputGrid.RowDefinitions.Add(new RowDefinition());
         this.inputGrid.RowDefinitions.Add(new RowDefinition());

         this.inputGrid.Children.Add(tb);
         this.inputGrid.Children.Add(pointButt);

         System.Windows.Controls.Grid.SetRow(pointButt, 0);
         System.Windows.Controls.Grid.SetRow(tb, 1);

         base.RegisterInputsAndOutputs();

         this.topControl.Height = 60;
         UpdateLayoutDelegate uld = new UpdateLayoutDelegate(CallUpdateLayout);
         Dispatcher.Invoke(uld, System.Windows.Threading.DispatcherPriority.Background, new object[] { this });


      }



      public ReferencePoint pickedRefPoint;

      public ReferencePoint PickedRefPoint
      {
          get { return pickedRefPoint; }
          set
          {
              pickedRefPoint = value;
              NotifyPropertyChanged("PickedRefPoint");
          }
      }

      private ElementId refPointID;

      private ElementId RefPointID
      {
          get { return refPointID; }
          set
          {
              refPointID = value;
              NotifyPropertyChanged("RefPointID");
          }
      }
      void pointButt_Click(object sender, System.Windows.RoutedEventArgs e)
      {
          data = Expression.NewList(FSharpList<Expression>.Empty);

          PickedRefPoint = SelectionHelper.RequestReferencePointSelection(dynElementSettings.SharedInstance.Doc, "Select a reference point.", dynElementSettings.SharedInstance);
          // this.result = FSharpList<Expression>.Cons(
          //           Expression.NewContainer(rp),
          //           result);

          //dynElementSettings.SharedInstance.UserSelectedElements.Insert(rp); // MDJ HOOK remember the one we selected for comparison in DMU code. 



          if (PickedRefPoint != null)
          {
              RefPointID = pickedRefPoint.Id;
              this.RegisterEvalOnModified(RefPointID);
              this.data = Expression.NewContainer(PickedRefPoint);

              pointButt.Content = "Change Point";
              string successString = "Point " + PickedRefPoint.Name + PickedRefPoint.Id;
              this.tb.Text = successString;
              Bench.Log("Selected " + successString);
          }
          else
          {
              pointButt.Content = "Select Point";
              string failureString = "Nothing Selected";
              this.tb.Text = failureString;
              Bench.Log(failureString);
          }
      }

      public override Expression Evaluate(FSharpList<Expression> args)
      {
          if (PickedRefPoint.Id.IntegerValue == refPointID.IntegerValue) // sanity check
          {

              this.data = Expression.NewContainer(PickedRefPoint);
              return data;
          }
          else
              throw new Exception("SANITY CHECK FAILED");
      }

      public override void SaveElement(XmlDocument xmlDoc, XmlElement dynEl)
      {
          //Debug.WriteLine(pd.Object.GetType().ToString());
          if (this.PickedRefPoint != null)
          {
              XmlElement outEl = xmlDoc.CreateElement("instance");
              outEl.SetAttribute("id", this.PickedRefPoint.Id.ToString());
              dynEl.AppendChild(outEl);
          }
      }

      public override void LoadElement(XmlNode elNode)
      {
          foreach (XmlNode subNode in elNode.ChildNodes)
          {
              if (subNode.Name.Equals("instance"))
              {
                  try
                  {
                      this.PickedRefPoint = dynElementSettings.SharedInstance.Doc.Document.get_Element(
                         new ElementId(Convert.ToInt32(subNode.Attributes[0].Value))
                      ) as ReferencePoint;
                      if (this.PickedRefPoint != null)
                      {
                          refPointID = PickedRefPoint.Id;
                          this.tb.Text = this.PickedRefPoint.Name;
                          this.pointButt.Content = "Select Point";
                      }
                  }
                  catch { }
              }
          }
      }
   }

   [ElementName("SunPath Direction")]
   [ElementCategory(BuiltinElementCategories.REVIT)]
   [ElementDescription("An element which returns the current Sun Path diretion.")]
   [RequiresTransaction(true)]
   public class dynSunPathDirection : dynElement
   {
       System.Windows.Controls.TextBox tb;
       System.Windows.Controls.Button sunPathButt;
       Expression data = Expression.NewList(FSharpList<Expression>.Empty);


       public dynSunPathDirection()
       {

           OutPortData = new PortData("XYZ", "XYZ", typeof(XYZ));

           //add a button to the inputGrid on the dynElement
           sunPathButt = new System.Windows.Controls.Button();
           //this.inputGrid.Children.Add(sunPathButt);
           sunPathButt.Margin = new System.Windows.Thickness(0, 0, 0, 0);
           sunPathButt.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
           sunPathButt.VerticalAlignment = System.Windows.VerticalAlignment.Center;
           sunPathButt.Click += new System.Windows.RoutedEventHandler(registerButt_Click);
           sunPathButt.Content = "Use SunPath\nfrom Current View";
           sunPathButt.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
           sunPathButt.VerticalAlignment = System.Windows.VerticalAlignment.Center;

           tb = new TextBox();
           tb.Text = "No SunPath Registered";
           tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
           tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
           SolidColorBrush backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
           tb.Background = backgroundBrush;
           tb.BorderThickness = new Thickness(0);
           tb.IsReadOnly = true;
           tb.IsReadOnlyCaretVisible = false;

           this.inputGrid.RowDefinitions.Add(new RowDefinition());
           this.inputGrid.RowDefinitions.Add(new RowDefinition());

           this.inputGrid.Children.Add(tb);
           this.inputGrid.Children.Add(sunPathButt);

           System.Windows.Controls.Grid.SetRow(sunPathButt, 0);
           System.Windows.Controls.Grid.SetRow(tb, 1);

           base.RegisterInputsAndOutputs();

           this.topControl.Height = 60;
           UpdateLayoutDelegate uld = new UpdateLayoutDelegate(CallUpdateLayout);
           Dispatcher.Invoke(uld, System.Windows.Threading.DispatcherPriority.Background, new object[] { this });

       }


       /// <summary>
       /// Description of ShadowCalculatorUtils.
       /// NOTE: this is derived from Scott Connover's great class "Geometry API in Revit" from DevCamp 2012, source files accesed 6-8-12 from here 
       /// https://projectpoint.buzzsaw.com/_bz_rest/Web/Home/Index?folder=44#/_bz_rest/Web/Item/Items?folder=152&count=50&start=0&ownership=Homehttps://projectpoint.buzzsaw.com/_bz_rest/Web/Home/Index?folder=44#/_bz_rest/Web/Item/Items?folder=152&count=50&start=0&ownership=Home
       /// </summary>

       public static XYZ GetSunDirection(SunAndShadowSettings sunSettings)
       {
           //SunAndShadowSettings sunSettings = view.SunAndShadowSettings;

           XYZ initialDirection = XYZ.BasisY;

           //double altitude = sunSettings.Altitude;
           double altitude = sunSettings.GetFrameAltitude(sunSettings.ActiveFrame);
           Autodesk.Revit.DB.Transform altitudeRotation = Autodesk.Revit.DB.Transform.get_Rotation(XYZ.Zero, XYZ.BasisX, altitude);
           XYZ altitudeDirection = altitudeRotation.OfVector(initialDirection);

           //double azimuth = sunSettings.Azimuth;
           double azimuth = sunSettings.GetFrameAzimuth(sunSettings.ActiveFrame);
           double actualAzimuth = 2 * Math.PI - azimuth;
           Autodesk.Revit.DB.Transform azimuthRotation = Autodesk.Revit.DB.Transform.get_Rotation(XYZ.Zero, XYZ.BasisZ, actualAzimuth);
           XYZ sunDirection = azimuthRotation.OfVector(altitudeDirection);
           XYZ scaledSunVector = sunDirection.Multiply(100);

           return scaledSunVector;

       }

       public SunAndShadowSettings pickedSunAndShadowSettings;

       public SunAndShadowSettings PickedSunAndShadowSettings
       {
           get { return pickedSunAndShadowSettings; }
           set
           {
               pickedSunAndShadowSettings = value;
               NotifyPropertyChanged("PickedSunAndShadowSettings");
           }
       }

       private ElementId sunAndShadowSettingsID;

       private ElementId SunAndShadowSettingsID
       {
           get { return sunAndShadowSettingsID; }
           set
           {
               sunAndShadowSettingsID = value;
               NotifyPropertyChanged("SunAndShadowSettingsID");
           }
       }
       void registerButt_Click(object sender, System.Windows.RoutedEventArgs e)
       {
           //data = Expression.NewList(FSharpList<Expression>.Empty);

           View activeView = this.UIDocument.ActiveView;
           PickedSunAndShadowSettings = activeView.SunAndShadowSettings;


           if (PickedSunAndShadowSettings != null)
           {
               sunAndShadowSettingsID = activeView.SunAndShadowSettings.Id;
               this.RegisterEvalOnModified(sunAndShadowSettingsID); // register with the DMU, TODO - watch out for view changes, as sun is view specific
               XYZ sunVector = GetSunDirection(PickedSunAndShadowSettings);


               this.data = Expression.NewContainer(sunVector);

               string successString = PickedSunAndShadowSettings.Name;
               this.tb.Text = successString;
               Bench.Log("Registered " + successString);
           }
           else
           {
               //sunPathButt.Content = "Select Instance";
               string failureString = "Nothing Registered";
               this.tb.Text = failureString;
               Bench.Log(failureString);
           }
       }

       public override Expression Evaluate(FSharpList<Expression> args)
       {
           if (PickedSunAndShadowSettings.Id.IntegerValue == sunAndShadowSettingsID.IntegerValue) // sanity check
           {

               XYZ sunVector = GetSunDirection(PickedSunAndShadowSettings);
               this.data = Expression.NewContainer(sunVector);
               return data;
           }
           else
               throw new Exception("SANITY CHECK FAILED");
       }

       public override void SaveElement(XmlDocument xmlDoc, XmlElement dynEl)
       {
           //Debug.WriteLine(pd.Object.GetType().ToString());
           if (this.PickedSunAndShadowSettings != null)
           {
               XmlElement outEl = xmlDoc.CreateElement("instance");
               outEl.SetAttribute("id", this.PickedSunAndShadowSettings.Id.ToString());
               dynEl.AppendChild(outEl);
           }
       }

       public override void LoadElement(XmlNode elNode)
       {
           foreach (XmlNode subNode in elNode.ChildNodes)
           {
               if (subNode.Name.Equals("instance"))
               {
                   try
                   {
                       this.PickedSunAndShadowSettings = dynElementSettings.SharedInstance.Doc.Document.get_Element(
                          new ElementId(Convert.ToInt32(subNode.Attributes[0].Value))
                       ) as SunAndShadowSettings;
                       if (this.PickedSunAndShadowSettings != null)
                       {
                           sunAndShadowSettingsID = PickedSunAndShadowSettings.Id;
                           this.tb.Text = this.PickedSunAndShadowSettings.Name;
                           this.sunPathButt.Content = "Use SunPath from Current View";
                       }
                   }
                   catch { }
               }
           }
       }

   }
}
