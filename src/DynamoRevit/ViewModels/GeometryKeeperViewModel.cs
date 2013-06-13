using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Dynamo.Connectors;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.Selection;

using Dynamo.Revit;

using Autodesk.Revit.DB;

namespace Dynamo.Controls
{
    public class GeometryKeeperViewModel : dynViewModelBase
    {
        private ElementId _keeperId = ElementId.InvalidElementId;

        public GeometryKeeperViewModel()
        {
        }

        public void GeometryKeeperViewModel_RunCompleted(object controller, bool success)
        {
            DisplayTransientObjects();
        }

        public void DisplayTransientObjects() 
        {
            List<GeometryObject> geometryObjects = new List<GeometryObject>();

            dynWorkspaceModel currentSpace = dynSettings.Controller.DynamoViewModel.CurrentSpace;

            foreach (dynNodeModel nodeModel in currentSpace.Nodes)
            {
                if (nodeModel.State != ElementState.ACTIVE)
                    continue;

                if (!nodeModel.IsVisible)
                    continue;

                dynGeometryBase geometryNode = nodeModel as dynGeometryBase;

                if (geometryNode != null)
                {
                    dynCurveBase curveNode = geometryNode as dynCurveBase;
                    dynSolidBase solidNode = geometryNode as dynSolidBase;
                    dynXYZBase xyzBase = nodeModel as dynXYZBase;

                    if (curveNode != null)
                        geometryObjects.AddRange(curveNode.crvs);
                    else if (solidNode != null)
                        geometryObjects.AddRange(solidNode.solids);
                    else if (xyzBase != null)
                        geometryObjects.AddRange(PointListFromXYZList(xyzBase.pts));

                    continue;
                }
            }

            dynRevitSettings.Controller.InitTransaction();

            if (_keeperId != ElementId.InvalidElementId)
                dynRevitSettings.Doc.Document.Delete(_keeperId);

            _keeperId = GeometryElement.SetForTransientDisplay(
                dynRevitSettings.Doc.Document, ElementId.InvalidElementId,
                geometryObjects, ElementId.InvalidElementId);

            dynRevitSettings.Controller.EndTransaction();
        }

        private List<Point> PointListFromXYZList(List<XYZ> xyzs)
        {
            List<Point> points = new List<Point>();

            foreach (XYZ xyz in xyzs)
            {
                points.Add(Point.CreatePoint(xyz.X, xyz.Y, xyz.Z));
            }

            return points;
        }
    }
}
