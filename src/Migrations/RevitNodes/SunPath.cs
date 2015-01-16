using System;
using System.Linq;
using System.Xml;

using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    public class SunPathDirection : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            var migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            // Create nodes

            XmlElement sunPathNode = MigrationManager.CloneAndChangeName(
                oldNode, /*NXLT*/"DSRevitNodesUI.SunPathDirection", "SunPath Direction");
            sunPathNode.SetAttribute(/*NXLT*/"guid", Guid.NewGuid().ToString());
            sunPathNode.SetAttribute(/*NXLT*/"x", (Convert.ToDouble(oldNode.GetAttribute(/*NXLT*/"x")) - 230).ToString());

            var vectorAsPoint = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(vectorAsPoint,/*NXLT*/"ProtoGeometry.dll",
                /*NXLT*/"Vector.AsPoint", /*NXLT*/"Vector.AsPoint");
            
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
                /*NXLT*/"DSRevitNodesUI.SunSettings",
                /*NXLT*/"SunSettings.Current");
            var sunSettingsNodeId = Guid.NewGuid().ToString();
            sunSettingNode.SetAttribute(/*NXLT*/"guid", sunSettingsNodeId);
            migrationData.AppendNode(sunSettingNode);

            var sunPathNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(
                sunPathNode,
               /*NXLT*/"RevitNodes.dll",
                /*NXLT*/"SunSettings.SunDirection",
                /*NXLT*/"SunSettings.SunDirection@var");
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
        [NodeMigration(from: /*NXLT*/"0.7.0.0", to: /*NXLT*/"0.7.3.0")]
        public static NodeMigrationData Migrate_0700_to_0730(NodeMigrationData data)
        {
            return Dynamo.Nodes.SunPathDirection.Migrate_0700_to_0730(data);
        }
    }
}
