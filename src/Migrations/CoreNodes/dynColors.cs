using System.Xml;
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
            return MigrateToDsFunction(data, "DSCoreNodes.dll", "Color.ByARGB",
                "Color.ByARGB@int,int,int,int");
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
        [NodeMigration("0.6.2.0","0.6.3.0")]
        public void UpdateLacability(XmlNode node)
        {
            //if the laceability has been set on this node to disabled, then set it to longest
            if (node.Attributes["lacing"].Value == "Disabled")
            {
                node.Attributes["lacing"].Value = "Longest";
            }
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll", "Color.ColorRange",
                "Color.ColorRange@DSColor,DSColor,double");
        }
    }
}
