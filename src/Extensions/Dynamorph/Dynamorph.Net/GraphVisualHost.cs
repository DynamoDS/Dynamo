using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Dynamorph
{
    class GraphVisualHost : FrameworkElement
    {
        private VisualCollection childVisuals = null;
        private DrawingVisual nodeVisuals = new DrawingVisual();
        private DrawingVisual edgeVisuals = new DrawingVisual();

        internal GraphVisualHost()
        {
            childVisuals = new VisualCollection(this);
            childVisuals.Add(edgeVisuals);
            childVisuals.Add(nodeVisuals);
        }

        internal void RefreshGraph(SynthesizedGraph graph)
        {
        }
    }
}
