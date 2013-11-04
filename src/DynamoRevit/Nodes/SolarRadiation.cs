using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;

namespace Dynamo.Nodes
{
    [NodeName("Solar Radiation Values from CSV")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_SOLAR)]
    [NodeDescription("Extracts and computes the average solar radiation value based on a CSV file.")]
    public class ComputeSolarRadiationValue: NodeWithOneOutput
    {
        public ComputeSolarRadiationValue()
        {
            InPortData.Add(new PortData("raw", "The solar radiation data file", typeof(Value.String)));
            OutPortData.Add(new PortData("data", "The solar radiation computed data", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            string data = ((Value.String)args[0]).Item;

            var SumValue = 0.0;
            double doubleSRValue = 0;

            foreach (string line in data.Split(new char[] { '\r', '\n' }).Where(x => x.Length > 0))
            {
                string[] values = line.Split(',');

                //int i = 0;
                int intTest = 0;// used in TryParse below. returns 0 if not an int and >0 if an int.

                if (int.TryParse(values[0], out intTest)) // test the first value. if the first value is an int, then we know we are passed the header lines and into data
                {
                    doubleSRValue = double.Parse(values[1]); // the 2nd value is the one we want
                    SumValue += doubleSRValue; // compute the sum but adding current value with previous values
                }
            }

            return Value.NewNumber(SumValue);
        }
    }

    [NodeName("Select Analysis Results")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Select an analysis result object from the document.")]
    public class AnalysisResultsBySelection: NodeWithOneOutput
    {
        public AnalysisResultsBySelection()
        {
            OutPortData.Add(new PortData("ar", "Analysis Results referenced by this operation.", typeof(Value.Container)));
            RegisterAllPorts();

        }

        public override void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

            //add a button to the inputGrid on the dynElement
            Button analysisResultButt = new dynNodeButton();
            nodeUI.inputGrid.Children.Add(analysisResultButt);
            analysisResultButt.Margin = new Thickness(0, 0, 0, 0);
            analysisResultButt.HorizontalAlignment = HorizontalAlignment.Center;
            analysisResultButt.VerticalAlignment = VerticalAlignment.Center;
            analysisResultButt.Click += new RoutedEventHandler(analysisResultButt_Click);
            analysisResultButt.Content = "Select AR";
            analysisResultButt.HorizontalAlignment = HorizontalAlignment.Stretch;
            analysisResultButt.VerticalAlignment = VerticalAlignment.Center;
        }

        public Element pickedAnalysisResult;

        public Element PickedAnalysisResult
        {
            get { return pickedAnalysisResult; }
            set
            {
                pickedAnalysisResult = value;
                //NotifyPropertyChanged("PickedAnalysisResult");
                this.RequiresRecalc = true;
            }
        }

        private ElementId analysisResultID;

        private ElementId AnalysisResultID
        {
            get { return analysisResultID; }
            set
            {
                analysisResultID = value;
                //NotifyPropertyChanged("AnalysisResultID");
            }
        }
        void analysisResultButt_Click(object sender, RoutedEventArgs e)
        {
            PickedAnalysisResult =
               dynRevitSettings.SelectionHelper.RequestAnalysisResultInstanceSelection("Select Analysis Result Object");

            if (PickedAnalysisResult != null)
            {
                AnalysisResultID = PickedAnalysisResult.Id;
            }
        }


        public override Value Evaluate(FSharpList<Value> args)
        {
            if (PickedAnalysisResult != null)
            {
                if (PickedAnalysisResult.Id.IntegerValue == AnalysisResultID.IntegerValue) // sanity check
                {
                    Autodesk.Revit.DB.Analysis.SpatialFieldManager dmu_sfm = dynRevitSettings.SpatialFieldManagerUpdated as Autodesk.Revit.DB.Analysis.SpatialFieldManager;

                    if (pickedAnalysisResult.Id.IntegerValue == dmu_sfm.Id.IntegerValue)
                    {
                        TaskDialog.Show("ah hah", "picked sfm equals saved one from dmu");
                    }

                    return Value.NewContainer(this.PickedAnalysisResult);
                }
            }

            throw new Exception("No data selected!");
        }
    }
}