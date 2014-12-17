﻿using Dynamo.Models;
using DynamoMSOffice.Properties;

namespace Dynamo.Nodes
{
    [NodeName(/*NXLT*/"New Excel Workbook")]
    [NodeCategory("Input/Output.Office.Excel")]
    [NodeDescription(/*NXLT*/"NewExelWorkbookDescription", typeof(Resources))]
    public class NewExcelWorkbook : NodeModel
    {
        public NewExcelWorkbook(WorkspaceModel workspace)
            : base(workspace)
        {
            OutPortData.Add(new PortData(/*NXLT*/"workbook", Resources.NewExcelWorkBookPortDataOutputToolTip));
            RegisterAllPorts();
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSOffice.dll", "Excel.NewExcelWorkbook",
                "Excel.NewExcelWorkbook");
        }
    }

    [NodeName(/*NXLT*/"Open Excel Workbook")]
    [NodeCategory("Input/Output.Office.Excel")]
    [NodeDescription(/*NXLT*/"OpenExelWorkbookDescription", typeof(Resources))]
    public class ReadExcelFile : FileReaderBase
    {

        public ReadExcelFile(WorkspaceModel workspace)
            : base(workspace)
        {
            OutPortData.Add(new PortData(/*NXLT*/"workbook", Resources.ReadExcelFilePortDataOutputToolTip));
            RegisterAllPorts();
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSOffice.dll", "Excel.ReadExcelFile",
                "Excel.ReadExcelFile@string");
        }
    }

    [NodeName(/*NXLT*/"Get Worksheets From Excel Workbook")]
    [NodeCategory("Input/Output.Office.Excel")]
    [NodeDescription(/*NXLT*/"GetWorksheetsFromExcelWorkbookDescription", typeof(Resources))]
    public class GetWorksheetsFromExcelWorkbook : NodeModel
    {

        public GetWorksheetsFromExcelWorkbook(WorkspaceModel workspace)
            : base(workspace)
        {
            InPortData.Add(new PortData(/*NXLT*/"workbook", Resources.PortDataWorkbookToolTip));
            OutPortData.Add(new PortData(/*NXLT*/"worksheets", Resources.PortDataWorkSheetsToolTip));
            RegisterAllPorts();
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSOffice.dll", "Excel.GetWorksheetsFromExcelWorkbook",
                "Excel.GetWorksheetsFromExcelWorkbook@var");
        }
    }

    [NodeName(/*NXLT*/"Get Excel Worksheet By Name")]
    [NodeCategory("Input/Output.Office.Excel")]
    [NodeDescription(/*NXLT*/"GetExcelWorksheetByName", typeof(Resources))]
    public class GetExcelWorksheetByName : NodeModel
    {

        public GetExcelWorksheetByName(WorkspaceModel workspace)
            : base(workspace)
        {
            InPortData.Add(new PortData(/*NXLT*/"workbook", Resources.PortDataWorkbookToolTip));
            InPortData.Add(new PortData(/*NXLT*/"name", Resources.PortDataWorkSheetNameToolTip));
            OutPortData.Add(new PortData(/*NXLT*/"worksheet", Resources.PortDataWorkSheetToolTip));
            RegisterAllPorts();
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSOffice.dll", "Excel.GetExcelWorksheetByName",
                "Excel.GetExcelWorksheetByName@var,string");
        }
    }

    [NodeName(/*NXLT*/"Get Data From Excel Worksheet")]
    [NodeCategory("Input/Output.Office.Excel")]
    [NodeDescription(/*NXLT*/"GetDataFromExcelWorksheetDescription", typeof(Resources))]
    public class GetDataFromExcelWorksheet : NodeModel
    {

        public GetDataFromExcelWorksheet(WorkspaceModel workspace)
            : base(workspace)
        {
            InPortData.Add(new PortData(/*NXLT*/"workbook", Resources.PortDataWorkbookToolTip));
            OutPortData.Add(new PortData(/*NXLT*/"worksheet", Resources.PortDataWorkSheetToolTip));
            RegisterAllPorts();
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSOffice.dll", "Excel.GetDataFromExcelWorksheet",
                "Excel.GetDataFromExcelWorksheet@var");
        }
    }

    [NodeName(/*NXLT*/"Write Data To Excel Worksheet")]
    [NodeCategory("Input/Output.Office.Excel")]
    [NodeDescription(/*NXLT*/"WriteDataToExcelWorksheetDescription", typeof(Resources))]
    public class WriteDataToExcelWorksheet : NodeModel
    {

        public WriteDataToExcelWorksheet(WorkspaceModel workspace)
            : base(workspace)
        {
            InPortData.Add(new PortData(/*NXLT*/"worksheet", Resources.PortDataExcelWorkbookToWriteToToolTip));
            InPortData.Add(new PortData(/*NXLT*/"start row", Resources.PortDataStartRowToolTip, 0));
            InPortData.Add(new PortData(/*NXLT*/"start column", Resources.PortDataStartColumnToolTip, 0));
            InPortData.Add(new PortData(/*NXLT*/"data", Resources.PortDataDataToolTip));

            OutPortData.Add(new PortData(/*NXLT*/"worksheet", Resources.PortDataModifiedWorkSheetToolTip));

            RegisterAllPorts();
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSOffice.dll", "Excel.WriteDataToExcelWorksheet",
                "Excel.WriteDataToExcelWorksheet@var,int,int,var[][]");
        }
    }

    [NodeName(/*NXLT*/"Add Excel Worksheet To Workbook")]
    [NodeCategory("Input/Output.Office.Excel")]
    [NodeDescription(/*NXLT*/"AddExcelWorksheetToWorkbookDescription", typeof(Resources))]
    public class AddExcelWorksheetToWorkbook : NodeModel
    {

        public AddExcelWorksheetToWorkbook(WorkspaceModel workspace)
            : base(workspace)
        {
            InPortData.Add(new PortData(/*NXLT*/"workbook", Resources.PortDataExcelWorkbookToWriteToToolTip));
            InPortData.Add(new PortData(/*NXLT*/"name", Resources.PortDataNewWorkSheetNameToolTip));

            OutPortData.Add(new PortData(/*NXLT*/"worksheet", Resources.PortDataNewlyAddedWorkbookToolTip));

            RegisterAllPorts();
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSOffice.dll", "Excel.AddExcelWorksheetToWorkbook",
                "Excel.AddExcelWorksheetToWorkbook@var,string");
        }
    }

    [NodeName(/*NXLT*/"Save Excel Workbook As")]
    [NodeCategory("Input/Output.Office.Excel")]
    [NodeDescription(/*NXLT*/"SaveExcelWorkbookAsDescription", typeof(Resources))]
    public class SaveAsExcelWorkbook : NodeModel
    {
        public SaveAsExcelWorkbook(WorkspaceModel workspace)
            : base(workspace)
        {
            InPortData.Add(new PortData(/*NXLT*/"workbook", Resources.PortDataExcelWorkbookToSaveToolTip));
            InPortData.Add(new PortData(/*NXLT*/"filename", Resources.PortDataWorkbookFileNameToolTip));

            OutPortData.Add(new PortData(/*NXLT*/"workbook", Resources.PortDataWorkbookToolTip));

            RegisterAllPorts();
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSOffice.dll", "Excel.SaveAsExcelWorkbook",
                "Excel.SaveAsExcelWorkbook@var,string");
        }
    }
}