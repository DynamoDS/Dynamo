﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using DSCore;
using DSCoreNodesUI.Properties;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.Mirror;
using System.Globalization;

using Dynamo.Engine;
using Dynamo.Graph.Nodes;

namespace DSCoreNodesUI
{
    [IsDesignScriptCompatible]
    [NodeName("Color Range")]
    [NodeCategory("Core.Color.Create")]
    [NodeDescription("ColorRangeDescription",typeof(DSCoreNodesUI.Properties.Resources))]
    [NodeSearchTags("ColorRangeSearchTags", typeof(DSCoreNodesUI.Properties.Resources))]

    [InPortNames("colors", "indices", "value")]
    [InPortTypes("Color[]", "double[]", "double")]
    [InPortDescriptions(typeof(Resources),
        "ColorRangePortDataColorsToolTip",
        "ColorRangePortDataIndicesToolTip",
        "ColorRangePortDataValueToolTip")]
    [OutPortNames("color")]
    [OutPortTypes("Color")]
    [OutPortDescriptions(typeof(Resources),
        "ColorRangePortDataResultToolTip")]

    public class ColorRange : NodeModel
    {
        public event Action RequestChangeColorRange;
        protected virtual void OnRequestChangeColorRange()
        {
            if (RequestChangeColorRange != null)
                RequestChangeColorRange();
        }

        public ColorRange()
        {
            RegisterAllPorts();

            this.PropertyChanged += ColorRange_PropertyChanged;
            foreach (var port in InPorts)
            {
                port.Connectors.CollectionChanged += Connectors_CollectionChanged;
            }

            ShouldDisplayPreviewCore = false;
        }

        void Connectors_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnRequestChangeColorRange();
        }

        void ColorRange_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "CachedValue")
                return;

            if (InPorts.Any(x => x.Connectors.Count == 0))
                return;

            OnRequestChangeColorRange();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            if (!HasConnectedInput(0) && !HasConnectedInput(1) && !HasConnectedInput(2))
            {
                return new[] {AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode())};
            }
            AssociativeNode buildColorRangeNode = null;

            // If either of the first two inputs does not have a connector
            // then build a default color range.
            if (!HasConnectedInput(0) || !HasConnectedInput(1))
            {
                buildColorRangeNode =
                    AstFactory.BuildFunctionCall(
                        new Func<ColorRange1D>(ColorRange1D.Default),
                        new List<AssociativeNode>());
            }
            else
            {
                buildColorRangeNode =
                AstFactory.BuildFunctionCall(
                    new Func<List<Color>, List<double>, ColorRange1D>(ColorRange1D.ByColorsAndParameters),
                    new List<AssociativeNode>() { inputAstNodes[0], inputAstNodes[1] });
            }

            // The last inputAstNode is assumed to be the value.
            var functionCall =
                AstFactory.BuildFunctionCall(
                    new Func<ColorRange1D,double, Color>(ColorRange1D.GetColorAtParameter),
                    new List<AssociativeNode>(){buildColorRangeNode, inputAstNodes.Last()});

            return new[]
            {
                AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall),
            };
        }

        public ColorRange1D ComputeColorRange(EngineController engine)
        {
            List<Color> colors;
            List<double> parameters;

            // If there are colors supplied
            if (HasConnectedInput(0))
            {
                var colorsNode = InPorts[0].Connectors[0].Start.Owner;
                var colorsIndex = InPorts[0].Connectors[0].Start.Index;
                var startId = colorsNode.GetAstIdentifierForOutputIndex(colorsIndex).Name;
                var colorsMirror = engine.GetMirror(startId);
                colors = GetColorsFromMirrorData(colorsMirror);
            }
            else
            {
                colors = new List<Color>();
                colors.AddRange(DefaultColorRanges.Analysis);
            }

            // If there are indices supplied
            if (HasConnectedInput(1))
            {
                var valuesNode = InPorts[1].Connectors[0].Start.Owner;
                var valuesIndex = InPorts[1].Connectors[0].Start.Index;
                var endId = valuesNode.GetAstIdentifierForOutputIndex(valuesIndex).Name;
                var valuesMirror = engine.GetMirror(endId);
                parameters = GetValuesFromMirrorData(valuesMirror);
            }
            else
            {
                parameters = CreateParametersForColors(colors);
            }

            return ColorRange1D.ByColorsAndParameters(colors, parameters);
        }

        private static List<double> CreateParametersForColors(List<Color> colors)
        {
            var parameters = new List<double>();

            var step = 1.0 / (colors.Count() - 1);
            for (var i = 0; i < colors.Count(); i++)
            {
                parameters.Add(i * step);
            }

            return parameters;
        }

        private static List<double> GetValuesFromMirrorData(RuntimeMirror valuesMirror)
        {
            var values = new List<double>();

            if (valuesMirror == null || valuesMirror.GetData() == null) return values;

            var data = valuesMirror.GetData();
            if (data.IsCollection)
            {
                var elements = data.GetElements().Select(e => e.Data);
                foreach (var element in elements)
                {
                    double parsed;
                    if (TryConvertToDouble(element, out parsed))
                        values.Add(parsed);
                }
            }
            else
            {
                double parsed;
                if (TryConvertToDouble(data.Data, out parsed))
                    values.Add(parsed);
            }
            return values;
        }

        private static List<Color> GetColorsFromMirrorData(RuntimeMirror colorsMirror)
        {
            var colors = new List<Color>();

            if (colorsMirror == null || colorsMirror.GetData() == null) return colors;

            var data = colorsMirror.GetData();
            if (data != null)
            {
                if (data.IsCollection)
                {
                    colors.AddRange(data.GetElements().Select(e => e.Data).OfType<Color>());
                }
                else
                {
                    var color = data.Data as Color;
                    if (color != null)
                        colors.Add(color);
                }
            }

            return colors;
        }

        private static bool TryConvertToDouble(object value, out double parsed)
        {
            parsed = default(double);

            try
            {
                parsed = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
