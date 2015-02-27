using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    public class FileReader : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll",
                "File.ReadText", "File.ReadText@string");
        }
    }

    public class FileWriter : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll",
                "File.WriteText", "File.WriteText@string,string");
        }
    }

    public class ListToCsv : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll",
                "File.ExportToCSV", "File.ExportToCSV@string,double[][]");
        }
    }

    public class ImageFileWriter : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll",
                "File.WriteImage", "File.WriteImage@string,string,var");
        }
    }

    public class FileWatcher : MigrationNode
    {
    }

    public class FileWatcherChanged : MigrationNode
    {
    }

    public class FileWatcherWait : MigrationNode
    {
    }

    public class FileWatcherReset : MigrationNode
    {
    }

    public class ImageFileReader : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSCoreNodes.dll",
                "File.ReadImage", "File.ReadImage@string,int,int");
        }
    }
}
