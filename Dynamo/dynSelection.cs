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

using Dynamo.Nodes.SyncedNodeExtensions; //Gives the RegisterEval... methods

using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using Expression = Dynamo.FScheme.Expression;
using Dynamo.FSchemeInterop;

namespace Dynamo.Nodes
{
    [IsInteractive(true)]
    public abstract class dynElementSelection : dynNode
    {
        TextBox tb;
        System.Windows.Controls.Button selectButton;

        protected dynElementSelection(PortData outPortData)
        {
            this.outPortData = outPortData;

            //add a button to the inputGrid on the dynElement
            selectButton = new System.Windows.Controls.Button();
            selectButton.Margin = new System.Windows.Thickness(0, 0, 0, 0);
            selectButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            selectButton.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            selectButton.Click += new System.Windows.RoutedEventHandler(selectButton_Click);
            selectButton.Content = "Select Instance";
            selectButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            selectButton.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            tb = new TextBox();
            tb.Text = "Nothing Selected";
            tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            SolidColorBrush backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
            tb.Background = backgroundBrush;
            tb.BorderThickness = new Thickness(0);
            tb.IsReadOnly = true;
            tb.IsReadOnlyCaretVisible = false;

            NodeUI.SetRowAmount(2);

            NodeUI.inputGrid.Children.Add(tb);
            NodeUI.inputGrid.Children.Add(selectButton);

            System.Windows.Controls.Grid.SetRow(selectButton, 0);
            System.Windows.Controls.Grid.SetRow(tb, 1);

            NodeUI.RegisterInputsAndOutput();

            NodeUI.topControl.Height = 60;
            NodeUI.UpdateLayout();
        }

        private PortData outPortData;
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        private void selectButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.selectButton.IsEnabled = false;
            IdlePromise.ExecuteOnIdle(new Action(
                delegate
                {
                    this.OnSelectClick();
                    this.selectButton.IsEnabled = true;
                }
            ));
        }

        /// <summary>
        /// Callback for when the "Select" button has been clicked.
        /// </summary>
        /// 
        /// 
        protected abstract void OnSelectClick();

        private Element selected;
        /// <summary>
        /// The Element which is selected. Setting this property will automatically register the Element
        /// for proper updating, and will update this node's IsDirty value.
        /// </summary>
        public virtual Element SelectedElement
        {
            get { return selected; }
            set
            {
                var dirty = false;
                if (this.selected != null)
                {
                    if (value != null && value.Id.Equals(this.selected.Id))
                        return;

                    dirty = true;
                    this.UnregisterEvalOnModified(this.selected.Id);
                }
                else
                {
                    dirty = value != null;
                }

                this.selected = value;
                if (value != null)
                {
                    this.RegisterEvalOnModified(
                       value.Id,
                       delAction: delegate { this.selected = null; this.SelectedElement = null; }
                    );

                    this.tb.Text = this.SelectionText;
                    this.selectButton.Content = "Change";
                }
                else
                {
                    this.tb.Text = "Nothing Selected.";
                    this.selectButton.Content = "Select";
                }

                if (dirty)
                    this.RequiresRecalc = true;
            }
        }

        /// <summary>
        /// Determines what the text should read on the node when the selection has been changed.
        /// Is ignored in the case where nothing is selected.
        /// </summary>
        protected abstract string SelectionText { get; }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            if (this.SelectedElement == null)
                throw new Exception("Nothing selected.");

