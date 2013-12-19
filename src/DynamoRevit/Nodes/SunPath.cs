using System;
using System.Linq;
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
using Color = System.Windows.Media.Color;
using Grid = System.Windows.Controls.Grid;
using Transform = Autodesk.Revit.DB.Transform;

namespace Dynamo.Nodes
{
    [NodeName("SunPath Direction")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_SOLAR)]
    [NodeDescription("Returns the current Sun Path direction.")]
    public class SunPathDirection : NodeWithOneOutput
    {
        TextBox tb;
        Button sunPathButt;
        FScheme.Value data = FScheme.Value.NewList(FSharpList<FScheme.Value>.Empty);

        public SunPathDirection()
        {
            OutPortData.Add(new PortData("XYZ", "XYZ", typeof(FScheme.Value.Container)));
            RegisterAllPorts();

            dynRevitSettings.Controller.RevitDocumentChanged += Controller_RevitDocumentChanged;
        }

        void Controller_RevitDocumentChanged(object sender, EventArgs e)
        {
            _pickedSunAndShadowSettings = null;
        }

        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            //add a button to the inputGrid on the dynElement
            sunPathButt = new NodeButton
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Content = "Use SunPath\nfrom Current View"
            };

            sunPathButt.Click += registerButt_Click;

            tb = new TextBox
            {
                Text = "No SunPath Registered",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)),
                BorderThickness = new Thickness(0),
                IsReadOnly = true,
                IsReadOnlyCaretVisible = false
            };

            nodeUI.inputGrid.RowDefinitions.Add(new RowDefinition());
            nodeUI.inputGrid.RowDefinitions.Add(new RowDefinition());

            nodeUI.inputGrid.Children.Add(tb);
            nodeUI.inputGrid.Children.Add(sunPathButt);

            Grid.SetRow(sunPathButt, 0);
            Grid.SetRow(tb, 1);

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
            Transform altitudeRotation = Transform.get_Rotation(XYZ.Zero, XYZ.BasisX, altitude);
            XYZ altitudeDirection = altitudeRotation.OfVector(initialDirection);

            //double azimuth = sunSettings.Azimuth;
            double azimuth = sunSettings.GetFrameAzimuth(sunSettings.ActiveFrame);
            double actualAzimuth = 2 * Math.PI - azimuth;
            Transform azimuthRotation = Transform.get_Rotation(XYZ.Zero, XYZ.BasisZ, actualAzimuth);
            XYZ sunDirection = azimuthRotation.OfVector(altitudeDirection);
            XYZ scaledSunVector = sunDirection.Multiply(100);

            return scaledSunVector;

        }

        private SunAndShadowSettings _pickedSunAndShadowSettings;

        public SunAndShadowSettings PickedSunAndShadowSettings
        {
            get { return _pickedSunAndShadowSettings; }
            set
            {
                _pickedSunAndShadowSettings = value;
                //NotifyPropertyChanged("PickedSunAndShadowSettings");
            }
        }

        private ElementId _sunAndShadowSettingsID;

        void registerButt_Click(object sender, RoutedEventArgs e)
        {
            //data = Value.NewList(FSharpList<Value>.Empty);

            View activeView = dynRevitSettings.Doc.ActiveView;
            PickedSunAndShadowSettings = activeView.SunAndShadowSettings;


            if (PickedSunAndShadowSettings != null)
            {
                _sunAndShadowSettingsID = activeView.SunAndShadowSettings.Id;
                this.RegisterEvalOnModified(_sunAndShadowSettingsID); // register with the DMU, TODO - watch out for view changes, as sun is view specific
                XYZ sunVector = GetSunDirection(PickedSunAndShadowSettings);


                data = FScheme.Value.NewContainer(sunVector);

                tb.Text = PickedSunAndShadowSettings.Name;
            }
            else
            {
                //sunPathButt.Content = "Select Instance";
                tb.Text = "Nothing Selected";
            }
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            if (PickedSunAndShadowSettings == null)
            {
                throw new Exception("The sun and shadow settings have not been selected. Click to pick the sun and shadow settings from the active view.");
            }

            if (PickedSunAndShadowSettings.Id.IntegerValue == _sunAndShadowSettingsID.IntegerValue) // sanity check
            {

                XYZ sunVector = GetSunDirection(PickedSunAndShadowSettings);
                data = FScheme.Value.NewContainer(sunVector);
                return data;
            }
            else
                throw new Exception("SANITY CHECK FAILED");
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            if (PickedSunAndShadowSettings != null)
            {
                XmlElement outEl = xmlDoc.CreateElement("instance");
                outEl.SetAttribute("id", PickedSunAndShadowSettings.Id.ToString());
                nodeElement.AppendChild(outEl);
            }
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            foreach (
                XmlNode subNode in
                    nodeElement.ChildNodes.Cast<XmlNode>()
                               .Where(subNode => subNode.Name.Equals("instance")))
            {
                try
                {
                    PickedSunAndShadowSettings =
                        dynRevitSettings.Doc.Document.GetElement(
                            new ElementId(Convert.ToInt32(subNode.Attributes[0].Value))) as
                            SunAndShadowSettings;
                    if (PickedSunAndShadowSettings != null)
                    {
                        _sunAndShadowSettingsID = PickedSunAndShadowSettings.Id;
                        tb.Text = PickedSunAndShadowSettings.Name;
                        sunPathButt.Content = "Use SunPath from Current View";
                    }
                }
                catch { }
            }
        }
    }
}
