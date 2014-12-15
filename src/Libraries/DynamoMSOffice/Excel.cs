using Dynamo.Models;

namespace Dynamo.Nodes
{
    [NodeName("New Excel Workbook")]
    [NodeCategory("Input/Output.Office.Excel")]
    [NodeDescription("NewExelWorkbookDescription",typeof(Properties.Resources))]
    public class NewExcelWorkbook : NodeModel
    {
        public NewExcelWorkbook(WorkspaceModel workspace)
            : base(workspace)
        {
            OutPortData.Add(new PortData("workbook", "The new Excel Workbook "));
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
    [NodeDescription("OpenExelWorkbookDescription",typeof(Properties.Resources))]
    public class ReadExcelFile : FileReaderBase
    {

        public ReadExcelFile(WorkspaceModel workspace) : base(workspace)
        {
            OutPortData.Add(new PortData("workbook", "The workbook opened from the file"));
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
    [NodeDescription("GetWorksheetsFromExcelWorkbookDescription", typeof(Properties.Resources))]
    public class GetWorksheetsFromExcelWorkbook : NodeModel
    {

        public GetWorksheetsFromExcelWorkbook(WorkspaceModel workspace)
            : base(workspace)
        {
            InPortData.Add(new PortData("workbook", "The excel workbook"));
            OutPortData.Add(new PortData("worksheets", "A list of worksheets"));
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
    [NodeDescription("GetExcelWorksheetByName", typeof(Properties.Resources))]
    public class GetExcelWorksheetByName : NodeModel
    {

        public GetExcelWorksheetByName(WorkspaceModel workspace)
            : base(workspace)
        {
            InPortData.Add(new PortData("workbook", "The excel workbook"));
            InPortData.Add(new PortData("name", "Name of the worksheet to get"));
            OutPortData.Add(new PortData("worksheet", "The worksheet with the given name"));
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
    [NodeDescription("GetDataFromExcelWorksheetDescription", typeof(Properties.Resources))]
    public class GetDataFromExcelWorksheet : NodeModel
    {

        public GetDataFromExcelWorksheet(WorkspaceModel workspace)
            : base(workspace)
        {
            InPortData.Add(new PortData("worksheet", "The excel workbook"));
            OutPortData.Add(new PortData("worksheet", "The worksheet with the given name"));
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
    [NodeDescription("WriteDataToExcelWorksheetDescription", typeof(Properties.Resources))]
    public class WriteDataToExcelWorksheet : NodeModel
    {

        public WriteDataToExcelWorksheet(WorkspaceModel workspace)
            : base(workspace)
        {
            InPortData.Add(new PortData("worksheet", "The Excel Worksheet to write to."));
            InPortData.Add(new PortData("start row", "Row index to insert data.", 0));
            InPortData.Add(new PortData("start column", "Column index to insert data.", 0));
            InPortData.Add(new PortData("data", "A single item, a 1d list, or a 2d list to write to the worksheet"));

            OutPortData.Add(new PortData("worksheet", "The modified excel worksheet"));

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
    [NodeDescription("AddExcelWorksheetToWorkbookDescription", typeof(Properties.Resources))]
    public class AddExcelWorksheetToWorkbook : NodeModel
    {

        public AddExcelWorksheetToWorkbook(WorkspaceModel workspace)
            : base(workspace)
        {
            InPortData.Add(new PortData("workbook", "The Excel Worksheet to write to"));
            InPortData.Add(new PortData("name", "Name of new Worksheet to add"));

            OutPortData.Add(new PortData("worksheet", "The Worksheet newly added to the Workbook"));

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
    [NodeDescription("SaveExcelWorkbookAsDescription", typeof(Properties.Resources))]
    public class SaveAsExcelWorkbook : NodeModel
    {
        public SaveAsExcelWorkbook(WorkspaceModel workspace)
            : base(workspace)
        {
            InPortData.Add(new PortData("workbook", "The Excel Workbook to save"));
            InPortData.Add(new PortData("filename", "Filename to save the Workbook to"));

            OutPortData.Add(new PortData("workbook", "The Excel Workbook"));

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
