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
                "FileReader.ReadText", "FileReader.ReadText@string");
        }
    }

    public class FileWriter : MigrationNode
    {
    }

    public class ListToCsv : MigrationNode
    {
    }

    public class ImageFileWriter : MigrationNode
    {
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
                "FileReader.ReadImage", "FileReader.ReadImage@string,int,int");
        }
    }
}
