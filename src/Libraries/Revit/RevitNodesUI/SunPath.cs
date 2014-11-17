//using System;
//using System.Collections.Generic;
//using System.Linq;

//using Autodesk.Revit.UI.Events;

//using Dynamo.Applications.Models;
//using Dynamo.Models;
//using Dynamo.Nodes;
//<<<<<<< HEAD
//using Dynamo.UI;
//using Dynamo.Utilities;
//using Dynamo.Wpf;
//=======
//>>>>>>> upstream/master

//using ProtoCore.AST.AssociativeAST;
//using RevitServices.Persistence;

//namespace DSRevitNodesUI
//{
//<<<<<<< HEAD
//    public class SunPathDirectionNodeViewCustomization : INodeViewCustomization<SunPathDirection>
//    {
//        TextBox _tb;
//        Button _sunPathButt;
//        private  SunPathDirection model;

//        public void CustomizeView(SunPathDirection model, dynNodeView nodeUI)
//        {
//            this.model = model;

//            if (model.PickedSunAndShadowSettings != null)
//            {
//                _tb.Text = model.PickedSunAndShadowSettings.Name;
//                _sunPathButt.Content = "Use SunPath from Current View";
//            }

//            //add a button to the inputGrid on the dynElement
//            _sunPathButt = new DynamoNodeButton
//            {
//                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
//                VerticalAlignment = System.Windows.VerticalAlignment.Center
//            };

//            _sunPathButt.Click += new System.Windows.RoutedEventHandler(registerButt_Click);
//            _sunPathButt.Content = "Use SunPath\nfrom Current View";
//            _sunPathButt.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
//            _sunPathButt.VerticalAlignment = System.Windows.VerticalAlignment.Center;

//            _tb = new System.Windows.Controls.TextBox
//            {
//                Text = "No SunPath Registered",
//                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
//                VerticalAlignment = System.Windows.VerticalAlignment.Center
//            };
//            var backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
//            _tb.Background = backgroundBrush;
//            _tb.BorderThickness = new System.Windows.Thickness(0);
//            _tb.IsReadOnly = true;
//            _tb.IsReadOnlyCaretVisible = false;

//            nodeUI.inputGrid.RowDefinitions.Add(new RowDefinition());
//            nodeUI.inputGrid.RowDefinitions.Add(new RowDefinition());

//            nodeUI.inputGrid.Children.Add(_tb);
//            nodeUI.inputGrid.Children.Add(_sunPathButt);

//            System.Windows.Controls.Grid.SetRow(_sunPathButt, 0);
//            System.Windows.Controls.Grid.SetRow(_tb, 1);
//        }

//        void registerButt_Click(object sender, System.Windows.RoutedEventArgs e)
//        {
//            var activeView = DocumentManager.Instance.CurrentDBDocument.ActiveView;
//            model.PickedSunAndShadowSettings = activeView.SunAndShadowSettings;

//            if (model.PickedSunAndShadowSettings != null)
//            {
//                model.UpdateSunVector();
//                _tb.Text = model.PickedSunAndShadowSettings.Name;
//            }
//            else
//            {
//                _tb.Text = "Nothing Selected";
//            }
//        }

//        public void Dispose()
//        {
 	        
//        }
//    }


//    [NodeName("SunPath Direction")]
//    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
//    [NodeDescription("Returns the current Sun Path direction.")]
//    [IsDesignScriptCompatible]
//    public class SunPathDirection : NodeModel
//    {
//        internal XYZ sunVector;
//        public SunAndShadowSettings PickedSunAndShadowSettings { get; set; }

//        public SunPathDirection(WorkspaceModel workspaceModel) : base(workspaceModel)
//        {
//            OutPortData.Add(new PortData("direction", "The sun direction vector."));
//            RegisterAllPorts();

//            // we need to obtain the dynamo model directly from the workspace model 
//            // here, as it is not yet initialized on the base class
//            var revMod = workspaceModel.DynamoModel as RevitDynamoModel;
//            if (revMod == null) return;

//            revMod.RevitDocumentChanged += Controller_RevitDocumentChanged;
//            revMod.RevitServicesUpdater.ElementsModified += Updater_ElementsModified;
//        }

//        private void Updater_ElementsModified(IEnumerable<string> updated)
//        {
//            if (PickedSunAndShadowSettings != null)
//            {
//                if (!updated.Contains(PickedSunAndShadowSettings.UniqueId)) return;

//                sunVector = GetSunDirection(PickedSunAndShadowSettings);
//                RequiresRecalc = true;
//            }
//        }

//        void Controller_RevitDocumentChanged(object sender, EventArgs e)
//        {
//            PickedSunAndShadowSettings = null;
//        }

//        /// <summary>
//        /// Description of ShadowCalculatorUtils.
//        /// NOTE: this is derived from Scott Connover's great class "Geometry API in Revit" from DevCamp 2012, source files accesed 6-8-12 from here 
//        /// https://projectpoint.buzzsaw.com/_bz_rest/Web/Home/Index?folder=44#/_bz_rest/Web/Item/Items?folder=152&count=50&start=0&ownership=Homehttps://projectpoint.buzzsaw.com/_bz_rest/Web/Home/Index?folder=44#/_bz_rest/Web/Item/Items?folder=152&count=50&start=0&ownership=Home
//        /// </summary>
//        public static XYZ GetSunDirection(SunAndShadowSettings sunSettings)
//        {
//            //SunAndShadowSettings sunSettings = view.SunAndShadowSettings;

