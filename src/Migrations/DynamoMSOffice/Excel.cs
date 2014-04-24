using Migrations;
using Dynamo.Models;
using System.Linq;
using System.Xml;

namespace Dynamo.Nodes
{
    public class NewExcelWorkbook : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSOffice.dll", "Excel.NewExcelWorkbook",
                "Excel.NewExcelWorkbook");
        }
    }

    public class ReadExcelFile : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSOffice.dll", "Excel.ReadExcelFile",
                "Excel.ReadExcelFile@string");
        }
    }

    public class GetWorksheetsFromExcelWorkbook : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSOffice.dll", "Excel.GetWorksheetsFromExcelWorkbook",
                "Excel.GetWorksheetsFromExcelWorkbook@var");
        }
    }

    public class GetExcelWorksheetByName : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSOffice.dll", "Excel.GetExcelWorksheetByName",
                "Excel.GetExcelWorksheetByName@var,string");
        }
    }

    public class GetDataFromExcelWorksheet : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSOffice.dll", "Excel.GetDataFromExcelWorksheet",
                "Excel.GetDataFromExcelWorksheet@var");
        }
    }

    public class WriteDataToExcelWorksheet : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "DSOffice.dll",
                "Excel.WriteDataToExcelWorksheet",
                "Excel.WriteDataToExcelWorksheet@var,int,int,var[][]");

            migrationData.AppendNode(newNode);

            // Add default values
            foreach (XmlNode child in oldNode.ChildNodes)
                newNode.AppendChild(child.Clone());

            return migrationData;
        }
    }

    public class AddExcelWorksheetToWorkbook : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSOffice.dll", "Excel.AddExcelWorksheetToWorkbook",
                "Excel.AddExcelWorksheetToWorkbook@var,string");
        }
    }

    public class SaveAsExcelWorkbook : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSOffice.dll", "Excel.SaveAsExcelWorkbook",
                "Excel.SaveAsExcelWorkbook@var,string");
        }
    }
}
