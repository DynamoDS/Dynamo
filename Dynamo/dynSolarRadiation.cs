using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using Autodesk.Revit.UI;
using Dynamo.Connectors;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Expression = Dynamo.FScheme.Expression;

namespace Dynamo.Elements
{
   [ElementName("Extract Solar Radiation Value")]
   [ElementCategory(BuiltinElementCategories.REVIT)]
   [ElementDescription("Create an element for extracting and computing the average solar radiation value based on a csv file.")]
   [RequiresTransaction(false)]
   public class dynComputeSolarRadiationValue : dynElement
   {
      //System.Windows.Controls.Label label;
      //System.Windows.Controls.ListBox listBox;
      System.Windows.Controls.TextBox tb;

      string watchValue;
      double sumValue = 0.0;

      //public event PropertyChangedEventHandler PropertyChanged;

      //private void NotifyPropertyChanged(String info)
      //{
      //   if (PropertyChanged != null)
      //   {
      //      PropertyChanged(this, new PropertyChangedEventArgs(info));
      //   }
      //}

      public string WatchValue
      {
         get { return watchValue; }
         set
         {
            watchValue = value;
            NotifyPropertyChanged("WatchValue");
         }
      }


      public double SumValue
      {
         get { return sumValue; }
         set
         {
            sumValue = value;
            NotifyPropertyChanged("SumValue");
         }
      }

      public dynComputeSolarRadiationValue()
      {
         //add a list box
         //label = new System.Windows.Controls.Label();
         tb = new System.Windows.Controls.TextBox();
         tb.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
         tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;

         WatchValue = "Ready to compute solar radiation value!";

         //http://learnwpf.com/post/2006/06/12/How-can-I-create-a-data-binding-in-code-using-WPF.aspx

         System.Windows.Data.Binding b = new System.Windows.Data.Binding("WatchValue");
         b.Source = this;
         //label.SetBinding(System.Windows.Controls.Label.ContentProperty, b);
         tb.SetBinding(System.Windows.Controls.TextBox.TextProperty, b);

         this.inputGrid.Children.Add(tb);
         tb.TextWrapping = System.Windows.TextWrapping.Wrap;
         tb.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
         //tb.AcceptsReturn = true;

         InPortData.Add(new PortData("s", "The solar radiation data file", typeof(DataTree)));

         OutPortData = new PortData("s", "The solar radiation computed data", typeof(double));
         //this.Tree.Trunk.Branches.Add(new DataTreeBranch());
         //this.Tree.Trunk.Branches[0].Leaves.Add(SumValue); //MDJ TODO - cleanup input tree and output tree
         //OutPortData[0].Object = this.Tree;
         //OutPortData[0].Object = SumValue;


         base.RegisterInputsAndOutputs();

         //resize the panel
         this.topControl.Height = 100;
         this.topControl.Width = 300;
         UpdateLayoutDelegate uld = new UpdateLayoutDelegate(CallUpdateLayout);
         Dispatcher.Invoke(uld, System.Windows.Threading.DispatcherPriority.Background, new object[] { this });
         //this.UpdateLayout();
      }

      public void Process(DataTreeBranch bIn)
      {
         string line = "";
         double doubleSRValue = 0;

         // SR export schema:
         //Source,Date,Time,Model,Type,Study Date Range,Study Time Range,Longitude,Latitude,Unit
         //Vasari v1.0,11/19/2011,2:33 PM,insolationProjectMockUp.rvt,Cumulative,"1/1/2010,12/31/2010","10:00 AM,4:00 PM",-71.0329971313477,42.2130012512207,BTU/ft²
         //
         //Analysis point index,Insolation value,point x,point y,point z,normal x,normal y,normal z
         //1,153823.9528125,7.23744587802689,-32.6932900007427,70.7843137254902,0.2871833,-0.2871833,0.9138116
         //2,159066.52853125,4.74177488560379,-30.1976190083196,72.3529411764706,0.2871833,-0.2871833,0.9138116

         //DataTree treeIn = InPortData[0].Object as DataTree;
         //    if (treeIn != null)
         //    {

         foreach (object o in bIn.Leaves)
         {
            line = o.ToString();

            string[] values = line.Split(',');
            //index = int.Parse(values[0]); // seems a little hacky

            //int i = 0;
            int intTest = 0;// used in TryParse below. returns 0 if not an int and >0 if an int.

            if (int.TryParse(values[0], out intTest)) // test the first value. if the first value is an int, then we know we are passed the header lines and into data
            {
               // string stringSRValue = values[1];

               doubleSRValue = double.Parse(values[1]); // the 2nd value is the one we want

               SumValue = SumValue + doubleSRValue; // compute the sum but adding current value with previous values
            }
         }

         foreach (DataTreeBranch nextBranch in bIn.Branches)
         {
            Process(nextBranch);
         }
      }

