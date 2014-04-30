using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.UI;
using Dynamo.Utilities;
using ProtoCore.AST.AssociativeAST;
using RevitServices.Persistence;
using Autodesk.Revit.DB;

namespace DSRevitNodesUI
{
    [NodeName("SunPath Direction")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_SOLAR)]
    [NodeDescription("Returns the current Sun Path direction.")]
    [IsDesignScriptCompatible]
    public class SunPathDirection : NodeModel, IWpfNode
    {
        TextBox _tb;
        Button _sunPathButt;
        private XYZ _sunVector;

        public SunAndShadowSettings PickedSunAndShadowSettings { get; set; }

        public SunPathDirection()
        {
            OutPortData.Add(new PortData("direction", "The sun direction vector."));
            RegisterAllPorts();

            dynRevitSettings.Controller.RevitDocumentChanged += Controller_RevitDocumentChanged;
            dynRevitSettings.Controller.Updater.ElementsModified += Updater_ElementsModified;
        }

        private void Updater_ElementsModified(IEnumerable<string> updated)
        {
            if (PickedSunAndShadowSettings != null)
            {
                if (!updated.Contains(PickedSunAndShadowSettings.UniqueId)) return;

                _sunVector = GetSunDirection(PickedSunAndShadowSettings);
                RequiresRecalc = true;
            }
        }

        void Controller_RevitDocumentChanged(object sender, EventArgs e)
        {
            PickedSunAndShadowSettings = null;
        }

        public void SetupCustomUIElements(dynNodeView nodeUI)
        {
            //add a button to the inputGrid on the dynElement
            _sunPathButt = new DynamoNodeButton
            {
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            };

            _sunPathButt.Click += new System.Windows.RoutedEventHandler(registerButt_Click);
            _sunPathButt.Content = "Use SunPath\nfrom Current View";
            _sunPathButt.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            _sunPathButt.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            _tb = new System.Windows.Controls.TextBox
            {
                Text = "No SunPath Registered",
                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            };
            var backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
            _tb.Background = backgroundBrush;
            _tb.BorderThickness = new Thickness(0);
            _tb.IsReadOnly = true;
            _tb.IsReadOnlyCaretVisible = false;

            nodeUI.inputGrid.RowDefinitions.Add(new RowDefinition());
            nodeUI.inputGrid.RowDefinitions.Add(new RowDefinition());

            nodeUI.inputGrid.Children.Add(_tb);
            nodeUI.inputGrid.Children.Add(_sunPathButt);

            System.Windows.Controls.Grid.SetRow(_sunPathButt, 0);
            System.Windows.Controls.Grid.SetRow(_tb, 1);
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

        void registerButt_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var activeView = DocumentManager.Instance.CurrentDBDocument.ActiveView;
            PickedSunAndShadowSettings = activeView.SunAndShadowSettings;

            if (PickedSunAndShadowSettings != null)
            {
                _sunVector = GetSunDirection(PickedSunAndShadowSettings);
                _tb.Text = PickedSunAndShadowSettings.Name;
            }
            else
            {
                _tb.Text = "Nothing Selected";
            }
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            AssociativeNode node = null;

            if (PickedSunAndShadowSettings == null)
            {
                node = AstFactory.BuildNullNode();
            }
            else
            {
                _sunVector = GetSunDirection(PickedSunAndShadowSettings).Normalize();

                var xNode = AstFactory.BuildDoubleNode(_sunVector.X);
                var yNode = AstFactory.BuildDoubleNode(_sunVector.Y);
                var zNode = AstFactory.BuildDoubleNode(_sunVector.Z);
                node = AstFactory.BuildFunctionCall(
                    new Func<double, double, double, Autodesk.DesignScript.Geometry.Vector>(Autodesk.DesignScript.Geometry.Vector.ByCoordinates),
                    new List<AssociativeNode> { xNode, yNode, zNode });
            }

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) }; 
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            if (this.PickedSunAndShadowSettings != null)
            {
                XmlElement outEl = xmlDoc.CreateElement("instance");
                outEl.SetAttribute("id", PickedSunAndShadowSettings.Id.ToString());
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
                        PickedSunAndShadowSettings = DocumentManager.Instance.CurrentDBDocument.GetElement(
                           new ElementId(Convert.ToInt32(subNode.Attributes[0].Value))
                        ) as SunAndShadowSettings;
                        if (PickedSunAndShadowSettings != null)
                        {
                            _tb.Text = PickedSunAndShadowSettings.Name;
                            _sunPathButt.Content = "Use SunPath from Current View";
                        }
                    }
                    catch { }
                }
            }
        }
    }
}
