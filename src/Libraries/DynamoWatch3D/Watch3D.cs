using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Controls;
using Dynamo.Core.Threading;
using Dynamo.DSEngine;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Utilities;
using Dynamo.Wpf;
using Dynamo.Wpf.Rendering;
using Dynamo.Wpf.ViewModels.Watch3D;
using DynamoWatch3D.Properties;
using ProtoCore.AST.AssociativeAST;
using VMDataBridge;

namespace Dynamo.Nodes
{
    public class Watch3DNodeViewCustomization : INodeViewCustomization<Watch3D>
    {
        private Watch3D watch3dModel;

        public void CustomizeView(Watch3D model, NodeView nodeView)
        {
            var dynamoViewModel = nodeView.ViewModel.DynamoViewModel;
            watch3dModel = model;
            
            var renderingTier = (RenderCapability.Tier >> 16);
            if (renderingTier < 2) return;

            var vmParams = new Watch3DViewModelStartupParams()
            {
                Model = dynamoViewModel.Model,
                Factory = dynamoViewModel.RenderPackageFactoryViewModel.Factory,
                ViewModel = dynamoViewModel,
                IsActiveAtStart = true,
                Name = string.Format("{0} Preview", watch3dModel.GUID)
            };

            model.viewModel = HelixWatch3DNodeViewModel.Start(watch3dModel, vmParams);
            if (model.initialCameraData != null)
            {
                model.viewModel.SetCameraData(model.initialCameraData);
            }

            model.View = new Watch3DView()
            {
                Width = model.WatchWidth,
                Height = model.WatchHeight,
                DataContext = model.viewModel
            };

            // When user sizes a watch node, only view gets resized. The actual 
            // NodeModel does not get updated. This is where the view updates the 
            // model whenever its size is updated. 
            //Updated from (Watch3d)View.SizeChanged to nodeView.SizeChanged - height 
            // and width should correspond to node model and not watch3Dview
            nodeView.SizeChanged += (sender, args) =>
                model.SetSize(args.NewSize.Width, args.NewSize.Height);

            var mi = new MenuItem { Header = Resources.ZoomToFit };
            mi.Click += mi_Click;

            nodeView.MainContextMenu.Items.Add(mi);

            var backgroundRect = new Rectangle
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                IsHitTestVisible = false,
            };
            var bc = new BrushConverter();
            var strokeBrush = (Brush)bc.ConvertFrom("#313131");
            backgroundRect.Stroke = strokeBrush;
            backgroundRect.StrokeThickness = 1;
            var backgroundBrush = new SolidColorBrush(Color.FromRgb(240, 240, 240));
            backgroundRect.Fill = backgroundBrush;

            nodeView.PresentationGrid.Children.Add(backgroundRect);
            nodeView.PresentationGrid.Children.Add(model.View);
            nodeView.PresentationGrid.Visibility = Visibility.Visible;

            DataBridge.Instance.RegisterCallback(
                model.GUID.ToString(),
                obj =>
                    nodeView.Dispatcher.Invoke(
                        new Action<object>(RenderData),
                        DispatcherPriority.Render,
                        obj));       
        }

        private IRenderPackage CreateRenderPackageFromGraphicItem(IGraphicItem gItem)
        {
            var factory = new HelixRenderPackageFactory();
            var renderPackage = factory.CreateRenderPackage();
            gItem.Tessellate(renderPackage, factory.TessellationParameters);
            return renderPackage;
        }

        private void RenderData(object data)
        {
            if (watch3dModel.View == null) return;

            watch3dModel.viewModel.OnRequestCreateModels(UnpackRenderData(data).Select(CreateRenderPackageFromGraphicItem));
        }

        void mi_Click(object sender, RoutedEventArgs e)
        {
            if (watch3dModel.View == null) return;

            watch3dModel.View.View.ZoomExtents();
        }

        private static IEnumerable<IGraphicItem> UnpackRenderData(object data)
        {
            var item = data as IGraphicItem;
            if (item != null)
            {
                yield return item;
            }
            else
            {
                var enumerable = data as IEnumerable;
                if (enumerable == null) yield break;
                var graphics = enumerable.Cast<object>().SelectMany(UnpackRenderData);
                foreach (var g in graphics)
                    yield return g;
            }
        }

        public void Dispose()
        {

        }
    }

    [NodeName("Watch 3D")]
    [NodeCategory(BuiltinNodeCategories.CORE_VIEW)]
    [NodeDescription("Watch3DDescription",typeof(Resources))]
    [NodeSearchTags("Watch3DSearchTags", typeof(Resources))]
    [AlsoKnownAs("Dynamo.Nodes.dyn3DPreview", "Dynamo.Nodes.3DPreview")]
    [IsDesignScriptCompatible]
    public class Watch3D : NodeModel
    {

        // Because the view model, which maintains the camera, 
        // is not created until the view customization is applied,
        // we cache the camera position data returned from the file
        // to be applied when the view model is constructed.
        internal CameraData initialCameraData;

        internal HelixWatch3DNodeViewModel viewModel;
        internal Watch3DView View { get; set; }

        public double WatchWidth { get; private set; }
        public double WatchHeight { get; private set; }

        public delegate void VoidHandler();

        #region constructors

        public Watch3D()
        {
            InPortData.Add(new PortData("", Resources.Watch3DPortDataInputToolTip));
            OutPortData.Add(new PortData("", Resources.Watch3DPortDataOutputToolTip));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;

            WatchWidth = 200;
            WatchHeight = 200;

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

            if (nodeElement.OwnerDocument == null) return;

            var viewElement = nodeElement.OwnerDocument.CreateElement("view");
            nodeElement.AppendChild(viewElement);
            var viewHelper = new XmlElementHelper(viewElement);

            WatchWidth = Width;
            WatchHeight = Height;
            viewHelper.SetAttribute("width", WatchWidth);
            viewHelper.SetAttribute("height", WatchHeight);

            if (viewModel == null) return;
            viewModel.SerializeCamera(nodeElement);
        }

        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.DeserializeCore(nodeElement, context);
            try
            {
                foreach (var node in nodeElement.ChildNodes.Cast<XmlNode>().Where(node => node.Name == "view"))
                {
                    if (node.Attributes == null || node.Attributes.Count == 0) continue;

                    WatchWidth = Convert.ToDouble(node.Attributes["width"].Value);
                    WatchHeight = Convert.ToDouble(node.Attributes["height"].Value);

                    // The deserialization logic is unified between the view model and this node model.
                    // For the node model, we need to supply the deserialization method with the camera node.
                    var cameraElement = nodeElement.ChildNodes.Cast<XmlNode>().FirstOrDefault(innerNode => innerNode.Name == "Camera");
                    initialCameraData = HelixWatch3DViewModel.DeserializeCamera(cameraElement);
                }
            }
            catch (Exception ex)
            {
                Log(LogMessage.Error(ex));
                Log("View attributes could not be read from the file.");
                initialCameraData = new CameraData();
            }

        }

        protected override void RequestVisualUpdateAsyncCore(
            IScheduler scheduler, EngineController engine, IRenderPackageFactory factory)
        {
            // No visualization update is required for this node type.
        }
    }
}
