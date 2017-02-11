using System.Linq;
using System.Xml;
using Dynamo.Graph.Nodes;
using Dynamo.Migration;

namespace Dynamo.Nodes
{
    [AlsoKnownAs("DSCore.SortByKey")]
    public class SortByKey : MigrationNode
    {
        [NodeMigration(version: "0.8.3.0")]
        public static NodeMigrationData Migrate_0830_to_0900(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            XmlElement newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            migrationData.AppendNode(newNode);

            MigrationManager.SetFunctionSignature(newNode, "DSCoreNodes.dll",
                    "List.SortByKey", "List.SortByKey@IList,IList");

            return migrationData;
        }

        [NodeMigration(version: "0.9.0.0")]
        public static NodeMigrationData Migrate_0900_to_0910(NodeMigrationData data)
        {
            return Migrate_0830_to_0900(data);
        }
    }

    [AlsoKnownAs("DSCore.GroupByKey")]
    public class GroupByKey : MigrationNode
    {
        [NodeMigration(version: "0.8.3.0")]
        public static NodeMigrationData Migrate_0830_to_0900(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            XmlElement newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            migrationData.AppendNode(newNode);

            MigrationManager.SetFunctionSignature(newNode, "DSCoreNodes.dll",
                    "List.GroupByKey", "List.GroupByKey@IList,IList");

            return migrationData;
        }

        [NodeMigration(version: "0.9.0.0")]
        public static NodeMigrationData Migrate_0900_to_0910(NodeMigrationData data)
        {
            return Migrate_0830_to_0900(data);
        }
    }

    [AlsoKnownAs("Dynamo.Nodes.dynBuildSeq", "Dynamo.Nodes.BuildSeq", "DSCoreNodesUI.NumberRange")]
    public class NumberRange : MigrationNode
    {
        [NodeMigration(version: "0.8.2.0")]
        public static NodeMigrationData Migrate_0820_to_0830(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            XmlElement newNode = MigrationManager.CloneAndChangeName(
                oldNode, "CoreNodeModels.Range", "Range");

            migrationData.AppendNode(newNode);
            return migrationData;
        }
    }

    [AlsoKnownAs("DSCoreNodesUI.NumberSeq")]
    public class NumberSeq : MigrationNode
    {
        [NodeMigration(version: "0.8.2.0")]
        public static NodeMigrationData Migrate_0820_to_0830(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            XmlElement newNode = MigrationManager.CloneAndChangeName(
                oldNode, "CoreNodeModels.Sequence", "Sequence");

            migrationData.AppendNode(newNode);
            return migrationData;
        }
    }
}