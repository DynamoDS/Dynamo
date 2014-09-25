using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.UI;
using Dynamo.Utilities;
using ProtoCore.AST.AssociativeAST;
using Color = DSCore.Color;

namespace DSCoreNodesUI
{
    [IsDesignScriptCompatible]
    [NodeName("Color Range")]
    [NodeCategory("Core.Color.Create")]
    [NodeDescription("Get a color given a color range.")]
    public class ColorRange : NodeModel, IWpfNode
    {
        public event EventHandler RequestChangeColorRange;
        protected virtual void OnRequestChangeColorRange(object sender, EventArgs e)
        {
            if (RequestChangeColorRange != null)
                RequestChangeColorRange(sender, e);
        }

        public ColorRange(WorkspaceModel workspace)
            : base(workspace)
        {
            InPortData.Add(new PortData("start", "The start color."));
            InPortData.Add(new PortData("end", "The end color."));
            InPortData.Add(new PortData("value", "The value between 0 and 1 of the selected color."));
            OutPortData.Add(new PortData("color", "The selected color."));

            RegisterAllPorts();

            this.PropertyChanged += ColorRange_PropertyChanged;
        }

        void ColorRange_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "IsUpdated")
                return;

            if (InPorts.Any(x => x.Connectors.Count == 0))
                return;

            OnRequestChangeColorRange(this, EventArgs.Empty);
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var functionCall = AstFactory.BuildFunctionCall("Color", "BuildColorFromRange", inputAstNodes);

            return new[]
            {
                AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall)
            };
        }

        public void SetupCustomUIElements(dynNodeView view)
        {
            var drawPlane = new Image
            {
                Stretch = Stretch.Fill,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Width = 100,
                Height = 200
            };

            var dm = this.Workspace.DynamoModel;

            view.inputGrid.Children.Add(drawPlane);

            RequestChangeColorRange += delegate
            {
                DispatchOnUIThread(delegate
                {
                    var colorStartNode = InPorts[0].Connectors[0].Start.Owner;
                    var startIndex = InPorts[0].Connectors[0].Start.Index;
                    var colorEndNode = InPorts[1].Connectors[0].Start.Owner;
                    var endIndex = InPorts[1].Connectors[0].Start.Index;

                    var startId = colorStartNode.GetAstIdentifierForOutputIndex(startIndex).Name;
                    var endId = colorEndNode.GetAstIdentifierForOutputIndex(endIndex).Name;

                    var startMirror = dm.EngineController.GetMirror(startId);
                    var endMirror = dm.EngineController.GetMirror(endId);

                    object start = null;
                    object end = null;

                    if (startMirror == null)
                    {
                        start = Color.ByARGB(255, 192, 192, 192);
                    }
                    else
                    {
                        if (startMirror.GetData().IsCollection)
                        {
                            start = startMirror.GetData().GetElements().
                                Select(x => x.Data).FirstOrDefault();
                        }
                        else
                        {
                            start = startMirror.GetData().Data;
                        }
                    }

                    if (endMirror == null)
                    {
                        end = Color.ByARGB(255, 64, 64, 64);
                    }
                    else
                    {
                        if (endMirror.GetData().IsCollection)
                        {
                            end = endMirror.GetData().GetElements().
                                Select(x => x.Data).FirstOrDefault();
                        }
                        else
                        {
                            end = endMirror.GetData().Data;
                        }
                    }

                    Color startColor = start as Color;
                    Color endColor = end as Color;
                    if (null != startColor && null != endColor)
                    {
                        WriteableBitmap bmp = CompleteColorScale(startColor, endColor);
                        drawPlane.Source = bmp;
                    }
                });
            };
        }

        protected override bool ShouldDisplayPreviewCore()
        {
            return false; // Previews are not shown for this node type.
        }

        //http://gaggerostechnicalnotes.blogspot.com/2012/01/wpf-colors-scale.html
        private WriteableBitmap CompleteColorScale(Color start, Color end)
        {
            const int size = 64;

            const int width = 1;
            const int height = size;

            var bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            var pixels = new uint[width * height];

            for (int i = 0; i < size; i++)
            {
                var newRed = start.Red + ((end.Red - start.Red) / size) * i;
                var newGreen = start.Green + ((end.Green - start.Green) / size) * i;
                var newBlue = start.Blue + ((end.Blue - start.Blue) / size) * i;

                pixels[i] = (uint)((255 << 24) + (newRed << 16) + (newGreen << 8) + newBlue);

            }
            bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * 4, 0);

            return bitmap;
        }
    }
}
