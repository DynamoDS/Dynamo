﻿using Dynamo.Migration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace Dynamo.Nodes
{
    public class Python : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            System.Xml.XmlElement xmlNode = data.MigratedNodes.ElementAt(0);
            var element = MigrationManager.CloneAndChangeName(xmlNode, "PythonNodeModels.PythonNode", "Python Script");
            element.SetAttribute("nickname", "Python Script");
            element.SetAttribute("inputcount", "1");
            element.RemoveAttribute("inputs");

            foreach (XmlElement subNode in xmlNode.ChildNodes)
            {
                XmlNode node = subNode.Clone();
                node.InnerText = Regex.Replace(node.InnerText, @"\bIN\b", "IN[0]");
                element.AppendChild(node);
            }

            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(element);
            return migrationData;
        }
    }

    public class PythonVarIn : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            System.Xml.XmlElement xmlNode = data.MigratedNodes.ElementAt(0);
            var element = MigrationManager.CloneAndChangeName(xmlNode, "PythonNodeModels.PythonNode", "Python Script");
            element.SetAttribute("nickname", "Python Script");
            element.SetAttribute("inputcount", xmlNode.GetAttribute("inputs"));
            element.RemoveAttribute("inputs");

            foreach (XmlElement subNode in xmlNode.ChildNodes)
            {
                XmlNode node = subNode.Clone();
                node.InnerText = Regex.Replace(node.InnerText, @"\bIN[0-9]+\b", delegate(Match m)
                {
                    return "IN[" + m.ToString().Substring(2) + "]";
                });
                element.AppendChild(node.Clone());
            }

            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(element);
            return migrationData;
        }
    }

    public class PythonString : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            System.Xml.XmlElement xmlNode = data.MigratedNodes.ElementAt(0);
            var element = MigrationManager.CloneAndChangeName(xmlNode, "PythonNodeModels.PythonStringNode", "Python Script From String");
            element.SetAttribute("inputcount", "2");

            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(element);
            return migrationData;
        }
    }
}

namespace DSIronPythonNode
{
    public class PythonNode : MigrationNode
    {
        [NodeMigration(from: "0.8.3.0", to: "0.9.0.0")]
        public static NodeMigrationData Migrate_0830_to_0900(NodeMigrationData data)
        {
            System.Xml.XmlElement xmlNode = data.MigratedNodes.ElementAt(0);
            var element = MigrationManager.CloneAndChangeName(xmlNode, "PythonNodeModels.PythonNode", "Python Script", true);

            var migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(element);
            return migrationData;
        }
    }

    public class PythonStringNode : MigrationNode
    {
        [NodeMigration(from: "0.8.3.0", to: "0.9.0.0")]
        public static NodeMigrationData Migrate_0830_to_0900(NodeMigrationData data)
        {
            System.Xml.XmlElement xmlNode = data.MigratedNodes.ElementAt(0);
            var element = MigrationManager.CloneAndChangeName(xmlNode, "PythonNodeModels.PythonStringNode", "Python Script From String");

            var migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(element);
            return migrationData;
        }
    }
}