using Dynamo.Migration;
using System.Linq;

namespace DSIronPythonNode
{
    public class PythonNode : MigrationNode
    {
        [NodeMigration(version: "0.8.3.0")]
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
        [NodeMigration(version: "0.8.3.0")]
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