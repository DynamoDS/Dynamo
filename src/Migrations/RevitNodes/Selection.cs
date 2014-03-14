using System.Linq;
using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    public class FamilyInstanceCreatorSelection : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(MigrationManager.CloneAndChangeType(
                data.MigratedNodes.ElementAt(0), "Dynamo.Nodes.DSFamilyInstanceSelection"));

            return migrationData;
        }
    }

    public class DividedSurfaceBySelection : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(MigrationManager.CloneAndChangeType(
                data.MigratedNodes.ElementAt(0), "Dynamo.Nodes.DSDividedSurfaceFamiliesSelection"));

            return migrationData;
        }
    }

    public class FormElementBySelection : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(MigrationManager.CloneAndChangeType(
                data.MigratedNodes.ElementAt(0), "Dynamo.Nodes.DSFaceSelection"));

            return migrationData;
        }
    }

    public class EdgeOnElementBySelection : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(MigrationManager.CloneAndChangeType(
                data.MigratedNodes.ElementAt(0), "Dynamo.Nodes.DSEdgeSelection"));

            return migrationData;
        }
    }

    public class CurvesBySelection : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(MigrationManager.CloneAndChangeType(
                data.MigratedNodes.ElementAt(0), "Dynamo.Nodes.DSCurveElementSelection"));

            return migrationData;
        }
    }

    public class MultipleCurvesBySelection : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(MigrationManager.CloneAndChangeType(
                data.MigratedNodes.ElementAt(0), "Dynamo.Nodes.DSModelElementsSelection"));

            return migrationData;
        }
    }

    public class PointBySelection : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(MigrationManager.CloneAndChangeType(
                data.MigratedNodes.ElementAt(0), "Dynamo.Nodes.DSReferencePointSelection"));

            return migrationData;
        }
    }

    public class LevelBySelection : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(MigrationManager.CloneAndChangeType(
                data.MigratedNodes.ElementAt(0), "Dynamo.Nodes.DSLevelSelection"));

            return migrationData;
        }
    }

    public class ModelElementSelection : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(MigrationManager.CloneAndChangeType(
                data.MigratedNodes.ElementAt(0), "Dynamo.Nodes.DSModelElementSelection"));

            return migrationData;
        }
    }

    public class XyzBySelection : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(MigrationManager.CloneAndChangeType(
                data.MigratedNodes.ElementAt(0), "Dynamo.Nodes.DSPointOnElementSelection"));

            return migrationData;
        }
    }
    public class AllElementsOfCategory : MigrationNode
    {
    }

    public class ElementTypes : MigrationNode
    {
    }

    public class AllElementsOfType : MigrationNode
    {
    }
}