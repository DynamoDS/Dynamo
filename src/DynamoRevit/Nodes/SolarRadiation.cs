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

            int intTest;

            var sumValue = (from line in data.Split(new[] { '\r', '\n' }).Where(x => x.Length > 0)
                            select line.Split(',')
                            into values
                            where int.TryParse(values[0], out intTest)
                            select double.Parse(values[1])).Sum();

            return Value.NewNumber(sumValue);
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

        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            //add a button to the inputGrid on the dynElement
            var analysisResultButt = new NodeButton
            {
                Margin = new Thickness(0, 0, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Content = "Select AR"
            };

            analysisResultButt.Click += analysisResultButt_Click;

            nodeUI.inputGrid.Children.Add(analysisResultButt);
        }

        private Element _pickedAnalysisResult;

        public Element PickedAnalysisResult
        {
            get { return _pickedAnalysisResult; }
            set
            {
                _pickedAnalysisResult = value;
                //NotifyPropertyChanged("PickedAnalysisResult");
                RequiresRecalc = true;
            }
        }

        private ElementId _analysisResultID;

        void analysisResultButt_Click(object sender, RoutedEventArgs e)
        {
            PickedAnalysisResult =
               dynRevitSettings.SelectionHelper.RequestAnalysisResultInstanceSelection("Select Analysis Result Object");

            if (PickedAnalysisResult != null)
            {
                _analysisResultID = PickedAnalysisResult.Id;
            }
        }


        public override Value Evaluate(FSharpList<Value> args)
        {
            if (PickedAnalysisResult != null)
            {
                if (PickedAnalysisResult.Id.IntegerValue == _analysisResultID.IntegerValue) // sanity check
                {
                    var dmuSfm =
                        dynRevitSettings.SpatialFieldManagerUpdated as
                            Autodesk.Revit.DB.Analysis.SpatialFieldManager;

                    if (_pickedAnalysisResult.Id.IntegerValue == dmuSfm.Id.IntegerValue)
                    {
                        TaskDialog.Show("ah hah", "picked sfm equals saved one from dmu");
                    }

                    return Value.NewContainer(PickedAnalysisResult);
                }
            }

            throw new Exception("No data selected!");
        }
    }
}