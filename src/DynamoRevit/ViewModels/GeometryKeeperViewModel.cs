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
            dynSettings.Controller.RunCompleted += new DynamoController.RunCompletedHandler(GeometryKeeperViewModel_RunCompleted);
        }

        public void GeometryKeeperViewModel_RunCompleted(object controller, bool success)
        {
            DisplayTransientObjects();
        }

        public void DisplayTransientObjects() 
        {
            List<IDrawable> drawables = new List<IDrawable>();
            List<GeometryObject> geometryObjects = new List<GeometryObject>();

            dynWorkspaceModel currentSpace = dynSettings.Controller.DynamoViewModel.CurrentSpace;

            foreach (dynNodeModel nodeModel in currentSpace.Nodes)
            {
                if (nodeModel.State != ElementState.ACTIVE)
                    continue;

                if (!nodeModel.IsVisible)
                    continue;

                dynGeometryBase geometryNode = nodeModel as dynGeometryBase;

                if (geometryNode == null)
                    continue;

                foreach (GeometryObject geomObject in geometryNode.GeometryObjects)
                {
                    geometryObjects.Add(geomObject);
                }

                //if the node is function then get all the 
                //drawables inside that node. only do this if the
                //node's workspace is the home space to avoid infinite
                //recursion in the case of custom nodes in custom nodes
                //if (nodeModel is dynFunction && nodeModel.WorkSpace == dynSettings.Controller.DynamoModel.HomeSpace)
                //{
                //    dynFunction func = (dynFunction)nodeModel;
                //    foreach (dynNodeModel innerNode in func.Definition.Workspace.Nodes)
                //    {
                //        if (innerNode is IDrawable)
                //        {
                //            drawables.Add(innerNode as IDrawable);
                //        }
                //    }
                //}
            }

            //if (_keeperId != ElementId.InvalidElementId)
            //{
            //    dynRevitSettings.Controller.InitTransaction();

            //    //dynRevitSettings.Doc.Document.Delete(_keeperId);

            //    dynRevitSettings.Controller.EndTransaction();
            //}

            _keeperId = GeometryElement.SetForTransientDisplay(
                dynRevitSettings.Doc.Document, ElementId.InvalidElementId,
                geometryObjects, ElementId.InvalidElementId);
        }
    }
}
