﻿using System.Linq;
using System.Xml;
using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    public class FamilyTypeSelector : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            var migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(MigrationManager.CloneAndChangeName(
                data.MigratedNodes.ElementAt(0), /*NXLT*/"DSRevitNodesUI.FamilyTypes", "Family Types"));

            return migrationData;
        }
    }

    public class FamilyTypeParameterSetter : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data,/*NXLT*/"RevitNodes.dll", /*NXLT*/"Element.SetParameterByName", /*NXLT*/"Element.SetParameterByName@string,object");
        }
    }

    public class FamilyTypeParameterGetter : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return FamilyInstanceParameterGetter.Migrate_0630_to_0700(data);
        }
    }

    public class GetFamilyInstancesByType : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            var migrationData = new NodeMigrationData(data.Document);

            var oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CloneAndChangeName(
                oldNode, /*NXLT*/"DSRevitNodesUI.ElementsOfFamilyType", "All Elements of Family Type");
            migrationData.AppendNode(newNode);

            foreach (XmlElement subNode in oldNode.ChildNodes)
                newNode.AppendChild(subNode.Clone());

            return migrationData;
        }
    }
}
