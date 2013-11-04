using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using Autodesk.Revit.DB;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Revit.SyncedNodeExtensions;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;

namespace Dynamo.Nodes
{
    [NodeName("SunPath Direction")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_SOLAR)]
    [NodeDescription("Returns the current Sun Path direction.")]
    public class SunPathDirection : NodeWithOneOutput
    {
        System.Windows.Controls.TextBox tb;
        System.Windows.Controls.Button sunPathButt;
        FScheme.Value data = FScheme.Value.NewList(FSharpList<FScheme.Value>.Empty);

        public SunPathDirection()
        {
            OutPortData.Add(new PortData("XYZ", "XYZ", typeof(FScheme.Value.Container)));
            RegisterAllPorts();

            dynRevitSettings.Controller.RevitDocumentChanged += Controller_RevitDocumentChanged;
        }

        void Controller_RevitDocumentChanged(object sender, EventArgs e)
        {
            pickedSunAndShadowSettings = null;
        }

        public override void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

            //add a button to the inputGrid on the dynElement
            sunPathButt = new dynNodeButton();

            sunPathButt.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            sunPathButt.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            sunPathButt.Click += new System.Windows.RoutedEventHandler(registerButt_Click);
            sunPathButt.Content = "Use SunPath\nfrom Current View";
            sunPathButt.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            sunPathButt.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            tb = new System.Windows.Controls.TextBox();
            tb.Text = "No SunPath Registered";
            tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            SolidColorBrush backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
            tb.Background = backgroundBrush;
            tb.BorderThickness = new Thickness(0);
            tb.IsReadOnly = true;
            tb.IsReadOnlyCaretVisible = false;

            nodeUI.inputGrid.RowDefinitions.Add(new RowDefinition());
            nodeUI.inputGrid.RowDefinitions.Add(new RowDefinition());

            nodeUI.inputGrid.Children.Add(tb);
            nodeUI.inputGrid.Children.Add(sunPathButt);

            System.Windows.Controls.Grid.SetRow(sunPathButt, 0);
            System.Windows.Controls.Grid.SetRow(tb, 1);

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
                //NotifyPropertyChanged("PickedSunAndShadowSettings");
            }
        }

        private ElementId sunAndShadowSettingsID;

        private ElementId SunAndShadowSettingsID
        {
            get { return sunAndShadowSettingsID; }
            set
            {
                sunAndShadowSettingsID = value;
                //NotifyPropertyChanged("SunAndShadowSettingsID");
            }
        }

        void registerButt_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //data = Value.NewList(FSharpList<Value>.Empty);

            View activeView = dynRevitSettings.Doc.ActiveView;
            PickedSunAndShadowSettings = activeView.SunAndShadowSettings;


            if (PickedSunAndShadowSettings != null)
            {
                sunAndShadowSettingsID = activeView.SunAndShadowSettings.Id;
                this.RegisterEvalOnModified(sunAndShadowSettingsID); // register with the DMU, TODO - watch out for view changes, as sun is view specific
                XYZ sunVector = GetSunDirection(PickedSunAndShadowSettings);


                this.data = FScheme.Value.NewContainer(sunVector);

                this.tb.Text = PickedSunAndShadowSettings.Name;
            }
            else
            {
                //sunPathButt.Content = "Select Instance";
                this.tb.Text = "Nothing Selected";
            }
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            if (PickedSunAndShadowSettings == null)
            {
                throw new Exception("The sun and shadow settings have not been selected. Click to pick the sun and shadow settings from the active view.");
            }

            if (PickedSunAndShadowSettings.Id.IntegerValue == sunAndShadowSettingsID.IntegerValue) // sanity check
            {

                XYZ sunVector = GetSunDirection(PickedSunAndShadowSettings);
                this.data = FScheme.Value.NewContainer(sunVector);
                return data;
            }
            else
                throw new Exception("SANITY CHECK FAILED");
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            if (this.PickedSunAndShadowSettings != null)
            {
                XmlElement outEl = xmlDoc.CreateElement("instance");
                outEl.SetAttribute("id", this.PickedSunAndShadowSettings.Id.ToString());
                nodeElement.AppendChild(outEl);
            }
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                if (subNode.Name.Equals("instance"))
                {
                    try
                    {
                        this.PickedSunAndShadowSettings = dynRevitSettings.Doc.Document.GetElement(
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
