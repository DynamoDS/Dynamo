using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    public class Simplex1D: MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "SimplexNoise.dll", "Noise.Generate", "Noise.Generate@double");
        }
    }

    public class Simplex2D: MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "SimplexNoise.dll", "Noise.Generate", "Noise.Generate@double,double");
        }
    }

    public class Simplex3D: MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "SimplexNoise.dll", "Noise.Generate", "Noise.Generate@double,double,double");
        }
    }
}
