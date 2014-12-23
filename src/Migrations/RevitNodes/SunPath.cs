using System;
using System.Linq;
using System.Xml;

using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    public class SunPathDirection : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            var migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            // Create nodes

            XmlElement sunPathNode = MigrationManager.CloneAndChangeName(
                oldNode, "DSRevitNodesUI.SunPathDirection", "SunPath Direction");
            sunPathNode.SetAttribute("guid", Guid.NewGuid().ToString());
            sunPathNode.SetAttribute("x", (Convert.ToDouble(oldNode.GetAttribute("x")) - 230).ToString());

            var vectorAsPoint = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(vectorAsPoint, "ProtoGeometry.dll",
                "Vector.AsPoint", "Vector.AsPoint");
            
            migrationData.AppendNode(sunPathNode);
            migrationData.AppendNode(vectorAsPoint);

            // Update connectors
            migrationData.CreateConnector(sunPathNode, 0, vectorAsPoint, 0);

            return migrationData;
        }

        [NodeMigration(from: "0.7.0.0", to: "0.7.3.0")]
        public static NodeMigrationData Migrate_0700_to_0730(NodeMigrationData data)
        {
            var migrationData = new NodeMigrationData(data.Document);
            var oldNode = data.MigratedNodes.ElementAt(0);

            var sunSettingNode = MigrationManager.CloneAndChangeName(
                oldNode,
                "DSRevitNodesUI.SunSettings",
                "SunSettings.Current");
            var sunSettingsNodeId = Guid.NewGuid().ToString();
            sunSettingNode.SetAttribute("guid", sunSettingsNodeId);
            migrationData.AppendNode(sunSettingNode);

            var sunPathNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(
                sunPathNode,
                "RevitNodes.dll",
                "SunSettings.SunDirection",
                "SunSettings.SunDirection@var");
            migrationData.AppendNode(sunPathNode);

            migrationData.CreateConnector(sunSettingNode, 0, sunPathNode, 0);

            var oldConnector = data.FindFirstConnector(
                new PortId(MigrationManager.GetGuidFromXmlElement(oldNode), 0, PortType.Output));

            if (oldConnector != null)
            {
                migrationData.ReconnectToPort(
                    oldConnector,
                    new PortId(
                        MigrationManager.GetGuidFromXmlElement(sunPathNode),
                        0,
                        PortType.Output));
            }

            return migrationData;
        }
    }
}

namespace DSRevitNodesUI
{
    public class SunPathDirection : MigrationNode
    {
        [NodeMigration(from: "0.7.0.0", to: "0.7.3.0")]
        public static NodeMigrationData Migrate_0700_to_0730(NodeMigrationData data)
        {
            return Dynamo.Nodes.SunPathDirection.Migrate_0700_to_0730(data);
        }
    }
}
