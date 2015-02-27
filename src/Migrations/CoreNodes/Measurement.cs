using System.Linq;
using System.Xml;
using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    public class LengthInput : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            var migrationData = new NodeMigrationData(data.Document);

            var oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CloneAndChangeName(
                oldNode, "UnitsUI.LengthFromString", "Length From String");
            migrationData.AppendNode(newNode);

            foreach (XmlElement subNode in oldNode.ChildNodes)
                newNode.AppendChild(subNode);

            return migrationData;
        } 
    }

    public class AreaInput : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            var migrationData = new NodeMigrationData(data.Document);

            var oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CloneAndChangeName(
                oldNode, "UnitsUI.AreaFromString","Area From String");
            migrationData.AppendNode(newNode);

            foreach (XmlElement subNode in oldNode.ChildNodes)
                newNode.AppendChild(subNode);

            return migrationData;
        } 
    }

    public class VolumeInput : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            var migrationData = new NodeMigrationData(data.Document);

            var oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CloneAndChangeName(
                oldNode, "UnitsUI.VolumeFromString", "Volume From String");
            migrationData.AppendNode(newNode);

            foreach (XmlElement subNode in oldNode.ChildNodes)
                newNode.AppendChild(subNode);

            return migrationData;
        }
    }

    public class LengthFromNumber : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DynamoUnits.dll", "Length",
                "Length.FromDouble@double");
        }
    }

    public class AreaFromNumber : MigrationNode
    {

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DynamoUnits.dll", "Area",
                "Area.FromDouble@double");
        }
    }

    public class VolumeFromNumber : MigrationNode
    {

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DynamoUnits.dll", "Volume",
                "Volume.FromDouble@double");
        }
    }
}

