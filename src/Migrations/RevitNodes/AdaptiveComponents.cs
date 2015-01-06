using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    public class AdaptiveComponentByPoints : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"RevitNodes.dll",
                /*NXLT*/"AdaptiveComponent.ByPoints", /*NXLT*/"AdaptiveComponent.ByPoints@Point[],FamilySymbol");
        }
    }

    public class AdaptiveComponentBatchByPoints : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data,/*NXLT*/"RevitNodes.dll",
                /*NXLT*/"AdaptiveComponent.ByPoints", /*NXLT*/"AdaptiveComponent.ByPoints@Point[],FamilySymbol");
        }
    }

    public class AdaptiveComponentByUvsOnFace : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"RevitNodes.dll",
                /*NXLT*/"AdaptiveComponent.ByParametersOnFace",
                /*NXLT*/"AdaptiveComponent.ByParametersOnFace@double[][],Face,FamilySymbol");
        }
    }

    public class AdaptiveComponentByParametersOnCurve : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"RevitNodes.dll",
                /*NXLT*/"AdaptiveComponent.ByParametersOnCurveReference",
                /*NXLT*/"AdaptiveComponent.ByParametersOnCurveReference@double[],CurveReference,FamilySymbol");
        }
    }
}
