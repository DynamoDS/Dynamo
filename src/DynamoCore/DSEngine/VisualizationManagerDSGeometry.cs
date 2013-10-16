using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Models;

namespace Dynamo.DSEngine
{
    class VisualizationManagerDSGeometry: VisualizationManager
    {
        public static void DrawDesignScriptGraphicItem(NodeModel node, object geom, RenderDescription rd)
        {
            IGraphicItem graphItem = geom as IGraphicItem;
            if (graphItem == null)
            {
                return;
            }

            using (var renderPackage = new RenderPackage(rd))
            {
                graphItem.Tessellate(renderPackage);
                renderPackage.AddToRenderDescription(rd);
            }
        }
    }
}
