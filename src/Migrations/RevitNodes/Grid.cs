﻿using Dynamo.Models;
using Dynamo.Migration;

namespace Dynamo.Nodes
{
    public class ColumnGrid : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "RevitNodes.dll",
                "Grid.ByLine", "Grid.ByLine@Line");
        }
    }

}