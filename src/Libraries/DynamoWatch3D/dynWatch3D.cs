using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Controls;
using Dynamo.DSEngine;
using Dynamo.Models;
using Dynamo.UI;
using Dynamo.UI.Commands;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using ProtoCore.AST.AssociativeAST;

using VMDataBridge;

using Color = System.Windows.Media.Color;

namespace Dynamo.Nodes
{
    [NodeName("Watch 3D")]
    [NodeCategory(BuiltinNodeCategories.CORE_VIEW)]
    [NodeDescription("Shows a dynamic preview of geometry.")]
    [AlsoKnownAs("Dynamo.Nodes.dyn3DPreview", "Dynamo.Nodes.3DPreview")]
    [IsDesignScriptCompatible]
    public class Watch3D : NodeModel, IWatchViewModel, IWpfNode
    {
        private bool _canNavigateBackground = true;
        private double _watchWidth = 200;
        private double _watchHeight = 200;
        private Point3D _camPosition = new Point3D(10,10,10);
        private Vector3D _lookDirection = new Vector3D(-1,-1,-1);

        public DelegateCommand GetBranchVisualizationCommand { get; set; }
        public DelegateCommand CheckForLatestRenderCommand { get; set; }

        public DelegateCommand ToggleCanNavigateBackgroundCommand
        {
            get
            {
                return this.DynamoViewModel.ToggleCanNavigateBackgroundCommand;
            }
        }

        public bool WatchIsResizable { get; set; }
        public bool IsBackgroundPreview { get { return false; } }

        public Watch3DView View { get; private set; }

        public bool CanNavigateBackground
        {
            get
            {
                return _canNavigateBackground;
            }
            set
            {
                _canNavigateBackground = value;
                RaisePropertyChanged("CanNavigateBackground");
            }
        }

        public Watch3D(WorkspaceModel workspace) : base(workspace)
        {
            InPortData.Add(new PortData("", "Incoming geometry objects."));
            OutPortData.Add(new PortData("", "Watch contents, passed through"));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;

            GetBranchVisualizationCommand = new DelegateCommand(GetBranchVisualization, CanGetBranchVisualization);
            CheckForLatestRenderCommand = new DelegateCommand(CheckForLatestRender, CanCheckForLatestRender);
            WatchIsResizable = true;
        }

        public override void Destroy()
        {
            base.Destroy();
            DataBridge.Instance.UnregisterCallback(GUID.ToString());
        }

        private static IEnumerable<IGraphicItem> UnpackRenderData(object data)
        {
            if (data is IGraphicItem)
                yield return data as IGraphicItem;
            else if (data is IEnumerable)
            {
                var graphics = (data as IEnumerable).Cast<object>().SelectMany(UnpackRenderData);
                foreach (var g in graphics)
                    yield return g;
            }
        }

        private RenderPackage PackageRenderData(IGraphicItem gItem)
        {
            var renderPackage = new RenderPackage();
            gItem.Tessellate(renderPackage, -1.0, this.DynamoViewModel.VisualizationManager.MaxTesselationDivisions);
            renderPackage.ItemsCount++;
            return renderPackage;
        }

        private void RenderData(object data)
        {
            View.RenderDrawables(
                new VisualizationEventArgs(UnpackRenderData(data).Select(PackageRenderData), GUID.ToString(), -1));
        }

        public void SetupCustomUIElements(dynNodeView nodeUI)
        {
            this.DynamoViewModel = nodeUI.ViewModel.DynamoViewModel;

            var mi = new MenuItem { Header = "Zoom to Fit" };
            mi.Click += mi_Click;

            nodeUI.MainContextMenu.Items.Add(mi);

            //add a 3D viewport to the input grid
            //http://helixtoolkit.codeplex.com/wikipage?title=HelixViewport3D&referringTitle=Documentation
            //_watchView = new WatchView();
            View = new Watch3DView(GUID.ToString())
            {
                DataContext = this,
                Width = _watchWidth,
                Height = _watchHeight
            };

            View.View.Camera.Position = _camPosition;
            View.View.Camera.LookDirection = _lookDirection;

            var backgroundRect = new Rectangle
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                IsHitTestVisible = false
            };
            var bc = new BrushConverter();
            var strokeBrush = (Brush)bc.ConvertFrom("#313131");
            backgroundRect.Stroke = strokeBrush;
            backgroundRect.StrokeThickness = 1;
            var backgroundBrush = new SolidColorBrush(Color.FromRgb(240, 240, 240));
            backgroundRect.Fill = backgroundBrush;

            nodeUI.PresentationGrid.Children.Add(backgroundRect);
            nodeUI.PresentationGrid.Children.Add(View);
            nodeUI.PresentationGrid.Visibility = Visibility.Visible;

