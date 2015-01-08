<<<<<<< HEAD
﻿﻿using Dynamo.Models;
using DynamoMSOffice.Properties;

namespace Dynamo.Nodes
{
    [NodeName(/*NXLT*/"New Excel Workbook")]
    [NodeCategory(/*NXLT*/"Input/Output.Office.Excel")]
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
            return MigrateToDsFunction(data, /*NXLT*/"DSOffice.dll", /*NXLT*/"Excel.NewExcelWorkbook",
                /*NXLT*/"Excel.NewExcelWorkbook");
        }
    }

    [NodeName(/*NXLT*/"Open Excel Workbook")]
    [NodeCategory(/*NXLT*/"Input/Output.Office.Excel")]
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
            return MigrateToDsFunction(data, /*NXLT*/"DSOffice.dll", /*NXLT*/"Excel.ReadExcelFile",
                /*NXLT*/"Excel.ReadExcelFile@string");
        }
    }

    [NodeName(/*NXLT*/"Get Worksheets From Excel Workbook")]
    [NodeCategory(/*NXLT*/"Input/Output.Office.Excel")]
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
            return MigrateToDsFunction(data, /*NXLT*/"DSOffice.dll", /*NXLT*/"Excel.GetWorksheetsFromExcelWorkbook",
                /*NXLT*/"Excel.GetWorksheetsFromExcelWorkbook@var");
        }
    }

    [NodeName(/*NXLT*/"Get Excel Worksheet By Name")]
    [NodeCategory(/*NXLT*/"Input/Output.Office.Excel")]
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
            return MigrateToDsFunction(data, /*NXLT*/"DSOffice.dll", /*NXLT*/"Excel.GetExcelWorksheetByName",
                /*NXLT*/"Excel.GetExcelWorksheetByName@var,string");
        }
    }

    [NodeName(/*NXLT*/"Get Data From Excel Worksheet")]
    [NodeCategory(/*NXLT*/"Input/Output.Office.Excel")]
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
            return MigrateToDsFunction(data, /*NXLT*/"DSOffice.dll", /*NXLT*/"Excel.GetDataFromExcelWorksheet",
                /*NXLT*/"Excel.GetDataFromExcelWorksheet@var");
        }
    }

    [NodeName(/*NXLT*/"Write Data To Excel Worksheet")]
    [NodeCategory(/*NXLT*/"Input/Output.Office.Excel")]
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
            return MigrateToDsFunction(data, /*NXLT*/"DSOffice.dll", /*NXLT*/"Excel.WriteDataToExcelWorksheet",
                /*NXLT*/"Excel.WriteDataToExcelWorksheet@var,int,int,var[][]");
        }
    }

    [NodeName(/*NXLT*/"Add Excel Worksheet To Workbook")]
    [NodeCategory(/*NXLT*/"Input/Output.Office.Excel")]
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
            return MigrateToDsFunction(data, /*NXLT*/"DSOffice.dll", /*NXLT*/"Excel.AddExcelWorksheetToWorkbook",
                /*NXLT*/"Excel.AddExcelWorksheetToWorkbook@var,string");
        }
    }

    [NodeName(/*NXLT*/"Save Excel Workbook As")]
    [NodeCategory(/*NXLT*/"Input/Output.Office.Excel")]
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
            return MigrateToDsFunction(data, /*NXLT*/"DSOffice.dll", /*NXLT*/"Excel.SaveAsExcelWorkbook",
                /*NXLT*/"Excel.SaveAsExcelWorkbook@var,string");
        }
    }
}
=======
﻿
>>>>>>> master
