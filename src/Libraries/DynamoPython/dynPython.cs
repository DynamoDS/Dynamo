using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

using Dynamo.Models;

namespace Dynamo.Nodes
{
    [NodeName(/*NXLT*/"LEGACY Python Script")]
    [NodeCategory(BuiltinNodeCategories.CORE_SCRIPTING + /*NXLT*/".Legacy")]
    [NodeDescription(/*NXLT*/"LEGACYPythonScriptDescription", typeof(Properties.Resources))]
    public class Python : NodeModel
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            XmlElement xmlNode = data.MigratedNodes.ElementAt(0);
            var element = MigrationManager.CloneAndChangeName(xmlNode, /*NXLT*/"DSIronPythonNode.PythonNode", /*NXLT*/"Python Script");
            element.SetAttribute("nickname", "Python Script");
            element.SetAttribute("inputcount", "1");
            element.RemoveAttribute("inputs");

            foreach (XmlElement subNode in xmlNode.ChildNodes)
            {
                XmlNode node = subNode.Clone();
                node.InnerText = Regex.Replace(node.InnerText, /*NXLT*/@"\bIN\b", /*NXLT*/"IN[0]");
                element.AppendChild(node);
            }

            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(element);
            return migrationData;
        }
    }

    [NodeName("LEGACY Python Script With Variable Number of Inputs")]
    [NodeCategory(BuiltinNodeCategories.CORE_SCRIPTING + ".Legacy")]
    [NodeDescription(/*NXLT*/"LEGACYPythonScriptDescription", typeof(Properties.Resources))]
    public class PythonVarIn : NodeModel
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            XmlElement xmlNode = data.MigratedNodes.ElementAt(0);
            var element = MigrationManager.CloneAndChangeName(
                xmlNode,
                /*NXLT*/"DSIronPythonNode.PythonNode",
                /*NXLT*/"Python Script");
            element.SetAttribute("nickname", "Python Script");
            element.SetAttribute("inputcount", xmlNode.GetAttribute("inputs"));
            element.RemoveAttribute("inputs");

            foreach (XmlElement subNode in xmlNode.ChildNodes)
            {


                XmlNode node = subNode.Clone();
                node.InnerText = Regex.Replace(
                    node.InnerText,
                    /*NXLT*/@"\bIN[0-9]+\b",
                    m => "IN[" + m.ToString().Substring(2) + "]");
                element.AppendChild(subNode.Clone());
            }

            var migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(element);
            return migrationData;
        }
    }

    [NodeName(/*NXLT*/"LEGACY Python Script From String")]
    [NodeCategory(BuiltinNodeCategories.CORE_SCRIPTING + /*NXLT*/".Legacy")]
    [NodeDescription(/*NXLT*/"LEGACYPythonScriptFromStringDescription", typeof(Properties.Resources))]
    public class PythonString : NodeModel
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            XmlElement xmlNode = data.MigratedNodes.ElementAt(0);
            var element = MigrationManager.CloneAndChangeName(xmlNode, /*NXLT*/"DSIronPythonNode.PythonStringNode", /*NXLT*/"Python Script From String");
            element.SetAttribute("inputcount", "2");

            var migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(element);
            return migrationData;
        }
    }
}
