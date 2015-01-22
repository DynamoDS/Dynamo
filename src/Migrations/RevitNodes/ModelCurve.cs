using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    public class ModelCurveNurbSpline : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
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
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "RevitNodes.dll", 
                "ModelCurve.ByCurve", "ModelCurve.ByCurve@Curve");
        }
    }

    public class ModelCurveFromCurveLoop : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            var migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            var newNode = MigrationManager.CreateCustomNodeFrom(oldNode.OwnerDocument, oldNode,
                "cc5f75c8-09a6-409c-b61d-2bf4b1b4091d",
                "ModelCurveFromCurveLoop",
                "This node represents an upgrade of the 0.6.3 ModelCurveFromCurveLoop node to 0.7.x",
                new List<string>() { "polyCurve" },
                new List<string>() { "modelCurve" });

            migratedData.AppendNode(newNode);
            return migratedData;
        }
    }
}
