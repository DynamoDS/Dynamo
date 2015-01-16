using Migrations;
using Dynamo.Models;
using System.Linq;
using System.Xml;

namespace Dynamo.Nodes
{
    public class NewExcelWorkbook : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSOffice.dll", /*NXLT*/"Excel.NewExcelWorkbook",
                /*NXLT*/"Excel.NewExcelWorkbook");
        }
    }

    public class ReadExcelFile : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSOffice.dll", /*NXLT*/"Excel.ReadExcelFile",
                /*NXLT*/"Excel.ReadExcelFile@string");
        }
    }

    public class GetWorksheetsFromExcelWorkbook : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSOffice.dll", /*NXLT*/"Excel.GetWorksheetsFromExcelWorkbook",
                /*NXLT*/"Excel.GetWorksheetsFromExcelWorkbook@var");
        }
    }

    public class GetExcelWorksheetByName : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSOffice.dll", /*NXLT*/"Excel.GetExcelWorksheetByName",
                /*NXLT*/"Excel.GetExcelWorksheetByName@var,string");
        }
    }

    public class GetDataFromExcelWorksheet : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSOffice.dll",/*NXLT*/"Excel.GetDataFromExcelWorksheet",
                /*NXLT*/"Excel.GetDataFromExcelWorksheet@var");
        }
    }

    public class WriteDataToExcelWorksheet : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, /*NXLT*/"DSOffice.dll",
                /*NXLT*/"Excel.WriteDataToExcelWorksheet",
                /*NXLT*/"Excel.WriteDataToExcelWorksheet@var,int,int,var[][]");

            migrationData.AppendNode(newNode);

            // Add default values
            foreach (XmlNode child in oldNode.ChildNodes)
                newNode.AppendChild(child.Clone());

            return migrationData;
        }
    }

    public class AddExcelWorksheetToWorkbook : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSOffice.dll", /*NXLT*/"Excel.AddExcelWorksheetToWorkbook",
                /*NXLT*/"Excel.AddExcelWorksheetToWorkbook@var,string");
        }
    }

    public class SaveAsExcelWorkbook : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, /*NXLT*/"DSOffice.dll", /*NXLT*/"Excel.SaveAsExcelWorkbook",
                /*NXLT*/"Excel.SaveAsExcelWorkbook@var,string");
        }
    }
}
