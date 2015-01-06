using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    public class NewtonRootFind1DNoDeriv : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "", /*NXLT*/"NewtonRootFind1DNoDeriv",
                /*NXLT*/"NewtonRootFind1DNoDeriv@_FunctionObject,double,int");
        }
    }

    public class NewtonRootFind1DWithDeriv : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "", /*NXLT*/"NewtonRootFind1DWithDeriv",
                /*NXLT*/"NewtonRootFind1DWithDeriv@_FunctionObject,double,int");
        }
    }
}


