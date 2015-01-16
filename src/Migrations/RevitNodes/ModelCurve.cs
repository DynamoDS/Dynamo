using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    public class ModelCurveNurbSpline : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement dummyNode = MigrationManager.CreateDummyNode(oldNode, 1, 1);
            migrationData.AppendNode(dummyNode);

            return migrationData;
        }
    }

    public class ModelCurve : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data,/*NXLT*/"RevitNodes.dll",
                /*NXLT*/"ModelCurve.ByCurve", /*NXLT*/"ModelCurve.ByCurve@Curve");
        }
    }

    public class ModelCurveFromCurveLoop : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            var migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            var newNode = MigrationManager.CreateCustomNodeFrom(oldNode.OwnerDocument, oldNode,
                /*NXLT*/"cc5f75c8-09a6-409c-b61d-2bf4b1b4091d",
                "ModelCurveFromCurveLoop",
                "This node represents an upgrade of the 0.6.3 ModelCurveFromCurveLoop node to 0.7.x",
                new List<string>() { /*NXLT*/"polyCurve" },
                new List<string>() { /*NXLT*/"modelCurve" });

            migratedData.AppendNode(newNode);
            return migratedData;
        }
    }
}
