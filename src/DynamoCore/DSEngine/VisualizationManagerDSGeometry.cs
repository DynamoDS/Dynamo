using Autodesk.DesignScript.Interfaces;
using Dynamo.Models;
using Dynamo.Selection;

namespace Dynamo.DSEngine
{
    class VisualizationManagerDSGeometry: VisualizationManager
    {
        public static void DrawDesignScriptGraphicItem(NodeModel node, object geom, string tag, RenderDescription rd, Octree.OctreeSearch.Octree octree)
        {
            IGraphicItem graphItem = geom as IGraphicItem;
            if (graphItem == null)
            {
                return;
            }

            bool selected = DynamoSelection.Instance.Selection.Contains(node);
            using (var renderPackage = new RenderPackage(selected))
            {
                graphItem.Tessellate(renderPackage);
                renderPackage.AddToRenderDescription(node, rd, octree);
            }
        }
    }
}
