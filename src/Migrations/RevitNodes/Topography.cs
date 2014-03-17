using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    class TopographyFromPoints:MigrationNode
    {
        [NodeMigration(from:"0.6.3.0")]
        public static NodeMigrationData Migrate(NodeMigrationData data)
        {
            //Migrate the topography node from 0.6.3 to 0.7.0
            //No connectors need to be altered as the new version of this node
            //will have the same number of input and outputs connectors.

            //Get the executing assembly location to build the assembly path
            //for the node library. This will need work as we might be loading
            //the assemblies from a package. How do we convey this?
            var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var assemblyPath = Path.Combine(assemblyDir, "RevitNodes.dll");

            var oldNodeElement = data.MigratedNodes.ElementAt(0);
            var newNodeElement = MigrationManager.CreateFunctionNodeFrom(oldNodeElement);
            MigrationManager.SetFunctionSignature(newNodeElement, assemblyPath,
                "Topography.ByPoints", "Topography.ByPoints@Autodesk.DesignScript.Geometry.Point[]..[]");

            NodeMigrationData migrated = new NodeMigrationData(data.Document);
            migrated.AppendNode(newNodeElement);
            return migrated;
        }
    }

    class PointsFromTopography : MigrationNode
    {
        [NodeMigration(from:"0.7.0")]
        public static XmlElement Migrate(XmlElement element)
        {
            //DSRevitNodes.Topography.Points
            return element;
        }
    }


}