            DataBridge.Instance.RegisterCallback(
                GUID.ToString(),
                obj =>
                    nodeUI.Dispatcher.Invoke(
                        new Action<object>(RenderData),
                        DispatcherPriority.Render,
                        obj));
        }

        void mi_Click(object sender, RoutedEventArgs e)
        {
            View.View.ZoomExtents();
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            base.SaveNode(xmlDoc, nodeElement, context);
            
            var viewElement = xmlDoc.CreateElement("view");
            nodeElement.AppendChild(viewElement);
            var viewHelper = new XmlElementHelper(viewElement);

            viewHelper.SetAttribute("width", Width);
            viewHelper.SetAttribute("height", Height);

            //Bail out early if the view hasn't been created.
            if (View == null)
                return;

            var camElement = xmlDoc.CreateElement("camera");
            viewElement.AppendChild(camElement);
            var camHelper = new XmlElementHelper(camElement);

            camHelper.SetAttribute("pos_x", View.View.Camera.Position.X);
            camHelper.SetAttribute("pos_y", View.View.Camera.Position.Y);
            camHelper.SetAttribute("pos_z", View.View.Camera.Position.Z);
            camHelper.SetAttribute("look_x", View.View.Camera.LookDirection.X);
            camHelper.SetAttribute("look_y", View.View.Camera.LookDirection.Y);
            camHelper.SetAttribute("look_z", View.View.Camera.LookDirection.Z);
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            base.LoadNode(nodeElement);
            try
            {
                foreach (XmlNode node in nodeElement.ChildNodes)
                {
                    if (node.Name == "view")
                    {
                        _watchWidth = Convert.ToDouble(node.Attributes["width"].Value);
                        _watchHeight = Convert.ToDouble(node.Attributes["height"].Value);

                        foreach (XmlNode inNode in node.ChildNodes)
                        {
                            if (inNode.Name == "camera")
                            {
                                var x = Convert.ToDouble(inNode.Attributes["pos_x"].Value);
                                var y = Convert.ToDouble(inNode.Attributes["pos_y"].Value);
                                var z = Convert.ToDouble(inNode.Attributes["pos_z"].Value);
                                var lx = Convert.ToDouble(inNode.Attributes["look_x"].Value);
                                var ly = Convert.ToDouble(inNode.Attributes["look_y"].Value);
                                var lz = Convert.ToDouble(inNode.Attributes["look_z"].Value);
                                _camPosition = new Point3D(x,y,z);
                                _lookDirection = new Vector3D(lx,ly,lz);
                            }
                        }
                    }
                }
                
            }
            catch(Exception ex)
            {
                this.Workspace.DynamoModel.Logger.Log(ex);
                this.Workspace.DynamoModel.Logger.Log("View attributes could not be read from the file.");
            }
            
        }

#if ENABLE_DYNAMO_SCHEDULER

        protected override void RequestVisualUpdateCore(int maxTesselationDivisions)
        {
            return; // No visualization update is required for this node type.
        }

#else

        public override void UpdateRenderPackage(int maxTessDivisions)
        {
            //do nothing
            //a watch should not draw its outputs
        }

#endif

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            if (IsPartiallyApplied)
            {
                return new[]
                {
                    AstFactory.BuildAssignment(
                        GetAstIdentifierForOutputIndex(0),
                        AstFactory.BuildFunctionObject(
                            new IdentifierListNode
                            {
                                LeftNode = AstFactory.BuildIdentifier("DataBridge"),
                                RightNode = AstFactory.BuildIdentifier("BridgeData")
                            },
                            2,
                            new[] { 0 },
                            new List<AssociativeNode>
                            {
                                AstFactory.BuildStringNode(GUID.ToString()),
                                AstFactory.BuildNullNode()
                            }))
                };
            }

            var resultAst = new[]
            {
                AstFactory.BuildAssignment(
                    GetAstIdentifierForOutputIndex(0),
                    DataBridge.GenerateBridgeDataAst(GUID.ToString(), inputAstNodes[0])),
                AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), inputAstNodes[0])
            };

            return resultAst;
        }

        #region IWatchViewModel interface

        public void GetBranchVisualization(object parameters)
        {
            var taskId = (long)parameters;
            this.DynamoViewModel.VisualizationManager.AggregateUpstreamRenderPackages(new RenderTag(taskId, this));
        }

        public bool CanGetBranchVisualization(object parameter)
        {
            return true;
        }

        private bool CanCheckForLatestRender(object obj)
        {
            return true;
        }

        private void CheckForLatestRender(object obj)
        {
            this.DynamoViewModel.VisualizationManager.CheckIfLatestAndUpdate((long)obj);
        }

        #endregion

        protected override bool ShouldDisplayPreviewCore()
        {
            return false; // Previews are not shown for this node type.
        }

        public DynamoViewModel DynamoViewModel { get; set; }
    }
}
