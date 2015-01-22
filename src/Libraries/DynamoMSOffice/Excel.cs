/*<<<<<<< HEAD
﻿﻿using Dynamo.Models;
using DynamoMSOffice.Properties;

namespace Dynamo.Nodes
{
    [NodeName("New Excel Workbook")]
    [NodeCategory("Input/Output.Office.Excel")]
    [NodeDescription("NewExelWorkbookDescription", typeof(Resources))]
    public class NewExcelWorkbook : NodeModel
    {
        public NewExcelWorkbook(WorkspaceModel workspace)
            : base(workspace)
        {
            OutPortData.Add(new PortData("workbook", Resources.NewExcelWorkBookPortDataOutputToolTip));
            RegisterAllPorts();
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSOffice.dll", "Excel.NewExcelWorkbook",
                "Excel.NewExcelWorkbook");
        }
    }

    [NodeName("Open Excel Workbook")]
    [NodeCategory("Input/Output.Office.Excel")]
    [NodeDescription("OpenExelWorkbookDescription", typeof(Resources))]
    public class ReadExcelFile : FileReaderBase
    {

        public ReadExcelFile(WorkspaceModel workspace)
            : base(workspace)
        {
            OutPortData.Add(new PortData("workbook", Resources.ReadExcelFilePortDataOutputToolTip));
            RegisterAllPorts();
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSOffice.dll", "Excel.ReadExcelFile",
                "Excel.ReadExcelFile@string");
        }
    }

    [NodeName("Get Worksheets From Excel Workbook")]
    [NodeCategory("Input/Output.Office.Excel")]
    [NodeDescription("GetWorksheetsFromExcelWorkbookDescription", typeof(Resources))]
    public class GetWorksheetsFromExcelWorkbook : NodeModel
    {

        public GetWorksheetsFromExcelWorkbook(WorkspaceModel workspace)
            : base(workspace)
        {
            InPortData.Add(new PortData("workbook", Resources.PortDataWorkbookToolTip));
            OutPortData.Add(new PortData("worksheets", Resources.PortDataWorkSheetsToolTip));
            RegisterAllPorts();
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSOffice.dll", "Excel.GetWorksheetsFromExcelWorkbook",
                "Excel.GetWorksheetsFromExcelWorkbook@var");
        }
    }

    [NodeName("Get Excel Worksheet By Name")]
    [NodeCategory("Input/Output.Office.Excel")]
    [NodeDescription("GetExcelWorksheetByName", typeof(Resources))]
    public class GetExcelWorksheetByName : NodeModel
    {

        public GetExcelWorksheetByName(WorkspaceModel workspace)
            : base(workspace)
        {
            InPortData.Add(new PortData("workbook", Resources.PortDataWorkbookToolTip));
            InPortData.Add(new PortData("name", Resources.PortDataWorkSheetNameToolTip));
            OutPortData.Add(new PortData("worksheet", Resources.PortDataWorkSheetToolTip));
            RegisterAllPorts();
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSOffice.dll", "Excel.GetExcelWorksheetByName",
                "Excel.GetExcelWorksheetByName@var,string");
        }
    }

    [NodeName("Get Data From Excel Worksheet")]
    [NodeCategory("Input/Output.Office.Excel")]
    [NodeDescription("GetDataFromExcelWorksheetDescription", typeof(Resources))]
    public class GetDataFromExcelWorksheet : NodeModel
    {

        public GetDataFromExcelWorksheet(WorkspaceModel workspace)
            : base(workspace)
        {
            InPortData.Add(new PortData("workbook", Resources.PortDataWorkbookToolTip));
            OutPortData.Add(new PortData("worksheet", Resources.PortDataWorkSheetToolTip));
            RegisterAllPorts();
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSOffice.dll", "Excel.GetDataFromExcelWorksheet",
                "Excel.GetDataFromExcelWorksheet@var");
        }
    }

    [NodeName("Write Data To Excel Worksheet")]
    [NodeCategory("Input/Output.Office.Excel")]
    [NodeDescription("WriteDataToExcelWorksheetDescription", typeof(Resources))]
    public class WriteDataToExcelWorksheet : NodeModel
    {

        public WriteDataToExcelWorksheet(WorkspaceModel workspace)
            : base(workspace)
        {
            InPortData.Add(new PortData("worksheet", Resources.PortDataExcelWorkbookToWriteToToolTip));
            InPortData.Add(new PortData("start row", Resources.PortDataStartRowToolTip, 0));
            InPortData.Add(new PortData("start column", Resources.PortDataStartColumnToolTip, 0));
            InPortData.Add(new PortData("data", Resources.PortDataDataToolTip));

            OutPortData.Add(new PortData("worksheet", Resources.PortDataModifiedWorkSheetToolTip));

            RegisterAllPorts();
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSOffice.dll", "Excel.WriteDataToExcelWorksheet",
                "Excel.WriteDataToExcelWorksheet@var,int,int,var[][]");
        }
    }

    [NodeName("Add Excel Worksheet To Workbook")]
    [NodeCategory("Input/Output.Office.Excel")]
    [NodeDescription("AddExcelWorksheetToWorkbookDescription", typeof(Resources))]
    public class AddExcelWorksheetToWorkbook : NodeModel
    {

        public AddExcelWorksheetToWorkbook(WorkspaceModel workspace)
            : base(workspace)
        {
            InPortData.Add(new PortData("workbook", Resources.PortDataExcelWorkbookToWriteToToolTip));
            InPortData.Add(new PortData("name", Resources.PortDataNewWorkSheetNameToolTip));

            OutPortData.Add(new PortData("worksheet", Resources.PortDataNewlyAddedWorkbookToolTip));

            RegisterAllPorts();
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "DSOffice.dll", "Excel.AddExcelWorksheetToWorkbook",
                "Excel.AddExcelWorksheetToWorkbook@var,string");
        }
    }

    [NodeName("Save Excel Workbook As")]
    [NodeCategory("Input/Output.Office.Excel")]
    [NodeDescription("SaveExcelWorkbookAsDescription", typeof(Resources))]
    public class SaveAsExcelWorkbook : NodeModel
    {
        public SaveAsExcelWorkbook(WorkspaceModel workspace)
            : base(workspace)
        {
            InPortData.Add(new PortData("workbook", Resources.PortDataExcelWorkbookToSaveToolTip));
            InPortData.Add(new PortData("filename", Resources.PortDataWorkbookFileNameToolTip));

            OutPortData.Add(new PortData("workbook", Resources.PortDataWorkbookToolTip));

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
=======
﻿
>>>>>>> master
*/