using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    public class NewtonRootFind1DNoDeriv : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "", "NewtonRootFind1DNoDeriv",
                "NewtonRootFind1DNoDeriv@_FunctionObject,double,int");
        }
    }

    public class NewtonRootFind1DWithDeriv : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "", "NewtonRootFind1DWithDeriv",
                "NewtonRootFind1DWithDeriv@_FunctionObject,double,int");
        }
    }
}


