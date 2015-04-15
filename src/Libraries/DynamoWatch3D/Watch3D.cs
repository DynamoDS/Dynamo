﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Annotations;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Controls;
using Dynamo.Core.Threading;
using Dynamo.DSEngine;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.UI.Commands;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf;

using ProtoCore.AST.AssociativeAST;

using VMDataBridge;

using Color = System.Windows.Media.Color;
using DynamoWatch3D.Properties;

namespace Dynamo.Nodes
{
    public class Watch3DNodeViewCustomization : INodeViewCustomization<Watch3D>
    {
        private Watch3D watch3dModel;
        public Watch3DView View { get; private set; }

        public void CustomizeView(Watch3D model, NodeView nodeView)
        {
            model.ViewModel = nodeView.ViewModel.DynamoViewModel;
            this.watch3dModel = model;

            var renderingTier = (RenderCapability.Tier >> 16);
            if (renderingTier < 2) return;

            View = new Watch3DView(model.GUID, watch3dModel)
            {
                Width = model.WatchWidth,
                Height = model.WatchHeight
            };

            var pos = model.CameraPosition;
            var viewDir = model.LookDirection;

            View.View.Camera.Position = new Point3D(pos.X, pos.Z, -pos.Y);
            View.View.Camera.LookDirection = new Vector3D(viewDir.X, viewDir.Z, -viewDir.Y);

            // When user sizes a watch node, only view gets resized. The actual 
            // NodeModel does not get updated. This is where the view updates the 
            // model whenever its size is updated. 
            //Updated from (Watch3d)View.SizeChanged to nodeView.SizeChanged - height 
            // and width should correspond to node model and not watch3Dview
            nodeView.SizeChanged += (sender, args) =>
                model.SetSize(args.NewSize.Width, args.NewSize.Height);

            model.RequestUpdateLatestCameraPosition += this.UpdateLatestCameraPosition;

            var mi = new MenuItem { Header = "Zoom to Fit" };
            mi.Click += mi_Click;

            nodeView.MainContextMenu.Items.Add(mi);

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

            nodeView.PresentationGrid.Children.Add(backgroundRect);
            nodeView.PresentationGrid.Children.Add(View);
            nodeView.PresentationGrid.Visibility = Visibility.Visible;

            DataBridge.Instance.RegisterCallback(
                model.GUID.ToString(),
                obj =>
                    nodeView.Dispatcher.Invoke(
                        new Action<object>(RenderData),
                        DispatcherPriority.Render,
                        obj));
        }

        private void UpdateLatestCameraPosition()
        {
            if (View == null) return;

            var pos = View.View.Camera.Position;
            var viewDir = View.View.Camera.LookDirection;

            // Convert Helix3D coordinates from +Y up to +Z up.
            watch3dModel.CameraPosition = new Point3D(pos.X, -pos.Z, pos.Y);
            watch3dModel.LookDirection = new Vector3D(viewDir.X, -viewDir.Z, viewDir.Y);
        }

        private void RenderData(object data)
        {
            if (View == null) return;

            View.RenderDrawables(
                new VisualizationEventArgs(UnpackRenderData(data).Select(this.watch3dModel.VisualizationManager.CreateRenderPackageFromGraphicItem), watch3dModel.GUID));
        }

        void mi_Click(object sender, RoutedEventArgs e)
        {
            if (View == null) return;

            View.View.ZoomExtents();
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

        public void Dispose()
        {
            watch3dModel.RequestUpdateLatestCameraPosition -= this.UpdateLatestCameraPosition;
        }
    }

    [NodeName("Watch 3D")]
    [NodeCategory(BuiltinNodeCategories.CORE_VIEW)]
    [NodeDescription("Watch3DDescription",typeof(DynamoWatch3D.Properties.Resources))]
    [NodeSearchTags("Watch3DSearchTags", typeof(DynamoWatch3D.Properties.Resources))]
    [AlsoKnownAs("Dynamo.Nodes.dyn3DPreview", "Dynamo.Nodes.3DPreview")]
    [IsDesignScriptCompatible]
    public class Watch3D : NodeModel, IWatchViewModel
    {
        public bool _canNavigateBackground { get; private set; }

        public double WatchWidth { get; private set; }
        public double WatchHeight { get; private set; }
        public Point3D CameraPosition { get; set; }
        public Vector3D LookDirection { get; set; }

        public delegate void VoidHandler();
        public event VoidHandler RequestUpdateLatestCameraPosition;
        private void OnRequestUpdateLatestCameraPosition()
        {
            if (RequestUpdateLatestCameraPosition != null)
            {
                RequestUpdateLatestCameraPosition();
            }
        }