//            XYZ initialDirection = XYZ.BasisY;

//            //double altitude = sunSettings.Altitude;
//            double altitude = sunSettings.GetFrameAltitude(sunSettings.ActiveFrame);
//            Autodesk.Revit.DB.Transform altitudeRotation = Autodesk.Revit.DB.Transform.get_Rotation(XYZ.Zero, XYZ.BasisX, altitude);
//            XYZ altitudeDirection = altitudeRotation.OfVector(initialDirection);

//            //double azimuth = sunSettings.Azimuth;
//            double azimuth = sunSettings.GetFrameAzimuth(sunSettings.ActiveFrame);
//            double actualAzimuth = 2 * Math.PI - azimuth;
//            Autodesk.Revit.DB.Transform azimuthRotation = Autodesk.Revit.DB.Transform.get_Rotation(XYZ.Zero, XYZ.BasisZ, actualAzimuth);
//            XYZ sunDirection = azimuthRotation.OfVector(altitudeDirection);
//            XYZ scaledSunVector = sunDirection.Multiply(100);

//            return scaledSunVector;

//        }

//        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
//=======
//    [NodeName("SunSettings.Current"), NodeCategory(BuiltinNodeCategories.REVIT_VIEW),
//     NodeDescription("Returns the SunSettings of the current View."), IsDesignScriptCompatible]
//    public class SunSettings : RevitNodeModel
//    {
//        private string settingsID;

//        public SunSettings(WorkspaceModel workspaceModel) : base(workspaceModel)
//        {
//            OutPortData.Add(new PortData("SunSettings", "SunSettings element."));
            
//            RegisterAllPorts();
            
//            RevitDynamoModel.RevitServicesUpdater.ElementsModified += Updater_ElementsModified;
//            DocumentManager.Instance.CurrentUIApplication.ViewActivated += CurrentUIApplication_ViewActivated;

//            CurrentUIApplicationOnViewActivated();
//        }

//        public override void Destroy()
//        {
//            base.Destroy();

//            RevitDynamoModel.RevitServicesUpdater.ElementsModified -= Updater_ElementsModified;
//            DocumentManager.Instance.CurrentUIApplication.ViewActivated -=
//                CurrentUIApplication_ViewActivated;
//        }

//        private void CurrentUIApplication_ViewActivated(object sender, ViewActivatedEventArgs e)
//        {
//            CurrentUIApplicationOnViewActivated();
//        }

//        private void CurrentUIApplicationOnViewActivated()
//        {
//            settingsID =
//                DocumentManager.Instance.CurrentDBDocument.ActiveView.SunAndShadowSettings.UniqueId;
//            ForceReExecuteOfNode = true;
//            RequiresRecalc = true;
//        }

//        private void Updater_ElementsModified(IEnumerable<string> updated)
//>>>>>>> upstream/master
//        {
//            if (updated.Contains(settingsID))
//            {
//<<<<<<< HEAD
//                sunVector = GetSunDirection(PickedSunAndShadowSettings).Normalize();

//                var xNode = AstFactory.BuildDoubleNode(sunVector.X);
//                var yNode = AstFactory.BuildDoubleNode(sunVector.Y);
//                var zNode = AstFactory.BuildDoubleNode(sunVector.Z);
//                node = AstFactory.BuildFunctionCall(
//                    new Func<double, double, double, Autodesk.DesignScript.Geometry.Vector>(Autodesk.DesignScript.Geometry.Vector.ByCoordinates),
//                    new List<AssociativeNode> { xNode, yNode, zNode });
//=======
//                ForceReExecuteOfNode = true;
//                RequiresRecalc = true;
//>>>>>>> upstream/master
//            }
//        }

//        public override IEnumerable<AssociativeNode> BuildOutputAst(
//            List<AssociativeNode> inputAstNodes)
//        {
//            Func<Revit.Elements.SunSettings> func = Revit.Elements.SunSettings.Current;

//<<<<<<< HEAD
//        internal void UpdateSunVector()
//        {
//            this.sunVector = GetSunDirection(this.PickedSunAndShadowSettings);
//        }

//        protected override void LoadNode(XmlNode nodeElement)
//        {
//            foreach (XmlNode subNode in nodeElement.ChildNodes)
//            {
//                if (subNode.Name.Equals("instance"))
//                {
//                    try
//                    {
//                        PickedSunAndShadowSettings = DocumentManager.Instance.CurrentDBDocument.GetElement(
//                           new ElementId(Convert.ToInt32(subNode.Attributes[0].Value))
//                        ) as SunAndShadowSettings;
//                    }
//                    catch { }
//                }
//            }
//        }

//        protected override bool ShouldDisplayPreviewCore()
//        {
//            return false; // Previews are not shown for this node type.
//=======
//            return new[]
//            {
//                AstFactory.BuildAssignment(
//                    GetAstIdentifierForOutputIndex(0),
//                    AstFactory.BuildFunctionCall(func, new List<AssociativeNode>()))
//            };
//>>>>>>> upstream/master
//        }
    

//    }
//}