      public override Expression Evaluate(FSharpList<Expression> args)
      {
         string data = ((Expression.String)args[0]).Item;

         SumValue = 0.0; // reset to ensue we don't count on refresh


         double doubleSRValue = 0;

         foreach (string line in data.Split(new char[] { '\r', '\n' }).Where(x => x.Length > 0))
         {
            string[] values = line.Split(',');

            //int i = 0;
            int intTest = 0;// used in TryParse below. returns 0 if not an int and >0 if an int.

            if (int.TryParse(values[0], out intTest)) // test the first value. if the first value is an int, then we know we are passed the header lines and into data
            {
               // string stringSRValue = values[1];

               doubleSRValue = double.Parse(values[1]); // the 2nd value is the one we want

               SumValue = SumValue + doubleSRValue; // compute the sum but adding current value with previous values
            }
         }

         return Expression.NewNumber(SumValue);
      }
   }

   [ElementName("Analysis Results by Selection")]
   [ElementCategory(BuiltinElementCategories.REVIT)]
   [ElementDescription("An element which allows you to select an analysis result object from the document and reference it in Dynamo.")]
   [RequiresTransaction(true)]
   public class dynAnalysisResultsBySelection : dynElement
   {
      public dynAnalysisResultsBySelection()
      {
         OutPortData = new PortData("ar", "Analysis Results referenced by this operation.", typeof(Element));

         //add a button to the inputGrid on the dynElement
         Button analysisResultButt = new Button();
         this.inputGrid.Children.Add(analysisResultButt);
         analysisResultButt.Margin = new Thickness(0, 0, 0, 0);
         analysisResultButt.HorizontalAlignment = HorizontalAlignment.Center;
         analysisResultButt.VerticalAlignment = VerticalAlignment.Center;
         analysisResultButt.Click += new RoutedEventHandler(analysisResultButt_Click);
         analysisResultButt.Content = "Select AR";
         analysisResultButt.HorizontalAlignment = HorizontalAlignment.Stretch;
         analysisResultButt.VerticalAlignment = VerticalAlignment.Center;

         base.RegisterInputsAndOutputs();

      }

      public Element pickedAnalysisResult;

      public Element PickedAnalysisResult
      {
         get { return pickedAnalysisResult; }
         set
         {
            this.IsDirty = true;
            pickedAnalysisResult = value;
            NotifyPropertyChanged("PickedAnalysisResult");
         }
      }

      private ElementId analysisResultID;

      private ElementId AnalysisResultID
      {
         get { return analysisResultID; }
         set
         {
            analysisResultID = value;
            NotifyPropertyChanged("AnalysisResultID");
         }
      }
      void analysisResultButt_Click(object sender, RoutedEventArgs e)
      {
         PickedAnalysisResult =
            Dynamo.Utilities.SelectionHelper.RequestAnalysisResultInstanceSelection(
               this.UIDocument,
               "Select Analysis Result Object",
               dynElementSettings.SharedInstance
            );

         if (PickedAnalysisResult != null)
         {
            AnalysisResultID = PickedAnalysisResult.Id;
         }
      }


      public override Expression Evaluate(FSharpList<Expression> args)
      {
         if (PickedAnalysisResult != null)
         {
            if (PickedAnalysisResult.Id.IntegerValue == AnalysisResultID.IntegerValue) // sanity check
            {
               SpatialFieldManager dmu_sfm = dynElementSettings.SharedInstance.SpatialFieldManagerUpdated as SpatialFieldManager;

               if (pickedAnalysisResult.Id.IntegerValue == dmu_sfm.Id.IntegerValue)
               {
                  TaskDialog.Show("ah hah", "picked sfm equals saved one from dmu");
               }

               return Expression.NewContainer(this.PickedAnalysisResult);
            }
         }

         throw new Exception("No data selected!");
      }
   }
}
