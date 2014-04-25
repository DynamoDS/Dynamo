using System.Xml;
using System.Linq;
using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    class ColorBrightness : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll", "Color.Brightness", "Color.Brightness@DSColor");
        }
    }

    class ColorSaturation : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll", "Color.Saturation", "Color.Saturation@DSColor");
        }
    }

    class Color : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "DSCoreNodes.dll",
                "Color.ByARGB", "Color.ByARGB@int,int,int,int");

            migrationData.AppendNode(newNode);

            // Add default values
            foreach (XmlNode child in oldNode.ChildNodes)
                newNode.AppendChild(child.Clone());

            return migrationData;
        }
    }

    class ColorComponents : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll", "Color.Components",
                "Color.Components@DSColor");
        }
    }

    class ColorHue : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll", "Color.Hue", "Color.Hue@DSColor");
        }
    }

    class ColorRange : MigrationNode
    {
        [NodeMigration(from: "0.6.2.0", to: "0.6.3.0")]
        public static NodeMigrationData Migrate_0620_to_0630(NodeMigrationData data)
        {
            var node = data.MigratedNodes.ElementAt(0);

            //if the laceability has been set on this node to disabled, then set it to longest
            if (node.Attributes["lacing"].Value == "Disabled")
                node.Attributes["lacing"].Value = "Longest";

            var migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(node);
            return migrationData;
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            var migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(MigrationManager.CloneAndChangeName(
                data.MigratedNodes.ElementAt(0), "DSCoreNodesUI.ColorRange", "Color Range"));

            return migrationData;
        }


    }
}
