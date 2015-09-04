using Dynamo.Models;
using Dynamo.Migration;

namespace Dynamo.Nodes
{
    public class AdaptiveComponentByPoints : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "RevitNodes.dll",
                "AdaptiveComponent.ByPoints", "AdaptiveComponent.ByPoints@Point[],FamilySymbol");
        }
    }

    public class AdaptiveComponentBatchByPoints : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "RevitNodes.dll",
                "AdaptiveComponent.ByPoints", "AdaptiveComponent.ByPoints@Point[],FamilySymbol");
        }
    }

    public class AdaptiveComponentByUvsOnFace : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "RevitNodes.dll",
                "AdaptiveComponent.ByParametersOnFace",
                "AdaptiveComponent.ByParametersOnFace@double[][],Face,FamilySymbol");
        }
    }

    public class AdaptiveComponentByParametersOnCurve : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "RevitNodes.dll",
                "AdaptiveComponent.ByParametersOnCurveReference",
                "AdaptiveComponent.ByParametersOnCurveReference@double[],CurveReference,FamilySymbol");
        }
    }
}
