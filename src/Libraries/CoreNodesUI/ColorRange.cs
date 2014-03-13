using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using DSCore;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.UI;
using Dynamo.Utilities;
using ProtoCore.AST.AssociativeAST;

namespace DSCoreNodesUI
{
    [IsDesignScriptCompatible]
    [NodeName("Color Range")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_COLOR)]
    [NodeDescription("Get a color given a color range.")]
    public class ColorRange : NodeModel, IWpfNode
    {
        public event EventHandler RequestChangeColorRange;
        protected virtual void OnRequestChangeColorRange(object sender, EventArgs e)
        {
            if (RequestChangeColorRange != null)
                RequestChangeColorRange(sender, e);
        }

        public ColorRange()
        {
            InPortData.Add(new PortData("start", "The start color.", typeof(object)));
            InPortData.Add(new PortData("end", "The end color.", typeof(object)));
            InPortData.Add(new PortData("value", "The value between 0 and 1 of the selected color.", typeof(object)));
            OutPortData.Add(new PortData("color", "The selected color.", typeof(object)));

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
            var functionCall = AstFactory.BuildFunctionCall("DSColor", "BuildColorFromRange", inputAstNodes);

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

                    var startMirror = dynSettings.Controller.EngineController.GetMirror(startId);
                    var endMirror = dynSettings.Controller.EngineController.GetMirror(endId);

                    DSColor start = null;
                    DSColor end = null;

                    if (startMirror.GetData().IsCollection)
                    {
                        start = (DSColor) startMirror.GetData().GetElements().Select(x => x.Data).FirstOrDefault();
                    }
                    else
                    {
                        start = (DSColor)startMirror.GetData().Data;
                    }

                    if (endMirror.GetData().IsCollection)
                    {
                        end = (DSColor) endMirror.GetData().GetElements().Select(x => x.Data).FirstOrDefault();
                    }
                    else
                    {
                        end = (DSColor)endMirror.GetData().Data;
                    }
                    
                    WriteableBitmap bmp = CompleteColorScale(start, end);
                    drawPlane.Source = bmp;
                });
            };
        }

        //http://gaggerostechnicalnotes.blogspot.com/2012/01/wpf-colors-scale.html
        private WriteableBitmap CompleteColorScale(DSColor start, DSColor end)
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
