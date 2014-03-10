using System;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Dynamo.Models;
using Dynamo.Revit;
using Dynamo.Utilities;
using RevitServices.Persistence;
using System.Xml;

namespace Dynamo.Nodes
{
    [NodeName("Wall by Curve")]
    [NodeCategory(BuiltinNodeCategories.REVIT_DOCUMENT)]
    [NodeDescription("Create a wall given an arc or line, a level, a wall type, and a height.")]
    public class WallByCurve : RevitTransactionNodeWithOneOutput
    {
        public WallByCurve()
        {
            InPortData.Add(new PortData("curve", "A curve.", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("level", "A level to associate this wall with.", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("wall type", "The wall type to use for the wall.", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("height", "The height of the wall.", typeof(FScheme.Value.Number)));

            OutPortData.Add(new PortData("wall", "The wall.", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(Microsoft.FSharp.Collections.FSharpList<FScheme.Value> args)
        {
            //if we're in a family document, don't even try to add a floor
            if (DocumentManager.Instance.CurrentUIDocument.Document.IsFamilyDocument)
            {
                throw new Exception("Walls can not be created in family documents.");
            }

            var curve = (Curve)((FScheme.Value.Container)args[0]).Item;
            var level = (Autodesk.Revit.DB.Level)((FScheme.Value.Container)args[1]).Item;
            var wallType = (WallType)((FScheme.Value.Container)args[2]).Item;
            var height = ((FScheme.Value.Number)args[3]).Item;

            Wall wall = null;
            var document = DocumentManager.Instance.CurrentDBDocument;
            if (this.Elements.Any())
            {
                bool bSuccess = false;
                if (dynUtils.TryGetElement(this.Elements[0], out wall))
                {
                    if (wall.Location is LocationCurve)
                    {
                        var wallLocation = wall.Location as LocationCurve;
                        if ((wallLocation.Curve is Line == curve is Line) || (wallLocation.Curve is Arc == curve is Arc))
                        {
                            wallLocation.Curve = curve;

                            Parameter baseLevelParameter =
                                wall.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.WALL_BASE_CONSTRAINT);
                            Parameter topOffsetParameter =
                                wall.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.WALL_USER_HEIGHT_PARAM);
                            Parameter wallTypeParameter =
                                wall.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.ELEM_TYPE_PARAM);
                            if (baseLevelParameter.AsElementId() != level.Id)
                                baseLevelParameter.Set(level.Id);
                            if (Math.Abs(topOffsetParameter.AsDouble() - height) > 1.0e-10)
                                topOffsetParameter.Set(height);
                            if (wallTypeParameter.AsElementId() != wallType.Id)
                                wallTypeParameter.Set(wallType.Id);
                            bSuccess = true;
                        }
                    }
                    if (!bSuccess)
                    {
                        DocumentManager.Instance.CurrentDBDocument.Delete(wall.Id);
                    }
                    //Delete the existing floor. Revit API does not allow update of floor sketch.
                }
                if (!bSuccess)
                {
                    wall = Wall.Create(document, curve, wallType.Id, level.Id, height, 0.0, false, false);
                    this.Elements[0] = wall.Id;
                }

            }
            else
            {
                wall = Wall.Create(document, curve, wallType.Id, level.Id, height, 0.0, false, false);
                Elements.Add(wall.Id);
            }

            return FScheme.Value.NewContainer(wall);
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement dsRevitNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(dsRevitNode, "RevitNodes.dll", 
                "Wall.ByCurveAndHeight", 
                "Wall.ByCurveAndHeight@Curve,double,Level,WallType");

            migratedData.AppendNode(dsRevitNode);
            string dsRevitNodeId = MigrationManager.GetGuidFromXmlElement(dsRevitNode);

            //create and reconnect the connecters
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.INPUT);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.INPUT);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            PortId oldInPort2 = new PortId(oldNodeId, 2, PortType.INPUT);
            XmlElement connector2 = data.FindFirstConnector(oldInPort2);

            PortId oldInPort3 = new PortId(oldNodeId, 3, PortType.INPUT);
            XmlElement connector3 = data.FindFirstConnector(oldInPort3);

            PortId newInPort0 = new PortId(dsRevitNodeId, 0, PortType.INPUT);
            PortId newInPort1 = new PortId(dsRevitNodeId, 1, PortType.INPUT);
            PortId newInPort2 = new PortId(dsRevitNodeId, 2, PortType.INPUT);
            PortId newInPort3 = new PortId(dsRevitNodeId, 3, PortType.INPUT);

            data.ReconnectToPort(connector0, newInPort0);
            data.ReconnectToPort(connector1, newInPort2);
            data.ReconnectToPort(connector2, newInPort3);
            data.ReconnectToPort(connector3, newInPort1);

            return migratedData;
        }
    }

    [NodeName("Select Wall Type")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Select a wall type.")]
    public class SelectWallType : DropDrownBase
    {
        public SelectWallType()
        {
            OutPortData.Add(new PortData("wall type", "The selected wall type.", typeof(FScheme.Value.Container)));

            RegisterAllPorts();

            PopulateItems();
        }

        public override void PopulateItems()
        {
            var document = DocumentManager.Instance.CurrentUIDocument.Document;
            var wallTypesColl = new FilteredElementCollector(document);
            wallTypesColl.OfClass(typeof(WallType));

            Items.Clear();

            wallTypesColl.ToElements().ToList().ForEach(x => Items.Add(new DynamoDropDownItem(x.Name, x)));

            Items = Items.OrderBy(x => x.Name).ToObservableCollection<DynamoDropDownItem>();
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(MigrationManager.CloneAndChangeName(
                data.MigratedNodes.ElementAt(0), "DSRevitNodesUI.WallType", "Wall Type"));

            return migrationData;
        }
    }
}
