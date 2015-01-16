using System.Linq;
using System.Xml;
using Dynamo.Models;

namespace Migrations
{
    public class MigrationNode
    {
        protected static NodeMigrationData MigrateToDsFunction(
            NodeMigrationData data, string nickname, string funcName)
        {
            return MigrateToDsFunction(data, "", nickname, funcName);
        }

        protected static NodeMigrationData MigrateToDsFunction(
            NodeMigrationData data, string assembly, string nickname, string funcName)
        {
            XmlElement xmlNode = data.MigratedNodes.ElementAt(0);
            var element = MigrationManager.CreateFunctionNodeFrom(xmlNode);
            element.SetAttribute(/*NXLT*/"assembly", assembly);
            element.SetAttribute(/*NXLT*/"nickname", nickname);
            element.SetAttribute(/*NXLT*/"function", funcName);

            var lacingAttribute = xmlNode.Attributes[/*NXLT*/"lacing"];
            if (lacingAttribute != null)
            {
                element.SetAttribute(/*NXLT*/"lacing", lacingAttribute.Value);
            }

            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(element);
            return migrationData;
        }

        protected static NodeMigrationData MigrateToDsVarArgFunction(
            NodeMigrationData data, string assembly, string nickname, string funcName)
        {
            XmlElement xmlNode = data.MigratedNodes.ElementAt(0);
            var element = MigrationManager.CreateVarArgFunctionNodeFrom(xmlNode);
            element.SetAttribute(/*NXLT*/"assembly", assembly);
            element.SetAttribute(/*NXLT*/"nickname", nickname);
            element.SetAttribute(/*NXLT*/"function", funcName);
            element.SetAttribute(/*NXLT*/"lacing", xmlNode.Attributes[/*NXLT*/"lacing"].Value);

            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(element);
            return migrationData;
        }
    }
}
