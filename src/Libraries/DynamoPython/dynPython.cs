using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

using Dynamo.Models;

namespace Dynamo.Nodes
{
    [NodeName("LEGACY Python Script")]
    [NodeCategory(BuiltinNodeCategories.CORE_SCRIPTING + ".Legacy")]
    [NodeDescription("Runs an embedded IronPython script")]
    public class Python : NodeModel
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            XmlElement xmlNode = data.MigratedNodes.ElementAt(0);
            var element = MigrationManager.CloneAndChangeName(xmlNode, "DSIronPythonNode.PythonNode", "Python Script");
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

    [NodeName("LEGACY Python Script With Variable Number of Inputs"),
     NodeCategory(BuiltinNodeCategories.CORE_SCRIPTING + ".Legacy"),
     NodeDescription("Runs an embedded IronPython script")]
    public class PythonVarIn : NodeModel
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            XmlElement xmlNode = data.MigratedNodes.ElementAt(0);
            var element = MigrationManager.CloneAndChangeName(
                xmlNode,
                "DSIronPythonNode.PythonNode",
                "Python Script");
            element.SetAttribute("nickname", "Python Script");
            element.SetAttribute("inputcount", xmlNode.GetAttribute("inputs"));
            element.RemoveAttribute("inputs");

            foreach (XmlElement subNode in xmlNode.ChildNodes)
            {


                XmlNode node = subNode.Clone();
                node.InnerText = Regex.Replace(
                    node.InnerText,
                    @"\bIN[0-9]+\b",
                    m => "IN[" + m.ToString().Substring(2) + "]");
                element.AppendChild(subNode.Clone());
            }

            var migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(element);
            return migrationData;
        }
    }

    [NodeName("LEGACY Python Script From String")]
    [NodeCategory(BuiltinNodeCategories.CORE_SCRIPTING + ".Legacy")]
    [NodeDescription("Runs a IronPython script from a string")]
    public class PythonString : NodeModel
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            XmlElement xmlNode = data.MigratedNodes.ElementAt(0);
            var element = MigrationManager.CloneAndChangeName(xmlNode, "DSIronPythonNode.PythonStringNode", "Python Script From String");
            element.SetAttribute("inputcount", "2");

            var migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(element);
            return migrationData;
        }
    }
}