        #region public properties

        public DelegateCommand GetBranchVisualizationCommand { get; set; }

        public DelegateCommand ToggleCanNavigateBackgroundCommand
        {
            get
            {
                return this.ViewModel.ToggleCanNavigateBackgroundCommand;
            }
        }

        public bool WatchIsResizable { get; set; }

        public bool IsBackgroundPreview { get { return false; } }

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

        public DynamoViewModel ViewModel { get; set; }

        public IVisualizationManager VisualizationManager
        {
            get { return ViewModel.VisualizationManager; }

        }

        #endregion

        #region constructors

        public Watch3D()
        {
            InPortData.Add(new PortData("", Resources.Watch3DPortDataInputToolTip));
            OutPortData.Add(new PortData("", Resources.Watch3DPortDataOutputToolTip));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;

            GetBranchVisualizationCommand = new DelegateCommand(GetBranchVisualization, CanGetBranchVisualization);

            WatchIsResizable = true;

            WatchWidth = 200;
            WatchHeight = 200;

            // Camera coordinates stored on the model assume +Z up.
            CameraPosition = new Point3D(10, -10, 10);
            LookDirection = new Vector3D(-1, 1, -1);

            ShouldDisplayPreviewCore = false;
        }

        #endregion

        #region public methods

        public override void Dispose()
        {
            base.Dispose();
            DataBridge.Instance.UnregisterCallback(GUID.ToString());
        }

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
                AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), inputAstNodes[0])
            };

            return resultAst;
        }

        #endregion

        protected override void SerializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.SerializeCore(nodeElement, context);

            var viewElement = nodeElement.OwnerDocument.CreateElement("view");
            nodeElement.AppendChild(viewElement);
            var viewHelper = new XmlElementHelper(viewElement);

            WatchWidth = Width;
            WatchHeight = Height;
            viewHelper.SetAttribute("width", WatchWidth);
            viewHelper.SetAttribute("height", WatchHeight);

            // the view stores the latest position
            OnRequestUpdateLatestCameraPosition();

            var camElement = nodeElement.OwnerDocument.CreateElement("camera");
            viewElement.AppendChild(camElement);
            var camHelper = new XmlElementHelper(camElement);

            // Camera coordinates are saved to the xml assuming +Z up.
            camHelper.SetAttribute("pos_x", CameraPosition.X);
            camHelper.SetAttribute("pos_y", CameraPosition.Y);
            camHelper.SetAttribute("pos_z", CameraPosition.Z);
            camHelper.SetAttribute("look_x", LookDirection.X);
            camHelper.SetAttribute("look_y", LookDirection.Y);
            camHelper.SetAttribute("look_z", LookDirection.Z);
        }

        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.DeserializeCore(nodeElement, context);
            try
            {
                foreach (XmlNode node in nodeElement.ChildNodes)
                {
                    if (node.Name == "view")
                    {
                        WatchWidth = Convert.ToDouble(node.Attributes["width"].Value);
                        WatchHeight = Convert.ToDouble(node.Attributes["height"].Value);

                        foreach (XmlNode inNode in node.ChildNodes)
                        {
                            if (inNode.Name == "camera")
                            {
                                var x = Convert.ToDouble(inNode.Attributes["pos_x"].Value, CultureInfo.InvariantCulture);
                                var y = Convert.ToDouble(inNode.Attributes["pos_y"].Value, CultureInfo.InvariantCulture);
                                var z = Convert.ToDouble(inNode.Attributes["pos_z"].Value, CultureInfo.InvariantCulture);
                                var lx = Convert.ToDouble(inNode.Attributes["look_x"].Value, CultureInfo.InvariantCulture);
                                var ly = Convert.ToDouble(inNode.Attributes["look_y"].Value, CultureInfo.InvariantCulture);
                                var lz = Convert.ToDouble(inNode.Attributes["look_z"].Value, CultureInfo.InvariantCulture);
                                CameraPosition = new Point3D(x, y, z);
                                LookDirection = new Vector3D(lx, ly, lz);
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Log(LogMessage.Error(ex));
                Log("View attributes could not be read from the file.");
            }

        }

        protected override void RequestVisualUpdateAsyncCore(
            IScheduler scheduler, EngineController engine, int maxTesselationDivisions)
        {
            // No visualization update is required for this node type.
        }

        #region IWatchViewModel interface

        public void GetBranchVisualization(object parameters)
        {
            ViewModel.VisualizationManager.RequestBranchUpdate(this);
        }

        public bool CanGetBranchVisualization(object parameter)
        {
            return true;
        }

        #endregion
    }
}