            return Expression.NewContainer(this.SelectedElement);
        }

        public override void SaveElement(XmlDocument xmlDoc, XmlElement dynEl)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            if (this.SelectedElement != null)
            {
                XmlElement outEl = xmlDoc.CreateElement("instance");
                outEl.SetAttribute("id", this.SelectedElement.Id.ToString());
                dynEl.AppendChild(outEl);
            }
        }

        public override void LoadElement(XmlNode elNode)
        {
            foreach (XmlNode subNode in elNode.ChildNodes)
            {
                if (subNode.Name.Equals("instance"))
                {
                    Element saved = null;
                    var id = new ElementId(Convert.ToInt32(subNode.Attributes[0].Value));
                    try
                    {
                        saved = dynSettings.Instance.Doc.Document.GetElement(id) as FamilyInstance;
                    }
                    catch
                    {
                        this.Bench.Log("Unable to find element with ID: " + id.IntegerValue);
                    }
                    this.SelectedElement = saved;
                }
            }
        }
    }

    [ElementName("Family Instance by Selection")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [ElementDescription("An element which allows you to select a family instance from the document and reference it in Dynamo.")]
    public class dynFamilyInstanceCreatorSelection : dynElementSelection
    {
        public dynFamilyInstanceCreatorSelection()
            : base(new PortData("fi", "Family instances created by this operation.", typeof(FamilyInstance)))
        { }

        protected override void OnSelectClick()
        {
            this.SelectedElement = Dynamo.Utilities.SelectionHelper.RequestFamilyInstanceSelection(
               dynSettings.Instance.Doc, "Select Massing Family Instance", dynSettings.Instance
            );
        }

        protected override string SelectionText
        {
            get { return this.SelectedElement.Name; }
        }
    }

    [ElementName("Divided Surface by Selection")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [ElementDescription("An element which allows the user to select a divided surface.")]
    public class dynDividedSurfaceBySelection : dynElementSelection
    {
        Expression data;

        public dynDividedSurfaceBySelection()
            : base(new PortData("srf", "The divided surface family instance(s)", typeof(dynNodeUI)))
        { }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            return data;
        }

        protected override string SelectionText
        {
            get { return "Loft ID: " + this.SelectedElement.Id; }
        }

        public override Element SelectedElement
        {
            get
            {
                return base.SelectedElement;
            }
            set
            {
                base.SelectedElement = value;

                var result = new List<List<FamilyInstance>>();

                DividedSurfaceData dsd = this.SelectedElement.GetDividedSurfaceData();

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

        protected override void OnSelectClick()
        {
            this.SelectedElement = SelectionHelper.RequestFormSelection(
               dynSettings.Instance.Doc, "Select a form element.", dynSettings.Instance
            );
        }
    }

    [ElementName("Face by Selection")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [ElementDescription("An element which allows the user to select a face.")]
    public class dynFormElementBySelection : dynElementSelection
    {
        Reference f;

        public dynFormElementBySelection()
            : base(new PortData("face", "The face", typeof(dynNodeUI)))
        { }

        protected override void OnSelectClick()
        {
            var doc = dynSettings.Instance.Doc;

            f = SelectionHelper.RequestFaceReferenceSelection(
               doc, "Select a face.", dynSettings.Instance
            );
            this.SelectedElement = doc.Document.GetElement(f);
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            return Expression.NewContainer(f);
        }

        protected override string SelectionText
        {
            get { return "Face ID: " + this.SelectedElement.Id; }
        }
    }

    [ElementName("Curve by Selection")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [ElementDescription("An element which allows the user to select a curve.")] //or set of curves in the future
    public class dynCurvesBySelection : dynElementSelection
    {
        public dynCurvesBySelection()
            : base(new PortData("curve", "The curve", typeof(CurveElement)))
        { }

        protected override void OnSelectClick()
        {
            this.SelectedElement = SelectionHelper.RequestCurveElementSelection(
               dynSettings.Instance.Doc, "Select a curve.", dynSettings.Instance
            );
        }

        protected override string SelectionText
        {
            get { return "Curve ID: " + this.SelectedElement.Id; }
        }
    }

    [ElementName("Point by Selection")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [ElementDescription("An element which allows the user to select a reference point.")]
    public class dynPointBySelection : dynElementSelection
    {
        public dynPointBySelection() :
            base(new PortData("pt", "The point", typeof(ReferencePoint)))
        { }

        protected override void OnSelectClick()
        {
            this.SelectedElement = SelectionHelper.RequestReferencePointSelection(
               dynSettings.Instance.Doc, "Select a reference point.", dynSettings.Instance
            );
        }

        protected override string SelectionText
        {
            get { return this.SelectedElement.Name + " (" + this.SelectedElement.Id + ")"; }
        }
    }

    [ElementName("SunPath Direction")]
    [ElementCategory(BuiltinElementCategories.REVIT)]
    [ElementDescription("An element which returns the current Sun Path direction.")]
    public class dynSunPathDirection : dynNode
    {
        System.Windows.Controls.TextBox tb;
        System.Windows.Controls.Button sunPathButt;
        Expression data = Expression.NewList(FSharpList<Expression>.Empty);


        public dynSunPathDirection()
        {
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

            NodeUI.inputGrid.RowDefinitions.Add(new RowDefinition());
            NodeUI.inputGrid.RowDefinitions.Add(new RowDefinition());

            NodeUI.inputGrid.Children.Add(tb);
            NodeUI.inputGrid.Children.Add(sunPathButt);

            System.Windows.Controls.Grid.SetRow(sunPathButt, 0);
            System.Windows.Controls.Grid.SetRow(tb, 1);

            NodeUI.RegisterInputsAndOutput();

            NodeUI.topControl.Height = 60;
            NodeUI.UpdateLayout();
        }

        private PortData outPortData = new PortData("XYZ", "XYZ", typeof(XYZ));
        public override PortData OutPortData
        {
            get { return outPortData; }
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

        public SunAndShadowSettings PickedSunAndShadowSettings;

        private ElementId sunAndShadowSettingsID;

        void registerButt_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            View activeView = dynSettings.Instance.Doc.ActiveView;
            PickedSunAndShadowSettings = activeView.SunAndShadowSettings;

            if (PickedSunAndShadowSettings != null)
            {
                sunAndShadowSettingsID = activeView.SunAndShadowSettings.Id;
                this.RegisterEvalOnModified(sunAndShadowSettingsID); // register with the DMU, TODO - watch out for view changes, as sun is view specific
                XYZ sunVector = GetSunDirection(PickedSunAndShadowSettings);

                this.data = Expression.NewContainer(sunVector);

                this.tb.Text = PickedSunAndShadowSettings.Name;
            }
            else
            {
                //sunPathButt.Content = "Select Instance";
                this.tb.Text = "Nothing Selected";
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
                        this.PickedSunAndShadowSettings = dynSettings.Instance.Doc.Document.GetElement(
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
