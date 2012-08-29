//Copyright © Autodesk, Inc. 2012. All rights reserved.
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

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

namespace Dynamo.Nodes
{
    [NodeName("Extract Solar Radiation Value")]
    [NodeCategory(BuiltinNodeCategories.REVIT)]
    [NodeDescription("Create an element for extracting and computing the average solar radiation value based on a csv file.")]
    public class dynComputeSolarRadiationValue : dynNode
    {
        public dynComputeSolarRadiationValue()
        {
            InPortData.Add(new PortData("raw", "The solar radiation data file", typeof(string)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("data", "The solar radiation computed data", typeof(double));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            string data = ((Expression.String)args[0]).Item;

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

            return Expression.NewNumber(SumValue);
        }
    }

    [NodeName("Analysis Results by Selection")]
    [NodeCategory(BuiltinNodeCategories.REVIT)]
    [NodeDescription("An element which allows you to select an analysis result object from the document and reference it in Dynamo.")]
    public class dynAnalysisResultsBySelection : dynRevitNode
    {
        public dynAnalysisResultsBySelection()
        {
            //add a button to the inputGrid on the dynElement
            Button analysisResultButt = new Button();
            NodeUI.inputGrid.Children.Add(analysisResultButt);
            analysisResultButt.Margin = new Thickness(0, 0, 0, 0);
            analysisResultButt.HorizontalAlignment = HorizontalAlignment.Center;
            analysisResultButt.VerticalAlignment = VerticalAlignment.Center;
            analysisResultButt.Click += new RoutedEventHandler(analysisResultButt_Click);
            analysisResultButt.Content = "Select AR";
            analysisResultButt.HorizontalAlignment = HorizontalAlignment.Stretch;
            analysisResultButt.VerticalAlignment = VerticalAlignment.Center;

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("ar", "Analysis Results referenced by this operation.", typeof(Element));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        public Element pickedAnalysisResult;

        public Element PickedAnalysisResult
        {
            get { return pickedAnalysisResult; }
            set
            {
                pickedAnalysisResult = value;
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
            }
        }

        void analysisResultButt_Click(object sender, RoutedEventArgs e)
        {
            PickedAnalysisResult =
               Dynamo.Utilities.SelectionHelper.RequestAnalysisResultInstanceSelection(
                  this.UIDocument,
                  "Select Analysis Result Object",
                  dynSettings.Instance
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
                    SpatialFieldManager dmu_sfm = dynSettings.Instance.SpatialFieldManagerUpdated as SpatialFieldManager;

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
