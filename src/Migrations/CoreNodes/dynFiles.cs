using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    public class FileReader : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSCoreNodes.dll",
                /*NXLT*/"File.ReadText", /*NXLT*/"File.ReadText@string");
        }
    }

    public class FileWriter : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSCoreNodes.dll",
                /*NXLT*/"File.WriteText", /*NXLT*/"File.WriteText@string,string");
        }
    }

    public class ListToCsv : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSCoreNodes.dll",
                /*NXLT*/"File.ExportToCSV", /*NXLT*/"File.ExportToCSV@string,double[][]");
        }
    }

    public class ImageFileWriter : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSCoreNodes.dll",
                /*NXLT*/"File.WriteImage", /*NXLT*/"File.WriteImage@string,string,var");
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
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSCoreNodes.dll",
                /*NXLT*/"File.ReadImage", /*NXLT*/"File.ReadImage@string,int,int");
        }
    }
}
